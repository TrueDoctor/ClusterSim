
namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System.Collections.Generic;

    public class SubCluster : Cluster
    {
        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4) : base(stars, dt)
        {
        }

        
    }
}