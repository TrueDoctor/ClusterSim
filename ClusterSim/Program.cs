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
            
            if (args.Length > 0)//if the program gets called with arguments
            {
                List<String> list = SQL.readTables();
                if (list != null)
                {
                    List<string> res = new List<string>();
                    foreach (string s in args)
                    {
                        if (list.Contains(s))//check if argument is a valid table name
                            rtable = s;
                        

                        Console.WriteLine(rtable);
                    }

                }
                
            }

            if (rtable == "")//if argument handover failed or was not  given
            {
                Console.WriteLine("Auswahltabelle: ");//input name maualy 
                rtable = Console.ReadLine();
            }
            int last=0;

            Console.WriteLine("\nLeer lassen, für gleiche Liste, oder Speichern nach: ");
            string wtable = Console.ReadLine();

            if (wtable == "")
            {
                wtable = rtable;
                last = SQL.lastStep(rtable);//get last step of given table
            }

            Console.WriteLine("\nDelta t: ");

            decimal dt = Convert.ToDecimal(Console.ReadLine());

            Console.WriteLine("\nSchritte: ");

            int n = Convert.ToInt32(Console.ReadLine());

            XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            broadcaster.SendToChannel("steps", "s"+n);// send max step to steps channel
            

            StarCluster cluster = new StarCluster(rtable,wtable,last,dt);     //instatiate Starcluster
            for (int i = 1; i <= n; Console.WriteLine(i++))//for steps
            {
                cluster.doStep(i,Misc.Method.RK5);
                broadcaster.SendToChannel("steps", "i"+i);//send "i"+step in channel steps
                Console.WriteLine("\n" + i + "\n \n");
            }
            cluster.export(new List<Star>(), 0, wtable);   //save data
            Console.WriteLine("Direkt in Dataview öffnen? (y/n)");
            string view = Console.ReadLine();                         //wait for input
            if (view=="y"||view=="Y")
                System.Diagnostics.Process.Start(@"..\..\..\Dataview\bin\Debug\DataView.exe", wtable);
        }
    }
}
