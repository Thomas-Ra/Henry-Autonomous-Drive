using System;

namespace HwrBerlin.Bot.Scanner
{
    /// <inheritdoc />
    /// <summary>
    /// will be thrown if critical commands aren't activated
    /// </summary>
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
