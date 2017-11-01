using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using ClusterSim.ClusterLib;

namespace ClusterSim.Net.Server
{
    class Program
    {
        static List<Client> Clients = new List<Client>();

        static void Main(string[] args)
        {
            new System.Threading.Thread(Listen).Start();
            
            StarCluster Cluster = new StarCluster("test", "lang", 0, 1);
            int i=0,step=0;

            StringBuilder msg= new StringBuilder();

            Console.CursorVisible=false;
            while (true)
                try
                {
                    List<Client> ready;
                    do {
                        ready = Clients.FindAll(x => x.ready&&x.performance!=0);
                        foreach (Client c in Clients)
                            if (!c.ctThread.IsAlive)
                                Clients.Remove(c);

                    } while (ready.Count < (double)Clients.Count/1.5);
                    
                        
                    var NewStars = new List<Star>();
                    foreach (Client c in Clients)
                        if (c.step == step + 1)
                            NewStars.AddRange(c.NewStars);
                    var tepmlist = NewStars.Distinct().ToList();
                    NewStars.Clear(); NewStars.AddRange(tepmlist);


                    if (NewStars.Count == Cluster.Stars.Count)
                    {
                        Cluster.Stars = NewStars;
                        step++;
                    }
                    else
                        Console.WriteLine("Schritt Fehlgeschlagen");// throw new Exception();
                    
                    
                    double gesper=0;
                    

                    foreach (Client c in ready)
                        gesper += c.performance;
                    double coe = (double)(Cluster.Stars.Count-ready.Count) / gesper;
                    
                    msg.Append(String.Format("{0,5} {1,11} {2,20}\n\n", "Step: "+step, "Client", "Performance"));
                    

                    int start = 0;
                    int end;
                    foreach (Client c in ready)
                    {
                        
                        end = start + (int) Math.Round(c.performance * coe);
                        c.Send(step, start, end, ref Cluster.Stars);
                        start = end + 1;
                        msg.Append(String.Format("{0,5} {1,11} {2,20} \n", "G: "+Math.Round(gesper,1),c.id, Math.Round(c.performance,3)));
                    }
                    
                    

                    Console.CursorTop = 0;//-= 2 + Clients.Count;
                    Console.Write(msg);
                    
                    msg = new StringBuilder();
                }
                catch { }

        }

        private static void Listen()
        {
            var listener = new TcpListener(4242);
            var tempClient = default(TcpClient);

            Console.WriteLine("Server started istening on 4242\n");
            
            listener.Start();

            while (true)
            {
                tempClient = listener.AcceptTcpClient();
                Clients.Add(new Client());
                Console.WriteLine(" >> Client {0} connected", Clients.Last().id);
                Clients.Last().StarClient(tempClient);
            }

        }

        

    }
}
