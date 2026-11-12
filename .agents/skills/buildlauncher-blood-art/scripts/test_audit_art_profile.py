"""Behavioral checks for malformed resources and the measured export profile."""

from pathlib import Path
import struct
import tempfile
import unittest

from PIL import Image

from audit_art_profile import audit, ProfileError


class AuditProfileTests(unittest.TestCase):
    """Exercise fidelity and rejection using isolated tiny resource packages."""

    def setUp(self):
        """Create a nonsquare, animated tile with signed offsets and transparency."""
        self.temp = tempfile.TemporaryDirectory(prefix='bl-art-audit-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source = self.root / 'source'
        self.output = self.root / 'output'
        self.source.mkdir()
        (self.output / 'tiles').mkdir(parents=True)
        # Tile 4 is animated forward; the column-major image has transparent pixels.
        meta = 1 | (2 << 6) | (255 << 8) | (2 << 16) | (4 << 24)
        self.art = struct.pack('<4i2hI', 1, 1, 4, 4, 2, 3, meta) + bytes([0, 1, 255, 3, 4, 5])
        (self.source / 'TILES001.ART').write_bytes(self.art)
        (self.source / 'SURFACE.DAT').write_bytes(bytes([0, 0, 0, 0, 3]))
        self.palette = bytes(index for index in range(256) for _ in range(3))
        self.palette_path = self.root / 'game.act'
        self.palette_path.write_bytes(self.palette)
        self.png = Image.frombytes('P', (2, 3), bytes([1, 4, 2, 5, 0, 6]))
        self.png.putpalette(self.palette[-3:] + self.palette[:-3])
        self.png.save(self.output / 'tiles/4.png', transparency=0)
        self.definition = 'tilefromtexture 4 { file "tiles/4.png" xoffset -1 yoffset 2 surface 3 }\nanimtilerange 4 5 4 2\n'
        (self.output / 'test.def').write_text(self.definition)

    def check(self):
        """Audit the temporary package against its supplied palette."""
        return audit(self.source, self.output, 'test.def', self.palette_path)

    def test_correct_orientation_metadata_and_palette(self):
        """A valid package preserves pixels, offsets, surfaces and animations."""
        result = self.check()
        self.assertEqual('passed', result['resource_profile_checks'])
        self.assertEqual(1, result['nonzero_offsets'])
        self.assertEqual(1, result['animations'])

    def test_corrupt_art_rejected(self):
        """Truncated pixels and overlapping ranges fail before success is reported."""
        (self.source / 'TILES001.ART').write_bytes(self.art[:-1])
        with self.assertRaisesRegex(ProfileError, 'Truncated ART pixels'):
            self.check()
        (self.source / 'TILES001.ART').write_bytes(self.art)
        (self.source / 'TILES002.ART').write_bytes(self.art)
        with self.assertRaisesRegex(ProfileError, 'Overlapping ART range'):
            self.check()

    def test_resource_fidelity_failures(self):
        """Reject incorrect metadata, duplicate IDs, animation and escaping references."""
        cases = [('xoffset -1', 'xoffset 1', 'Offset mismatch'),
                 ('surface 3', 'surface 2', 'Surface mismatch'),
                 ('4 5 4 2', '4 6 4 2', 'Animation definitions differ'),
                 ('tiles/4.png', '../outside.png', 'Escaping resource path')]
        for before, after, error in cases:
            with self.subTest(error=error):
                (self.output / 'test.def').write_text(self.definition.replace(before, after))
                with self.assertRaisesRegex(ProfileError, error):
                    self.check()
        (self.output / 'test.def').write_text(self.definition + self.definition)
        with self.assertRaisesRegex(ProfileError, 'Duplicate DEF tile'):
            self.check()

    def test_present_short_surface_table_rejected(self):
        """A present short DAT must not silently become a zero surface value."""
        (self.source / 'SURFACE.DAT').write_bytes(bytes(4))
        (self.output / 'test.def').write_text(self.definition.replace('surface 3', 'surface 0'))
        with self.assertRaisesRegex(ProfileError, 'SURFACE.DAT does not cover'):
            self.check()

    def test_pixel_palette_and_voxel_failures(self):
        """Reject changed pixels/palette and unsupported voxel data explicitly."""
        self.png.putpixel((0, 0), 7)
        self.png.save(self.output / 'tiles/4.png', transparency=0)
        with self.assertRaisesRegex(ProfileError, 'Pixel mismatch'):
            self.check()
        self.png.putpixel((0, 0), 1)
        self.png.putpalette(bytes(768))
        self.png.save(self.output / 'tiles/4.png', transparency=0)
        with self.assertRaisesRegex(ProfileError, 'Game palette mismatch'):
            self.check()
        (self.source / 'VOXEL.DAT').write_bytes(b'\xff\xff')
        with self.assertRaisesRegex(ProfileError, 'VOXEL.DAT requires'):
            self.check()


if __name__ == '__main__':
    unittest.main()
