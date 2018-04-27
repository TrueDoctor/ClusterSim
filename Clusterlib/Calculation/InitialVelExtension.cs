namespace ClusterSim.ClusterLib.Calculation
{
    using System;

    using ClusterSim.ClusterLib.Calculation.Cluster;

    using Conditions;

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
                    acc.add(cluster.CalcAcc(cluster.Stars[id].Pos, s, cluster.Stars[id].Mass)); // add all acceleration vectors
                }
            }

//            acc = cluster.Stars[id].Pos;
            double bAcc = acc.distance(); // magnitude|x| of the vector
            /*var r = cluster.Stars[id].Pos.distance();
            bAcc = cluster.Stars[id].Mass * (mass - cluster.Stars[id].Mass) * 1 * Cluster.Cluster.Gravitation / r;*/
            double v = Math.Sqrt(
                bAcc * cluster.Stars[id].Pos.distance()); /*Math.Sqrt(
                    Gravitation * (mass - this.Stars[id].Mass)
                    / bAcc)); // Velocity=Square root(|acc|*r) r=Square root((G*m)/|acc|)*/
            var sPos = cluster.Stars[id].Pos.vec;
            double x1 = (sPos[1] > 0)  ? rand.NextDouble() : -rand.NextDouble(); // x1 = random -1,1
            double x2 = (sPos[0] > 0) ? rand.NextDouble() : -rand.NextDouble(); // x2 = random -1,1
            double
                x3 = (x1 * acc.vec[0] + x2 * acc.vec[1])
                     / -acc.vec[2]; // x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orthogonal vector by using the dot product

            var vel = new Vector(new double[] { x1, x2, x3 });
            cluster.Stars[id].Vel.add(vel.scale(v)); // scale vector to match the V magnitude and add to the random variance
        }
    }
}
