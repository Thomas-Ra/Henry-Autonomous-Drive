using System;
using System.ComponentModel;
using System.Text;

namespace HwrBerlin.Bot.Scanner
{
    /// <summary>
    /// provides commands for the sick TiM5xx laser scanner
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// the possible bit resolutions defined in the documentation
        /// </summary>
        public enum BitResolution
        {
            [Description("0")]
            Bit8,
            [Description("1")]
            Bit16
        }

        /// <summary>
        /// the possible angular resolutions defined in the documentation
        /// </summary>
        public enum AngularResolution
        {
            [Description("D05")]
            OneThirdDegree,
            [Description("2710")]
            OneDegree
        }

        /// <summary>
        /// the possible interfaces defined in the documentation
        /// </summary>
        public enum InterfaceType
        {
            [Description("0")]
            TX_RS232,
            [Description("1")]
            TX_RS485_2WIRE,
            [Description("2")]
            TX_RS422_485_4WIRE
        }

        /// <summary>
        /// returns the description value of an enumeration
        /// </summary>
        /// <typeparam name="T">the enumeration type</typeparam>
        /// <param name="enumerationValue">the enumeration value</param>
        /// <returns>the description</returns>
        private static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            var type = enumerationValue.GetType();
            var memberInfo = type.GetMember(enumerationValue.ToString());
            var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)attrs[0]).Description;
        }



        private static bool _criticalCommands = false;

        /// <summary>
        /// activates to receive critical commands
        /// </summary>
        public static void ActivateCriticalCommands()
        {
            _criticalCommands = true;
        }


        /// <summary>
        /// generates the message which will be send to the laser scanner
        /// </summary>
        /// <param name="input">the command</param>
        /// <returns>the request message</returns>
        public static byte[] GenerateMessage(string input)
        {
            byte[] msgBegin = { 0x02 };
            byte[] msgEnd = { 0x03 };
            var msg = Encoding.ASCII.GetBytes(input);
            var fullRequest = new byte[msgBegin.Length + msg.Length + msgEnd.Length];
            Buffer.BlockCopy(msgBegin, 0, fullRequest, 0, msgBegin.Length);
            Buffer.BlockCopy(msg, 0, fullRequest, msgBegin.Length, msg.Length);
            Buffer.BlockCopy(msgEnd, 0, fullRequest, msgBegin.Length + msg.Length, msgEnd.Length);

            return fullRequest;
        }



        // sending this a user logs in as Authorized client with password F4724744
        // see telegram listing page 12
        public static string SignInAsAuthorizedClient()
        {
            return "sMN SetAccessMode 03 F4724744";
        }

        // page 24
        public static string ReadForFrequencyAndAngularResolution()
        {
            return "sRN LMPscancfg";
        }

        // page 32
        public static string StartMeasurement()
        {
            return "sMN LMCstartmeas";
        }

        // page 33
        public static string StopMeasurement()
        {
            return "sMN LMCstopmeas";
        }

        // be careful, deletes all parametrization!
        // is an critical command
        // page 40
        private static string LoadFactoryDefaults()
        {
            if (_criticalCommands)
            {
                return "sMN mSCloadfacdef";
            }

            throw new CriticalCommandsNotActivated();
        }

        // be careful, deletes user settings (but keeps field and evaluation settings)
        // is an critical command
        // page 41
        private static string LoadApplicationDefaults()
        {
            if (_criticalCommands)
            {
                return "mSCloadappdef";
            }

            throw new CriticalCommandsNotActivated();
        }

        // page 45
        public static string CheckPassword()
        {
            return "sMN checkPassword 03 19 20 E4 C9";
        }

        // page 46
        public static string RebootDevice()
        {
            return "sMN mSCreboot";
        }

        // page 53
        public static string SaveParametersPermanently()
        {
            return "sMN mEEwriteall";
        }

        // page 53
        public static string SetToRun()
        {
            return "sMN Run";
        }

        // page 55
        public static string ConfigureDataContent(bool remission, BitResolution resolution, bool position, bool deviceName, bool comment, bool time, ushort scanRate)
        {
            return "sWN LMDscandatacfg"
                + " " + "01 00"
                + " " + (remission ? "1" : "0")
                + " " + GetDescription(resolution)
                + " " + "0"
                + " " + "00 00"
                + " " + (position ? "1" : "0")
                + " " + (deviceName ? "1" : "0")
                + " " + (comment ? "1" : "0")
                + " " + (time ? "1" : "0")
                + " +" + scanRate.ToString("X4");
        }

        // page 58
        // angles from -45 to 225
        public static string ConfigureMeasurementAngle(AngularResolution angularResolution, int startAngle, int stopAngle)
        {
            return "SWN LMPoutputRange"
                + " " + "1"
                + " " + GetDescription(angularResolution)
                + " " + (startAngle * 10000).ToString("X8")
                + " " + (stopAngle * 10000).ToString("X8");
        }

        // page 60
        public static string ReadOutputRange()
        {
            return "sRN LMPoutputRange";
        }

        // page 62
        public static string PollOneTelegram()
        {
            return "sRN LMDscandata";
        }

        // page 63
        public static string SendDataPermanentlyStart()
        {
            return "sEN LMDscandata 1";
        }

        // page 63
        public static string SendDataPermanentlyStop()
        {
            return "sEN LMDscandata 0";
        }


        // page 95
        public static string SetParticleFilterInactive()
        {
            return "sWN LFPparticle 0 +500";
        }

        // page 95
        public static string SetParticlefilterActive()
        {
            return "sWN LFPparticle 1 +500";
        }

        // page 116
        public static string ReadStateOfOutput()
        {
            return "sRN LIDoutputstate";
        }

        // page 117
        public static string SendOutputStateByEventStart()
        {
            return "sEN LIDoutputstate 1";
        }

        // page 117
        public static string SendOutputStateByEventStop()
        {
            return "sEN LIDoutputstate 0";
        }

        // page 120
        public static string SetOutputStateInactive()
        {
            return "sMN mDOSetOutput 1 0";
        }

        // page 120
        public static string SetOutputStateActive()
        {
            return "sMN mDOSetOutput 1 1";
        }

        // page 136
        public static string ReadDeviceIdent()
        {
            return "sRN DeviceIdent";
        }

        // page 137
        public static string ReadDeviceState()
        {
            return "sRN SCdevicestate";
        }

        // page 145
        public static string DeviceOrderNumber()
        {
            return "sRN DIornr";
        }

        // page 146
        public static string DeviceType()
        {
            return "sRN DItype";
        }

        // page 148
        public static string ReadOperatingHours()
        {
            return "sRN ODoprh";
        }

        // page 149
        public static string ReadPowerOnCounter()
        {
            return "sRN ODpwrc";
        }

        // page 152
        public static string SetDeviceName(string deviceName)
        {
            if (deviceName.Length <= 16)
            {
                return "sWN LocationName"
                    + " +" + (deviceName.Length).ToString()
                    + " " + deviceName;
            }

            throw new DeviceNameTooLong();
        }

        // page 153
        public static string ReadForDeviceName()
        {
            return "sRN LocationName";
        }

        // page 156
        public static string ResetOutputCounter()
        {
            return "sMN LIDrstoutpcnt";
        }

        // page 158
        // set IP adress left out so that no one's doing this by mistake
        // can be done through custom command though

        // page 159
        public static string ReadIpAddress()
        {
            return "sRN EIIpAddr";
        }

        // page 160
        // set ethernet gate left out so that no one's doing this by mistake
        // can be done through custom command though

        // page 161
        public static string ReadEthernetGateway()
        {
            return "sRN EIgate";
        }

        // page 162
        // set IP mask left out so that no one's doing this by mistake
        // can be done through custom command though

        // page 164
        public static string ReadIpMask()
        {
            return "sRN EImask";
        }

        // page 168
        public static string SetInterfaceType(InterfaceType interfaceType)
        {
            return "SWN LMPoutputRange"
                + " " + GetDescription(interfaceType);
        }

        // page 169
        public static string ReadInterfaceType()
        {
            return "sRN SIHstHw";
        }
    }
}
