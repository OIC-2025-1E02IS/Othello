using Othello.Core;

namespace Othello.Tests;

public class PositionTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(7, 7, true)]
    [InlineData(0, 7, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(8, 0, false)]
    [InlineData(0, 8, false)]
    public void IsOnBoard_境界値を正しく判定する(int row, int col, bool expected)
    {
        Assert.Equal(expected, new Position(row, col).IsOnBoard);
    }

    [Theory]
    [InlineData(0, 0, "a1")]
    [InlineData(7, 7, "h8")]
    [InlineData(2, 3, "d3")]
    [InlineData(-1, 8, "(-1,8)")]
    public void ToString_座標の表記を返す(int row, int col, string expected)
    {
        Assert.Equal(expected, new Position(row, col).ToString());
    }
}
