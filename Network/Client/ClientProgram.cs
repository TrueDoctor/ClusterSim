// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientProgram.cs" company="">
//   
// </copyright>
// <summary>
//   The client program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Client
{
    #region

    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using Client.Properties;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.ClusterLib.Utility;
    using ClusterSim.Net.Lib;

    #endregion

    /// <summary>
    /// The client program.
    /// </summary>
    internal class ClientProgram
    {
        /// <summary>
        /// The ready.
        /// </summary>
        public static bool ready;

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        private static void Main(string[] args)
        {
            var clientSocket = new TcpClient();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "CluserSim - Slave Client";

            Console.WriteLine("Client Started");
            loopback:
            string HostName = Dns.GetHostName();
            var hostInfo = Dns.GetHostEntry(HostName);
            string ip = hostInfo.AddressList.First(x => x.ToString().Contains('.'))
                .ToString();

            if (!Properties.Settings.Default.IP.Equals(String.Empty))
                ip = Settings.Default.IP;

             //ip = "192.168..42";
            try
            {
                clientSocket.Connect(ip, Settings.Default.Port);
            }
            catch
            {
                Console.WriteLine("Server nicht erreichbar");
                if (!Console.ReadLine().Equals("retry")) return;
                goto loopback;
            }

            Console.WriteLine(
                "Client Socket Program - Server Connected ... on " + Settings.Default.IP + ":" + Settings.Default.Port);
            Console.Beep();
            var Cluster = new BoxCluster();

            // Cluster.Stars = new List<Star>(1);
            // Cluster.Stars.Add(Misc.randomize(1, 1, 1, 1, 1));
            int step, min, max;
            step = min = 0;
            max = 1;
            try
            {//*/
                while (true)
                {
                    var serverStream = clientSocket.GetStream();

                    ready = true;

                    var header = new byte[Message.headerSize];
                    if (serverStream.Read(header, 0, Message.headerSize) != Message.headerSize)
                    {
                        throw new Exception("header zu kurz");
                    }

                    int size = BitConverter.ToInt32(header, 4);
                    var read = 0;
                    var data = new byte[size * Star.size + Message.headerSize];

                    do
                    {
                        read += serverStream.Read(data, read + Message.headerSize, size * Star.size - read);
                    }
                    while (read < size * Star.size);
                    Array.Copy(header, data, Message.headerSize);

                    var msg = new Message(size);
                    msg.DeSerialize(data);

                    ready = false;
                    Cluster.Stars = msg.Stars.ToList();
                    Cluster.Dt = msg.dt;
                    step = msg.step;
                    min = msg.min;
                    max = msg.max;
                    
                    var newStars = Cluster.DoStep(Misc.Method.Rk5, multiThreading: true, min: min, max: max);
                    step++;

                    // if(NewStars.Count)
                    Console.WriteLine(step);

                    // if (Cluster.Stars.Count != 120)
                    // return;
                    var message = new Message(step, msg.dt, min, max, newStars).Serialize(newStars.Length);
                    serverStream.Write(message, 0, newStars.Length * Star.size + Message.headerSize);
                    serverStream.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Verbindung verloren\n\n" + e.Message);
                Thread.Sleep(2000);
            }//*/

            // Console.ReadLine()););return; }
        }
    }
}