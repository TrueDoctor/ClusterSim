using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.OpenCl
{
    using System.Runtime.CompilerServices;

    using OpenCL.Net;

    static class double4StarExtesion
    {
        public static double4 GetDouble4(this IMassive s)
        {
            return new double4(s.pos.vec[0], s.pos.vec[1], s.pos.vec[2], s.mass);
        }

        public static double4 GetVel4(this Star s)
        {
            return new double4(s.Vel.vec[0], s.Vel.vec[1], s.Vel.vec[2], 0.0);
        }

        public static void Load(this Star s, double4 pos, double4 vel, double4 acc, double dt)
        {
            s.Pos.fromD4(pos);
            s.Vel.fromD4(vel);
            var newAcc = new Vector();
            newAcc.fromD4(acc);
            if (!s.Acc.IsNull())
            {
                var change = (s.Acc - newAcc).distance();
                var old = s.Acc.distance();
                s.DAcc = change / old;
            }

            s.Dt = dt;
            s.Acc.fromD4(acc);
        }

        public static void fromD4(this Vector v, double4 d4)
        {
            v.vec[0] = d4[0];
            v.vec[1] = d4[1];
            v.vec[2] = d4[2];
        }
    }
}
