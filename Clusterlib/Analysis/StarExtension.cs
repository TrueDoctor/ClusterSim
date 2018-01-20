namespace ClusterSim.ClusterLib.Analysis
{
    public static class StarExtension
    {
        public static double GetMetric(this Star star, Parameters param)
        {
            switch (param)
            {
                case Parameters.Kinetic:
                    var vel = star.vel.distance();
                    return star.mass * vel * vel * 0.5;
                
                case Parameters.Mass:
                    return star.mass;
                case Parameters.Pulse:
                    return star.mass * star.vel.distance();
                case Parameters.Vel:
                    return star.vel.distance();
                default:
                    return double.NaN;
            }
        }

        public static double GetMetric(this Star star, double mass, Parameters param)
        {
            switch (param)
            {
                case Parameters.Kinetic:
                    var vel = star.vel.distance();
                    return star.mass * vel * vel * 0.5;
                case Parameters.Potential:
                    var r = star.pos.distance();
                    return star.mass * mass * 1 * -StarCluster.Gravitation  / r;
                case Parameters.Mass:
                    return star.mass;
                case Parameters.Pulse:
                    return star.mass * star.vel.distance();
                case Parameters.Vel:
                    return star.vel.distance();
                default:
                    return double.NaN;
            }
        }
        
    }
}