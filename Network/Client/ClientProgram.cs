using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using ClusterSim.Net.Lib;
using ClusterSim.ClusterLib;

namespace Client
{
    class ClientProgram
    {
        public static bool ready = false;

        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "CluserSim - Slave Client";

            Console.WriteLine("Client Started");
            loopback:
            string HostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry hostInfo = Dns.GetHostEntry(HostName);
            string ip = /*hostInfo.AddressList.First(x => x.ToString().Contains('.')).ToString();/*/Properties.Settings.Default.IP;
            //string ip = "10.165.4.148";
            try
            {
                clientSocket.Connect(ip, Properties.Settings.Default.Port);
            }
            catch { Console.WriteLine("Server nicht erreichbar"); if (!Console.ReadLine().Equals("retry")) { return; } else goto loopback; }
            Console.WriteLine("Client Socket Program - Server Connected ... on "+Properties.Settings.Default.IP+":"+Properties.Settings.Default.Port);
            Console.Beep();
            var Cluster = new StarCluster(1,1);
            //Cluster.Stars = new List<Star>(1);
            //Cluster.Stars.Add(Misc.randomize(1, 1, 1, 1, 1));
            int step,min,max;
            step = min =  0;
            max = 1;
            //try
            //{
                while (true)
                {
                    NetworkStream serverStream = clientSocket.GetStream();

                    ready = true;

                    var header = new byte[16];
                    if (serverStream.Read(header, 0, 16) != 16)
                        throw new Exception("header zu kurz");
                    int size = BitConverter.ToInt32(header, 4);
                    int read = 0;
                    var data = new byte[size*60+16];

                    do
                    {
                        read += serverStream.Read(data, read+16, size*60-read);
                        
                    } while (read<size*60);
                    Array.Copy(header, data, 16);

                    Message msg = new Message(size);
                    msg.Deserialize(data);

                    ready = false;
                    Cluster.Stars = msg.Stars.ToList();
                    step = msg.step; min = msg.min; max = msg.max;
                    var NewStars = Cluster.doStep(++step, min, max, Misc.Method.RK5);

                    //if(NewStars.Count)

                    Console.WriteLine(step);
                   // if (Cluster.Stars.Count != 120)
                        //return;
                    
                    serverStream.Write(new Message(step, min, max, NewStars).Serialize(NewStars.Length), 0, NewStars.Length * 60 + 16);
                    serverStream.Flush();
                }
            //}
            //catch(Exception e) { Console.WriteLine("Verbindung verloren\n\n"+e.Message);Console.ReadLine();return; }
        }
    }
}
