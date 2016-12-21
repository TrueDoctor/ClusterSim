using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim
{
    class Program
    {
        static void Main(string[] args)
        {
            StarCluster test = new StarCluster(10);
            test.calcForce();
        }
    }
}
