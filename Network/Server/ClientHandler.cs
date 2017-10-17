using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

using ClusterSim.Net.Lib;
using ClusterSim.ClusterLib;

namespace ClusterSim.Net.Server
{
    public class ClientHandler
    {
        public TcpClient clientSocket;

        public List<Star> NewStars { get; private set; }
        public Thread ctThread;
        public Message msg { get; set; }
        public List<Star> OldStars { get => oldStars; }

        private readonly List<Star> oldStars = new List<Star>();

        static int Id;
        public int step, mstep, min, max, id = Id++;
        public bool abort = false, reciveFinished = false, ready = false, send = false;
        public double performance = 1;


        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            
            NewStars = new List<Star>();

            ctThread = new Thread(ConnectionLoop)
            {
                Name = "Client:" + id
            };
            ctThread.Start();

        }
        private void ConnectionLoop()
        {
            while (!abort)
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();

                    if (performance != 0)
                    {
                        ready = true;
                        while (!send) { Thread.Sleep(20); }
                        ready = false;
                    }
                    else
                    {
                        min = 0;
                        max = OldStars.Count - 1;
                        send = false;
                    }
                    watch.Reset();


                    watch.Start();

                    int size = OldStars.Count * 60 + 16;

                    msg = new Message(step, min, max, OldStars.ToArray());

                    networkStream.Write(msg.Serialize(OldStars.Count), 0, size);
                    networkStream.Flush();
                    send = false;


                    reciveFinished = false;
                    var temp = new byte[size];
                    networkStream.Read(temp, 0, size);

                    NewStars = msg.Deserialize(temp).ToList();

                    /*if (tempstars != null)
                        if ((max - min != tempStars.Count - 1))
                            throw new Exception("Falsche Rüchgabelänge bei:" + clientSocket.Client.LocalEndPoint.ToString());
                        else
                        {
                            NewStars.Clear();
                            foreach (Star d in tempStars)                       //remove for performance
                                if (OldStars.Exists(x => x.pos == d.pos && d.id != x.id))
                                    throw new NotImplementedException();
                            foreach (Star s in tempStars)
                                NewStars.Add(s.Clone());
                        }*/
                    mstep = msg.step;
                    watch.Stop();

                    if (max - min != NewStars.Count - 1)
                        NewStars = null;
                    performance = (double)(max - min) + 1 < 1 ? 1 : ((max - min) + 1) / (double)watch.ElapsedMilliseconds;

                    reciveFinished = true;
                    networkStream.Flush();

                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine(" >> Client {0} disconnected\n\n{1}", id, e);
                    return;
                }
            }
            Console.Clear();
        }
        public void Send(int step, int min, int max, List<Star> DStars)
        {
            this.step = step;
            this.min = min;
            this.max = max;
            //this.oldStars = new List<Star>();
            if (oldStars.Count==0)
            foreach (var S in DStars)
            {
                oldStars.Add(S.Clone());
            }
            send = true;
        }

    }
}