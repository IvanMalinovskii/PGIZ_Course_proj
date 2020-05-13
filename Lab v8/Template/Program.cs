using System;

namespace Template
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GameProcess game = new GameProcess();
            game.Run();
            game.Dispose();
        }
    }
}

