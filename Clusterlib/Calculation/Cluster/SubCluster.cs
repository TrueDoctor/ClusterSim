namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using ClusterSim.ClusterLib.Analysis;
    using ClusterSim.ClusterLib.Utility;

    public class SubCluster : BoxCluster
    {
        private const double MaxRecursion = -2;

        private double avgDAcc;

        private List<int> remaining;

        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4, double minPrecision = 0.0003)
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

        public double EstimateStepTime(bool network)
        {
            return EstimateStepTime(this.Stars.Count, this.ComputeCount, network);
        }

        public override Star[] DoStep(Misc.Method m, bool multiThreading, bool forceCluster = false, double distanceFromGalaxy = -1)
        {
            this.DistanceFormGalaxy = distanceFromGalaxy;

            if (forceCluster)
            {
                return base.DoStep(m, multiThreading, distanceFromGalaxy: this.DistanceFormGalaxy);
            }

            int count = this.GetComputeCount();
            

            var minDt = Stars.Where(x => x.ToCompute).Min(c => CalcRequiredDt(c));
            var stepTime = EstimateStepTime(this.Stars.Count, count, false);
            var totalTime = Math.Ceiling(this.ParentDt / minDt) * stepTime;

            var subClusters = this.DivideIntoSubClusters();

            if (subClusters == null || subClusters.Sum(x => x.CalculationComplexity) > totalTime)
            {
                return base.DoStep(m, multiThreading, distanceFromGalaxy: this.DistanceFormGalaxy);
            }

            this.CalculationComplexity = subClusters.Sum(x => x.CalculationComplexity);
            List<Star> newStars;

            for (double time = 0; time < this.ParentDt;)
             {
                if (!time.Equals(0))
                {
                    subClusters = this.DivideIntoSubClusters();
                }

                var temp = new ConcurrentBag<Star>();
                totalTime = stepTime * Math.Ceiling(this.Dt / subClusters.Min(x => x.Dt));

                if (subClusters.Sum(x => x.CalculationComplexity) > totalTime * 1.5
                    || subClusters.Count < 2
                    || subClusters.All(x => x.Dt * 2 > subClusters.First().Dt))
                {
                    var tempDt = this.ParentDt;
                    this.ParentDt = this.Dt;
                    this.Dt = subClusters.Min(x => x.Dt);
                    var tStars = base.DoStep(m, multiThreading, distanceFromGalaxy: this.DistanceFormGalaxy);
                    foreach (var tStar in tStars)
                    {
                        temp.Add(tStar);
                    }

                    this.Dt = this.ParentDt;
                    this.ParentDt = tempDt;

                    time += this.Dt;
                }
                else
                {
                    time += subClusters.First().Dt;
                    if (multiThreading)
                    {
                        Parallel.ForEach(
                            subClusters,
                            c =>
                                {
                                    var stars = c.DoStep(Misc.Method.Rk5, multiThreading, distanceFromGalaxy: this.DistanceFormGalaxy);
                                    foreach (var star in stars)
                                    {
                                        temp.Add(star);
                                    }
                                });
                    }
                    else
                    {
                        foreach (var c in subClusters)
                        {
                            var stars = c.DoStep(Misc.Method.Rk5, multiThreading, distanceFromGalaxy: this.DistanceFormGalaxy);
                            foreach (var star in stars)
                            {
                                temp.Add(star);
                            }
                        }
                    }

                    this.Dt = subClusters.First().Dt * 10;
                }

                newStars = temp.OrderBy(s => s.id).ToList();

                newStars = this.RemoveDuplicates(newStars);

                this.Stars.RemoveAll(x => newStars.Select(c => c.id).Contains(x.id));
                this.Stars.AddRange(newStars);
                this.Stars = this.Stars.OrderBy(star => star.id).ToList();

                // this.Stars.ForEach(x => x.ToCompute = false);
            }

            this.Stars = this.Stars.OrderBy(star => star.id).ToList();
            return this.Stars.Where(x => x.Computed && x.ToCompute).ToArray();
        }

        public async Task<List<Star>> DoStep(Misc.Method m, List<IComputationNode> clients, double distanceFromGalaxy)
        {
            int count = this.GetComputeCount();
            this.DistanceFormGalaxy = distanceFromGalaxy;

            var subClusters = this.DivideIntoSubClusters();

            this.CalculationComplexity = subClusters.Sum(x => x.CalculationComplexity);
            List<Star> newStars = new List<Star>();

            var first = subClusters.First();

            var tasks = new List<Task<List<Star>>>();
            subClusters = subClusters.OrderByDescending(x => x.CalculationComplexity).ToList();
            /*var clientsPerformance = clients.Sum(x => x.Performance);*/

            foreach (var subCluster in subClusters)
            {
                if (subCluster is SubCluster)
                {
                    //this.SkipInstructionRefresh = true;
                    tasks.Add((subCluster as SubCluster).DoStep(m, clients, this.DistanceFormGalaxy));
                    //this.SkipInstructionRefresh = false;
                }
                else
                {
                    var ready = clients.Where(x => x.Available).OrderBy(y => y.Performance).ToList();
                    var remaining = tasks.Where(x => !x.IsCompleted);
                    if (ready.Count == 0)
                    {
                        await Task.WhenAny(remaining);
                        ready = clients.Where(x => x.Available).OrderBy(y => y.Performance).ToList();
                    }
                    
                    var client = ready.Last();
                    tasks.Add(client.DoStep(subCluster, this.DistanceFormGalaxy));
                }
            }


            await Task.WhenAll(tasks);

            tasks.ForEach(t => newStars.AddRange(t.Result));

            this.Dt = first.Dt;
            

            newStars = this.RemoveDuplicates(newStars);

            this.Stars.RemoveAll(x => newStars.Select(c => c.id).Contains(x.id));
            this.Stars.AddRange(newStars);
            this.Stars = this.Stars.OrderBy(star => star.id).ToList();

            // this.Stars.ForEach(x => x.ToCompute = false);
            this.SkipInstructionRefresh = false;

            this.Stars = this.Stars.OrderBy(star => star.id).ToList();
            return this.Stars.Where(x => x.ToCompute).ToList();
        }

        public List<Cluster> DivideIntoSubClusters(bool all)
        {
            if (!all)
            {
                return null;
            }

            foreach (var star in this.Stars)
            {
                star.ToCompute = true;
            }

            return this.DivideIntoSubClusters();
        }

        public List<Cluster> DivideIntoSubClusters()
        {
            // return list of SubClusters with set dt
            var newClusters = new List<Cluster>();
            var seeds = this.GetSubsetSeeds();
            if (seeds == null || seeds.Count == 0)
            {
                this.CalculationComplexity = EstimateStepTime(this.Stars.Count, this.ComputeCount, false);
                newClusters.Add(new BoxCluster(this.Stars, this.Dt)
                {
                    CalculationComplexity = this.CalculationComplexity,
                    ParentDt = this.Dt,
                    ComputeCount = this.Stars.Count,
                    RecursionLevel = this.RecursionLevel -1
                });
                return newClusters;
            }

            var subClusters = this.FormSubs(seeds);

            var res = new List<Star>();
            foreach (var s in this.Stars)
            {
                res.Add(new Star(s));
                res.Last().ToCompute = this.remaining.Contains(s.id);
            }

            var minMaxDAcc = res.Where(x => x.ToCompute).Min(c => CalcRequiredDt(c));

            this.Dt = minMaxDAcc;
            if (this.Dt > this.ParentDt)
            {
                this.Dt = this.ParentDt;
            }

            double complexity;
            if (subClusters == null)
            {
                return null;
            }

            foreach (var cluster in subClusters)
            {
                var result = new List<Star>();
                foreach (Star s in this.Stars)
                {
                    result.Add(new Star(s));
                    result.Last().ToCompute = cluster.Contains(s.id);
                }

                double minDt = result.Where(x => x.ToCompute).Min(c => CalcRequiredDt(c));
                double newDt = minDt > this.Dt ? this.Dt : minDt; // this.calcRequiredDt(minDt, this.Dt);
                complexity = Math.Ceiling(this.Dt / newDt) * EstimateStepTime(this.Stars.Count, cluster.Count, false);

                if (this.Dt / newDt < 2)
                {
                    this.Dt = Math.Min(this.Dt, newDt);
                    this.remaining.AddRange(cluster);
                    continue;
                }

                newClusters.Add(
                    cluster.Count < 20 || this.RecursionLevel < MaxRecursion
                    || result.Where(x => x.ToCompute).Average(c => CalcRequiredDt(c)) < newDt * 2
                        ? new BoxCluster(result, newDt)
                        {
                            CalculationComplexity = complexity,
                            ParentDt = this.Dt,
                            ComputeCount = cluster.Count,
                            RecursionLevel = this.RecursionLevel - 1
                        }
                        : new SubCluster(result, newDt)
                        {
                            CalculationComplexity = complexity,
                            ParentDt = this.Dt,
                            ComputeCount = cluster.Count,
                            RecursionLevel = this.RecursionLevel - 1
                        });
            }


            if (res.Count < 2)
            {
                throw new ArgumentException();
            }

            complexity = EstimateStepTime(this.Stars.Count, this.remaining.Count, false);
            res.ForEach(x => x.ToCompute = this.remaining.Contains(x.id));

            newClusters.Add(
                new BoxCluster(res, this.Dt)
                {
                    CalculationComplexity = complexity,
                    ParentDt = this.Dt,
                    ComputeCount = this.remaining.Count,
                    RecursionLevel = this.RecursionLevel - 1
                });
            this.CalculationComplexity = newClusters.Sum(c => c.CalculationComplexity);
            newClusters.Reverse();
            return newClusters;
        }

        public List<double> CalcDt()
        {
            var stepSize = this.Stars.Select(x => x.DAcc).ToList();

            stepSize.Sort();

            GnuPlot.Set("logscale y 10", "xlabel 'Sterne'", "ylabel '\\Symbol d acc \\frac{1}/{2}'");

            GnuPlot.Plot(stepSize.ToArray());

            return stepSize;
        }

        public static double CalcRequiredDt(Star s)
        {
            var temp = s.Dt;
            var res = s.Dt * SubCluster.MinPrecision / s.DAcc;

            if (res > temp)
            {
                return temp + (res - temp) / 10;
            }

            return res;

            // return this.Dt + (s.Dt * SubCluster.MinPrecision / s.DAcc - s.Dt) / 2;
        }

        private List<int> GetSubsetSeeds()
        {
            var heap = this.Stars.Where(c => c.ToCompute).OrderBy(x => x.DAcc).ToArray();
            var subset = new List<int>();

            if (heap == null)
            {
                throw new ArgumentException("Keine Sterne zu berechnen");
            }

            this.avgDAcc = heap.Average(x => x.DAcc);

            for (int i = heap.Length - 1; i >= 1; i--)
            {
                double currentMax = heap[i].DAcc;
                if (currentMax > heap[i - 1].DAcc * 1.3 || currentMax > avgDAcc * 2)
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

        private List<Star> RemoveDuplicates(List<Star> newStars)
        {
            if (newStars.Count == this.Stars.Count(x => x.ToCompute))
            {
                return newStars;
            }

            var duplicates = newStars.Where(x => newStars.Count(c => c.id == x.id) > 1).Select(d => d.id)
                .Distinct()
                .ToList();
            foreach (var duplicate in duplicates)
            {
                while (newStars.Remove(newStars.Where(x => x.id == duplicate).OrderBy(c => c.DAcc).First())
                       && newStars.Count(c => c.id == duplicate) > 1)
                {
                }
            }

            // throw new Exception("Duplicate!");

            return newStars;
        }

        private IEnumerable<HashSet<int>> FormSubs(IEnumerable<int> seeds)
        {
            this.CalcBoxes();
            this.GenerateInstructions();

            this.Stars = this.Stars.OrderBy(s => s.id).ToList();

            var cluster = new ConcurrentBag<List<int>>();
            var finalClusters = new List<HashSet<int>>();

            /*Parallel.ForEach(
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
                    });*/

            // Degug
            foreach (int seed in seeds)
            {
                var ids = new HashSet<int>();
                var temp = this.Boxes.First(x => x.ids[0] == seed && x.root);
                ids.Add(seed);
                this.GrowCluster(temp.id, ref ids);
                var newIds = ids.ToList();
                newIds.Sort();
                cluster.Add(newIds);
            }

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
                    if (list.Any(x => finalCluster.Contains(x)))
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
                if (this.Stars[i].ToCompute)
                {
                    remain.Add(i);
                }
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

        private void GrowCluster(int id, ref HashSet<int> ids)
        {
            Box seed = this.Boxes[id - this.Stars.Count];
            var instructions = this.Instructions[seed.ids[0]];
            foreach (int i in instructions)
            {
                Box node = this.Boxes[i - this.Stars.Count];
                if ((ids.First() == seed.ids.First() ? !(seed.size * 4 >= node.size) : !(seed.size >= node.size))
                    || !node.root)
                {
                    continue;
                }

                var iD = node.ids.First();
                var star = this.Stars[iD];
                if (star.DAcc < this.avgDAcc * 2 || !star.ToCompute)
                {
                    continue;
                }

                if (ids.Add(iD))
                {
                    this.GrowCluster(node.id, ref ids);
                }
            }
        }

        private void GenerateInstructions()
        {
            this.Instructions = new List<int>[this.Stars.Count];
            this.MassLayer.Clear();

            foreach (var s in this.Stars)
            {
                s.Computed = false; // reset computation status
                this.MassLayer.Add(s);
            }

            this.CalcBoxes(true);
            Parallel.ForEach(this.Stars.Where(x => x.ToCompute), s => this.GetInstruction(s, true));
        }
    }
}