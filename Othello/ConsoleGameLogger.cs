namespace Othello
{
    /// <summary>対局のログの出力先。</summary>
    internal interface IGameLogger
    {
        void Log(string message);
    }

    /// <summary>コンソールに時刻つきでログを出す。</summary>
    internal sealed class ConsoleGameLogger : IGameLogger
    {
        public void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
