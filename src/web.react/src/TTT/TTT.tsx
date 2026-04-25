import {useState} from 'react';
import './App.css'
import {CalculateWinner, type SquareValues} from "./Helpers.ts";
import {Square} from "./Primitives.tsx";


function Board({xIsNext, squares, onPlay}: {
    xIsNext: boolean,
    squares: SquareValues[],
    onPlay: (a: SquareValues[]) => void
}) {

    function handleClick(i: number) {
        if (CalculateWinner(squares) || squares[i]) {
            return;
        }
        const nextSquares = squares.slice();
        if (xIsNext) {
            nextSquares[i] = 'X';
        } else {
            nextSquares[i] = 'O';
        }
        onPlay(nextSquares);
    }

    const winner = CalculateWinner(squares);
    let status;
    if (winner) {
        status = 'Winner: ' + winner;
    } else {
        status = 'Next player: ' + (xIsNext ? 'X' : 'O');
    }

    const renderSquare = (i: number) => (
        <Square key={i}
                value={squares[i]}
                onSquareClick={() => handleClick(i)}
        />
    );

    return (
        <>
            <div>{status}</div>
            {[0, 3, 6].map((rowStart) => (
                <div key={rowStart} className="board-row">
                    {renderSquare(rowStart)}
                    {renderSquare(rowStart + 1)}
                    {renderSquare(rowStart + 2)}
                </div>
            ))}
        </>
    );
}

export default function Game() {
    const [history, setHistory] = useState<SquareValues[][]>([Array(9).fill(null)]);
    const [currentMove, setCurrentMove] = useState<number>(0);

    const xIsNext = currentMove % 2 === 0;
    const currentSquares = history[currentMove];

    function handlePlay(nextSquares: SquareValues[]) {
        const nextHistory = [...history.slice(0, currentMove + 1), nextSquares];
        setHistory(nextHistory);
        setCurrentMove(nextHistory.length - 1);
    }

    function jumpTo(nextMove: number) {
        setCurrentMove(nextMove);
    }

    const moves = history.map((_, move) => {
        let description;
        if (move > 0) {
            description = 'Go to move #' + move;
        } else {
            description = 'Go to game start';
        }
        return (
            <li key={move}>
                <button onClick={() => jumpTo(move)}>{description}</button>
            </li>
        );
    });

    return (
        <div className="game">
            <div className="game-board">
                <Board xIsNext={xIsNext} squares={currentSquares} onPlay={handlePlay}/>
            </div>
            <div className="game-info">
                <ol>{moves}</ol>
            </div>
        </div>
    );
}
