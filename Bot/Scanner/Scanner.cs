using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HwrBerlin.Bot.Scanner
{
    /// <summary>
    /// provides function to communicate with the sick TiM5xx laser scanner
    /// </summary>
    public class Scanner
    {
        
        private readonly Socket _socket;
        private readonly NetworkStream _networkStream;
        private readonly BinaryReader _binaryReader;
        private readonly BinaryWriter _binaryWriter;
        
        /// <summary>
        /// initialize the connection to the laser scanner
        /// </summary>
        /// <param name="ip">default is 192.168.0.1</param>
        /// <param name="port">default is 2111</param>
        public Scanner(string ip = "192.168.0.1", int port = 2111)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(ipEndPoint);

                //get the stream to the server
                _networkStream = new NetworkStream(_socket);

                //when sending and receiving bytes, a BinaryWriter/BinaryReader can be used
                _binaryWriter = new BinaryWriter(_networkStream);
                _binaryReader = new BinaryReader(_networkStream);

            }
            catch (SocketException)
            {
                ConsoleFormatter.Error("Failed to connect to laser scanner at " + ip + ":" + port, 
                                       "Help: Check if the cable is plugged in properly and the right network settings are set.");
                throw;
            }
        }

        /// <summary>
        /// closes the connection when the laser scanner won't be used anymore
        /// </summary>
        ~Scanner()
        {
            _binaryReader.Close();
            _binaryWriter.Close();
            _networkStream.Close();

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        /// <summary>
        /// sends a command to the laser scanner and returns the response
        /// </summary>
        /// <param name="message">command for the laser scanner</param>
        /// <returns>the response of the laser scanner</returns>
        public byte[] SendAndReceiveData(byte[] message)
        {
            //send the message
            _binaryWriter.Write(message, 0, message.Length);
            _binaryWriter.Flush();

            //create buffer for the servers answer and read the answer
            var dataReceived = new byte[2048];
            _binaryReader.Read(dataReceived, 0, dataReceived.Length);

            //delete all values that are null and the begin and the end
            return dataReceived.Where((val) => val != 0 && val != 2 && val != 3).ToArray();
        }

        /// <summary>
        ///     returns the current measurement as list of integers from left to right
        /// </summary>
        /// <returns>list with the distance in mm</returns>
        public List<int> GetDataList()
        {
            var message = Commands.GenerateMessage(Commands.PollOneTelegram());

            //receive scanData
            var data = SendAndReceiveData(message);

            var values = Encoding.ASCII.GetString(data).Split(new[] { " " }, StringSplitOptions.None);

            var amountOfData = int.Parse(values[25], System.Globalization.NumberStyles.HexNumber);

            //convert hex numbers to int numbers that represent the measured distances
            var distanceData = new List<int>();
            for (var i = 25 + amountOfData; i > 25; i--)
            {
                distanceData.Add(int.Parse(values[i], System.Globalization.NumberStyles.HexNumber));
            }

            return distanceData;
        }

        /// <summary>
        /// applies the median filter for scan data to remove outlier values
        /// </summary>
        /// <param name="scanData">the list of the measured values</param>
        /// <returns>the list of the corrected values</returns>
        public List<int> MedianFilter(List<int> scanData)
        {
            //how many numbers should be compared in each direction
            const int width = 6;
            //index of median value

            //loop through every item of scan data list
            for (var i = 0; i < scanData.Count; i++)
            {
                //new list for comparison of items
                var comparison = new List<int>();
                for (var l = 0; l <= width; l++)
                {
                    //add items in increasing direction
                    if (i + l < scanData.Count)
                    {
                        comparison.Add(scanData[i + l]);
                    }
                    //add items in decreasing direction
                    if (i - l >= 0 && l > 0)
                    {
                        comparison.Add(scanData[i - l]);
                    }
                    //sort comparison list
                    comparison.Sort();
                    //find index of median value
                    var middle = comparison.Count / 2;
                    //replace item with new median value
                    scanData[i] = comparison[middle];
                }
                //clear list
                comparison.Clear();
            }
            return scanData;
        }
    }
}
