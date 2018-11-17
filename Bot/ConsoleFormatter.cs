using System;

namespace HwrBerlin.Bot
{
    public static class ConsoleFormatter
    {
        public static void Text(params string[] msg)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            foreach (string s in msg)
            {
                Console.WriteLine(s);
            }
            Console.ForegroundColor = currentColor;
        }

        public static void Custom(ConsoleColor color, params string[] msg)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            foreach (string s in msg)
            {
                Console.WriteLine(s);
            }
            Console.ForegroundColor = currentColor;
        }

        public static void Info(params string[] msg)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("*");
            foreach (string s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
            Console.ForegroundColor = currentColor;
        }

        public static void Warning(params string[] msg)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("*");
            foreach (string s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
            Console.ForegroundColor = currentColor;
        }

        public static void Error(params string[] msg)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("*");
            foreach (string s in msg)
            {
                Console.WriteLine("* " + s);
            }
            Console.WriteLine("*");
            Console.ForegroundColor = currentColor;
        }
    }
}
