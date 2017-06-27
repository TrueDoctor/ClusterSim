using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ClusterSim.ClusterLib;
using XDMessaging;

//using Newtonsoft.Json.Converters; 


namespace ClusterSim.ClusterLib
{
    public class StarCluster
    {
        
        
        //fields
        private const double Gravitation = 0.0002959122083;  //0.0000000000667384;//gravitation constant in AE^3 /Sunmass*Day^2
        private string rtable;//table to read from
        private string wtable;//table to write at
        private int start;//starting step
        private int starCount;
        public List<Thread> savethreads = new List<Thread>();
        public List <Star> Stars;//Array of Stars
        private List<Star> OldStars;//While calculating a step the same values have to be used
        private List<List<Star>> Steps = new List<List<Star>>();//list of Clusters stored in ram
        
        
        double dt;//delta time

        public StarCluster(string rtable,string wtable,int start,double dt=1)   //constructor 
        {
            this.starCount = SQL.starsCount(rtable);
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.dt = dt;
            Stars = new List<Star>();

            Stars = SQL.readStars(rtable, start);                   //initialize
        }

        public StarCluster(int count, double dt = 1)   //constructor 
        {
            this.dt = dt;
            Stars = new List<Star>();

            Stars = null;                   //initialize
        }




        public Star[] doStep(int step,Misc.Method m)    
        {
            if (Stars == null)
                return null;
            else
                foreach (Star s in Stars)//reset computation status
                    s.computed = false;

            int processors = Environment.ProcessorCount;//get number of processors
            int perCore = (int)starCount/ processors;//divide the cluster in equal parts
            int left = starCount % processors;//calc remainder
            List<Thread> threads = new List<Thread>();

            OldStars = new List<Star>();//save the current values
            foreach (Star s in Stars)//clone each to prevent shallow copys
                OldStars.Add(s.Clone());

            for (int i = 0; i < processors; i++)
            {
                int start = i * perCore;

                if (left > 0)
                {
                    start++;
                    left--;
                }


                switch (m)//case(Method) of...
                    {

                        case Misc.Method.RK4:
                            threads.Add(new Thread(delegate () { RK4(step, start, starCount-1); }));//new Thread(start step,steps to process)
                            break;
                        case Misc.Method.RK5:
                            threads.Add(new Thread(delegate () { RK5(step, start, starCount-1); }));//new Thread(start step,steps to process)
                            break;
                    }
                    threads.Last().Priority = ThreadPriority.AboveNormal;
                    threads.Last().Start();
                

            }

            
            while (threads.Exists(x => x.IsAlive))
                Thread.Sleep(10);

            List<Star> temp = new List<Star>();
            foreach (Star s in Stars)//prevent shallow copys
                temp.Add(s.Clone());

            if (rtable != "" && wtable != "")
            {
                Steps.Add(new List<Star>(temp.OrderBy(x => x.id)));//add step to steps array



                Thread save = new Thread(delegate () { export(new List<Star>(Steps.Last()), step + start, wtable); });
                save.Priority = ThreadPriority.Highest;
                save.Start();
                save.Name = "save" + step.ToString();
                savethreads.Add(save);
                //export(new List<Star>(Steps.Last()), step + start, wtable);//currently disabled create new save Thread;
                return null;
            }
            else
                return new List<Star>(temp.OrderBy(x => x.id)).ToArray();
        }

        public Star[]  doStep(int step,int min,int max, Misc.Method m)
        {
            starCount = Stars.Count;
            if (Stars == null)
                return null;
            else
                foreach (Star s in Stars)//reset computation status
                    s.computed = false;

            int processors = Environment.ProcessorCount;//get number of processors
            int perCore = (int) ((max-min)+1) / processors;//divide the cluster in equal parts
            int left = ((max-min)+1) % processors;//calc remainder
            List<Thread> threads = new List<Thread>();

            OldStars = new List<Star>();//save the current values
            foreach (Star s in Stars)//clone each to prevent shallow copys
                OldStars.Add(s.Clone());

            int end;
            start = min;
            for (int i = 0; i < processors  ; i++)
            {
                end = start + perCore;
                if (left > 0)
                {
                    end++;
                    left--;
                }

                int test = start;

                switch (m)//case(Method) of...
                {
                    
                    case Misc.Method.RK4:
                        threads.Add(new Thread(delegate () { RK4(step, test, end-1); }));//new Thread(start step,steps to process)
                        break;
                    case Misc.Method.RK5:
                        threads.Add(new Thread(delegate () { RK5(step, test, end- 1); }));//new Thread(start step,steps to process)
                        break;
                }
                threads.Last().Priority = ThreadPriority.AboveNormal;
                threads.Last().Name = String.Format("Calc{0}-{1}",start,end-1);
                threads.Last().Start();

                //while (true) { }
                start = end;
                

            }


            while (threads.Exists(x => x.IsAlive))
                Thread.Sleep(10);
            
            
            return Stars.Where(x => x.computed).OrderBy(x => x.id).ToArray();
        }

        private void RK4(int step, int cstart,int end)
        {
            int i=0;
            //try
            //{
                //bool ready;
                //do
                //{
                    while (!(cstart < Stars.Count && cstart < OldStars.Count))
                        cstart = 0;
                    for (i = cstart; i <= end; i++)//for seqence of Stars[]
                    {
                        Star s = Stars[i];
                        if (s.computed == false)
                        {
                            s.computed = true;
                            Vec6 Star = new Vec6(OldStars[i].pos, OldStars[i].vel);//convert star to vec6
                            Vec6 KA = f(Star, s.id);//calculate help values
                            Vec6 KB = f(Star + ((dt / 2) * KA), s.id);
                            Vec6 KC = f(Star + ((dt / 2) * KB), s.id);
                            Vec6 KD = f(Star + KC, s.id);
                            Vec6 F = ((dt / 6) * (KA + (2 * KB) + (2 * KC) + KD));//calculate resulting Vector
                            s.pos = s.pos + F.ToVector(0);
                            s.vel = s.vel + F.ToVector(1);
                            s.print();
                        }
                        else
                            Console.WriteLine("\n!!!Konflikt!!!\n");

                    }
                    /*ready = false;//initialize bool

                    foreach (Star s in Stars)//check for not yet computed stars
                        if (s.computed == false)
                            break;//abort if not computed
                        else if (s == Stars.Last())
                            ready = true;//sucessfully finished
                    if (ready == false)
                        cstart = 0;//start form 0/*
                //} while (ready == false);//repete until everythig is computed
            }
            catch
            {
                Console.WriteLine("Thread Fehler bei: "+i);
            }*/
        }
        
        public void RK5(int step, int cstart,int end)//as RK4
        {
            //Thread.CurrentThread.Name = "working";
            /*int i = 0;
            try
            {
                bool ready;
                //do
                //{*/
                    //while (!(cstart < Stars.Count && cstart < OldStars.Count))
                        //cstart = 0;
                    for (int i = cstart; i <= end; i++)
                        {
                            Star s = Stars[i];
                            if (!(s.computed || s.dead))
                            {
                    try
                    {
                        s.computed = true;
                        Vec6 Star = new Vec6(OldStars[i].pos, OldStars[i].vel);
                        Vec6 KA = dt * f(Star, s.id);
                        Vec6 FF = dt * f(KA, s.id);
                        Vec6 KB = dt * f(Star + (1.0 / 3) * KA + 1.0 / 18 * FF, s.id);
                        Vec6 KC = dt * f(Star - 1.216 * KA + (252.0 / 125) * KB - (44.0 / 125) * FF, s.id);
                        Vec6 KD = dt * f(Star + 9.5 * KA - (72.0 / 7) * KB + (25.0 / 14) * KC + (44.0 / 125) * FF, s.id);
                        Vec6 F = ((5.0 / 48 * KA) + (27.0 / 56 * KB) + (125.0 / 336 * KC) + (1.0 / 24 * KD));
                        s.pos = s.pos + F.ToVector(0);
                        s.vel = s.vel + F.ToVector(1);
                        s.print();
                    }
                    catch(DivideByZeroException)
                    {
                        s.dead = true;
                    }
                            }

                    }
                    

                    //ready = false;

                    /*foreach (Star s in Stars)
                        if (s.computed == false)
                            break;
                        else if (s == Stars.Last())
                            ready = true;
                    if (ready == false)
                        cstart = 0;
                } while (ready == false);*/
            /*}
            catch
            {
                Console.WriteLine("Thread Fehler bei: " + i);

            }*/
        }

        private Vec6 f(Vec6 Star,int id)
        {
            Vector acc = new Vector();
            for (int j = 0; j < Stars.Count; j++)
                if (id!=Stars[j].id&&!Stars[j].dead)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), Stars[j], id));//add all acceleration vectors

            return new Vec6(Star.ToVector(1),acc);
        }

        private Vector calcacc(Vector a,Star b, int id = -1)
        {
            
                Vector tempDirection;

                tempDirection = a - b.pos; //direction vector to the other star

                double D = 1 / tempDirection.distance();//Sterne und Weltraum Grundlagen der Himmelsmechanik S.91

                double acceleration = b.getMass() * Gravitation * (double)Math.Pow((double)D, 3);
                acceleration = (b.getMass() / Stars[id].getRelativMass()) * acceleration;
                Vector AccVec = b.pos - a;
                AccVec.mult(acceleration);
                return AccVec;
            
        }

        

        public void initialvel(int id)
        {
            Random rand = new Random();
            Vector acc = new Vector();
            
            foreach (Star s in Stars)
                if (s.id != Stars[id].id)
                    acc.add(calcacc(Stars[id].pos, s,id));//add all acceleration vectors
                    

            double Bacc = (double)acc.distance();//magnitude|x| of the vector
            double V = (double)Math.Sqrt(Bacc*Math.Sqrt(((double)Gravitation*(double)Stars[id].getMass())/Bacc));//Velocity=Squareroot(|acc|*r) r=Squareroot((G*m)/|acc|)

            double x1 = 2 * rand.NextDouble() - 1;//x1 = random -1,1
            double x2 = 2 * rand.NextDouble() - 1;//x2 = random -1,1
            double x3 = (x1 * acc.vec[0] + x2 * acc.vec[1]) / -( acc.vec[2]);// x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orhogonal vector by using the dot product

            Vector vel = new Vector(x1, x2, x3);
            double skalar = vel.skalar(acc);
            Stars[id].vel.add(vel.scale(V)); //scale vector to match the V magnitude and add to the random variance
        }

        public void export(List<Star> data, int step, string table)
        {
            //XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            //IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            int i = step;
            //System.IO.File.WriteAllText(@"A:\Dennis\Clustersim\file" + step + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(Steps.ToArray(), Newtonsoft.Json.Formatting.Indented));//export as json file
            //Console.WriteLine("SQL speichern ");
            //for (int i = 0; i < Steps.Count; i++)
            //{
                foreach (Star s in data)
                {
                    while (SQL.addRow(s, i, table) == false) ;//do until succesfull
                    //Console.WriteLine("Step: "+i+" id: "+s.id);
                    //broadcaster.SendToChannel("steps", "i"+i);
                }
            //}
        }
    }
}
