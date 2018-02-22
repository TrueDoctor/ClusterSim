
namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms.VisualStyles;

    using ClusterSim.ClusterLib.Analysis;

    public class SubCluster : BoxCluster
    {
        public SubCluster(List<Star> stars, double dt = 1, double coe = 0.4, double minPrecision = 0.0003) : base(stars, dt, coe)
        {
            this.MinPrecision = minPrecision;
        }

        public double MinPrecision { get; set; }

        public List<double> CalcDt()
        {
            var stepSize = new List<double>();

            foreach (Star star in this.Stars)
            {
                //stepSize.Add(this.Dt * 0.0003 / star.DAcc);
                stepSize.Add(star.DAcc);
            }

            stepSize.Sort();
            GnuPlot.Set("logscale y 10");

            GnuPlot.Plot(stepSize.ToArray());

            return stepSize;
        }

        public List<int> GetSubsetSeeds()
        {
            var heap = this.Stars.OrderBy(x => x.DAcc).ToArray();
            var subset = new List<int>();

            for (int i = heap.Length - 1; i >= 1; i--)
            {
                double currentMax = heap[i].DAcc;
                if (currentMax > 1.3 * heap[i - 1].DAcc || i > heap.Length * 0.98)
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

        public void FormSubs(List<int> seeds)
        {
            var cluster = new ConcurrentBag<List<int>>();
            var finalClusters = new List<HashSet<int>>();
            //this.GenerateInstructions();

            Parallel.ForEach(
                seeds,
                seed =>
                    {
                        var ids = new HashSet<int>();
                        var temp = this.Boxes.First(x => x.ids[0] == seed);
                        this.GrowCluster(temp.id, ref ids);
                        var newIds = ids.ToList();
                        newIds.Sort();
                        cluster.Add(newIds);
                    });

            if (cluster.Count == 0)
            {
                return;
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

                if (added || list.Count == 0)
                {
                    continue;
                }

                var temp = new HashSet<int>();
                list.ForEach(x => temp.Add(x));
                finalClusters.Add(temp);
            }
        }

        public void GrowCluster(int id, ref HashSet<int> ids)
        {
            Box seed = this.Boxes[id - this.Stars.Count];
            foreach (int i in this.Instructions[seed.ids[0]])
            {
                Box node = this.Boxes[i - this.Stars.Count];
                if (!(seed.size >= node.size) || !node.root)
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