using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Analysis
{
    public static class EnumerableExtension
    {
        public static double GetRadius(this IEnumerable<Star> cluster)
        {
            List<Star> enumerable = cluster as List<Star> ?? cluster.ToList();
            var stars = enumerable.OrderBy(s => s.pos.distance2());
            double totalMass = 0, currentMass = 0;
            enumerable.ForEach(s => totalMass += s.mass);

            foreach (var star in stars)
            {
                currentMass += star.getMass();
                if (currentMass > totalMass / 2)
                {
                    return star.pos.distance();
                }
            }
            throw new Exception("bestimmen des Radius fehlgeschlagen");
        }
    }
}
