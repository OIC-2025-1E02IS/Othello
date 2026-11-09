using Othello.Core;

namespace Othello.Tests;

public class CpuPlayerTests
{
    [Fact]
    public void 一番多くひっくり返せる場所を選ぶ()
    {
        // (2,2) は1枚、(5,0) は2枚ひっくり返せる
        var board = BoardFactory.FromRows(
            "B.......",
            ".W......",
            "........",
            "........",
            "........",
            ".WWB....");

        var move = CpuPlayer.ChooseMove(board, Stone.Black);

        Assert.Equal(new Position(5, 0), move);
    }

    [Fact]
    public void 同じ枚数なら上の行左の列を選ぶ()
    {
        var move = CpuPlayer.ChooseMove(Board.CreateInitial(), Stone.Black);

        Assert.Equal(new Position(2, 3), move);
    }

    [Fact]
    public void 置ける場所がなければnull()
    {
        var board = BoardFactory.FromRows("BBB.....");

        Assert.Null(CpuPlayer.ChooseMove(board, Stone.White));
    }

    [Fact]
    public void 盤面nullは例外()
    {
        Assert.Throws<ArgumentNullException>(() => CpuPlayer.ChooseMove(null!, Stone.Black));
    }
}
