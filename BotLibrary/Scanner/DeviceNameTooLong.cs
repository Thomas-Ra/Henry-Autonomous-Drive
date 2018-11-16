using System;

namespace HwrBerlin.Bot.Scanner
{
    class DeviceNameTooLong : Exception
    {
        public DeviceNameTooLong()
        {
        }

        public DeviceNameTooLong(string message) : base(message)
        {
        }

        public DeviceNameTooLong(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
