using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System.Collections.Concurrent;

    using ClusterSim.ClusterLib.Utility;

    public interface ICluster
    {
        // fields

        List<Star> Stars { get; set; }

        double Dt { get; set; } // delta time

        Star[] DoStep(Misc.Method m, bool multiThreading, int min = 0, int max = 0, double distanceFromGalaxy = -1);

        Vector CalcAcc(Vector a, IMassive b, double mass);
    }
}
