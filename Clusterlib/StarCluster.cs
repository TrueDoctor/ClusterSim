using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ClusterLib;
using XDMessaging;

//using Newtonsoft.Json.Converters; 


namespace ClusterLib
{
    public class StarCluster
    {
        
        
        //fields
        private const decimal Gravitation = 0.0002959122083m;  //0.0000000000667384;//gravitation constant in AE^3 /Sunmass*Day^2
        private string rtable;//table to read from
        private string wtable;//table to write at
        private int start;//starting step
        private int starCount;
        public List <Star> Stars;//Array of Stars
        private List<Star> OldStars;//While calculating a step the same values have to be used
        private List<List<Star>> Steps = new List<List<Star>>();//list of Clusters stored in ram
        
        
        decimal dt;//delta time

        public StarCluster(string rtable,string wtable,int start,decimal dt=1)   //constructor 
        {
            this.starCount = SQL.starsCount(rtable);
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.dt = dt;
            Stars = new List<Star>();

            Stars = SQL.readStars(rtable, start);                   //initialize
        }

        


        public void doStep(int step,Misc.Method m)    
        {
            int processors = Environment.ProcessorCount;//get number of processors
            int perCore = (int)starCount/ processors;//divide the cluster in equal parts
            int left = starCount - perCore * processors ;//calc remainder
            

            OldStars = new List<Star>();//save the current values
            foreach (Star s in Stars)//clone each to prevent shallow copys
                OldStars.Add(s.Clone());
            int n = 0;
            for (int i=0;i<processors;i++)
            {
                n++;
                if (i == processors - 1)
                    perCore += left;
                try
                {
                    switch (m)//case(Method) of...
                    {

                        case Misc.Method.RK4:
                            new Thread(delegate () { RK4(step, perCore * i); }).Start();//new Thread(start step,steps to process)
                            break;
                        case Misc.Method.RK5:
                            new Thread(delegate () { RK5(step, perCore * i); }).Start();//new Thread(start step,steps to process)
                            break;
                    }
                }
                catch//fehlerabfang
                {
                    Console.WriteLine("Thread Fehler");
                    if (n <= i + 10)
                        i--;
                    else if (i == 0)
                    {
                        Console.WriteLine("Schwerer Threadfehler");
                        Thread.CurrentThread.Abort();
                    }
                    perCore -= left;
                }

            }

            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;//idle this thread

            bool ready;
            do
            {           //wait until all threads have finished
                ready = false;
                Thread.Sleep(10);
                foreach (Star s in Stars)
                    if (s.computed == false)
                        break;
                    else if (s == Stars.Last())
                        ready = true;
                        
            } while (ready == false);

            List<Star> temp = new List<Star>();
            foreach (Star s in Stars)//prevent shallow copys
                temp.Add(s.Clone());

            Steps.Add(new List<Star>(temp.OrderBy(x=>x.id)));//add step to steps array



            /*Thread save = new Thread(delegate () { export(new List<Star>(Steps.Last()), step+start,wtable); });
            save.Priority = ThreadPriority.Highest;
            save.Start();*/
            //export(new List<Star>(Steps.Last()), step + start, wtable);//currently disabled create new save Thread;

            foreach (Star s in Stars)//reset computation status
                s.computed = false;     
        }

        private void RK4(int step,int cstart)
        {
            bool ready;
            do
            {
                for (int i = cstart; i < starCount; i++)//for seqence of Stars[]
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

                }
                ready = false;//initialize bool

                foreach (Star s in Stars)//check for not yet computed stars
                    if (s.computed == false)
                        break;//abort if not computed
                    else if (s == Stars.Last())
                        ready = true;//sucessfully finished
                if (ready == false)
                    cstart = 0;//start form 0
            } while (ready == false);//repete until everythig is computed
        }
        
        public void RK5(int step, int cstart)//as RK4
        {
            bool ready;
            do
            {
                for (int i = cstart; i < starCount; i++)
                {
                    Star s = Stars[i];
                    if (s.computed == false)
                    {
                        s.computed = true;
                        Vec6 Star = new Vec6(OldStars[i].pos, OldStars[i].vel);
                        Vec6 KA = dt * f(Star, s.id);
                        Vec6 FF = dt * f(KA, s.id);
                        Vec6 KB = dt * f(Star + (1.0m / 3) * KA + 1.0m / 18 * FF, s.id);
                        Vec6 KC = dt * f(Star - 1.216m * KA + (252.0m / 125) * KB - (44.0m / 125) * FF, s.id);
                        Vec6 KD = dt * f(Star + 9.5m * KA - (72.0m / 7) * KB + (25.0m / 14) * KC + (44.0m / 125) * FF, s.id);
                        Vec6 F = ((5.0m / 48 * KA) + (27.0m / 56 * KB) + (125.0m / 336 * KC) + (1.0m / 24 * KD));
                        s.pos = s.pos + F.ToVector(0);
                        s.vel = s.vel + F.ToVector(1);
                        s.print();
                    }
                }

                ready = false;
                
                foreach (Star s in Stars)
                    if (s.computed == false)
                        break;
                    else if (s == Stars.Last())
                        ready = true;
                if (ready == false)
                    cstart = 0;
            } while (ready == false);
        }

        private Vec6 f(Vec6 Star,int id)
        {
            Vector acc = new Vector();
            for (int j = 0; j < Stars.Count; j++)
                if (id!=Stars[j].id)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), Stars[j], id));//add all acceleration vectors

            return new Vec6(Star.ToVector(1),acc);
        }

        private Vector calcacc(Vector a,Star b, int id = -1)
        {
            Vector tempDirection;

            tempDirection = a - b.pos; //direction vector to the other star

            decimal D = 1 / tempDirection.distance();//Sterne und Weltraum Grundlagen der Himmelsmechanik S.91

            decimal acceleration = b.getMass() * Gravitation * (decimal)Math.Pow((double)D, 3);
            acceleration = (b.getMass() / Stars[id].getRelativMass()) * acceleration ;
            Vector AccVec = b.pos-a;
            AccVec.mult(acceleration);
            return AccVec;
        }

        

        public void initialvel(int id)
        {
            Random rand = new Random();
            Vector acc = new Vector();
            
            foreach (Star s in Stars)
                if (s.id != id)
                    acc.add(calcacc(Stars[id].pos, s));//add all acceleration vectors
                    

            double Bacc = (double)acc.distance();//magnitude|x| of the vector
            decimal V = (decimal)Math.Sqrt(Bacc*Math.Sqrt(((double)Gravitation*(double)Stars[id].getMass())/Bacc));//Velocity=Squareroot(|acc|*r) r=Squareroot((G*m)/|acc|)

            decimal x1 = 2m * (decimal)rand.NextDouble() - 1m;//x1 = random -1,1
            decimal x2 = 2m * (decimal)rand.NextDouble() - 1m;//x2 = random -1,1
            decimal x3 = (x1 * acc.vec[0] + x2 * acc.vec[1]) / -( acc.vec[2]);// x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orhogonal vector by using the dot product

            Vector vel = new Vector(x1, x2, x3);
            decimal skalar = vel.skalar(acc);
            Stars[id].vel.add(vel.scale(V)); //scale vector to match the V magnitude and add to the random variance
        }

        public void export(List<Star> data, int step, string table)
        {
            XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);

            //System.IO.File.WriteAllText(@"A:\Dennis\Clustersim\file" + step + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(Steps.ToArray(), Newtonsoft.Json.Formatting.Indented));//export as json file
            Console.WriteLine("SQL speichern ");
            for (int i = 0; i < Steps.Count; i++)
            {
                foreach (Star s in Steps[i])
                {
                    while (SQL.addRow(s, i, table) == false) ;//do until succesfull
                    Console.WriteLine("Step: "+i+" id: "+s.id);
                    broadcaster.SendToChannel("steps", "i"+i);
                }
            }
        }
    }
}
