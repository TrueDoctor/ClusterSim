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

        public static double GetRelax(this IEnumerable<Star> cluster)
        {
            var stars = cluster.ToList();
            var radius = stars.GetRadius();
            stars = stars.Where(x => x.pos.distance2() < Math.Pow(radius * 4, 2)).ToList();
            var mass = stars.Sum(x => x.mass) / stars.Count;
            var n = stars.Count;
            var relax = 890000 * Math.Sqrt(n) * Math.Sqrt(Math.Pow(radius * 4.8481e-6, 3)) / (Math.Sqrt(mass) * Math.Log(n));
            return relax;
        }
    }
}
