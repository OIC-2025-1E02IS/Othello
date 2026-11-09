using Othello.Core;

namespace Othello.Tests;

public class GameTests
{
    // ---------- 正常系 ----------

    [Fact]
    public void 新しいゲームは黒の番から始まる()
    {
        var game = new Game();

        Assert.Equal(Stone.Black, game.CurrentPlayer);
        Assert.False(game.IsGameOver);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void PlayMove_置けたら白の番になる()
    {
        var game = new Game();

        var result = game.PlayMove(new Position(2, 3));

        Assert.True(result.Success);
        Assert.Equal(Stone.Black, result.Player);
        Assert.Equal(new Position(3, 3), Assert.Single(result.Flipped));
        Assert.Null(result.PassedPlayer);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(Stone.White, game.CurrentPlayer);
        Assert.True(game.CanUndo);
    }

    [Fact]
    public void PlayMove_相手が置けないときはパスして同じ人の番が続く()
    {
        var game = new Game(
            BoardFactory.FromRows(
                "BW......",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........",
                "BW......"),
            Stone.Black);

        var result = game.PlayMove(new Position(0, 2));

        Assert.True(result.Success);
        Assert.Equal(Stone.White, result.PassedPlayer);
        Assert.False(result.IsGameOver);
        Assert.Equal(Stone.Black, game.CurrentPlayer);
    }

    [Fact]
    public void PlayMove_両者置けなくなったらゲーム終了で石の多い方が勝ち()
    {
        var game = new Game(
            BoardFactory.FromRows(
                "BW......",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........",
                "BW......"),
            Stone.Black);
        game.PlayMove(new Position(0, 2));

        var result = game.PlayMove(new Position(7, 2));

        Assert.True(result.Success);
        Assert.True(result.IsGameOver);
        Assert.True(game.IsGameOver);
        Assert.Equal(Stone.Black, game.Winner);
        Assert.Equal(6, game.Board.Count(Stone.Black));
    }

    [Fact]
    public void Undo_1手前の盤面と手番に戻る()
    {
        var game = new Game();
        game.PlayMove(new Position(2, 3));
        game.PlayMove(new Position(2, 2));

        Assert.True(game.Undo());
        Assert.Equal(Stone.White, game.CurrentPlayer);
        Assert.Equal(4, game.Board.Count(Stone.Black));
        Assert.Equal(1, game.Board.Count(Stone.White));

        Assert.True(game.Undo());
        Assert.Equal(Stone.Black, game.CurrentPlayer);
        Assert.Equal(2, game.Board.Count(Stone.Black));
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void Undo_ゲーム終了後も戻すと続きができる()
    {
        var game = new Game(BoardFactory.FromRows("BW......", "", "", "", "", "", "", "BW......"), Stone.Black);
        game.PlayMove(new Position(0, 2));
        game.PlayMove(new Position(7, 2));

        Assert.True(game.Undo());
        Assert.False(game.IsGameOver);
        Assert.True(game.PlayMove(new Position(7, 2)).Success);
    }

    [Fact]
    public void CPU同士で最後まで対局できる()
    {
        var game = new Game();
        var moves = 0;

        while (!game.IsGameOver && moves < 100)
        {
            if (CpuPlayer.ChooseMove(game.Board, game.CurrentPlayer) is not { } move)
            {
                Assert.Fail("ゲーム中なのに置ける場所がありません。");
                return;
            }

            Assert.True(game.PlayMove(move).Success);
            moves++;
        }

        Assert.True(game.IsGameOver);
        Assert.True(moves <= 60);
        var total = game.Board.Count(Stone.Black) + game.Board.Count(Stone.White);
        Assert.Equal(moves + 4, total);
    }

    // ---------- 境界値 ----------

    [Fact]
    public void 開始時にどちらも置けなければすぐ終了し引き分けを判定できる()
    {
        var board = BoardFactory.FromRows("B.......", "", "", "", "", "", "", ".......W");

        var game = new Game(board, Stone.Black);

        Assert.True(game.IsGameOver);
        Assert.Equal(Stone.Empty, game.Winner);
    }

    [Fact]
    public void 開始時に先手が置けなければ相手の番になる()
    {
        var game = new Game(BoardFactory.FromRows("BW......"), Stone.White);

        Assert.False(game.IsGameOver);
        Assert.Equal(Stone.Black, game.CurrentPlayer);
    }

    // ---------- 異常系 ----------

    [Theory]
    [InlineData(0, 0)] // はさめない
    [InlineData(3, 3)] // 石がある
    [InlineData(8, 8)] // 盤外
    [InlineData(-1, 0)] // 盤外
    public void PlayMove_置けない場所は失敗し状態が変わらない(int row, int col)
    {
        var game = new Game();

        var result = game.PlayMove(new Position(row, col));

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Flipped);
        Assert.Equal(Stone.Black, game.CurrentPlayer);
        Assert.Equal(2, game.Board.Count(Stone.Black));
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void PlayMove_ゲーム終了後は置けない()
    {
        var game = new Game(BoardFactory.FromRows("B.......", "", "", "", "", "", "", ".......W"), Stone.Black);

        var result = game.PlayMove(new Position(4, 4));

        Assert.False(result.Success);
    }

    [Fact]
    public void Undo_履歴がなければfalse()
    {
        Assert.False(new Game().Undo());
    }

    [Fact]
    public void Winner_ゲーム中は例外()
    {
        Assert.Throws<InvalidOperationException>(() => new Game().Winner);
    }

    [Fact]
    public void コンストラクタ_先手にEmptyは指定できない()
    {
        Assert.Throws<ArgumentException>(() => new Game(Board.CreateInitial(), Stone.Empty));
    }

    [Fact]
    public void コンストラクタ_盤面nullは例外()
    {
        Assert.Throws<ArgumentNullException>(() => new Game(null!, Stone.Black));
    }
}
