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
            var stars = enumerable.OrderBy(s => s.Pos.distance2());
            double totalMass = 0, currentMass = 0;
            enumerable.ForEach(s => totalMass += s.Mass);

            foreach (var star in stars)
            {
                currentMass += star.GetMass();
                if (currentMass > totalMass / 2)
                {
                    return star.Pos.distance();
                }
            }
            throw new Exception("bestimmen des Radius fehlgeschlagen");
        }

        public static double GetRelax(this IEnumerable<Star> cluster)
        {
            var stars = cluster.ToList();
            var radius = stars.GetRadius();
            stars = stars.Where(x => x.Pos.distance2() < Math.Pow(radius * 4, 2)).ToList();
            var mass = stars.Sum(x => x.Mass) / stars.Count;
            var n = stars.Count;
            var relax = 890000 * Math.Sqrt(n) * Math.Sqrt(Math.Pow(radius * 4.8481e-6, 3)) / (Math.Sqrt(mass) * Math.Log(n));
            return relax;
        }

        public static double GetPercentEscaped(this IEnumerable<Star> cluster)
        {
            var stars = cluster.ToList();
            var radius2 = Math.Pow(stars.GetRadius() * 4,2);
            var starCluster = new StarCluster(stars.Count);
            var mass = stars.Where(y => y.Pos.distance2() < radius2).Sum(x => x.Mass);
            var zero = new Vector();
            zero.init();
            var center = new Star(zero, zero, mass, 0) as IMassive;
 /*           int escaped = 0;


            foreach (Star s in stars)
            {
                var acc = StarCluster.Calcacc(s.pos, center);
                var vel2 = Math.Sqrt(2 * acc.distance() * s.pos.distance());
                if (vel2 > s.vel.distance())
                {
                    escaped++;
                }
            }*/ // =>

            int count = stars.Count(x => 
                Math.Sqrt(1.5 * starCluster.Calcacc(x.Pos, center, mass: x.Mass).distance() * x.Pos.distance()) 
                < x.Vel.distance());

            return (double)count;
        }
    }
}
