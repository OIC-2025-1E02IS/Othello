namespace Othello.Core;

/// <summary>かんたんな CPU。一番多くひっくり返せる場所を選ぶ。</summary>
public static class CpuPlayer
{
    /// <summary>
    /// 置く場所を選ぶ。同じ枚数なら上の行・左の列を優先する。置ける場所がなければ null。
    /// </summary>
    public static Position? ChooseMove(Board board, Stone player)
    {
        ArgumentNullException.ThrowIfNull(board);

        Position? best = null;
        var bestCount = 0;
        foreach (var move in board.GetValidMoves(player))
        {
            var count = board.GetFlips(move, player).Count;
            if (count > bestCount)
            {
                best = move;
                bestCount = count;
            }
        }

        return best;
    }
}
