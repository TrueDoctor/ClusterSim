using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterLib;
using XDMessaging;

namespace ClusterSim
{
    class Program
    {
        private static bool abort = false;
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


            Thread Key = new Thread(listen);
            Key.Start();
            StarCluster cluster = new StarCluster(rtable,wtable,last,dt);     //instatiate Starcluster
            for (int i = 1; i <= n; Console.WriteLine(i++))//for steps
            {
                cluster.doStep(i,Misc.Method.RK5);
                broadcaster.SendToChannel("steps", "i"+i);//send "i"+step in channel steps
                Console.WriteLine("\n" + i + "\n \n");
                
                if (abort == true)
                    break;
            }
            
            while (cluster.savethreads.FindAll(x => x.IsAlive).Count>1)
                Console.WriteLine("Warte auf die Beendigung von {0} Speicher Threads", cluster.savethreads.FindAll(x => x.IsAlive).Count);
            Thread t = cluster.savethreads.Find(x => x.IsAlive);
            try {
                Console.WriteLine(t.ThreadState.ToString());
                Console.WriteLine(t.Name);
                while (t.IsAlive);
                Thread.Sleep(10);
            } catch { }
            SQL.order(wtable);

            
            Console.WriteLine("Direkt in Dataview öffnen? (y/n)");
            string view = Console.ReadLine();                         //wait for input
            if (view=="y"||view=="Y")
                System.Diagnostics.Process.Start(@"..\..\..\Dataview\bin\Debug\DataView.exe", wtable);
        }

        private static void listen()
        {
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();
                
            }
            while (keyinfo.Key != ConsoleKey.X);
            Console.WriteLine(" Beenden Eingeleitet");
            abort = true;
        }
    }
}
