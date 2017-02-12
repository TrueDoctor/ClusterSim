using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterLib;
using XDMessaging;

namespace ClusterSim
{
    class Program
    {
        static void Main(string[] args)
        {
            string rtable="";
            

            if (args.Length > 0)
            {
                List<String> list = SQL.readTables();
                if (list != null)
                {
                    
                    List<string> res = new List<string>();
                    foreach (string s in args)
                    {
                        if (list.Contains(s))
                            rtable = s;
                        

                        Console.WriteLine(rtable);
                    }

                }
                
            }

            if (rtable == "")
            {
                Console.WriteLine("Auswahltabelle: ");
                rtable = Console.ReadLine();
            }
            int last=0;

            Console.WriteLine("\nLeer lassen, für gleicheListe, oder Speichern nach: ");
            string wtable = Console.ReadLine();
            if (wtable == "")
            {
                wtable = rtable;
                last = SQL.lastStep(rtable);
            }

            Console.WriteLine("\nDelta t: ");

            decimal dt = Convert.ToDecimal(Console.ReadLine());

            Console.WriteLine("\nSchritte: ");

            int n = Convert.ToInt32(Console.ReadLine());

            XDMessagingClient client = new XDMessagingClient();
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            broadcaster.SendToChannel("steps", "s"+n);


            StarCluster test = new StarCluster(rtable,wtable,last,dt);     //instatiate Starcluster
            for (int i = 1; i <= n; Console.WriteLine(i++))
            {
                test.doStep(i,Misc.Method.RK5);
                broadcaster.SendToChannel("steps", "i"+i);
            } 
            Console.ReadLine();                         //wait for input
        }
    }
}
