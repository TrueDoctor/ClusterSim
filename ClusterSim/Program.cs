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
            StarCluster test = new StarCluster(200);     //instatiate Starcluster
            for (int i = 0; i < 200; i++)                //calculate 42 steps
                test.calcForce(i);
            
            Console.ReadLine();                         //wait for input
        }
    }
}
