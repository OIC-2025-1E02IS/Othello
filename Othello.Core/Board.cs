namespace Othello.Core;

/// <summary>8×8 の盤面と、石を置く・ひっくり返すルール。</summary>
public sealed class Board
{
    public const int Size = 8;

    private static readonly (int Dr, int Dc)[] Directions =
    {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1), (0, 1),
        (1, -1), (1, 0), (1, 1),
    };

    private readonly Stone[,] cells = new Stone[Size, Size];

    /// <summary>中央に4つの石が置かれた、ゲーム開始時の盤面を作る。</summary>
    public static Board CreateInitial()
    {
        var board = new Board();
        board.cells[3, 3] = Stone.White;
        board.cells[4, 4] = Stone.White;
        board.cells[3, 4] = Stone.Black;
        board.cells[4, 3] = Stone.Black;
        return board;
    }

    public Stone this[Position position]
    {
        get
        {
            EnsureOnBoard(position);
            return cells[position.Row, position.Col];
        }
    }

    public Stone this[int row, int col] => this[new Position(row, col)];

    /// <summary>ルールを無視して石を直接置く（盤面の準備・テスト用）。</summary>
    public void SetStone(Position position, Stone stone)
    {
        EnsureOnBoard(position);
        if (!Enum.IsDefined(stone))
        {
            throw new ArgumentOutOfRangeException(nameof(stone));
        }

        cells[position.Row, position.Col] = stone;
    }

    /// <summary>指定した色の石の数。</summary>
    public int Count(Stone stone)
    {
        var count = 0;
        foreach (var cell in cells)
        {
            if (cell == stone)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>空きマスがないかどうか。</summary>
    public bool IsFull => Count(Stone.Empty) == 0;

    /// <summary>
    /// その位置に置いたときにひっくり返る石の一覧。
    /// 盤外・石がある場所・はさめない場所では空の一覧を返す。
    /// </summary>
    public IReadOnlyList<Position> GetFlips(Position position, Stone player)
    {
        EnsurePlayer(player);
        if (!position.IsOnBoard || cells[position.Row, position.Col] != Stone.Empty)
        {
            return Array.Empty<Position>();
        }

        var opponent = player.Opponent();
        var flips = new List<Position>();
        var line = new List<Position>();

        foreach (var (dr, dc) in Directions)
        {
            line.Clear();
            var current = new Position(position.Row + dr, position.Col + dc);
            while (current.IsOnBoard && cells[current.Row, current.Col] == opponent)
            {
                line.Add(current);
                current = new Position(current.Row + dr, current.Col + dc);
            }

            if (line.Count > 0 && current.IsOnBoard && cells[current.Row, current.Col] == player)
            {
                flips.AddRange(line);
            }
        }

        return flips;
    }

    /// <summary>その位置に置けるかどうか。</summary>
    public bool IsValidMove(Position position, Stone player) => GetFlips(position, player).Count > 0;

    /// <summary>置ける場所の一覧（上の行から順、同じ行は左から順）。</summary>
    public IReadOnlyList<Position> GetValidMoves(Stone player)
    {
        EnsurePlayer(player);
        var moves = new List<Position>();
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                var position = new Position(row, col);
                if (IsValidMove(position, player))
                {
                    moves.Add(position);
                }
            }
        }

        return moves;
    }

    /// <summary>置ける場所が1つでもあるかどうか。</summary>
    public bool HasValidMove(Stone player) => GetValidMoves(player).Count > 0;

    /// <summary>石を置いて、はさんだ石をひっくり返す。ひっくり返した石の一覧を返す。</summary>
    /// <exception cref="InvalidOperationException">置けない場所のとき。</exception>
    public IReadOnlyList<Position> Place(Position position, Stone player)
    {
        var flips = GetFlips(position, player);
        if (flips.Count == 0)
        {
            throw new InvalidOperationException($"{position} には{player.ToJapanese()}を置けません。");
        }

        cells[position.Row, position.Col] = player;
        foreach (var flip in flips)
        {
            cells[flip.Row, flip.Col] = player;
        }

        return flips;
    }

    /// <summary>同じ内容の別の盤面を作る。</summary>
    public Board Clone()
    {
        var copy = new Board();
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                copy.cells[row, col] = cells[row, col];
            }
        }

        return copy;
    }

    private static void EnsureOnBoard(Position position)
    {
        if (!position.IsOnBoard)
        {
            throw new ArgumentOutOfRangeException(nameof(position), $"盤の外です: {position}");
        }
    }

    private static void EnsurePlayer(Stone player)
    {
        if (!player.IsPlayer())
        {
            throw new ArgumentException("黒か白を指定してください。", nameof(player));
        }
    }
}
