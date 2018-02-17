using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterSim.ClusterLib;
using XDMessaging;

namespace ClusterSim.Standalone
{
    using ClusterSim.ClusterLib.Analysis;

    public class Program
    {
        private static bool abort = false;
        static void Main(string[] args)
        {
            XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            string rtable = "";
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
                            rtable = s;
                        
                        Console.WriteLine(rtable);
                    }
                }
            }

            if (rtable == "")//if argument handover failed or was not  given
            {
                Console.WriteLine("Auswahltabelle: ");//input name maualy 
                rtable = Console.ReadLine();
            }
            int last = 0;
            int year, saveInterval = 100;
            Console.WriteLine("\nLeer lassen, für gleiche Liste, oder Speichern nach: ");
            string wtable = Console.ReadLine();


            if (wtable == "")
            {
                wtable = rtable;
                last = SQL.lastStep(rtable);//get last step of given table
            }
            

            Console.WriteLine("\nDelta t in Tagen: ");

            double dt = Convert.ToDouble(Console.ReadLine());

            //Console.WriteLine("\nSchritte: ");

            int n = 2;//Convert.ToInt32(Console.ReadLine());

            year = (last*saveInterval);
            Console.WriteLine(@"Warte auf die Beendigung von {0} Speicher Threads", Math.Round((dt * n) / 365, 2));
            Thread.Sleep(2000);
            broadcaster.SendToChannel("steps", "s" + n);// send max step to steps channel


            Thread Key = new Thread(listen);
            Key.Start();
            StarCluster cluster = new StarCluster(rtable, wtable, last, dt); // instatiate Starcluster

            for (int i = (last * saveInterval * 365) + 1;
                 !abort; i++) 
            {
                cluster.DoStep(i, 0, cluster.Stars.Count - 1, Misc.Method.Rk5);
                broadcaster.SendToChannel("steps", $"i{i}");

                // send "i"+step in channel steps
                // Console.WriteLine("\n");//+ i + "\n ");
                cluster.Stars.MoveCenter(cluster.Stars.GetCenter());
                
                if (Math.Ceiling((i - 1) * dt / 365) < Math.Ceiling(i * dt / 365) && ++year % saveInterval == 0)
                {
                    Console.WriteLine($@"Exportiere Daten... Jahr: {(int)i * dt / 365} = {year}");
                    while (!SQL.addRows(cluster.Stars, year / saveInterval, wtable))
                    {
                        Thread.Sleep(100);
                    }
                } 
            }

            Key.Abort();

            while (cluster.Savethreads.FindAll(x => x.IsAlive).Count > 1)
                Console.WriteLine(
                    @"Warte auf die Beendigung von {0} Speicher Threads",
                    cluster.Savethreads.FindAll(x => x.IsAlive).Count);
            Thread t = cluster.Savethreads.Find(x => x.IsAlive);
            try
            {
                Console.WriteLine(t.ThreadState.ToString());
                Console.WriteLine(t.Name);
                while (t.IsAlive) ;
                Thread.Sleep(10);
            }
            catch { }
            SQL.order(wtable);
            broadcaster.SendToChannel("steps", "abort");

            Console.WriteLine("Direkt in Dataview öffnen? (y/n)");
            string view = Console.ReadLine();                         //wait for input
            if (view == "y" || view == "Y")
                System.Diagnostics.Process.Start(@"DataView.exe", wtable);
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
