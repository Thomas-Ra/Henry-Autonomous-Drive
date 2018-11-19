using System;

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
            {
                Console.WriteLine(s);
            }
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
            {
                Console.WriteLine(s);
            }
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
            Console.WriteLine("*");
            foreach (var s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
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
            Console.WriteLine("*");
            foreach (var s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
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
            Console.WriteLine("*");
            foreach (var s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
            Console.ForegroundColor = currentColor;
        }
    }
}
