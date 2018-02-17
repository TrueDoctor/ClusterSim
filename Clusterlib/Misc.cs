namespace ClusterSim.ClusterLib
{
    using System;
    using System.IO;

    public static class Misc
    {
        public const double c = 178;

        private static readonly Random rand = new Random();

        public enum Method
        {
            // enumerator of different Methods
            Rk4,

            Rk5,

            Euler
        }

        public static int CountFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles().Length;
        }

        public static double random(double stdDev = 1, double mean = 0)
        {
            double u1 = rand.NextDouble(); // uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
            double randNormal =
                mean + stdDev * randStdNormal; // random normal(mean,stdDev^2)
            return randNormal;
        }

        public static Star randomize(double pos, double vel, double massV, double massM, int id)
        {
            return new Star(new Vector().random(pos), new Vector().random(vel), Math.Abs(random(massV, massM)), id);
        }
    }
}