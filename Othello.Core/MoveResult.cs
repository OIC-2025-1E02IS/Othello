namespace Othello.Core;

/// <summary>1手打った結果。</summary>
/// <param name="Success">置けたかどうか。</param>
/// <param name="Position">置こうとした位置。</param>
/// <param name="Player">置こうとしたプレイヤー。</param>
/// <param name="Flipped">ひっくり返した石。</param>
/// <param name="PassedPlayer">この手のあとにパスになったプレイヤー（なければ null）。</param>
/// <param name="IsGameOver">この手でゲームが終わったかどうか。</param>
/// <param name="ErrorMessage">置けなかった理由（置けたときは null）。</param>
public sealed record MoveResult(
    bool Success,
    Position Position,
    Stone Player,
    IReadOnlyList<Position> Flipped,
    Stone? PassedPlayer,
    bool IsGameOver,
    string? ErrorMessage)
{
    public static MoveResult Succeeded(
        Position position, Stone player, IReadOnlyList<Position> flipped, Stone? passedPlayer, bool isGameOver) =>
        new(true, position, player, flipped, passedPlayer, isGameOver, null);

    public static MoveResult Failed(Position position, Stone player, string errorMessage) =>
        new(false, position, player, Array.Empty<Position>(), null, false, errorMessage);
}
