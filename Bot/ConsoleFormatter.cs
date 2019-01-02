using System;
using System.Linq;

namespace HwrBerlin.Bot
{
    /// <summary>
    /// provides methods to write text in the console
    /// </summary>
    public static class ConsoleFormatter
    {
        /// <summary>
        /// writes white text in the console
        /// </summary>
        /// <param name="msg">text lines</param>
        public static void Text(params string[] msg)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var s in msg)
                Console.WriteLine(s);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// writes text with a custom color
        /// </summary>
        /// <param name="color">color of the text</param>
        /// <param name="msg">text lines</param>
        public static void Custom(ConsoleColor color, params string[] msg)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            foreach (var s in msg)
                Console.WriteLine(s);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// writes blue text highlighted with stars
        /// </summary>
        /// <param name="msg">text lines</param>
        public static void Info(params string[] msg)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteInBox(msg);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// writes yellow text highlighted with stars
        /// </summary>
        /// <param name="msg">text lines</param>
        public static void Warning(params string[] msg)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteInBox(msg);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// writes red text highlighted with stars
        /// </summary>
        /// <param name="msg">text lines</param>
        public static void Error(params string[] msg)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteInBox(msg);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// writes text in a box of stars
        /// </summary>
        /// <param name="msg">text lines</param>
        private static void WriteInBox(params string[] msg)
        {
            var width = msg.Select(s => s.Length).Concat(new[] {0}).Max();
            var line = new string('*', width + 4);
            Console.WriteLine(line);
            foreach (var s in msg)
                Console.WriteLine("* " + s + new string(' ', width - s.Length) + " *");
            Console.WriteLine(line);
        }
    }
}
