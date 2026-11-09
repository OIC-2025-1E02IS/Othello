using System.Drawing.Drawing2D;
using Othello.Core;

namespace Othello
{
    /// <summary>盤面を描画し、マスのクリックを知らせるコントロール。</summary>
    internal sealed class BoardControl : Control
    {
        private const int LabelMargin = 28;

        private static readonly Color BoardColor = Color.FromArgb(0, 128, 72);
        private static readonly Color GridColor = Color.FromArgb(0, 70, 40);
        private static readonly Color HintColor = Color.FromArgb(150, 255, 255, 255);
        private static readonly Color HoverValidColor = Color.FromArgb(90, 255, 255, 160);
        private static readonly Color HoverInvalidColor = Color.FromArgb(60, 255, 80, 80);
        private static readonly Color LastMoveColor = Color.FromArgb(230, 60, 60);

        private readonly HashSet<Position> hints = new();
        private Board? board;
        private Position? lastMove;
        private Position? hover;
        private bool inputEnabled;

        public BoardControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);
            BackColor = Color.FromArgb(32, 36, 40);
        }

        /// <summary>プレイヤーがマスをクリックしたとき。</summary>
        public event EventHandler<Position>? CellClicked;

        /// <summary>表示する内容を更新する。</summary>
        public void UpdateState(Board currentBoard, IEnumerable<Position> validMoves, Position? lastMovePosition, bool acceptInput)
        {
            board = currentBoard;
            hints.Clear();
            hints.UnionWith(validMoves);
            lastMove = lastMovePosition;
            inputEnabled = acceptInput;
            UpdateCursor();
            Invalidate();
        }

        private int CellSize =>
            Math.Max(1, (Math.Min(ClientSize.Width, ClientSize.Height) - (LabelMargin * 2)) / Board.Size);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var cellSize = CellSize;
            var boardSize = cellSize * Board.Size;

            using (var boardBrush = new SolidBrush(BoardColor))
            {
                g.FillRectangle(boardBrush, LabelMargin, LabelMargin, boardSize, boardSize);
            }

            if (inputEnabled && hover is { } hovered)
            {
                using var hoverBrush = new SolidBrush(hints.Contains(hovered) ? HoverValidColor : HoverInvalidColor);
                g.FillRectangle(hoverBrush, CellRect(hovered));
            }

            DrawGrid(g, cellSize, boardSize);
            DrawCoordinates(g, cellSize);

            if (board is null)
            {
                return;
            }

            for (var row = 0; row < Board.Size; row++)
            {
                for (var col = 0; col < Board.Size; col++)
                {
                    var position = new Position(row, col);
                    var stone = board[position];
                    if (stone != Stone.Empty)
                    {
                        DrawStone(g, CellRect(position), stone);
                    }
                    else if (inputEnabled && hints.Contains(position))
                    {
                        DrawHint(g, CellRect(position));
                    }
                }
            }

            if (lastMove is { } last)
            {
                var rect = CellRect(last);
                var size = Math.Max(4, cellSize / 8);
                using var markBrush = new SolidBrush(LastMoveColor);
                g.FillEllipse(markBrush, rect.X + ((rect.Width - size) / 2), rect.Y + ((rect.Height - size) / 2), size, size);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var position = HitTest(e.Location);
            if (position != hover)
            {
                hover = position;
                UpdateCursor();
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = null;
            UpdateCursor();
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (!inputEnabled || e.Button != MouseButtons.Left)
            {
                return;
            }

            if (HitTest(e.Location) is { } position)
            {
                CellClicked?.Invoke(this, position);
            }
        }

        private static void DrawStone(Graphics g, Rectangle cell, Stone stone)
        {
            var padding = Math.Max(3, cell.Width / 10);
            var rect = Rectangle.Inflate(cell, -padding, -padding);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (var shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                g.FillEllipse(shadow, rect.X + 2, rect.Y + 3, rect.Width, rect.Height);
            }

            var isBlack = stone == Stone.Black;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using var brush = new PathGradientBrush(path)
                {
                    CenterPoint = new PointF(rect.X + (rect.Width * 0.35f), rect.Y + (rect.Height * 0.3f)),
                    CenterColor = isBlack ? Color.FromArgb(110, 110, 110) : Color.White,
                    SurroundColors = new[] { isBlack ? Color.FromArgb(10, 10, 10) : Color.FromArgb(195, 195, 195) },
                };
                g.FillPath(brush, path);
            }

            using var outline = new Pen(isBlack ? Color.Black : Color.FromArgb(150, 150, 150), 1.5f);
            g.DrawEllipse(outline, rect);
        }

        private static void DrawHint(Graphics g, Rectangle cell)
        {
            var size = Math.Max(6, cell.Width / 6);
            using var brush = new SolidBrush(HintColor);
            g.FillEllipse(brush, cell.X + ((cell.Width - size) / 2), cell.Y + ((cell.Height - size) / 2), size, size);
        }

        private void DrawGrid(Graphics g, int cellSize, int boardSize)
        {
            using var pen = new Pen(GridColor, 2);
            for (var i = 0; i <= Board.Size; i++)
            {
                var offset = LabelMargin + (i * cellSize);
                g.DrawLine(pen, LabelMargin, offset, LabelMargin + boardSize, offset);
                g.DrawLine(pen, offset, LabelMargin, offset, LabelMargin + boardSize);
            }

            // 盤の目印（星）
            using var starBrush = new SolidBrush(GridColor);
            foreach (var (row, col) in new[] { (2, 2), (2, 6), (6, 2), (6, 6) })
            {
                var x = LabelMargin + (col * cellSize);
                var y = LabelMargin + (row * cellSize);
                g.FillEllipse(starBrush, x - 4, y - 4, 8, 8);
            }
        }

        private void DrawCoordinates(Graphics g, int cellSize)
        {
            using var font = new Font("Segoe UI", 10f, FontStyle.Bold);
            const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            for (var i = 0; i < Board.Size; i++)
            {
                var columnRect = new Rectangle(LabelMargin + (i * cellSize), 0, cellSize, LabelMargin);
                TextRenderer.DrawText(g, ((char)('a' + i)).ToString(), font, columnRect, Color.Gainsboro, flags);

                var rowRect = new Rectangle(0, LabelMargin + (i * cellSize), LabelMargin, cellSize);
                TextRenderer.DrawText(g, (i + 1).ToString(), font, rowRect, Color.Gainsboro, flags);
            }
        }

        private Rectangle CellRect(Position position)
        {
            var cellSize = CellSize;
            return new Rectangle(
                LabelMargin + (position.Col * cellSize),
                LabelMargin + (position.Row * cellSize),
                cellSize,
                cellSize);
        }

        private Position? HitTest(Point point)
        {
            var cellSize = CellSize;
            var x = point.X - LabelMargin;
            var y = point.Y - LabelMargin;
            if (x < 0 || y < 0)
            {
                return null;
            }

            var position = new Position(y / cellSize, x / cellSize);
            return position.IsOnBoard ? position : null;
        }

        private void UpdateCursor()
        {
            Cursor = inputEnabled && hover is { } hovered && hints.Contains(hovered) ? Cursors.Hand : Cursors.Default;
        }
    }
}
