namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using global::ClusterSim.ClusterLib.Utility;

    public class StarCluster : BoxCluster
    {

        // fields
        public StarCluster(string rtable, int start, double dt = 1, double coe = 0.4) : base(dt, coe)
        {
            this.Stars = SQL.readStars(rtable, start); // initialize
            if (this.Stars == null)
            {
                throw new Exception("Laden der Sterne Fehlgeschlagen!");
            }
        }
        
        public StarCluster(List<Star> stars, double dt = 1, double coe = 0.4) : base(dt, coe)
        {
            this.Stars = stars;

            if (this.Stars == null)
            {
                throw new NotImplementedException("Sternliste null");
            }
        }

        public StarCluster(double dt = 1, double coe = 0.4)
            : base(dt, coe)
        {
        }
    }
}