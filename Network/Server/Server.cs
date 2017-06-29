using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using ClusterSim.ClusterLib;

namespace ClusterSim.Net.Server
{
    class Server
    {
        static List<ClientHandler> Clients = new List<ClientHandler>();
        static List<ClientHandler> newClients = new List<ClientHandler>();

        static void Main(string[] args)
        {
            Thread listenThread = new Thread(Listen);
            listenThread.Name = "ListenThread";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Title = "CluserSim - Distribution Server";

            int step = SQL.lastStep("lang"),errors=0, dt = 1;
            double ovrper=1;

            Console.WriteLine("Load Stars...");
            StarCluster Cluster = new StarCluster("lang", "lang", SQL.lastStep("lang"), dt);
            Console.WriteLine("Loading finished");
            listenThread.Start();
            string wtable = "lang";

            StringBuilder msg = new StringBuilder();
            var watch = new System.Diagnostics.Stopwatch();
            
            Console.CursorVisible = false;
            while (true)
                try
                {
                    double gesper = 0, partper = 0;

                    #region Wait for clients
                    List<ClientHandler> ready;
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    do
                    {
                        if (Clients.Exists(x => x.abort))
                            Console.Clear();

                        newClients.RemoveAll(x => x.abort);
                        Clients.RemoveAll(x => x.abort);
                        Clients = Clients.Union(newClients).ToList();


                        gesper = 0; partper = 0;
                        ready = Clients.FindAll(x => x.ready && x.performance != 0);

                        try
                        {
                            foreach (ClientHandler c in Clients)
                            {
                                if (c.ctThread != null)
                                    if (!c.ctThread.IsAlive)
                                        c.abort = true;
                                gesper += c.performance;
                            }
                            Thread.Sleep(20);
                            foreach (ClientHandler c in ready)
                                partper += c.performance;

                        }
                        catch (Exception e) { Console.Clear(); Console.WriteLine(e); }//Console.ReadLine(); }
                    } while (partper <= gesper/1.2 || ready.Count == 0 || Cluster.Stars == null);
                    Thread.CurrentThread.Priority = ThreadPriority.Normal;

                    #endregion

                    #region give orders
                    watch.Reset();
                    watch.Start();

                    double coe = (double) (Cluster.Stars.Count) / partper;

                    msg.Append(String.Format("Step: {0,5} {1,11} {2,30} Errors: {3}\n\n", step, "Client", "Performance",errors));


                    int start = 0;
                    int end;
                    foreach (ClientHandler c in ready)
                    {

                        end = start + (int) Math.Round(c.performance * coe);
                        c.Send(step, start, c != ready.Last() ? end - 1 : Cluster.Stars.Count - 1, ref Cluster.Stars);
                        start = end;

                    }
                    Console.WriteLine(step);
                    foreach (ClientHandler c in Clients)
                        msg.Append(String.Format("{0}  G:   {1,4:f2} {2,5} : {3,13} {4,12:f2}    \n", ready.Contains(c) ? '*' : ' ', ovrper, c.id, c.clientSocket.Client.RemoteEndPoint.ToString().Split(':')[0], c.performance));
                    #endregion

                    #region merge Results

                    while (ready.Exists(x => x.ctThread.IsAlive && (!x.ready || !x.reciveFinished || x.step == x.mstep)) && !(watch.ElapsedMilliseconds / 10000.0 > ovrper)) { Thread.Sleep(20); }// if (watch.ElapsedMilliseconds / 10000.0 > ovrper) throw new Exception("Timeout"); }

                    //Console.WriteLine($"ready: {ready.Count}");
                    
                    var NewStars = new List<Star>();
                    foreach (ClientHandler c in ready)
                        if (c.mstep > step && c.NewStars != null)
                        {

                            NewStars.AddRange(c.NewStars.ToList()); 
                        }
                        else
                            Console.WriteLine(c.mstep);

                    for (int i = 0; i < Cluster.Stars.Count; i++)
                    {
                        var temp = NewStars.Find(x => x.id == i);
                        if (temp != null&&!temp.dead)
                        {
                            Cluster.Stars[i] = temp;
                        }
                        else
                        {
                            Console.Beep(); Console.WriteLine("\n\n\nSchritt Fehlgeschlagen");
                            //errors++;
                            //step--;
                            throw new Exception(String.Format($"client: {ready[0].step.ToString()}  \nServer: {step} \n0 = {(NewStars.Count - Cluster.Stars.Count)}"));
                        }
                    }
                    step++;
                    Console.Beep();

                    if (wtable.Length != 0 && Math.Ceiling((double)(step-1) * dt / 3650)< Math.Ceiling((double)step * dt / 3650))
                        foreach (Star s in Cluster.Stars)
                            if (!s.dead)
                                while (SQL.addRow(s, step, wtable) == false) ;//do until succesfull

                    
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
                    #endregion

                    //Console.CursorTop = 0;//-= 2 + Clients.Count;
                    Console.Clear();
                    Console.Write(msg);

                    msg = new StringBuilder();
                }
                catch (Exception e) { Console.Clear(); Console.WriteLine(e.Message); Console.Beep(); Thread.Sleep(2000); Console.Clear(); errors++; }
            
        }

        private static void Listen()
        {
            string HostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry hostInfo = Dns.GetHostEntry(HostName);
            var listener = new TcpListener(hostInfo.AddressList[4], Properties.Settings.Default.Port);
            var tempClient = default(TcpClient);

            Console.WriteLine("Server started listening on {0} : {1}\n",Properties.Settings.Default.Port,listener.LocalEndpoint);

            listener.Start();

            while (true)
            {
                tempClient = listener.AcceptTcpClient();
                newClients.Add(new ClientHandler());
                Console.Beep();
                Console.WriteLine(" >> Client {0} connected", newClients.Last().id);
                newClients.Last().StarClient(tempClient);
            }

        }



    }
}
