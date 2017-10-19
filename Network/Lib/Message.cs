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

    using ClusterSim.ClusterLib;

    #endregion

    public class Message
    {
        public int step;
        
        public int count;
        
        public int min;
        
        public int max;
        
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
            for (var i = 0; i < this.count; i++) this.Stars[i] = new Star(i);
        }
        
        public Message(int step, int min, int max, Star[] Stars)
        {
            this.step = step;
            this.min = min;
            this.max = max;
            this.Stars = Stars.Select(x => x.Clone()).ToArray();
            this.count = Stars.Length;
        }
        
        public Star[] Stars { get; }
        
        public List<Star> DeSerialize(byte[] input)
        {
            var DStars = new List<Star>();
            this.step = BitConverter.ToInt32(input, 0);
            this.count = BitConverter.ToInt32(input, 4);
            this.min = BitConverter.ToInt32(input, 8);
            this.max = BitConverter.ToInt32(input, 12);
            var star = new byte[60];

            for (var i = 0; i < this.count; i++)
            {
                Array.Copy(input, i * 60 + 16, star, 0, 60);
                DStars.Add(this.Stars[this.min+i].Deserialize(star).Clone());
            }

            return DStars;
        }
        
        public byte[] Serialize(int count)
        {
            this.count = count;
            var output = new byte[count * 60 + 16];
            Array.Copy(BitConverter.GetBytes(this.step), 0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(count), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(this.min), 0, output, 8, 4);
            Array.Copy(BitConverter.GetBytes(this.max), 0, output, 12, 4);
            for (var i = 0; i < count; i++) Array.Copy(this.Stars[i].Serialize(), 0, output, i * 60 + 16, 60);
            return output;
        }
    }
}