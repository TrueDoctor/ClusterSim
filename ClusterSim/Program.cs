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
            StarCluster test = new StarCluster(3);     //instatiate Starcluster
            for (int i = 0; i < 42; i++)                //calculate 42 steps
                test.calcForce();
            
            Console.ReadLine();                         //wait for input
        }
    }
}
