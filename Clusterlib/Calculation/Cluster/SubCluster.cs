
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

        public double oldDt;

        private List<int> remaining;

        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4, double minPrecision = 0.006)
            : base(stars, dt, coe)
        {
            MinPrecision = minPrecision;
        }

        public static double MinPrecision { get; set; }

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
                totalTime += 5.7142857142857142857142857142857e-4 * Math.Log(count) * toCalculate;
            }
            
            return totalTime;
        }

        public List<Cluster> DivideIntoSubclusters()
        {
            // return list of Subclusters with set dt
            
            var newClusters = new List<Cluster>();
            var subClusters = this.FormSubs(this.GetSubsetSeeds());
            
            var res = new List<Star>();
            foreach (Star s in this.Stars)
            {
                res.Add(new Star(s));
                res.Last().ToCompute = this.remaining.Contains(s.id);
            }

            var minMaxDAcc = res.Where(x => x.ToCompute).Min(c => this.calcRequiredDt(c));
            //var min = Stars.OrderBy(c => this.calcRequiredDt(c)).First();
            //var debug = this.calcRequiredDt(min);

            this.Dt = minMaxDAcc;
            if (this.Dt > this.ParentDt)
            {
                this.Dt = this.ParentDt;
            }

            if (res.Count < 2 )
            {
                throw new ArgumentException();
            }
            
            newClusters.Add(new BoxCluster(res, this.Dt) { CalculationComplexity = this.Dt / minMaxDAcc * EstimateStepTime(this.Stars.Count, this.remaining.Count, false), ParentDt = this.Dt });
            
            foreach (var cluster in subClusters)
            {

                var result = new List<Star>();
                foreach (Star s in this.Stars)
                {
                    result.Add(new Star(s));
                    result.Last().ToCompute = cluster.Contains(s.id);
                }


                double minDt = result.Where(x => x.ToCompute).Min(c => this.calcRequiredDt(c));
                double newDt = minDt > this.Dt ? this.Dt : minDt; // this.calcRequiredDt(minDt, this.Dt);

                if (cluster.Count < 2&&false)
                {
                    throw new ArgumentException();
                }

                newClusters.Add(
                    cluster.Count < BoxCluster.BoxPivot
                        ? new Cluster(result, newDt) { CalculationComplexity = this.Dt / newDt * EstimateStepTime(this.Stars.Count, cluster.Count, false), ParentDt = this.Dt }
                        : new BoxCluster(result, newDt) { CalculationComplexity = this.Dt / newDt * EstimateStepTime(this.Stars.Count, cluster.Count, false), ParentDt = this.Dt });
            }
            
            return newClusters;
        }

        private double calcRequiredDt(double maxDAcc)
        {
            return this.oldDt + (this.oldDt * SubCluster.MinPrecision / maxDAcc - this.oldDt) / 2;
            return this.Dt * SubCluster.MinPrecision / maxDAcc;
        }
        private double calcRequiredDt(Star s)
        {
            return s.Dt * SubCluster.MinPrecision / s.DAcc;
            return this.Dt + (s.Dt * SubCluster.MinPrecision / s.DAcc - s.Dt) / 2;
        }

        private double calcRequiredDt(double maxDAcc, double match)
        {
            var times = Math.Ceiling(match / (this.oldDt + (this.oldDt * SubCluster.MinPrecision / maxDAcc - this.oldDt) / 2));
            times = Math.Ceiling(match / (this.oldDt * SubCluster.MinPrecision / maxDAcc));
            return match / times;
        }

        public List<double> CalcDt()
        {
            var stepSize = new List<double>();

            foreach (Star star in this.Stars)
            {
                 stepSize.Add(calcRequiredDt(star));
                //stepSize.Add(star.DAcc);
            }

            stepSize.Sort();

            GnuPlot.Set("logscale y 10", "xlabel 'Sterne'", "ylabel '\\Symbol d acc \\frac{1}/{2}'");

            GnuPlot.Plot(stepSize.ToArray());

            return stepSize;
        }

        public List<int> GetSubsetSeeds()
        {
            var heap = this.Stars.OrderBy(x => x.Dt / x.DAcc).Reverse().ToArray();
            var subset = new List<int>();

            this.avgDAcc = heap.Average(x => x.Dt / x.DAcc);

            for (int i = heap.Length - 1; i >= 1; i--)
            {
                double currentMax = heap[i].Dt / heap[i].DAcc;
                if (currentMax * 1.3 < (heap[i - 1].Dt / heap[i - 1].DAcc) || i > heap.Length * 0.97)
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

            this.Stars = this.Stars.OrderBy(s => s.id).ToList();

            var cluster = new ConcurrentBag<List<int>>();
            var finalClusters = new List<HashSet<int>>();

            Parallel.ForEach(
                seeds,
                seed =>
                    {
                        var ids = new HashSet<int>();
                        var temp = this.Boxes.First(x => x.ids[0] == seed && x.root);
                        ids.Add(seed);
                        this.GrowCluster(temp.id, ref ids);
                        var newIds = ids.ToList();
                        newIds.Sort();
                        if (ids.Count > 1 || true)
                        {
                            cluster.Add(newIds);
                        }
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
                    var workSet = new HashSet<int>(finalCluster);
                    //workSet = new HashSet<int>(finalCluster);
                    //finalCluster.ToList().ForEach(x => workSet.Add(x));
                    if (list.Any(x => !workSet.Add(x)))
                    {
                        added = true;
                        list.ForEach(x => finalCluster.Add(x));
                        break;
                    }
                }

                if (added)
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

            var all = new HashSet<int>();
            foreach (var finalCluster in finalClusters)
            {
                all.UnionWith(finalCluster);
            }

            remain = remain.Except(all).ToList();
            this.remaining = remain;

            return finalClusters;
        }

        public void GrowCluster(int id, ref HashSet<int> ids)
        {
            Box seed = this.Boxes[id - this.Stars.Count];
            var instructions = this.Instructions[seed.ids[0]];
            foreach (int i in instructions)
            {
                Box node = this.Boxes[i - this.Stars.Count];
                if ((ids.First() == seed.ids.First() ? !(seed.size * 4 >= node.size) : !(seed.size >= node.size))
                    || !node.root || this.Stars[node.ids.First()].Dt / this.Stars[node.ids.First()].DAcc * 5 > this.avgDAcc)
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