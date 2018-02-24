
namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using ClusterSim.ClusterLib.Analysis;

    public class SubCluster : BoxCluster
    {
        private double avgDAcc;

        private List<int> remaining;

        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4, double minPrecision = 0.0003)
            : base(stars, dt, coe)
        {
            this.MinPrecision = minPrecision;
        }

        public double MinPrecision { get; set; }

        public static double EstimateStepTime(int count, int toCalculate, bool network)
        {
            double totalTime = 2;

            if (network)
            {
                totalTime += 20;
            }

            if (count < BoxCluster.BoxPivot)
            {
                totalTime += 8.5714285714285714285714285714286e-4 * count * toCalculate;
            }
            else
            {
                totalTime += Math.Log(5.7142857142857142857142857142857e-4 * count) * toCalculate;
            }
            
            return totalTime;
        }

        public List<Cluster> DivideIntoSubclusters()
        {
            // return list of Subclusters with set dt

            //var minMaxDAcc = this.Stars.Where(x => this.remaining.Contains(x.id)).Max(c => c.DAcc);

            var newClusters = (from cluster in this.FormSubs(this.GetSubsetSeeds())
                               let maxDAcc = this.Stars.Where(x => cluster.Contains(x.id)).Max(c => c.DAcc)
                               let newDt = this.Dt / this.calcRequiredDt(maxDAcc, this.Dt)
                               select cluster.Count < BoxCluster.BoxPivot
                                          ? new Cluster(this.Stars, newDt) { CalculationComplexity = this.Dt / newDt * EstimateStepTime(this.Stars.Count, cluster.Count, false)}
                                          : new BoxCluster(this.Stars, newDt) { CalculationComplexity = this.Dt / newDt * EstimateStepTime(this.Stars.Count, cluster.Count, false) }).ToList();
            return newClusters;
        }

        private double calcRequiredDt(double maxDAcc)
        {
            return this.Dt * this.MinPrecision / maxDAcc;
        }

        private double calcRequiredDt(double maxDAcc, double match)
        {
            var times = Math.Ceiling(match / this.Dt * this.MinPrecision / maxDAcc);
            return match / times;
        }

        public List<double> CalcDt()
        {
            var stepSize = new List<double>();

            foreach (Star star in this.Stars)
            {
                // stepSize.Add(this.Dt * 0.0003 / star.DAcc);
                stepSize.Add(star.DAcc);
            }

            stepSize.Sort();

            GnuPlot.Set("logscale y 10", "xlabel 'Sterne'", "ylabel '\\Symbol d acc \\frac{1}/{2}'");

            GnuPlot.Plot(stepSize.ToArray());

            return stepSize;
        }

        public List<int> GetSubsetSeeds()
        {
            var heap = this.Stars.OrderBy(x => x.DAcc).ToArray();
            var subset = new List<int>();

            this.avgDAcc = heap.Average(x => x.DAcc);

            for (int i = heap.Length - 1; i >= 1; i--)
            {
                double currentMax = heap[i].DAcc;
                if (currentMax > 1.3 * heap[i - 1].DAcc || i > heap.Length * 0.95)
                {
                    subset.Add(heap[i].id);
                }
                else
                {
                    return subset;
                }
            }

            return null;
        }

        public List<HashSet<int>> FormSubs(List<int> seeds)
        {
            if (this.Boxes.Count == 0)
            {
                this.CalcBoxes();
                this.GenerateInstructions();
            }

            var cluster = new ConcurrentBag<List<int>>();
            var finalClusters = new List<HashSet<int>>();

            Parallel.ForEach(
                seeds,
                seed =>
                    {
                        var ids = new HashSet<int>();
                        var temp = this.Boxes.First(x => x.ids[0] == seed);
                        ids.Add(seed);
                        this.GrowCluster(temp.id, ref ids);
                        var newIds = ids.ToList();
                        newIds.Sort();
                        cluster.Add(newIds);
                    });

            if (cluster.Count == 0)
            {
                return null;
            }

            finalClusters.Add(new HashSet<int>());
            cluster.First().ForEach(x => finalClusters[0].Add(x));

            foreach (var list in cluster)
            {
                bool added = false;
                foreach (var finalCluster in finalClusters)
                {
                    var workSet = new HashSet<int>();
                    finalCluster.ToList().ForEach(x => workSet.Add(x));
                    if (list.Any(x => !workSet.Add(x)))
                    {
                        added = true;
                        list.ForEach(x => finalCluster.Add(x));
                        break;
                    }
                }

                if (added || list.Count < 2)
                {
                    continue;
                }

                var temp = new HashSet<int>();
                list.ForEach(x => temp.Add(x));
                finalClusters.Add(temp);
            }

            var remain = new List<int>(); 
            for (int i = 0; i < this.Stars.Count; i++)
            {
                remain.Add(i);
            }
            foreach (var finalCluster in finalClusters)
            {
                remain = remain.Except(finalCluster).ToList();
            }
            this.remaining = remain;

            return finalClusters;
        }

        public void GrowCluster(int id, ref HashSet<int> ids)
        {
            Box seed = this.Boxes[id - this.Stars.Count];
            foreach (int i in this.Instructions[seed.ids[0]])
            {
                Box node = this.Boxes[i - this.Stars.Count];
                if (!(seed.size >= node.size) || !node.root || this.Stars[node.ids.First()].DAcc * 1.5 < this.avgDAcc)
                {
                    continue;
                }

                if (ids.Add(node.ids.First()))
                {
                    this.GrowCluster(node.id, ref ids);
                }
            }
        }

        public void GenerateInstructions()
        {
            this.Instructions = new List<int>[this.Stars.Count];
            this.MassLayer.Clear();

            foreach (var s in this.Stars)
            {
                s.Computed = false; // reset computation status
                this.MassLayer.Add(s);
            }

            this.CalcBoxes();
            Parallel.ForEach(this.Stars, this.GetInstruction);
        }
    }
}