using Othello.Core;

namespace Othello
{
    /// <summary>対戦モード。</summary>
    internal enum GameMode
    {
        HumanVsHuman,
        HumanVsCpu,
    }

    /// <summary>オセロのメイン画面。</summary>
    internal sealed class MainForm : Form
    {
        /// <summary>人 vs CPU のとき、人が持つ色（先手の黒）。</summary>
        private const Stone HumanStone = Stone.Black;

        private static readonly Color PanelBackColor = Color.FromArgb(32, 36, 40);
        private static readonly Color TextColor = Color.FromArgb(235, 235, 235);
        private static readonly Color SubTextColor = Color.FromArgb(170, 180, 190);

        private readonly IGameLogger logger;
        private readonly BoardControl boardControl = new();
        private readonly Label turnLabel = new();
        private readonly Label blackCountLabel = new();
        private readonly Label whiteCountLabel = new();
        private readonly Label modeLabel = new();
        private readonly Label messageLabel = new();
        private readonly RadioButton humanVsHumanRadio = new();
        private readonly RadioButton humanVsCpuRadio = new();
        private readonly Button newGameButton = new();
        private readonly Button undoButton = new();
        private readonly System.Windows.Forms.Timer cpuTimer = new() { Interval = 700 };

        private Game game = new();
        private GameMode mode = GameMode.HumanVsHuman;
        private Position? lastMove;

        public MainForm(IGameLogger logger)
        {
            this.logger = logger;
            InitializeLayout();

            boardControl.CellClicked += OnCellClicked;
            cpuTimer.Tick += OnCpuTimerTick;
            newGameButton.Click += (_, _) => StartNewGame(SelectedMode);
            undoButton.Click += (_, _) => UndoMove();
            humanVsHumanRadio.CheckedChanged += OnModeRadioChanged;
            humanVsCpuRadio.CheckedChanged += OnModeRadioChanged;
            FormClosing += (_, _) => logger.Log("ウィンドウを閉じました。");

            StartNewGame(GameMode.HumanVsHuman);
        }

        private GameMode SelectedMode => humanVsCpuRadio.Checked ? GameMode.HumanVsCpu : GameMode.HumanVsHuman;

        private bool IsCpuTurn => mode == GameMode.HumanVsCpu && !game.IsGameOver && game.CurrentPlayer != HumanStone;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cpuTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private static string ModeName(GameMode gameMode) =>
            gameMode == GameMode.HumanVsCpu ? "人 vs CPU" : "人 vs 人";

        private static string StoneSymbol(Stone stone) => stone == Stone.Black ? "●" : "○";

        private void InitializeLayout()
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "オセロ（コンソールアプリ + Windows Forms）";
            ClientSize = new Size(840, 600);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = PanelBackColor;
            ForeColor = TextColor;
            Font = new Font("Yu Gothic UI", 10f);

            // メニュー
            var menu = new MenuStrip();
            var gameMenu = new ToolStripMenuItem("ゲーム(&G)");
            gameMenu.DropDownItems.Add("新しいゲーム：人 vs 人", null, (_, _) => StartNewGame(GameMode.HumanVsHuman));
            gameMenu.DropDownItems.Add("新しいゲーム：人 vs CPU", null, (_, _) => StartNewGame(GameMode.HumanVsCpu));
            gameMenu.DropDownItems.Add(new ToolStripSeparator());
            gameMenu.DropDownItems.Add("終了(&X)", null, (_, _) => Close());
            var helpMenu = new ToolStripMenuItem("ヘルプ(&H)");
            helpMenu.DropDownItems.Add("遊び方", null, (_, _) => ShowHelp());
            menu.Items.AddRange(new ToolStripItem[] { gameMenu, helpMenu });
            MainMenuStrip = menu;

            // 盤面
            boardControl.Location = new Point(12, 36);
            boardControl.Size = new Size(552, 552);

            // 右側のパネル
            const int panelX = 584;
            const int panelWidth = 240;

            turnLabel.Location = new Point(panelX, 44);
            turnLabel.Size = new Size(panelWidth, 64);
            turnLabel.Font = new Font("Yu Gothic UI", 14f, FontStyle.Bold);

            blackCountLabel.Location = new Point(panelX, 116);
            blackCountLabel.Size = new Size(panelWidth, 32);
            blackCountLabel.Font = new Font("Yu Gothic UI", 13f);

            whiteCountLabel.Location = new Point(panelX, 150);
            whiteCountLabel.Size = new Size(panelWidth, 32);
            whiteCountLabel.Font = new Font("Yu Gothic UI", 13f);

            var modeGroup = new GroupBox
            {
                Text = "対戦モード",
                Location = new Point(panelX, 196),
                Size = new Size(panelWidth, 92),
                ForeColor = TextColor,
            };
            humanVsHumanRadio.Text = "人 vs 人";
            humanVsHumanRadio.Location = new Point(14, 26);
            humanVsHumanRadio.AutoSize = true;
            humanVsHumanRadio.Checked = true;
            humanVsCpuRadio.Text = "人 vs CPU（あなたは黒）";
            humanVsCpuRadio.Location = new Point(14, 56);
            humanVsCpuRadio.AutoSize = true;
            modeGroup.Controls.Add(humanVsHumanRadio);
            modeGroup.Controls.Add(humanVsCpuRadio);

            newGameButton.Text = "新しいゲーム";
            newGameButton.Location = new Point(panelX, 302);
            newGameButton.Size = new Size(panelWidth, 40);
            StyleButton(newGameButton, Color.FromArgb(0, 120, 70));

            undoButton.Text = "待った（1手戻す）";
            undoButton.Location = new Point(panelX, 350);
            undoButton.Size = new Size(panelWidth, 40);
            StyleButton(undoButton, Color.FromArgb(70, 80, 95));

            modeLabel.Location = new Point(panelX, 404);
            modeLabel.Size = new Size(panelWidth, 24);
            modeLabel.ForeColor = SubTextColor;

            messageLabel.Location = new Point(panelX, 432);
            messageLabel.Size = new Size(panelWidth, 150);
            messageLabel.ForeColor = SubTextColor;

            Controls.AddRange(new Control[]
            {
                menu, boardControl, turnLabel, blackCountLabel, whiteCountLabel,
                modeGroup, newGameButton, undoButton, modeLabel, messageLabel,
            });

            ResumeLayout(false);
            PerformLayout();
        }

        private static void StyleButton(Button button, Color backColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
        }

        private void StartNewGame(GameMode newMode)
        {
            cpuTimer.Stop();
            mode = newMode;
            game = new Game();
            lastMove = null;

            humanVsHumanRadio.Checked = mode == GameMode.HumanVsHuman;
            humanVsCpuRadio.Checked = mode == GameMode.HumanVsCpu;

            logger.Log($"===== 新しいゲーム（{ModeName(mode)}）=====");
            SetMessage("黒の番から始めます。\n白い点のマスに置けます。");
            RefreshView();
        }

        private void OnModeRadioChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton { Checked: true } && SelectedMode != mode)
            {
                SetMessage($"「新しいゲーム」を押すと、{ModeName(SelectedMode)} で始まります。");
            }
        }

        private void OnCellClicked(object? sender, Position position)
        {
            if (game.IsGameOver || IsCpuTurn)
            {
                return;
            }

            Play(position);
        }

        private void OnCpuTimerTick(object? sender, EventArgs e)
        {
            cpuTimer.Stop();
            if (!IsCpuTurn)
            {
                return;
            }

            if (CpuPlayer.ChooseMove(game.Board, game.CurrentPlayer) is { } move)
            {
                Play(move);
            }
        }

        private void Play(Position position)
        {
            var result = game.PlayMove(position);
            var playerName = result.Player.ToJapanese();
            if (!result.Success)
            {
                var error = result.ErrorMessage ?? "そこには置けません。";
                logger.Log($"{playerName}：{position} には置けません（{error}）");
                SetMessage($"{position} には置けません。\n{error}");
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            lastMove = position;
            var black = game.Board.Count(Stone.Black);
            var white = game.Board.Count(Stone.White);
            logger.Log($"{playerName}：{position} に置いて {result.Flipped.Count} 枚ひっくり返しました（黒 {black} - 白 {white}）");

            var message = $"{playerName}が {position} に置きました。\n{result.Flipped.Count} 枚ひっくり返しました。";
            if (result.PassedPlayer is { } passed)
            {
                logger.Log($"{passed.ToJapanese()}は置ける場所がないのでパスです。");
                message += $"\n\n{passed.ToJapanese()}は置ける場所がないため\nパスしました。";
            }

            SetMessage(message);
            RefreshView();

            if (result.IsGameOver)
            {
                ShowResult();
            }
            else if (IsCpuTurn)
            {
                cpuTimer.Start();
            }
        }

        private void UndoMove()
        {
            cpuTimer.Stop();
            if (!game.Undo())
            {
                return;
            }

            // CPU 戦では、人の番まで戻す
            while (mode == GameMode.HumanVsCpu && game.CurrentPlayer != HumanStone && game.CanUndo)
            {
                game.Undo();
            }

            lastMove = null;
            logger.Log("待った：手を戻しました。");
            SetMessage("待った：手を戻しました。");
            RefreshView();

            if (IsCpuTurn)
            {
                cpuTimer.Start();
            }
        }

        private void RefreshView()
        {
            var current = game.CurrentPlayer;
            if (game.IsGameOver)
            {
                turnLabel.Text = "ゲーム終了";
            }
            else
            {
                var who = mode == GameMode.HumanVsCpu
                    ? (IsCpuTurn ? "（CPU 考え中…）" : "（あなた）")
                    : string.Empty;
                turnLabel.Text = $"{StoneSymbol(current)} {current.ToJapanese()}の番\n{who}";
            }

            blackCountLabel.Text = $"● 黒：{game.Board.Count(Stone.Black),2} 枚";
            whiteCountLabel.Text = $"○ 白：{game.Board.Count(Stone.White),2} 枚";
            modeLabel.Text = $"モード：{ModeName(mode)}";
            undoButton.Enabled = game.CanUndo;

            IReadOnlyList<Position> validMoves = game.IsGameOver
                ? Array.Empty<Position>()
                : game.Board.GetValidMoves(current);
            boardControl.UpdateState(game.Board, validMoves, lastMove, !game.IsGameOver && !IsCpuTurn);
        }

        private void ShowResult()
        {
            var black = game.Board.Count(Stone.Black);
            var white = game.Board.Count(Stone.White);
            var winner = game.Winner;
            var resultText = winner == Stone.Empty ? "引き分けです！" : $"{winner.ToJapanese()}の勝ちです！";
            if (mode == GameMode.HumanVsCpu && winner != Stone.Empty)
            {
                resultText += winner == HumanStone ? "（あなたの勝ち）" : "（CPU の勝ち）";
            }

            logger.Log($"ゲーム終了：黒 {black} - 白 {white} → {resultText}");
            SetMessage($"ゲーム終了\n黒 {black} - 白 {white}\n{resultText}");
            MessageBox.Show(
                this,
                $"黒 {black} 枚 ／ 白 {white} 枚\n\n{resultText}",
                "ゲーム終了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                this,
                "・白い点が表示されたマスをクリックすると石を置けます。\n" +
                "・相手の石をはさむと、自分の色にひっくり返ります。\n" +
                "・置ける場所がないときは自動でパスになります。\n" +
                "・両方とも置けなくなったらゲーム終了です。石が多いほうの勝ち。\n" +
                "・「待った」で1手戻せます（CPU 戦ではあなたの番まで戻ります）。\n\n" +
                "コンソール画面には、対局のログが表示されます。",
                "遊び方",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetMessage(string message) => messageLabel.Text = message;
    }
}
