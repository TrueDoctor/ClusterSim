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

        public static int SaveInterval { get; set; } = 365000;
        
        public static void Main(string[] args)
        {
            XDMessagingClient client = new XDMessagingClient(); // https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            string rTable = string.Empty;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (args.Length > 0) // if the program gets called with arguments
            {
                var list = SQL.readTables();

                if (list != null)
                {
                    var res = new List<string>();
                    foreach (string s in args)
                    {
                        if (list.Contains(s))
                        {
                            //check if argument is a valid table name
                            rTable = s;
                        }

                        Console.WriteLine(rTable);
                    }
                }
            }

            if (rTable == string.Empty)
            {
                // if argument handover failed or was not  given
                Console.WriteLine("Auswahltabelle: "); // input name manually 
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

            int n = 2; // Convert.ToInt32(Console.ReadLine());

            int year = last;
            // Console.WriteLine(@"Warte auf die Beendigung von {0} Speicher Threads", Math.Round((dt * n) / 365, 2));
            // Thread.Sleep(2000);
            broadcaster.SendToChannel("steps", "s" + n);// send max step to steps channel


            Thread key = new Thread(listen);
            key.Start();
            var cluster = new BoxCluster(SQL.readStars(rTable, last), dt); // instatiate Starcluster
            var sub = new SubCluster(SQL.readStars(rTable, last), dt);

            var time = (double)last * SaveInterval;

            Cluster.GalaxyMass = 0; // 1.4e6;
            cluster.ParentDt = 365;
            cluster.DoStep(Misc.Method.Rk5, true, 0, -1, 3.162e+9);

            sub.Stars = new List<Star>(cluster.Stars.Select(x => x.Clone()));
            sub.ParentDt = cluster.ParentDt;
            sub.Dt = cluster.Dt;

            var tide = new WaveGenerator(1.4e6, 3.162e9);

            var X = new List<double>();

            var Y = new List<double>();
            

            for (int i = (last /** SaveInterval * 365*/) + 1;
                 !abort; i++)
            {
                var maxDAcc = cluster.Stars.Max(x => x.DAcc);
                if (maxDAcc > 0)
                {
                    //cluster.ParentDt = 3000;
                    //sub.ParentDt = 365;
                    var watch = Stopwatch.StartNew();

                    for (int j = 0; j < 1; j++)
                    {
                         //cluster.DoStep(Misc.Method.Rk5, true, 0, -1);
                        //DoStep(ref Sub);
                    }

                    watch.Stop();

                    //SQL.addRows(cluster.Stars, i, wTable);

                    //Console.WriteLine("n: " + watch.ElapsedMilliseconds / 1.0 / 1000.0);

                    X.Add(watch.ElapsedMilliseconds / 1.0 / 1000.0);

                    watch.Restart();

                    for (int j = 0; j < 1; j++)
                    {
                        //DoStep(ref Sub);
                        sub.DoStep(Misc.Method.Rk5, false, 0, -1, tide.GetVirtualDistance(time));
                    }

                    watch.Stop();

                    //Sub.CalcDt();

                    //SQL.addRows(Sub.Stars, i, wTable);
                    Console.WriteLine("sub: " + watch.ElapsedMilliseconds / 1.0 / 1000.0);
                    Y.Add(watch.ElapsedMilliseconds / 1.0 / 1000.0);

                    //cluster.CalcDt();
                     //Sub.GetSubsetSeeds().ForEach(s => Console.Write($"{s}, "));
                }

                time += sub.ParentDt;

                sub.ParentDt = sub.Dt * 50;

                if (sub.ParentDt > SaveInterval / 50)
                {
                    sub.ParentDt = SaveInterval / 50;
                }

                /*GnuPlot.HoldOn();
                GnuPlot.Unset("logscale y");
                GnuPlot.Set("key top left", "xlabel 'Dauer Normal'", "ylabel 'Gesamtdauer'");*/
                //GnuPlot.Plot(X.ToArray(), Y.ToArray(), "title 'SubCluster' ");
                //GnuPlot.Plot(X.ToArray(), X.ToArray(), "title 'Normal' w linespoints");

                //broadcaster.SendToChannel("steps", $"i{i}");

                // send "i"+step in channel steps
                // Console.WriteLine("\n");//+ i + "\n ");
                sub.Stars.MoveCenter(sub.Stars.GetCenter());


                if ((int)(time / SaveInterval) > year)
                {
                    ++year;
                    Console.WriteLine($@"Exportiere Daten... Jahr: {(int) time / SaveInterval} = {year}");
                    while (!SQL.addRows(sub.Stars, year, wTable))
                    {
                        Thread.Sleep(5000);
                    }
                }
            }

            key.Abort();

            SQL.order(wTable);
            broadcaster.SendToChannel("steps", "abort");

            Console.WriteLine("Direkt in Dataview öffnen? (y/n)");
            string view = Console.ReadLine();                         //wait for input
            if (view == "y" || view == "Y")
                System.Diagnostics.Process.Start(@"DataView.exe", wTable);
        }

        private static void DoStep(ref SubCluster cluster, double dt = 30)
        {
            List<Star> newStars;
            
            for (double time = 0; time < cluster.ParentDt;)
            {
                var subClusters = cluster.DivideIntoSubClusters(true);
                var temp = new ConcurrentBag<Star>();

                time += subClusters.First().Dt;

                Parallel.ForEach(
                    subClusters,
                    c =>
                        {
                            var stars = c.DoStep(Misc.Method.Rk5, true);
                            foreach (var star in stars)
                            {
                                temp.Add(star);
                            }
                        });
                


                newStars = temp.OrderBy(s => s.id).ToList();
                

                if (newStars.Count != cluster.Stars.Count)
                {
                    var duplicates = newStars.Where(x => newStars.Count(c => c.id == x.id) > 1).Select(d => d.id).Distinct()
                        .ToList();
                    foreach (var duplicate in duplicates)
                    {
                        while (newStars.Remove(newStars.Where(x => x.id == duplicate).OrderBy(c => c.DAcc).First()) && newStars.Count(c => c.id == duplicate) > 1)
                        {
                        }
                    }

                    // throw new Exception("Duplikate!");
                }

                cluster = new SubCluster(newStars, dt: subClusters.First().Dt)
                              {
                                  ParentDt = cluster.ParentDt,
                                  Stars = newStars.Select(
                                      x => x.Clone()).ToList()
                              };
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
