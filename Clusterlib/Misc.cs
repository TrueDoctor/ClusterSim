using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClusterLib
{
    public static class Misc
    {
        static Random rand = new Random();
        public const decimal c = 178m;

        public static int CountFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles().Length;
        }

        public static Star randomize(double pos,double vel,double massV,double massM,int id)
        {
            return new Star(new Vector().random(pos), new Vector().random(vel), Math.Abs(Misc.random(massV,massM)), id);

        }

        

        public static decimal random(double stdDev = 1,double mean = 0)
        {
            double u1 = rand.NextDouble(); //uniform(0,1) random decimals
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (decimal)randNormal;
        }


        public enum Method{     //enumerator of different Methods
            RK4,
            RK5
        };
    }
}
