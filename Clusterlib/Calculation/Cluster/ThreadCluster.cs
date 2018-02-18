using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System.Threading;

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

        public Star[] DoStep(int processors, int min, int max, Misc.Method m)
        {
            this.Instructions = new List<int>[this.Stars.Count];
            this.MassLayer.Clear();
            
            foreach (var s in this.Stars) 
            {
                s.Computed = false; // reset computation status
                this.MassLayer.Add(s);
            }

            this.CalcBoxes();

            if (processors > 1)
            {
                int perCore = (max - min + 1) / processors; // divide the cluster in equal parts
                int left = (max - min + 1) % processors; // calc remainder
                var threads = new List<Thread>();

                int start = min;
                for (var i = 0; i < (processors < max - min ? processors : max - min + 1); i++)
                {
                    int end = start + perCore;
                    if (left > 0)
                    {
                        end++;
                        left--;
                    }

                    threads.Add(
                        new Thread(
                            delegate()
                                {
                                    this.Integrate(min, max, m);
                                }));

                    threads.Last().Priority = ThreadPriority.Highest;
                    threads.Last().Name = $"Calc{start}-{end - 1}";
                    threads.Last().Start();

                    start = end;
                }

                while (threads.Exists(x => x.IsAlive) || this.Stars.Exists(x => !x.Computed && x.id >= min && x.id <= max))
                {
                    Thread.Sleep(10);
                }
            }
            else
            {
                this.Integrate(min, max, m);
            }
            return this.Stars.Where(x => x.Computed && x.id >= min && x.id <= max)
                .OrderBy(x => x.id).ToArray();
        }
    }
}
