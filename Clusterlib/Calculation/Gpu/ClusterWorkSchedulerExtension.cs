using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.Gpu
{
    using ClusterSim.ClusterLib.Calculation.Cluster;

    public static class ClusterWorkSchedulerExtension
    {
        public static List<int> GetStarIds(this BoxCluster cluster, int boxid)
        {
            var list = new List<int>();
            if (boxid > cluster.Stars.Count)
            {
                foreach (var i in cluster.Boxes[boxid - cluster.Stars.Count].ids)
                {
                    list.AddRange(cluster.GetStarIds(i));
                }
            }
            else
            {
                list.Add(boxid);
            }

            return list;
        }

        public static Dictionary<List<int>, int[]> GetWorkGroups(this BoxCluster cluster, int groupSize)
        {
            var groups = ClusterInstructions(cluster.Instructions, groupSize);
            var output = new Dictionary<List<int>, int[]>();
            var starIds = Enumerable.Range(0, cluster.Stars.Count).ToArray();

            foreach (var c in groups)
            {
                var f = cluster.Instructions[c.Value.First()].Where(x => c.Value.All(v => cluster.Instructions[v].Contains(x))).ToList();
                var stars = f.SelectMany(x => cluster.Boxes[x - cluster.Stars.Count].ids.SelectMany(cluster.GetStarIds)).ToList();
                f.AddRange(starIds.Except(stars));
                output.Add(c.Value, f.ToArray());
            }

            return output;
        }

        private static Dictionary<int, List<int>> ClusterInstructions(List<int>[] instructions, int unitSize)
        {
            var cluster = new Dictionary<int, List<int>>();
            for (int index = 0; index < instructions.Length; index++)
            {
                var key = instructions[index][0];
                if (cluster.ContainsKey(key))
                {
                    cluster[key].Add(index);
                }
                else
                {
                    cluster.Add(key, new List<int>());
                    cluster[key].Add(index);
                }
            }

            cluster = splitNodes(ref instructions, ref cluster, unitSize);
            
            return cluster;
        }

        private static Dictionary<int, List<int>> splitNodes(ref List<int>[] instructions, ref Dictionary<int, List<int>> cluster, int clusterSize)
        {
            bool modified = false;
            int[] current = new int[0];
            int ckey = 0;
            foreach (KeyValuePair<int, List<int>> pair in cluster)
            {
                if (pair.Value.Count <= clusterSize /** 1.1 && pair.Value.Count <= 256*/)
                {
                    continue;
                }

                modified = true;
                current = pair.Value.ToArray();
                ckey = pair.Key;
                break;
            }

            if (modified)
            {
                int runs = 0;
                foreach (int i in current)
                {
                    int element = instructions[i].First(cluster.ContainsKey);
                    int index = instructions[i].IndexOf(ckey);
                    var key = instructions[i][index + 1];
                    if (cluster.ContainsKey(key))
                    {
                        cluster[key].Add(i);
                        runs++;
                    }
                    else
                    {
                        cluster.Add(key, new List<int>());
                        cluster[key].Add(i);
                        runs++;
                    }
                }

                if (cluster.Select(x => x.Value.Count).Sum() == instructions.Length + cluster[ckey].Count)
                {
                    cluster.Remove(ckey);
                }
                else
                {

                }

                return splitNodes(ref instructions, ref cluster, clusterSize);
            }

            return cluster;
        }
    }
}

