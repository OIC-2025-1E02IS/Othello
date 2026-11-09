namespace Othello.Core;

/// <summary>手番・パス・終了判定・待ったを管理する1ゲーム。</summary>
public sealed class Game
{
    private readonly Stack<Snapshot> history = new();

    /// <summary>通常の開始盤面・黒の先手で始める。</summary>
    public Game()
        : this(Board.CreateInitial(), Stone.Black)
    {
    }

    /// <summary>指定した盤面と手番から始める（テストや途中局面用）。</summary>
    public Game(Board board, Stone firstPlayer)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (!firstPlayer.IsPlayer())
        {
            throw new ArgumentException("先手は黒か白を指定してください。", nameof(firstPlayer));
        }

        Board = board.Clone();
        CurrentPlayer = firstPlayer;
        ResolveTurn();
    }

    public Board Board { get; private set; }

    /// <summary>いまの手番。</summary>
    public Stone CurrentPlayer { get; private set; }

    public bool IsGameOver { get; private set; }

    /// <summary>待ったができるかどうか。</summary>
    public bool CanUndo => history.Count > 0;

    /// <summary>勝者。引き分けは Stone.Empty。</summary>
    /// <exception cref="InvalidOperationException">ゲームが終わっていないとき。</exception>
    public Stone Winner
    {
        get
        {
            if (!IsGameOver)
            {
                throw new InvalidOperationException("ゲームはまだ終わっていません。");
            }

            var black = Board.Count(Stone.Black);
            var white = Board.Count(Stone.White);
            return black > white ? Stone.Black : white > black ? Stone.White : Stone.Empty;
        }
    }

    /// <summary>いまの手番のプレイヤーとして石を置く。</summary>
    public MoveResult PlayMove(Position position)
    {
        var player = CurrentPlayer;
        if (IsGameOver)
        {
            return MoveResult.Failed(position, player, "ゲームは終了しています。");
        }

        if (!position.IsOnBoard)
        {
            return MoveResult.Failed(position, player, "盤の外には置けません。");
        }

        if (Board[position] != Stone.Empty)
        {
            return MoveResult.Failed(position, player, "すでに石があります。");
        }

        if (!Board.IsValidMove(position, player))
        {
            return MoveResult.Failed(position, player, "相手の石をはさめない場所には置けません。");
        }

        history.Push(new Snapshot(Board.Clone(), CurrentPlayer, IsGameOver));
        var flipped = Board.Place(position, player);
        CurrentPlayer = player.Opponent();
        var passed = ResolveTurn();
        return MoveResult.Succeeded(position, player, flipped, passed, IsGameOver);
    }

    /// <summary>1手前に戻す。戻せたら true。</summary>
    public bool Undo()
    {
        if (history.Count == 0)
        {
            return false;
        }

        var snapshot = history.Pop();
        Board = snapshot.Board;
        CurrentPlayer = snapshot.CurrentPlayer;
        IsGameOver = snapshot.IsGameOver;
        return true;
    }

    /// <summary>手番の人が置けなければパス、両者置けなければ終了にする。パスした人を返す。</summary>
    private Stone? ResolveTurn()
    {
        if (Board.HasValidMove(CurrentPlayer))
        {
            return null;
        }

        var opponent = CurrentPlayer.Opponent();
        if (Board.HasValidMove(opponent))
        {
            var passed = CurrentPlayer;
            CurrentPlayer = opponent;
            return passed;
        }

        IsGameOver = true;
        return null;
    }

    private sealed record Snapshot(Board Board, Stone CurrentPlayer, bool IsGameOver);
}
