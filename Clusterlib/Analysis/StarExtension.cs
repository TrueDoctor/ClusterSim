namespace ClusterSim.ClusterLib.Analysis
{
    using System;

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

        public static double GetMetric(this Star star, double mass, Parameters param)
        {
            switch (param)
            {
                case Parameters.Kinetic:
                    var vel = star.Vel.distance();
                    return star.Mass * vel * vel * 0.5;
                case Parameters.Potential:
                    var r = star.Pos.distance();
                    return star.Mass * mass * 70 * -StarCluster.Gravitation  / r;
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