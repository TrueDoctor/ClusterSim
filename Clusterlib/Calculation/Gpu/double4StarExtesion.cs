using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.OpenCl
{
    using OpenCL.Net;

    static class double4StarExtesion
    {
        public static double4 GetDouble4(this IMassive s)
        {
            return new double4(s.pos.vec[0], s.pos.vec[1], s.pos.vec[2], s.mass);
        }
    }
}
