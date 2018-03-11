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
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.Net.Lib;

    /// <summary>
    ///     The client handler.
    /// </summary>
    [SuppressMessage("ReSharper", "StyleCop.SA1201", Justification = "Reviewed. Suppression is OK here.")]
    public class ClientHandler : IComputationNode
    {
        // ReSharper disable once StyleCop.SA1201
        // ReSharper disable once StyleCop.SA1215
        public readonly int id = Id++;

        public TcpClient clientSocket;

        public Thread ctThread;

        private NetworkStream networkStream;

        public bool Abort { get; set; } = false;

        public int Mstep { get; set; }

        public List<Star> NewStars { get; private set; }

        public Func<Cluster, Task<List<Star>>> DoStep { get; set; }

        public bool Available { get; set; } = true;

        public double Performance { get; set; } = 1;

        public bool Ready { get; set; }

        public bool ReceiveFinished { get; set; }

        public bool Send { get; set; }

        public int Step { get; set; }

        private static int Id { get; set; }

        private List<Star> OldStars { get; set; } = new List<Star>();


        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;

            this.networkStream = this.clientSocket.GetStream();

            this.DoStep = cluster => this.Simulate(cluster, true);

            this.Performance = 1;

            //this.ctThread = new Thread(this.ConnectionLoop) { Name = "Client:" + this.id };
            //this.ctThread.Start();
        }
        
        public async Task<List<Star>> Simulate(Cluster cluster, bool sub)
        {
            this.Available = false;
            try
            {
                this.networkStream = this.clientSocket.GetStream();


                int size = cluster.Stars.Count * Star.size + Message.headerSize;

                var msg = new Message(this.Step, cluster, sub);

                this.networkStream.Write(msg.Serialize(cluster.Stars.ToArray()), 0, size);
                this.networkStream.Flush();
                this.Send = false;

                var watch = Stopwatch.StartNew();
                await this.Read(cluster.Stars.Count(x => x.ToCompute));
                watch.Stop();

                this.Performance = cluster.CalculationComplexity * 1000 / watch.ElapsedMilliseconds; 

                this.networkStream.Flush();
                this.Available = true;

                return this.NewStars;
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(" >> Client {0} disconnected\n\n{1}", this.id, e);
                return null;
            }
        }
        
        private async Task<bool> Read(int StarCount)
        {
            int size = StarCount * Star.size + Message.headerSize;
            this.ReceiveFinished = false;
            var buffer = new byte[size];
            //this.networkStream.Read(buffer, 0, size);
            await this.networkStream.ReadAsync(buffer, 0, size);
            this.networkStream.Flush();
            var msg = new Message();
            this.NewStars = msg.DeSerialize(buffer);

            // this.NewStars = new Star[msg.max - msg.min + 1];
            // Array.Copy(msg.Stars, msg.min, this.NewStars, 0, msg.max - msg.min + 1);
            this.Mstep = msg.step;
            return true;
        }
    }
}