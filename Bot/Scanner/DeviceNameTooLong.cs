using System;

namespace HwrBerlin.Bot.Scanner
{
    /// <inheritdoc />
    /// <summary>
    /// will be thrown if the device name is too long
    /// </summary>
    public class DeviceNameTooLong : Exception
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
