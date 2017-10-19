// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientHandler.cs" company="Test">
//   
// </copyright>
// <summary>
//   Defines the ClientHandler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterSim.Net.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Sockets;
    using System.Threading;

    using ClusterSim.ClusterLib;
    using ClusterSim.Net.Lib;

    /// <summary>
    ///     The client handler.
    /// </summary>
    [SuppressMessage("ReSharper", "StyleCop.SA1201", Justification = "Reviewed. Suppression is OK here.")]
    public class ClientHandler
    {
        // ReSharper disable once StyleCop.SA1201
        // ReSharper disable once StyleCop.SA1215
        public readonly int id = Id++;

        public TcpClient clientSocket;

        public Thread ctThread;

        private int min;

        private int max;

        private NetworkStream networkStream;

        public bool Abort { get; set; } = false;

        public int Mstep { get; set; }

        public Star[] NewStars { get; private set; }

        public double Performance { get; set; } = 1;

        public bool Ready { get; set; }

        public bool ReceiveFinished { get; set; }

        public bool Send { get; set; }

        public int Step { get; set; }

        private static int Id { get; set; }

        private List<Star> OldStars { get; set; } = new List<Star>();

        public void OnSend(object o, SendEventArgs e)
        {
            this.Step = e.Step;
            this.min = e.Orders.Find(x => x[0] == this.id)[1];
            this.max = e.Orders.Find(x => x[0] == this.id)[2];
            this.OldStars = new List<Star>();
            if (this.OldStars.Count == 0)
            {
                foreach (var s in e.Stars)
                {
                    this.OldStars.Add(s.Clone());
                }
            }

            this.Send = true;
        }

        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;

            this.ctThread = new Thread(this.ConnectionLoop) { Name = "Client:" + this.id };
            this.ctThread.Start();
        }

        public void Subscribe(Server s)
        {
            s.SendData += this.OnSend;
        }

        private void ConnectionLoop()
        {
            while (!this.Abort)
            {
                var watch = new Stopwatch();
                try
                {
                    this.networkStream = this.clientSocket.GetStream();

                    if (!this.Performance.Equals(0.0))
                    {
                        this.Ready = true;
                        while (!this.Send)
                        {
                            Thread.Sleep(20);
                            this.networkStream.Flush();
                        }

                        this.Ready = false;
                    }
                    else
                    {
                        this.min = 0;
                        this.max = this.OldStars.Count - 1;
                        this.Send = false;
                    }

                    watch.Reset();
                    watch.Start();

                    int size = this.OldStars.Count * 60 + 16;
                    var msg = new Message(this.Step, this.min, this.max, this.OldStars.ToArray());

                    this.networkStream.Write(msg.Serialize(this.OldStars.Count), 0, size);
                    this.networkStream.Flush();
                    this.Send = false;

                    this.Read();

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
                    watch.Stop();

                    if (this.max - this.min != this.NewStars.Length - 1)
                    {
                        this.NewStars = null;
                    }

                    this.Performance = (double)(this.max - this.min) + 1 < 1
                                           ? 1
                                           : (this.max - this.min + 1) / (double)watch.ElapsedMilliseconds;

                    this.Performance = 1;
                    this.ReceiveFinished = true;
                    this.networkStream.Flush();
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine(" >> Client {0} disconnected\n\n{1}", this.id, e);
                    return;
                }
            }

            Console.Clear();
        }

        private void Read()
        {
            int size = this.OldStars.Count * 60 + 16;
            this.ReceiveFinished = false;
            var buffer = new byte[size];
            this.networkStream.Read(buffer, 0, size);
            this.networkStream.Flush();
            var msg = new Message(this.OldStars.Count);
            msg.DeSerialize(buffer);
            this.NewStars = new Star[msg.max - msg.min + 1];
            Array.Copy(msg.Stars, msg.min, this.NewStars, 0, msg.max - msg.min + 1);

            for (var i = 0; i < this.NewStars.Length; i++)
            {
                this.NewStars[i] = this.NewStars[i].Clone();
            }

            this.Mstep = msg.step;
        }
    }
}