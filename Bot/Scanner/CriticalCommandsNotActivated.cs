using System;

namespace HwrBerlin.Bot.Scanner
{
    public class CriticalCommandsNotActivated : Exception
    {
        public CriticalCommandsNotActivated()
        {
        }

        public CriticalCommandsNotActivated(string message) : base(message)
        {
        }

        public CriticalCommandsNotActivated(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
