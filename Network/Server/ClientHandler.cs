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

        public Func<Cluster, double, Task<List<Star>>> DoStep { get; set; }

        public bool Available { get; set; } = true;

        public double Performance { get; set; } = 1;

        public bool Ready { get; set; }
        
        public int Step { get; set; }

        private static int Id { get; set; }


        public void StarClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;

            this.networkStream = this.clientSocket.GetStream();

            this.DoStep = (cluster, coe) => this.Simulate(cluster, coe, true);

            this.Performance = 1;
        }
        
        public async Task<List<Star>> Simulate(Cluster cluster, double coe, bool sub)
        {
            this.Available = false;
            try
            {
                this.networkStream = this.clientSocket.GetStream();


                int size = cluster.Stars.Count * Star.size + Message.headerSize;

                var msg = new Message(this.Step, cluster, coe, sub);

                this.networkStream.Write(msg.Serialize(cluster.Stars.ToArray()), 0, size);
                this.networkStream.Flush();

                var watch = Stopwatch.StartNew();
                await this.Read(cluster.Stars.Count(x => x.ToCompute));
                watch.Stop();

                var newTime = cluster.CalculationComplexity * 1000 / watch.ElapsedMilliseconds;
                this.Performance += (newTime - this.Performance) / 2; 

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
        
        private async Task<bool> Read(int starCount)
        {
            int size = starCount * Star.size + Message.headerSize;
            var buffer = new byte[size];
            await this.networkStream.ReadAsync(buffer, 0, size);
            this.networkStream.Flush();
            var msg = new Message();
            this.NewStars = msg.DeSerialize(buffer);
            this.Mstep = msg.step;
            return true;
        }
    }
}