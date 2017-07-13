using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib
{
    interface IMassive
    {
        Vector pos { get; set; }
        double mass { get; set; }
        int id { get; set; }
        bool dead { get; set; }
    }
}
