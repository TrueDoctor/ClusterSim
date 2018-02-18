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
            var Cluster = new StarCluster(1, 1);

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

                    var header = new byte[20];
                    if (serverStream.Read(header, 0, 20) != 20) throw new Exception("header zu kurz");
                    int size = BitConverter.ToInt32(header, 4);
                    var read = 0;
                    var data = new byte[size * 92 + 20];

                    do
                    {
                        read += serverStream.Read(data, read + 20, size * 92 - read);
                    }
                    while (read < size * 92);
                    Array.Copy(header, data, 20);

                    var msg = new Message(size);
                    msg.DeSerialize(data);

                    ready = false;
                    Cluster.Stars = msg.Stars.ToList();
                    Cluster.Dt = msg.dt;
                    step = msg.step;
                    min = msg.min;
                    max = msg.max;
                    var NewStars = Cluster.DoStep(++step, min, max, Misc.Method.Rk5);

                    // if(NewStars.Count)
                    Console.WriteLine(step);

                    // if (Cluster.Stars.Count != 120)
                    // return;
                    var message = new Message(step, 0, min, max, NewStars).Serialize(NewStars.Length);
                    serverStream.Write(message, 0, NewStars.Length * 92 + 20);
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