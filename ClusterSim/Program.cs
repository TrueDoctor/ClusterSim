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

            if (args.Length > 1)
            {
                List<String> list = SQL.readTables();
                if (list != null)
                {
                    
                    String[] parms = Environment.GetCommandLineArgs();
                    List<string> res = new List<string>();
                    foreach (string s in parms)
                    {
                        if (list.Contains(s))
                            rtable = s;
                    }

                }
                
            }

            /*if (rtable == "")
            {
                Console.WriteLine("Auswahltabelle: ");
                rtable = Console.ReadLine();
            }*/
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
            //broadcaster.SendToChannel("steps", "s"+n);


            StarCluster test = new StarCluster(rtable,wtable,last,dt);     //instatiate Starcluster
            for (int i = 0; i < n; Console.WriteLine(i++))
            {
                test.RK5(i);
                //broadcaster.SendToChannel("steps", "i"+i);
            } 
            Console.ReadLine();                         //wait for input
        }
    }
}
