// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="">
//   
// </copyright>
// <summary>
//   The message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterSim.Net.Lib
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Calculation;

    #endregion

    public class Message
    {
        public const int headerSize = 32;

        public int step;
        
        public int count;
        
        public int min;
        
        public int max; //testchange

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
            this.Stars = new Star[this.count];
            for (var i = 0; i < this.count; i++)
                this.Stars[i] = new Star(i);
        }
        
        public Message(int step,double dt, double ParentDt, int min, int max, Star[] Stars)
        {
            this.step = step;
            this.min = min;
            this.max = max;
            this.Stars = Stars.Select(x => x.Clone()).ToArray();
            this.count = Stars.Length;
            this.dt = dt;
            this.ParentDt = ParentDt;
        }
        
        public Star[] Stars { get; }
        
        public List<Star> DeSerialize(byte[] input)
        {
            var DStars = new List<Star>();
            this.step = BitConverter.ToInt32(input, 0);
            this.count = BitConverter.ToInt32(input, 4);
            this.min = BitConverter.ToInt32(input, 8);
            this.max = BitConverter.ToInt32(input, 12);
            this.dt = BitConverter.ToDouble(input, 16);
            this.ParentDt = BitConverter.ToDouble(input, 24);
            var star = new byte[Star.size];

            for (var i = 0; i < this.count; i++)
            {
                Array.Copy(input, i * Star.size + Message.headerSize, star, 0, Star.size);
                var s = new Star().Deserialize(star);
                this.Stars[s.id] = s.Clone();
                DStars.Add(this.Stars[s.id]);
            }

            return DStars;
        }
        
        public byte[] Serialize(int count)
        {
            this.count = count;
            var output = new byte[count * Star.size + Message.headerSize];
            Array.Copy(BitConverter.GetBytes(this.step), 0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(count), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(this.min), 0, output, 8, 4);
            Array.Copy(BitConverter.GetBytes(this.max), 0, output, 12, 4);
            Array.Copy(BitConverter.GetBytes(this.dt), 0, output, 16, 8);
            Array.Copy(BitConverter.GetBytes(this.dt), 0, output, 24, 8);
            for (var i = 0; i < count; i++) Array.Copy(this.Stars[i].Serialize(), 0, output, i * Star.size + Message.headerSize, Star.size);
            return output;
        }
    }
}