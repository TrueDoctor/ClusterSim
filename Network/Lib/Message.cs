// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="">
//   
// </copyright>
// <summary>
//   The message Object provides the possibility to serialize StarClusters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterSim.Net.Lib
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;

    #endregion

    public class Message
    {
        public const int headerSize = 25;

        public int step;
        
        public int count;
        
        public bool Subcluster;

        public double dt;

        public double ParentDt;
        
        public Message(int count)
        {
            this.Stars = new Star[count];
            for (var i = 0; i < count; i++) this.Stars[i] = new Star(i);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
        }
        
        public Message(int step, Cluster cluster, bool subCluster = false)
        {
            this.step = step;
            this.Subcluster = subCluster;
            this.Stars = cluster.Stars.Select(x => x.Clone()).ToArray();
            this.count = cluster.Stars.Count;
            this.dt = cluster.Dt;
            this.ParentDt = cluster.ParentDt;
        }

        public Star[] Stars { get; }
        
        public List<Star> DeSerialize(byte[] input)
        {
            var DStars = new List<Star>();
            this.step = BitConverter.ToInt32(input, 0);
            this.count = BitConverter.ToInt32(input, 4);
            this.dt = BitConverter.ToDouble(input, 8);
            this.ParentDt = BitConverter.ToDouble(input, 16);
            this.Subcluster = BitConverter.ToBoolean(input, 24);
            var star = new byte[Star.size];

            for (var i = 0; i < this.count; i++)
            {
                Array.Copy(input, i * Star.size + Message.headerSize, star, 0, Star.size);
                var s = new Star().Deserialize(star);
                DStars.Add(s.Clone());
            }

            return DStars;
        }
        
        public byte[] Serialize(Star[] stars)
        {
            this.count = stars.Length;
            var output = new byte[count * Star.size + Message.headerSize];
            Array.Copy(BitConverter.GetBytes(this.step), 0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(count), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(this.dt), 0, output, 8, 8);
            Array.Copy(BitConverter.GetBytes(this.ParentDt), 0, output, 16, 8);
            Array.Copy(BitConverter.GetBytes(this.Subcluster), 0, output, 24, 1);
            for (var i = 0; i < this.count; i++)
            {
                Array.Copy(stars[i].Serialize(), 0, output, i * Star.size + Message.headerSize, Star.size);
            }

            return output;
        }
    }
}