using System;

namespace ClusterSim.ClusterLib.Calculation
{
    public class WaveGenerator
    {
        public WaveGenerator(double mass, double orbit = -1, double time = -1)
        {
            var mu = Cluster.Cluster.Gravitation * mass;
            if (orbit.Equals(-1)  && !time.Equals(-1))
            {
                this.Orbit = Math.Pow(
                    (mu * time * time) / (4 * Math.Pow(Math.PI, 2)),
                    1.0 / 3);
                this.PeriodTime = time;
            }
            else if (!orbit.Equals(-1) && time.Equals(-1))
            {
                this.PeriodTime = 2 * Math.PI * Math.Sqrt(Math.Pow(orbit, 3) / mu);
                this.Orbit = orbit;
            }
            else
            {
                throw new ArgumentException("mindestens zwei parameter müssen übergeben werden");
            }
        }
        
        public double Orbit { get; set; }

        public double PeriodTime { get; set; }

        public double GetVirtualDistance(double time)
        {
            var angle = ((double)time / this.PeriodTime - ((int)(time / this.PeriodTime))) * 2 * Math.PI;
            var amp = (Math.Pow(Math.Pow(10, Math.Sin(angle)), 2) / 100 + 1) * this.Orbit;
            return amp;
        }
    }
}
