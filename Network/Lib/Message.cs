using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using ClusterSim.ClusterLib;
using ClusterSim.Net;

namespace ClusterSim.Net.Lib
{
    [Serializable]
    public class Message
    {
        public int step, count, min,max;
        public Star[] Stars;

        public Message(int count)
        {
            Stars = new Star[count];
            for (int i = 0; i < count; i++)
                Stars[i] = new Star(i);
        }
        public Message()
        {
            Stars = new Star[count];
            for (int i = 0; i < count; i++)
                Stars[i] = new Star(i);
        }

        public Message(int step, int min, int max, Star[] Stars)
        {
            this.step = step;
            this.min = min;
            this.max = max;
            this.Stars = Stars;
            count = Stars.Length; 
        }
        public byte[] Serialize(int count)
        {
            this.count = count;
            var output = new byte[count*60+16];
            Array.Copy(BitConverter.GetBytes(step),0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(count), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(min), 0, output, 8, 4);
            Array.Copy(BitConverter.GetBytes(max), 0, output, 12, 4);
            for(int i = 0; i < count; i++)
                Array.Copy(Stars[i].Serialize(), 0, output, i*60+16, 60);
            return output;   
        }

        public List<Star> Deserialize(byte[] input)
        {
            var DStars = new List<Star>();
            step = BitConverter.ToInt32(input, 0);
            count = BitConverter.ToInt32(input, 4);
            min = BitConverter.ToInt32(input, 8);
            max = BitConverter.ToInt32(input, 12);
            var star = new byte[60];
            
            for (int i = 0; i < count; i++)
            {
                Array.Copy(input, i * 60 + 16, star, 0, 60);
                DStars.Add(Stars[i].Deserialize(star).Clone());
            }
            return DStars;
        }
    }
}
