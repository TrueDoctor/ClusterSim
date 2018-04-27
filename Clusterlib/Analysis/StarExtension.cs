namespace ClusterSim.ClusterLib.Analysis
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Calculation.Cluster;

    public static class StarExtension
    {
        public static double GetMetric(this Star star, Parameters param)
        {
            switch (param)
            {
                case Parameters.Kinetic:
                    var vel = star.Vel.distance();
                    return star.Mass * vel * vel * 0.5;
                
                case Parameters.Mass:
                    return star.Mass;
                case Parameters.Pulse:
                    return star.Mass * star.Vel.distance();
                case Parameters.Vel:
                    return star.Vel.distance();
                default:
                    return double.NaN;
            }
        }

        public static double GetMetric(this Star star, IEnumerable<IMassive> cluster, Parameters param)
        {
            switch (param)
            {
                case Parameters.Kinetic:
                    var vel = star.Vel.distance();
                    return star.Mass * vel * vel * 0.5;
                case Parameters.Potential:

                    return cluster.Where(x => x.id != star.id).Sum(
                        massive => massive.mass * star.Mass * -Cluster.Gravitation / massive.pos.distance());

                /*var r = star.Pos.distance();
                return star.Mass * (cluster.Sum(x=>x.mass)- star.Mass) * 1 * -Cluster.Gravitation  / r;*/
                case Parameters.Mass:
                    return star.Mass;
                case Parameters.Pulse:
                    return star.Mass * star.Vel.distance();
                case Parameters.Vel:
                    return star.Vel.distance();
                default:
                    return double.NaN;
            }
        }

        public static bool Escaped(this Star star, double acc)
        {
            var vel = 2 * acc * star.Pos.distance();
            return vel * vel < star.Vel.distance2();
        }
    }
}