using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation
{
    public interface IComputationNode
    {
        Func<Cluster.Cluster, Task<List<Star>>> DoStep { get; set; }

        bool Available { get; set; }

        double Performance { get; set; }
    }
}
