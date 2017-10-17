using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

using ClusterSim.Net.Lib;
using ClusterSim.ClusterLib;
using System.Threading.Tasks;

namespace ClusterSim.Net.Server
{
    public class ClientHandler
    {
        public TcpClient clientSocket;

        private List<Star> newStars = new List<Star>();
        public Star[] NewStars { get; private set; } //{ return newStars; } }
        public Thread ctThread;
        public Message msg { get; set; }
        public List<Star> OldStars { get => oldStars; }

        private  List<Star> oldStars = new List<Star>();

        static int Id;
        public int step, mstep, min, max, id = Id++;
        public bool abort = false, reciveFinished = false, ready = false, send = false;
        public double performance = 1;
        NetworkStream networkStream;
        event MessageHandler SendMessage;
        delegate void MessageHandler(ClientHandler c, MessageEventArgs msg);


        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            
            ctThread = new Thread(ConnectionLoop)
            {
                Name = "Client:" + id
            };
            ctThread.Start();

        }
        private async void ConnectionLoop()
        {
            while (!abort)
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                try
                {
                    networkStream = clientSocket.GetStream();

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
                    SendMessage += async (c, msg) =>
                    {
                        await networkStream.WriteAsync(msg.Msg.Serialize(OldStars.Count), 0, size);
                        networkStream.Flush();
                    };
                    networkStream.Write(msg.Serialize(OldStars.Count), 0, size);
                    networkStream.Flush();
                    send = false;


                    reciveFinished = false;
                    var temp = new byte[size];
                    networkStream.Read(temp, 0, size);


                    //NewStars = msg.Deserialize(temp);
                    //NewStars.Clear();
                    //foreach (var s in msg.Deserialize(temp))
                    //{
                    //    NewStars.Add(s.Clone());
                    //}
                    msg.Deserialize(temp);
                    NewStars = new Star[msg.max-msg.min+1];
                    Array.Copy(msg.Stars,msg.min,NewStars,0,msg.max-msg.min+1);

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

                    if (max - min != NewStars.Length - 1)
                        NewStars = null;
                    performance = (double) (max - min) + 1 < 1 ? 1 : ((max - min) + 1) / (double) watch.ElapsedMilliseconds;

                    performance = 1;
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
            if (oldStars.Count == 0)
                foreach (var S in DStars)
                {
                    oldStars.Add(S.Clone());
                }
            int size = OldStars.Count * 60 + 16;

            msg = new Message(step, min, max, OldStars.ToArray());
            SendMessage(this,new MessageEventArgs(msg));
        }

        public void OnSend(object o, SendEventArgs e)
        {
            this.step = e.Step;
            this.min = e.Orders.Find(x=>x[0]==this.id)[1];
            this.max = e.Orders.Find(x => x[0] == this.id)[2];
            oldStars = new List<Star>();
            if (oldStars.Count == 0)
                foreach (Star S in e.Stars)
                {
                    oldStars.Add(S.Clone());
                }
            
            send = true;
        }

        public void Subscribe(Server s)
        {
            s.SendData += OnSend;
        }
    }
    public class SendEventArgs : EventArgs
    {
        public SendEventArgs(int step, List<int[]> orders, Star[] DStars)
        {
            this.step = step;
            dStars = DStars.ToList();
            this.orders = orders;
        }

        
        private int step;
        List<Star> dStars;
        List<int[]> orders;
#region get set
        public int Step
        {
            get { return step; }
        }
        public List<int[]> Orders
        {
            get { return orders; }
        }
        public List<Star> Stars
        {
            get { return dStars; }
        }
#endregion
    }
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(Message msg)
        {
            this.msg = msg;
        }
        private Message msg;
        
        public Message Msg
        {
            get { return msg; }
        }
        
    }
}