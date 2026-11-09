using Othello.Core;

namespace Othello.Tests;

public class BoardTests
{
    // ---------- 正常系 ----------

    [Fact]
    public void CreateInitial_中央に4つの石がある()
    {
        var board = Board.CreateInitial();

        Assert.Equal(Stone.White, board[3, 3]);
        Assert.Equal(Stone.White, board[4, 4]);
        Assert.Equal(Stone.Black, board[3, 4]);
        Assert.Equal(Stone.Black, board[4, 3]);
        Assert.Equal(2, board.Count(Stone.Black));
        Assert.Equal(2, board.Count(Stone.White));
        Assert.Equal(60, board.Count(Stone.Empty));
    }

    [Fact]
    public void GetValidMoves_初期盤面の黒は4か所()
    {
        var moves = Board.CreateInitial().GetValidMoves(Stone.Black);

        Assert.Equal(
            new[] { new Position(2, 3), new Position(3, 2), new Position(4, 5), new Position(5, 4) },
            moves);
    }

    [Fact]
    public void GetValidMoves_初期盤面の白は4か所()
    {
        var moves = Board.CreateInitial().GetValidMoves(Stone.White);

        Assert.Equal(
            new[] { new Position(2, 4), new Position(3, 5), new Position(4, 2), new Position(5, 3) },
            moves);
    }

    [Fact]
    public void Place_はさんだ石がひっくり返る()
    {
        var board = Board.CreateInitial();

        var flipped = board.Place(new Position(2, 3), Stone.Black);

        Assert.Equal(new Position(3, 3), Assert.Single(flipped));
        Assert.Equal(Stone.Black, board[2, 3]);
        Assert.Equal(Stone.Black, board[3, 3]);
        Assert.Equal(4, board.Count(Stone.Black));
        Assert.Equal(1, board.Count(Stone.White));
    }

    [Fact]
    public void Place_複数の方向をまとめてひっくり返す()
    {
        var board = BoardFactory.FromRows(
            "B.B.....",
            ".WW.....",
            "BW.WWB..");

        var flipped = board.Place(new Position(2, 2), Stone.Black);

        Assert.Equal(5, flipped.Count);
        Assert.Contains(new Position(2, 1), flipped); // 左
        Assert.Contains(new Position(1, 2), flipped); // 上
        Assert.Contains(new Position(1, 1), flipped); // 左上
        Assert.Contains(new Position(2, 3), flipped); // 右（2枚）
        Assert.Contains(new Position(2, 4), flipped);
        Assert.Equal(0, board.Count(Stone.White));
    }

    [Fact]
    public void Clone_元の盤面と独立している()
    {
        var board = Board.CreateInitial();
        var copy = board.Clone();

        copy.Place(new Position(2, 3), Stone.Black);

        Assert.Equal(2, board.Count(Stone.Black));
        Assert.Equal(4, copy.Count(Stone.Black));
    }

    // ---------- 境界値 ----------

    [Fact]
    public void GetFlips_角に置いて辺の石をはさめる()
    {
        var board = BoardFactory.FromRows(".WWB....");

        var flips = board.GetFlips(new Position(0, 0), Stone.Black);

        Assert.Equal(new[] { new Position(0, 1), new Position(0, 2) }, flips);
    }

    [Fact]
    public void GetFlips_盤の端まで相手の石が続くとはさめない()
    {
        var board = BoardFactory.FromRows(".....WWW");

        Assert.Empty(board.GetFlips(new Position(0, 4), Stone.Black));
        Assert.False(board.IsValidMove(new Position(0, 4), Stone.Black));
    }

    [Fact]
    public void GetFlips_自分の石が隣でも相手の石をはさんでいなければ置けない()
    {
        var board = BoardFactory.FromRows(".BW.....");

        Assert.False(board.IsValidMove(new Position(0, 0), Stone.Black));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(8, 0)]
    [InlineData(0, 8)]
    [InlineData(8, 8)]
    public void IsValidMove_盤外はfalse(int row, int col)
    {
        Assert.False(Board.CreateInitial().IsValidMove(new Position(row, col), Stone.Black));
    }

    [Fact]
    public void IsFull_すべて埋まった盤面はtrue()
    {
        var rows = Enumerable.Repeat("BWBWBWBW", Board.Size).ToArray();
        var board = BoardFactory.FromRows(rows);

        Assert.True(board.IsFull);
        Assert.Equal(32, board.Count(Stone.Black));
        Assert.Empty(board.GetValidMoves(Stone.Black));
        Assert.Empty(board.GetValidMoves(Stone.White));
    }

    [Fact]
    public void IsFull_初期盤面はfalse()
    {
        Assert.False(Board.CreateInitial().IsFull);
    }

    // ---------- 異常系 ----------

    [Fact]
    public void IsValidMove_石がある場所には置けない()
    {
        Assert.False(Board.CreateInitial().IsValidMove(new Position(3, 3), Stone.Black));
    }

    [Fact]
    public void Place_置けない場所は例外になり盤面は変わらない()
    {
        var board = Board.CreateInitial();

        Assert.Throws<InvalidOperationException>(() => board.Place(new Position(0, 0), Stone.Black));
        Assert.Equal(Stone.Empty, board[0, 0]);
        Assert.Equal(2, board.Count(Stone.Black));
    }

    [Fact]
    public void Indexer_盤外は例外()
    {
        var board = Board.CreateInitial();

        Assert.Throws<ArgumentOutOfRangeException>(() => board[8, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => board[0, -1]);
    }

    [Fact]
    public void GetFlips_プレイヤーにEmptyを指定すると例外()
    {
        Assert.Throws<ArgumentException>(() => Board.CreateInitial().GetFlips(new Position(2, 3), Stone.Empty));
    }

    [Fact]
    public void SetStone_盤外は例外()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Board().SetStone(new Position(-1, 0), Stone.Black));
    }
}
