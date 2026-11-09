namespace Othello
{
    internal static class Program
    {
        /// <summary>
        /// コンソールアプリとして起動し、Windows Forms のウィンドウでオセロを開く。
        /// コンソールには対局のログを表示する。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var logger = new ConsoleGameLogger();
            logger.Log("オセロを起動しました。ゲームはウィンドウで遊べます。");
            logger.Log("このコンソールには、置いた場所や結果などのログが表示されます。");

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(logger));

            logger.Log("オセロを終了しました。");
        }
    }
}
