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
        static double mean = 0;
        static Random rand = new Random();

        public static int CountFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles().Length;
        }

        public static Star randomize(double pos,double vel,double mass,int id)
        {
            return new Star(new Vector().random(pos), new Vector().random(vel), Math.Abs(Misc.random(mass)), id);

        }

        public static decimal random(double stdDev = 1)
        {
            //reuse this if you are generating many
            double u1 = rand.NextDouble(); //these are uniform(0,1) random decimals
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (decimal)randNormal;
        }
        public enum Method{
            RK4,
            RK5
        };
    }
}
