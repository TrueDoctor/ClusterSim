namespace ClusterSim.Net.Server
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.ClusterLib.Utility;
    using ClusterSim.Net.Server.Properties;

    #endregion

    public class Server
    {
        private readonly List<ClientHandler> newClients = new List<ClientHandler>();

        private List<ClientHandler> clients = new List<ClientHandler>();

        public string rtable { get; set; } = "n500dtv";

        public string wtable { get; set; } = "copy2";

        public delegate void SendHandler(Server s, Lib.SendEventArgs e);

        public async void Main()
        {
            var listenThread = new Thread(this.Listen) { Name = "ListenThread" };
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Title = "ClusterSim - Distribution Server";

            const int saveInterval = 365000, dt = 1;
            int step = SQL.lastStep(this.rtable) * saveInterval * 365 + 1;
            step = SQL.lastStep(this.rtable);
            int errors = 0, year = step;
            double ovrper = 1, time = 0;

            Console.WriteLine("Load Stars...");
            var cluster = new SubCluster(SQL.readStars(this.rtable, step - 1));
            Cluster.GalaxyMass = -1;
            Console.WriteLine("Loading finished");
            listenThread.Start();

            var msg = new StringBuilder();
            
            Console.CursorVisible = false;

            Cluster.GalaxyMass = 0; // 1.4e6;
            cluster.Dt = 1;
            cluster.ParentDt = 365;
            cluster.DoStep(Misc.Method.Rk5, true, 0, -1, 3.162e+9);
            
            var tide = new WaveGenerator(1.4e6, 3.162e9);

//            var logger = new DataLogger();

            while (true)
            {
                try
                {
                    var ready = this.RefreshClients();

                    var watch = Stopwatch.StartNew();

                    double partper = ready.Sum(c => c.Performance);

                    double coe = cluster.Stars.Count / partper;

                    msg.Append(
                        string.Format(
                            "Step: {0,5} {1,11} {2,30} Errors: {3}\n\n",
                            step,
                            "Client",
                            "Performance",
                            errors));
                    
                    cluster.Stars.ForEach(x => x.ToCompute = true);

                    var nodes = this.clients.Select(c => c as IComputationNode).ToList();

                    var computation = cluster.DoStep(Misc.Method.Rk5, nodes, tide.GetVirtualDistance(time));
                    
                    await computation;

                    Console.WriteLine(step);

                    foreach (var c in this.clients)
                    {
                        msg.Append(
                            string.Format(
                                "{0}  G:   {1,4:f2} {2,5} : {3,13} {4,12:f2}    \n",
                                ready.Contains(c) ? '*' : ' ',
                                ovrper,
                                c.id,
                                c.clientSocket.Client.RemoteEndPoint.ToString().Split(':')[0],
                                c.Performance));
                    }
                    
                    var newStars = computation.Result;

                    time += cluster.Dt;

                    if (newStars.Count != cluster.Stars.Count && step != 0)
                    {
                        throw new Exception("Falsche Rückgabelänge => Rechenergebnisse fehler- oder lückenhaft");
                    }


                    for (var i = 0; i < cluster.Stars.Count; i++)
                    {
                        var temp = newStars.Find(x => x.id == i);
                        if (temp != null)
                        {
                            cluster.Stars[i] = temp.Clone();
                        }
                        else
                        {
                            Console.Beep();
                            Console.WriteLine("\n\n\nSchritt Fehlgeschlagen");

                            // errors++;
                            // step--;
                            throw new Exception(
                                string.Format(
                                    $"client: {ready[0].Step}  \nServer: {step} \n0 = {newStars.Count - cluster.Stars.Count}"));
                        }
                    }

                    step++;
                    Console.Beep();

                    if (wtable.Length != 0
                        && Math.Ceiling((time - cluster.ParentDt) / saveInterval) < Math.Ceiling(time / saveInterval) && ++year % 1 == 0)
                    {
                        while (!SQL.addRows(cluster.Stars, year, wtable))
                        {
                            Thread.Sleep(100);
                        }
                    }

                    cluster.ParentDt = cluster.Dt * 50;

                    if (cluster.ParentDt > saveInterval / 50.0)
                    {
                        cluster.ParentDt = saveInterval / 50.0;
                    }

                    watch.Stop();
                    ovrper = (watch.ElapsedMilliseconds / 1000.0 + ovrper) / 2.0;

//                    var multiplier = cluster.Dt / cluster.Stars.Min(x => x.Dt);
//                    logger.Log(Cluster.EstimateStepTime(true) * multiplier, Cluster.CalculationComplexity, watch.ElapsedMilliseconds);

                    Console.Clear();
                    Console.Write(msg);

                    msg = new StringBuilder();
                }

                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine(e.Message);
                    Console.Beep();
                    Thread.Sleep(2000);
                    Console.Clear();
                    errors++;
                    foreach (var c in this.clients)
                    {
                        c.Performance = 1;
                    }
                }//*/
            }
        }
        
        private List<ClientHandler> RefreshClients()
        {
            double gesper, partper;
            List<ClientHandler> ready;

            // Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            do
            {
                if (this.clients.Exists(x => x.Abort)) Console.Clear();

                this.newClients.RemoveAll(x => x.Abort);
                this.clients.RemoveAll(x => x.Abort);
                this.clients = this.clients.Union(this.newClients).ToList();

                gesper = 0;
                partper = 0;
                ready = this.clients.FindAll(x => x.Available && x.Performance != 0);

                try
                {
                    foreach (var c in this.clients)
                    {
                        if (c.ctThread != null)
                        {
                            if (!c.ctThread.IsAlive)
                            {
                                c.Abort = true;
                            }
                        }

                        gesper += c.Performance;
                    }

                    Thread.Sleep(2);
                    foreach (var c in ready) partper += c.Performance;
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine(e);
                }

            } while (partper <= gesper / 1.2 || ready.Count == 0 );

            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            return ready;
        }

        /// <summary>
        ///     The listen Thread.
        /// </summary>
        private void Listen()
        {
            string HostName = Dns.GetHostName();
            var hostInfo = Dns.GetHostEntry(HostName);
            var listener = new TcpListener(
                hostInfo.AddressList.First(x => x.ToString().Contains('.')),
                Settings.Default.Port);
            var tempClient = default(TcpClient);

            Console.WriteLine("Server started listening on {0} : {1}\n", Settings.Default.Port, listener.LocalEndpoint);

            listener.Start();

            while (true)
            {
                tempClient = listener.AcceptTcpClient();
                this.newClients.Add(new ClientHandler());
                Console.Beep();
                Console.WriteLine(@"Warte auf die Beendigung von {0} Speicher Threads", this.newClients.Last().id);
                this.newClients.Last().StarClient(tempClient);
            }
        }
    }
}