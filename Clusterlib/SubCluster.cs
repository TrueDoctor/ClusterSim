

//using Newtonsoft.Json.Converters; 
using System.Collections.Generic;

namespace ClusterSim.ClusterLib
{
    public class SubCluster : StarCluster
    {
        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4) : base(stars, dt, coe)
        {
        }
    }
}