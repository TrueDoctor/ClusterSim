using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::ClusterSim.ClusterLib.Utility;

    public class ThreadCluster : Cluster
    {
        public ThreadCluster(double dt = 1)
            : base(dt)
        {
        }

        public ThreadCluster(List<Star> stars, double dt = 1)
            : base(stars, dt)
        {
        }

        public Star[] DoStep(Misc.Method m, int processors, int min = 0, int max = 0)
        {
            this.Instructions = new List<int>[this.Stars.Count];
            this.MassLayer.Clear();

            foreach (var s in this.Stars)
            {
                s.Computed = false; // reset computation status
                this.MassLayer.Add(s);
            }

            max = max == 0 ? this.Stars.Count - 1 : max;

            this.CalcBoxes();

            Parallel.For(min, max, i => Integrate(i, Misc.Method.Rk5));

            //this.Integrate(min, max, m);

            return this.Stars.Where(x => x.Computed && x.id >= min && x.id <= max)
                .OrderBy(x => x.id).ToArray();
        }

        protected virtual void Integrate(int id, Misc.Method method)
        {
            var s = this.Stars[id];

            try
            {
                this.GetInstruction(s);

                var oldAcc = s.Acc;

                switch (method)
                {
                    case Misc.Method.Rk4:
                        this.Rk4(ref s);
                        break;

                    case Misc.Method.Rk5:
                        this.Rk5(ref s);
                        break;
                    case Misc.Method.Euler:
                        this.Euler(ref s);
                        break;
                }

                if (!oldAcc.IsNull())
                {
                    var change = (oldAcc - s.Acc).distance();
                    var old = s.Acc.distance();
                    s.DAcc = change / old;
                }
            }
            catch (DivideByZeroException)
            {
                s.dead = true;
            }
        }
    }
}
