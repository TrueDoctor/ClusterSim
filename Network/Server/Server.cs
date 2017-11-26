﻿namespace ClusterSim.Net.Server
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

    using ClusterSim.ClusterLib;
    using ClusterSim.Net.Server.Properties;

    #endregion

    
    public class Server
    {
        private readonly List<ClientHandler> newClients = new List<ClientHandler>();

        private List<ClientHandler> clients = new List<ClientHandler>();

        public string rtable { get; set; } = "lang";

        public string wtable { get; set; } = "lang";

        public delegate void SendHandler(Server s, SendEventArgs e);
        
        public event SendHandler SendData;
        
        public void Main()
        {
            var listenThread = new Thread(this.Listen) { Name = "ListenThread" };
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Title = "ClusterSim - Distribution Server";

            int step = SQL.lastStep(rtable), errors = 0, dt = 1000000, year = step;
            double ovrper = 1;

            Console.WriteLine("Load Stars...");
            var Cluster = new StarCluster(rtable, wtable, step, dt);
            Console.WriteLine("Loading finished");
            listenThread.Start();

            var msg = new StringBuilder();
            var watch = new Stopwatch();

            Console.CursorVisible = false;
            while (true)
            {
                try
                {
                    double gesper = 0, partper = 0;

                    List<ClientHandler> ready;
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    do
                    {
                        if (this.clients.Exists(x => x.Abort)) Console.Clear();

                        this.newClients.RemoveAll(x => x.Abort);
                        this.clients.RemoveAll(x => x.Abort);
                        this.clients = this.clients.Union(this.newClients).ToList();

                        gesper = 0;
                        partper = 0;
                        ready = this.clients.FindAll(x => x.Ready && x.Performance != 0);

                        try
                        {
                            foreach (var c in this.clients)
                            {
                                if (c.ctThread != null) if (!c.ctThread.IsAlive) c.Abort = true;
                                gesper += c.Performance;
                            }

                            Thread.Sleep(20);
                            foreach (var c in ready) partper += c.Performance;
                        }
                        catch (Exception e)
                        {
                            Console.Clear();
                            Console.WriteLine(e);
                        }
                        
                    }
                    while (partper <= gesper / 1.2 || ready.Count == 0 || Cluster.Stars == null);
                    Thread.CurrentThread.Priority = ThreadPriority.Normal;

                    watch.Reset();
                    watch.Start();

                    double coe = Cluster.Stars.Count / partper;

                    msg.Append(
                        string.Format(
                            "Step: {0,5} {1,11} {2,30} Errors: {3}\n\n",
                            step,
                            "Client",
                            "Performance",
                            errors));

                    var start = 0;
                    int end;


                    var orders = new List<int[]>();

                    foreach (var c in ready)
                    {
                        end = start + (int)Math.Round(c.Performance * coe);
                        orders.Add(new[] { c.id, start, c != ready.Last() ? end - 1 : Cluster.Stars.Count - 1 });
                        start = end;
                    }

                    var send = new SendEventArgs(step,dt, orders, Cluster.Stars.ToArray());
                    this.SendData(this, send);

                    Console.WriteLine(step);

                    foreach (var c in this.clients)
                        msg.Append(
                            string.Format(
                                "{0}  G:   {1,4:f2} {2,5} : {3,13} {4,12:f2}    \n",
                                ready.Contains(c) ? '*' : ' ',
                                ovrper,
                                c.id,
                                c.clientSocket.Client.RemoteEndPoint.ToString().Split(':')[0],
                                c.Performance));

                    while (ready.Exists(
                            x => x.ctThread.IsAlive
                                 && (!x.Ready || !x.ReceiveFinished || x.Step == x.Mstep
                                     || x.NewStars
                                     == null)) /*&& (!(watch.ElapsedMilliseconds / 10000.0 > ovrper))||true*/
                    ) Thread.Sleep(20);
                    
                    var NewStars = new List<Star>();
                    foreach (var c in ready)
                    {
                        if (c.Mstep > step && c.NewStars != null)
                        {
                            NewStars.AddRange(c.NewStars);
                        }
                        else Console.WriteLine(c.Mstep);
                    }

                    if (NewStars.Count != Cluster.Stars.Count && step != 0)
                        throw new Exception("Falsche Rückgabelänge => Duplikate");


                    for (var i = 0; i < Cluster.Stars.Count; i++)
                    {
                        var temp = NewStars.Find(x => x.id == i);
                        if (temp != null)
                        {
                            Cluster.Stars[i] = temp.Clone();
                        }
                        else
                        {
                            Console.Beep();
                            Console.WriteLine("\n\n\nSchritt Fehlgeschlagen");

                            // errors++;
                            // step--;
                            throw new Exception(
                                string.Format(
                                    $"client: {ready[0].Step}  \nServer: {step} \n0 = {NewStars.Count - Cluster.Stars.Count}"));
                        }
                    }

                    step++;
                    Console.Beep();

                    if (wtable.Length != 0 && Math.Ceiling((double)(step-1) *dt / 120)< Math.Ceiling((double)step * dt / 120)&&year++>-1)
                        foreach (Star s in Cluster.Stars)
                            if (!s.dead)
                                while (SQL.addRow(s, year, wtable) == false) ;//do until succesfull
    
    
                    /*if (NewStars.Count == Cluster.Stars.Count)
                    {
                        step++;
                        Console.Beep();
    
                        if (wtable.Length != 0 && step * dt % 36500 == 0)
                            foreach (Star s in Cluster.Stars)
                                while (SQL.addRow(s, step, wtable) == false) ;//do until succesfull
                    }
                    else 
                    {
                        Console.Beep();Console.WriteLine("\n\n\nSchritt Fehlgeschlagen");
                        errors++;
                        //step--;
                        throw new Exception(String.Format($"client: {ready[0].step.ToString()}  \nServer: {step} \n0 = {(NewStars.Count - Cluster.Stars.Count)}"));
    
                    }
                    */
                    watch.Stop();
                    ovrper = (watch.ElapsedMilliseconds / 1000.0 + ovrper) / 2.0;

                    // Console.CursorTop = 0;//-= 2 + Clients.Count;
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
                    foreach (ClientHandler c in clients)
                    {
                        c.Performance = 1;
                    }
                }//*/
            }
        }

        /// <summary>
        ///     The listen.
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
                Console.WriteLine(" >> Client {0} connected", this.newClients.Last().id);
                this.newClients.Last().Subscribe(this);
                this.newClients.Last().StarClient(tempClient);
            }
        }

        /// <summary>
        ///     The startup.
        /// </summary>
        private void Startup()
        {
            throw new NotImplementedException();
        }
    }
}