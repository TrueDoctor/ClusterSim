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
    using ClusterSim.ClusterLib.Calculation.Gpu;
    using ClusterSim.ClusterLib.Utility;

    public class Program
    {
        private static bool abort = false;

        public static int SaveInterval { get; set; } = 3650;

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
                last = SQL.lastStep(rTable); // get last step of given table
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
            var cluster = new Cluster(SQL.readStars(rTable, last), dt); // instantiate Starcluster
            var sub = new GpuCluster(SQL.readStars(rTable, last));

            var time = (double)last * SaveInterval;

            //ComputeWorker.CalcAcc(cluster.Stars, cluster.Stars.Select(x=>x as IMassive).ToList());


            Cluster.GalaxyMass = 1.4e7;
            cluster.ParentDt = SaveInterval / 10;

            foreach (var subStar in sub.Stars)
            {
                subStar.Dt = dt;
                subStar.ToCompute = true;
            }

            foreach (var clusterStar in cluster.Stars)
            {
                clusterStar.ToCompute = true;
            }

            sub.Dt = cluster.Dt;
            sub.ParentDt = cluster.ParentDt;

            var tide = new WaveGenerator(Cluster.GalaxyMass, 3.162e14);

            //sub.Burst = 2;
            
            ComputeWorker.Initialize(sub.Stars.Length);
            //sub.DoStep(Misc.Method.Rk5, tide.GetVirtualDistance(time));
            //sub.Burst = 200;

            int prev = 0;

            for (int i = (last /** SaveInterval * 365*/)+1;
                 !abort; i++)
            {
                cluster.DoStep(Misc.Method.Rk5, false, false, tide.GetVirtualDistance(time));
                sub.DoStep(Misc.Method.Rk5, tide.GetVirtualDistance(time));
                var maxDAcc = cluster.Stars.Max(x => x.DAcc);
                if (maxDAcc > 0 && false)
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

                    //X.Add(watch.ElapsedMilliseconds / 1.0 / 1000.0);

                    watch.Restart();

                    for (int j = 0; j < 1; j++)
                    {
                        sub.DoStep(Misc.Method.Rk5, tide.GetVirtualDistance(time));
                    }

                    watch.Stop();

                    //Sub.CalcDt();

                    //SQL.addRows(Sub.Stars, i, wTable);
                    Console.WriteLine("sub: " + watch.ElapsedMilliseconds / 1.0 / 1000.0);
                    //Y.Add(watch.ElapsedMilliseconds / 1.0 / 1000.0);

                    //cluster.CalcDt();
                    //Sub.GetSubsetSeeds().ForEach(s => Console.Write($"{s}, "));
                }

                time += sub.ParentDt;

                sub.ParentDt = sub.Dt * 50 * sub.Burst;

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


                if ((int)(time / SaveInterval) > year)
                {
                    ++year;
                    sub.Stars.MoveCenter(sub.Stars.GetCenter());
                    Console.WriteLine($@"Exportiere Daten... Jahr: {(int)time / SaveInterval} = {year}");
                    while (!SQL.addRows(sub.Stars.ToList(), year, wTable))
                    {
                        Thread.Sleep(5000);
                    }
                }
                else if ((int)((time / SaveInterval - year) * 10) != prev)
                {
                    prev = (int)(((time / SaveInterval) - year) * 10);
                    Console.WriteLine($" Nächster Schritt zu {prev*10}% fertig");
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
