namespace Othello.Core;

/// <summary>マスの状態（石の色）。</summary>
public enum Stone
{
    Empty,
    Black,
    White,
}

public static class StoneExtensions
{
    /// <summary>相手の色を返す。</summary>
    public static Stone Opponent(this Stone stone) => stone switch
    {
        Stone.Black => Stone.White,
        Stone.White => Stone.Black,
        _ => throw new ArgumentException("黒か白を指定してください。", nameof(stone)),
    };

    /// <summary>「黒」「白」などの日本語名を返す。</summary>
    public static string ToJapanese(this Stone stone) => stone switch
    {
        Stone.Black => "黒",
        Stone.White => "白",
        _ => "なし",
    };

    /// <summary>黒か白（プレイヤーになれる色）かどうか。</summary>
    public static bool IsPlayer(this Stone stone) => stone is Stone.Black or Stone.White;
}
