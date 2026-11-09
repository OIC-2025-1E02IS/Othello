namespace Othello.Core;

/// <summary>盤面上の位置。Row は 0〜7（上から）、Col は 0〜7（左から）。</summary>
public readonly record struct Position(int Row, int Col)
{
    /// <summary>盤の中にある位置かどうか。</summary>
    public bool IsOnBoard => Row is >= 0 and < Board.Size && Col is >= 0 and < Board.Size;

    /// <summary>「a1」〜「h8」の形式で返す（盤外は「(行,列)」）。</summary>
    public override string ToString() =>
        IsOnBoard ? $"{(char)('a' + Col)}{Row + 1}" : $"({Row},{Col})";
}
