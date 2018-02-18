namespace ClusterSim.ClusterLib.Calculation
{
    using System;

    using ClusterSim.ClusterLib.Calculation.Cluster;

    public static class InitialVelExtension
    {
        public static void InitialVel(this Cluster.Cluster cluster, int id, double mass)
        {
            var rand = new Random();
            var acc = new Vector();

            foreach (var s in cluster.Stars)
            {
                if (s.id != cluster.Stars[id].id)
                {
                    acc.add(cluster.CalcAcc(cluster.Stars[id].Pos, s, id)); // add all acceleration vectors
                }
            }

            double bAcc = acc.distance(); // magnitude|x| of the vector
            double v = Math.Sqrt(
                bAcc * cluster.Stars[id].Pos.distance()); /*Math.Sqrt(
                    Gravitation * (mass - this.Stars[id].Mass)
                    / bAcc)); // Velocity=Square root(|acc|*r) r=Square root((G*m)/|acc|)*/

            double x1 = 2 * rand.NextDouble() - 1; // x1 = random -1,1
            double x2 = 2 * rand.NextDouble() - 1; // x2 = random -1,1
            double
                x3 = (x1 * acc.vec[0] + x2 * acc.vec[1])
                     / -acc.vec[2]; // x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orthogonal vector by using the dot product

            var vel = new Vector(x1, x2, x3);
            cluster.Stars[id].Vel.add(vel.scale(v)); // scale vector to match the V magnitude and add to the random variance
        }
    }
}
