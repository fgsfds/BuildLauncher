import type {SquareValues} from "./Helpers.ts";

export function Square({value, onSquareClick}: { value: SquareValues, onSquareClick: () => void }) {
    return (
        <button className="square" onClick={onSquareClick}>
            {value}
        </button>
    );
}