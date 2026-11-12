"""Read-only verification of the measured BAFEd ART/PNG/DEF export profile."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import sys

from PIL import Image


class ProfileError(ValueError):
    """A resource violates the supported reference profile."""


def require(condition, message):
    """Reject an invalid resource without writing any files."""
    if not condition:
        raise ProfileError(message)


def within(root, relative):
    """Resolve a resource strictly inside its selected output directory."""
    require(not Path(relative).is_absolute(), f"Absolute resource path: {relative}")
    path = (root / relative).resolve()
    require(path.is_relative_to(root), f"Escaping resource path: {relative}")
    return path


def read_art(path):
    """Decode ART v1 slots, preserving IDs and column-major pixel bytes."""
    data = path.read_bytes()
    require(len(data) >= 16, f"Truncated ART header: {path.name}")
    version, _, start, end = struct.unpack_from('<4i', data)
    require(version == 1 and 0 <= start <= end <= 65535,
            f"Unsupported ART header: {path.name}")
    count = end - start + 1
    offset = 16 + 8 * count
    require(len(data) >= offset, f"Truncated ART arrays: {path.name}")
    widths = struct.unpack_from(f'<{count}h', data, 16)
    heights = struct.unpack_from(f'<{count}h', data, 16 + 2 * count)
    metadata = struct.unpack_from(f'<{count}I', data, 16 + 4 * count)
    slots = {}
    for index, (width, height, meta) in enumerate(zip(widths, heights, metadata)):
        require(width >= 0 and height >= 0, f"Negative dimensions: {path.name}/{start + index}")
        size = width * height
        require(offset + size <= len(data), f"Truncated ART pixels: {path.name}/{start + index}")
        slots[start + index] = (width, height, meta, data[offset:offset + size])
        offset += size
    require(offset == len(data), f"Trailing ART data: {path.name}")
    return slots


def audit(source, output, def_name, palette_path):
    """Check exact reference resources; successful checks do not prove a launch."""
    slots = {}
    art_files = sorted(p for p in source.iterdir() if p.suffix.lower() == '.art')
    require(bool(art_files), 'No root ART files found')
    for path in art_files:
        current = read_art(path)
        require(not slots.keys() & current.keys(), f"Overlapping ART range: {path.name}")
        slots.update(current)
    used = {tile: value for tile, value in slots.items() if value[0] and value[1]}
    require(bool(used), 'No nonempty ART tiles found')
    require(all(value[2] == 0 for value in slots.values() if not value[0] or not value[1]),
            'Empty-slot metadata needs a different profile')
    require(not any(value[2] >> 28 for value in used.values()),
            'Blood view/extra metadata needs a different profile')
    require(not any(((value[2] >> 6) & 3) == 3 for value in used.values()),
            'Backward animation needs a separately verified profile')
    surfaces = [p for p in source.iterdir() if p.name.lower() == 'surface.dat']
    require(len(surfaces) <= 1, 'Ambiguous SURFACE.DAT filename case')
    surface = surfaces[0].read_bytes() if surfaces else b''
    require(not surfaces or len(surface) > max(used),
            'SURFACE.DAT does not cover all exported tile IDs in this strict profile')
    voxel = any(p.name.lower() == 'voxel.dat' for p in source.iterdir())
    require(not voxel, 'VOXEL.DAT requires voxel mapping verification; this profile cannot check it')
    expected_palette = None
    if palette_path:
        palette = palette_path.read_bytes()
        require(len(palette) in (768, 772), 'Expected a 256-color RGB ACT palette')
        expected_palette = palette[765:768] + palette[:765]
    textures, animations = {}, []
    tile_pattern = r'tilefromtexture\s+(\d+)\s*\{\s*file\s+"([^"]+)"\s+xoffset\s+(-?\d+)\s+yoffset\s+(-?\d+)(?:\s+surface\s+(\d+))?\s*\}'
    for line_number, line in enumerate(within(output, def_name).read_text(encoding='utf-8-sig').splitlines(), 1):
        if not line.strip():
            continue
        match = re.fullmatch(tile_pattern, line.strip())
        if match:
            tile, filename, x, y, surf = match.groups()
            tile = int(tile)
            require(tile not in textures, f"Duplicate DEF tile {tile}")
            textures[tile] = (filename, int(x), int(y), int(surf or 0))
            continue
        match = re.fullmatch(r'animtilerange\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)', line.strip())
        require(match is not None, f"Unsupported DEF syntax at line {line_number}")
        animations.append(tuple(map(int, match.groups())))
    require(textures.keys() == used.keys(), 'ART/DEF tile ID sets differ')
    expected_animations, palettes, referenced = [], set(), set()
    rotated_indices = bytes((index + 1) % 256 for index in range(256))
    for tile, (width, height, meta, pixels) in used.items():
        filename, x, y, surf = textures[tile]
        expected_x, expected_y = struct.unpack('bb', bytes([(meta >> 8) & 255, (meta >> 16) & 255]))
        require((x, y) == (expected_x, expected_y), f"Offset mismatch: tile {tile}")
        require(surf == (surface[tile] if tile < len(surface) else 0), f"Surface mismatch: tile {tile}")
        kind = (meta >> 6) & 3
        if kind:
            expected_animations.append((tile, tile + (meta & 63), (meta >> 24) & 15, kind))
        png_path = within(output, filename)
        require(png_path not in referenced, f"PNG reused by multiple tile IDs: {filename}")
        referenced.add(png_path)
        with Image.open(png_path) as png:
            require(png.format == 'PNG' and png.mode == 'P' and png.size == (width, height),
                    f"PNG format/mode/dimensions mismatch: tile {tile}")
            expected_pixels = Image.frombytes('P', (height, width), pixels).transpose(Image.Transpose.TRANSPOSE).tobytes().translate(rotated_indices)
            require(png.tobytes() == expected_pixels, f"Pixel mismatch: tile {tile}")
            require(png.info.get('transparency') == 0, f"Transparency mismatch: tile {tile}")
            png_palette = bytes(png.getpalette()[:768])
            if expected_palette is not None:
                require(png_palette == expected_palette, f"Game palette mismatch: tile {tile}")
            palettes.add(hashlib.sha256(png_palette).hexdigest())
    require(sorted(animations) == sorted(expected_animations), 'Animation definitions differ')
    require(len(palettes) == 1, 'Tile palettes are inconsistent')
    actual_pngs = {p.resolve() for p in (output / 'tiles').rglob('*') if p.suffix.lower() == '.png'}
    require(actual_pngs == referenced, 'Unreferenced or missing PNGs in tiles directory')
    return {'resource_profile_checks': 'passed', 'art_files': len(art_files), 'art_slots': len(slots),
            'png_tiles': len(used), 'empty_slots': len(slots) - len(used), 'animations': len(animations),
            'nonzero_offsets': sum(bool(x or y) for _, x, y, _ in textures.values()),
            'nonzero_surfaces': sum(bool(surf) for _, _, _, surf in textures.values()),
            'omitted_nonzero_surface_entries': sum(bool(value) for tile, value in enumerate(surface) if tile not in used),
            'palette_sha256': next(iter(palettes)), 'palette_reference': str(palette_path) if palette_path else 'not_checked',
            'voxel_dat': 'absent', 'manifest': 'not_checked', 'launch': 'not_checked'}


def main():
    """Print a compact JSON audit and return a process status."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('output', type=Path)
    parser.add_argument('--def', dest='def_name', required=True)
    parser.add_argument('--palette', type=Path, help='Optional source RGB ACT palette')
    args = parser.parse_args()
    try:
        result = audit(args.source.resolve(), args.output.resolve(), args.def_name, args.palette)
    except (ProfileError, OSError, ValueError, struct.error) as error:
        print(json.dumps({'resource_profile_checks': 'failed', 'error': str(error)}, ensure_ascii=False))
        return 1
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == '__main__':
    sys.exit(main())
