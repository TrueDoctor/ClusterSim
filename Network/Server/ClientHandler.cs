using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using ClusterSim.Net.Lib;
using ClusterSim.ClusterLib;

namespace ClusterSim.Net.Server
{
    public class ClientHandler
    {
        public TcpClient clientSocket;

        List<Star> OldStars = new List<Star>();
        public List<Star> NewStars = new List<Star>();
        public Thread ctThread;

        static int Id;
        public int step,mstep,min,max,id=Id++;
        public bool abort=false, reciveFinished=false, ready=false,send = false;
        public double performance = 1;
        

        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;

            ctThread = new Thread(ConnectionLoop);
            ctThread.Name = "Client:" + id;
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

                    Message msg = new Message(step, min, max, OldStars.ToArray());

                    networkStream.Write(msg.Serialize(OldStars.Count),0,size);
                    networkStream.Flush();
                    send = false;


                    reciveFinished = false;
                    var temp = new byte[size];
                    networkStream.Read(temp, 0, size);
                    
                    var tempStars = msg.Deserialize(temp);

                if (msg.Stars != null)
                    if ((max - min != tempStars.Count - 1))
                        throw new Exception("Falsche Rüchgabelänge bei:" + clientSocket.Client.LocalEndPoint.ToString());
                    else
                    {
                        NewStars.Clear();
                        foreach (Star s in tempStars)
                            NewStars.Add(s.Clone());
                    }
                    mstep = msg.step;
                    watch.Stop();

                    if (max-min!=NewStars.Count-1)
                        NewStars = null;
                    performance = (double)(max - min)+1<1?1:((max-min)+1) /(double) watch.ElapsedMilliseconds;

                    reciveFinished = true;
                    networkStream.Flush();
                    
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine(" >> Client {0} disconnected\n\n{1}",id,e);
                    return;
                }
            }
            Console.Clear();
        }
        public void Send(int step, int min, int max, ref List<Star> DStars)
        {
            this.step = step;
            this.min = min;
            this.max = max;
            this.OldStars = DStars;
            send = true;
        }
        
    }
}