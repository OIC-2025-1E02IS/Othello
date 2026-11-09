using Othello.Core;

namespace Othello.Tests;

/// <summary>文字列から盤面を作るテスト用ヘルパー。'B'=黒、'W'=白、それ以外=空き。</summary>
internal static class BoardFactory
{
    public static Board FromRows(params string[] rows)
    {
        if (rows.Length > Board.Size)
        {
            throw new ArgumentException("行が多すぎます。", nameof(rows));
        }

        var board = new Board();
        for (var row = 0; row < rows.Length; row++)
        {
            for (var col = 0; col < rows[row].Length && col < Board.Size; col++)
            {
                var stone = rows[row][col] switch
                {
                    'B' => Stone.Black,
                    'W' => Stone.White,
                    _ => Stone.Empty,
                };
                board.SetStone(new Position(row, col), stone);
            }
        }

        return board;
    }
}
