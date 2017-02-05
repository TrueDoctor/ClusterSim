using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterLib;

namespace ClusterSim
{
    class Program
    {
        static void Main(string[] args)
        {
            StarCluster test = new StarCluster(200);     //instatiate Starcluster
            for (int i = 0; i < 1000; i++)                //calculate 42 steps
                test.doStep(i);
            
            Console.ReadLine();                         //wait for input
        }
    }
}
