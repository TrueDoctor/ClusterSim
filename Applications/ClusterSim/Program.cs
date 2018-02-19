using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using XDMessaging;

namespace ClusterSim.Standalone
{
    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.ClusterLib.Utility;

    public class Program
    {
        private static bool abort = false;

        public static int SaveInterval { get; set; } = 1;

        public static double MinDAcc { get; set; } = 0.0003;

        public static void Main(string[] args)
        {
            XDMessagingClient client = new XDMessagingClient(); // https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            string rTable = string.Empty;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (args.Length > 0)//if the program gets called with arguments
            {
                List<String> list = SQL.readTables();
                if (list != null)
                {
                    List<string> res = new List<string>();
                    foreach (string s in args)
                    {
                        if (list.Contains(s))//check if argument is a valid table name
                            rTable = s;
                        
                        Console.WriteLine(rTable);
                    }
                }
            }

            if (rTable == "")//if argument handover failed or was not  given
            {
                Console.WriteLine("Auswahltabelle: ");//input name maualy 
                rTable = Console.ReadLine();
            }

            int last = 0;
            Console.WriteLine("\nLeer lassen, für gleiche Liste, oder Speichern nach: ");
            string wTable = Console.ReadLine();


            if (wTable == "")
            {
                wTable = rTable;
                last = SQL.lastStep(rTable);//get last step of given table
            }
            

            Console.WriteLine("\nDelta t in Tagen: ");

            double dt = Convert.ToDouble(Console.ReadLine());

            //Console.WriteLine("\nSchritte: ");

            int n = 2;//Convert.ToInt32(Console.ReadLine());

            int year = (last * SaveInterval);
            Console.WriteLine(@"Warte auf die Beendigung von {0} Speicher Threads", Math.Round((dt * n) / 365, 2));
            Thread.Sleep(2000);
            broadcaster.SendToChannel("steps", "s" + n);// send max step to steps channel


            Thread Key = new Thread(listen);
            Key.Start();
            var cluster = new StarCluster(SQL.readStars(rTable, 0), dt); // instatiate Starcluster


            var time = 0d;
            for (int i = (last * SaveInterval * 365) + 1;
                 !abort; i++)
            {
                cluster.Dt = dt;
                cluster.DoStep(Misc.Method.Rk5, 0);
                var maxDAcc = cluster.Stars.Max(x => x.DAcc);
                if (maxDAcc > 0)
                {
                    dt += ((dt * MinDAcc / maxDAcc) - dt) / 2;

                    //dt = dt * 0.0003 / maxDAcc;
                }

                time += dt;

                //broadcaster.SendToChannel("steps", $"i{i}");

                // send "i"+step in channel steps
                // Console.WriteLine("\n");//+ i + "\n ");
                cluster.Stars.MoveCenter(cluster.Stars.GetCenter());
                

                if (Math.Ceiling((time - dt) / 365) < Math.Ceiling(time / 365) && ++year % SaveInterval == 0)
                {
                    Console.WriteLine($@"Exportiere Daten... Jahr: {(int)i * dt / 365} = {year}");
                    while (!SQL.addRows(cluster.Stars, year / SaveInterval, wTable))
                    {
                        Thread.Sleep(100);
                    }
                } 
            }

            Key.Abort();
            
            SQL.order(wTable);
            broadcaster.SendToChannel("steps", "abort");

            Console.WriteLine("Direkt in Dataview öffnen? (y/n)");
            string view = Console.ReadLine();                         //wait for input
            if (view == "y" || view == "Y")
                System.Diagnostics.Process.Start(@"DataView.exe", wTable);
        }

        private static void listen()
        {
            XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);

            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();

            }
            while (keyinfo.Key != ConsoleKey.X);
            Console.WriteLine("\n\n\n\n Beenden Eingeleitet\n\n\n\n");
            abort = true;
            broadcaster.SendToChannel("steps", "abort");
        }
    }
}
