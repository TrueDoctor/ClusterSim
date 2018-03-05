using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using XDMessaging;

namespace ClusterSim.Standalone
{
    using System.CodeDom.Compiler;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Forms.VisualStyles;

    using ClusterSim.ClusterLib.Analysis;
    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.ClusterLib.Utility;

    public class Program
    {
        private static bool abort = false;

        public static int SaveInterval { get; set; } = 100;

        public static double MinDAcc { get; set; } = 0.001;

        private static double lastdt = 30;

        public static void Main(string[] args)
        {
            XDMessagingClient client = new XDMessagingClient(); // https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            string rTable = string.Empty;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (args.Length > 0) // if the program gets called with arguments
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


            if (wTable.Equals(string.Empty))
            {
                wTable = rTable;
                last = SQL.lastStep(rTable);//get last step of given table
            }


            Console.WriteLine("\nDelta t in Tagen: ");

            double dt = Convert.ToDouble(Console.ReadLine());

            //Console.WriteLine("\nSchritte: ");

            int n = 2;//Convert.ToInt32(Console.ReadLine());

            int year = last * SaveInterval;
            Console.WriteLine(@"Warte auf die Beendigung von {0} Speicher Threads", Math.Round((dt * n) / 365, 2));
            Thread.Sleep(2000);
            broadcaster.SendToChannel("steps", "s" + n);// send max step to steps channel


            Thread Key = new Thread(listen);
            Key.Start();
            var cluster = new SubCluster(SQL.readStars(rTable, last) , dt); // instatiate Starcluster
            var Sub = new SubCluster(SQL.readStars(rTable, last), dt);

            var time = 0d;


            cluster.ParentDt = 1000;
            cluster.DoStep(Misc.Method.Rk5, true, 0, -1);
            //cluster.DoStep(Misc.Method.Rk5, true, 0, -1);
            Sub.Stars = new List<Star>(cluster.Stars.Select(x=>x.Clone()));
            Sub.Dt = cluster.Dt;
            for (int i = (last /** SaveInterval * 365*/) + 1;
                 !abort; i++)
            {
//                cluster.Dt = dt;
//                cluster.ParentDt = dt;

//                cluster.DoStep(Misc.Method.Rk5, true, 0, -1);
                var maxDAcc = cluster.Stars.Max(x => x.DAcc);
                if (maxDAcc > 0)
                {
                    //dt += (dt * MinDAcc / maxDAcc - dt) / 2;

                    //cluster.CalcDt();
                    //dt = dt * SubCluster.MinPrecision / maxDAcc;
                    //cluster.Dt = dt;
                    cluster.ParentDt = 1000000;
                    Sub.ParentDt = 1000000;
                    Stopwatch watch = Stopwatch.StartNew();

                    for (int j = 0; j < 1; j++)
                    {
                       cluster.DoStep(Misc.Method.Rk5, true, 0, -1);
                    }

                    SQL.addRows(cluster.Stars, i, wTable);
                    watch.Stop();

                    Console.WriteLine(watch.ElapsedMilliseconds / 1.0 /1000.0);

                    watch.Restart();

                    for (int j = 0; j < 1; j++)
                    {
                        doStep(Sub);
                    }

                    //SQL.addRows(Sub.Stars, i, wTable);
                    watch.Stop();
                    Console.WriteLine(watch.ElapsedMilliseconds / 1.0 / 1000.0);
                    

                    //cluster.CalcDt();
                    // sub.GetSubsetSeeds().ForEach(s => Console.Write($"{s}, "));
                }

                time += dt;

                //broadcaster.SendToChannel("steps", $"i{i}");

                // send "i"+step in channel steps
                // Console.WriteLine("\n");//+ i + "\n ");
                cluster.Stars.MoveCenter(cluster.Stars.GetCenter());


                if (Math.Ceiling((time - dt) / 365) < Math.Ceiling(time / 365) && ++year % SaveInterval == 0)
                {
                    Console.WriteLine($@"Exportiere Daten... Jahr: {(int)i * dt / 365} = {year}");
//                    while (!SQL.addRows(cluster.Stars, year / SaveInterval, wTable))
//                    {
//                        Thread.Sleep(100);
//                    }
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

        private static void doStep(SubCluster cluster, double dt = 30)
        {
            var newStars = new List<Star>();
            
            //Parallel.ForEach(subclusters, c => newStars.AddRange(c.DoStep(Misc.Method.Rk5, true)));
            for (double time = 0; time < cluster.ParentDt;)
            {
                var subclusters = cluster.DivideIntoSubclusters();
                var temp = new ConcurrentBag<Star>();

                //var test = subclusters.Select(x => x.Stars.Count(s => s.ToCompute)).ToList();
                time += subclusters.First().Dt;

                Parallel.ForEach(subclusters,
                    c =>
                        {
                            var stars = c.DoStep(Misc.Method.Rk5, true);
                            foreach (var star in stars)
                            {
                                temp.Add(star);
                            }
                        });
                


                newStars = temp.OrderBy(s => s.id).ToList();

                var duplicates = newStars.Where(x => newStars.Count(c => c.id == x.id) > 1).Select(d=>d.id).Distinct().ToList();
                foreach (var duplicate in duplicates)
                {
                    while (newStars.Remove(newStars.Where(x => x.id == duplicate).OrderBy(c => c.DAcc).First()) && newStars.Count(c => c.id == duplicate) > 1)
                    {
                    }
                }

                cluster = new SubCluster(newStars, dt: subclusters.First().Dt) {ParentDt = cluster.ParentDt};
                cluster.Stars = newStars.Select(x=>x.Clone()).ToList();
                cluster.Stars.ForEach(x => x.ToCompute = false);
            }


            //cluster.Stars = newStars;
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
