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
        /// <summary>
        /// the ip adress of the laser scanner
        /// </summary>
        private readonly string Ip;

        /// <summary>
        /// the port to communicate with the laser scanner
        /// </summary>
        private readonly int Port;
        
        private Socket Socket;
        private NetworkStream NetworkStream;
        private BinaryReader BinaryReader;
        private BinaryWriter BinaryWriter;
        
        /// <summary>
        /// initialize the connection to the laser scanner
        /// </summary>
        /// <param name="ip">default is 192.168.0.1</param>
        /// <param name="port">default is 2111</param>
        public Scanner(string ip = "192.168.0.1", int port = 2111)
        {
            Ip = ip;
            Port = port;


            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Socket.Connect(ipEndPoint);

                //get the stream to the server
                NetworkStream = new NetworkStream(Socket);

                //when sending and receiving bytes, a BinaryWriter/BinaryReader can be used
                BinaryWriter = new BinaryWriter(NetworkStream);
                BinaryReader = new BinaryReader(NetworkStream);

            }
            catch (SocketException e)
            {
                ConsoleFormatter.Error("Failed to connect to server at IP " + Ip + ":" + Port);
                throw e;
            }
        }

        /// <summary>
        /// closes the connection when the laser scanner won't be used anymore
        /// </summary>
        ~Scanner()
        {
            BinaryReader.Close();
            BinaryWriter.Close();
            NetworkStream.Close();

            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        /// <summary>
        /// sends a command to the laser scanner and returns the response
        /// </summary>
        /// <param name="message">command for the laser scanner</param>
        /// <returns>the response of the laser scanner</returns>
        public byte[] SendAndReceiveData(byte[] message)
        {
            //send the message
            BinaryWriter.Write(message, 0, message.Length);
            BinaryWriter.Flush();

            //create buffer for the servers answer and read the answer
            byte[] dataReceived = new Byte[2048];
            BinaryReader.Read(dataReceived, 0, dataReceived.Length);

            //delete all values that are null and the begin and the end
            return dataReceived.Where(
                (val) => {
                    return val != 0 && val != 2 && val != 3;
                }).ToArray();
        }

        /// <summary>
        ///     returns the current measurement as list of integers from left to right
        /// </summary>
        /// <returns>list with the distance in mm</returns>
        public List<int> GetDataList()
        {
            byte[] message = Commands.GenerateMessage(Commands.PollOneTelegram());

            //receive scanData
            byte[] data = SendAndReceiveData(message);

            string[] values = Encoding.ASCII.GetString(data).Split(new string[] { " " }, StringSplitOptions.None);

            int amountOfData = int.Parse(values[25], System.Globalization.NumberStyles.HexNumber);

            //convert hex numbers to int numbers that represent the measured distances
            List<int> distanceData = new List<int>();
            for (int i = 25 + amountOfData; i > 25; i--)
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
            int width = 6;
            //index of median valuie
            int middle = 0;

            //loop through every item of scan data list
            for (int i = 0; i < scanData.Count; i++)
            {
                //new list for comparison of items
                List<int> comparison = new List<int>();
                for (int l = 0; l <= width; l++)
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
                    middle = comparison.Count / 2;
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
