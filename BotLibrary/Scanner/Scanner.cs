using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HwrBerlin.Bot.Scanner
{
    class Scanner
    {
        //the ip address of the laser scanner
        private readonly string Ip;

        //the port of the laser scanner, by default 2111 or 2112
        private readonly int Port;

        //create a tcp server and connect it to the endpoint of the scanner
        private Socket Socket;
        private NetworkStream NetworkStream;
        private BinaryReader BinaryReader;
        private BinaryWriter BinaryWriter;
        
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

        ~Scanner()
        {
            BinaryReader.Close();
            BinaryWriter.Close();
            NetworkStream.Close();

            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

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

        public List<int> GetDataList()
        {
            byte[] message = Commands.GenerateMessage(Commands.PollOneTelegram());

            //receive scanData
            byte[] data = SendAndReceiveData(message);

            string[] values = Encoding.ASCII.GetString(data).Split(new string[] { " " }, StringSplitOptions.None);

            //convert hex numbers to int numbers that represent the measured distances
            List<int> distanceData = new List<int>();
            for (int i = 26; i < 297; i++)
            {
                distanceData.Add(int.Parse(values[i], System.Globalization.NumberStyles.HexNumber));
            }

            return distanceData;
        }

        //apply median filter to scan data to remove outlier values
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
