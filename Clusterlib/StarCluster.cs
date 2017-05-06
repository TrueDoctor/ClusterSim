using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ClusterLib;

using Newtonsoft.Json.Converters; 


namespace ClusterLib
{
    public class StarCluster
    {
        
        SQL client = new SQL();
        //fields
        private const double Gravitation = 0.0002959122083;  //0.0000000000667384;
        private string rtable;
        private string wtable = "copy";
        private int start;
        private int starCount;
        private List <Star> Stars;
        private List<List<Star>> Steps = new List<List<Star>>();
        
        
        double dt=1;

        public StarCluster(string rtable,string wtable,int start,double dt)   //constructor takes Starcount
        {
            this.starCount = SQL.starsCount(rtable);
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.dt = dt;
            Stars = new List<Star>(0);
            Steps = new List<List<Star>>();
            //randomize();
            initialize();                   //initialize
            //Console.WriteLine(calcacc(new Vector(new double[3] { 1, 2, 3 }), new Star(new double[] { 3, 2, 1 },new double[3],new double[3],10)).toString());
        }



        private void initialize()   //download newest Data
        {
            Stars = SQL.readStars(rtable,start);
            //Stars = new List<Star> { new Star(new Vec6(new Vector( 1, 2, 3),new Vector( 3, 2, 1 )),5,0), new Star(new Vec6(new Vector(3, 2, 3), new Vector(3, 2, 1)), 10, 1) };           
            
        }

        


        public void doStep(int step,Misc.Method m)    
        {
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            int processors = Environment.ProcessorCount;
            int perCore = (int)starCount/ processors;
            int left = starCount - perCore * processors ;

            for (int i=0;i<processors;i++)
            {
                if (i == processors - 1)
                    perCore += left;
                switch (m)
                {

                    case Misc.Method.RK4:
                        new Thread(delegate () { RK4(step,perCore*i); }).Start();
                        break;
                    case Misc.Method.RK5:
                        new Thread(delegate () { RK5(step,perCore*i); }).Start();
                        break;
                }

            }

            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            bool ready;
            do
            {
                ready = false;
                Thread.Sleep(10);
                foreach (Star s in Stars)
                    if (s.computed == false)
                        break;
                    else if (s == Stars.Last())
                        ready = true;
                        
            } while (ready == false);

            List<Star> temp = new List<Star>();
            foreach (Star s in Stars)
                temp.Add(s);

            Steps.Add(new List<Star>(temp));

            

            Thread save = new Thread(delegate () { export(new List<Star>(Steps.Last()), step,wtable); });
            save.Priority = ThreadPriority.Highest;
            save.Start();

            foreach (Star s in Stars)
                s.computed = false;     
        }

        private void RK4(int step,int cstart)
        {
            bool ready;
            do
            {
                for (int i = cstart; i <= starCount; i++)
                {
                    Star s = Stars[i];
                    if (s.computed == false)
                    {
                        s.computed = true;
                        Vec6 Star = new Vec6(s.pos, s.vel);
                        Vec6 KA = f(Star, s.id);
                        Vec6 KB = f(Star + ((dt / 2) * KA), s.id);
                        Vec6 KC = f(Star + ((dt / 2) * KB), s.id);
                        Vec6 KD = f(Star + KC, s.id);
                        Vec6 F = ((dt / 6) * (KA + (2 * KB) + (2 * KC) + KD));
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
        
        public void RK5(int step, int cstart)
        {
            bool ready;
            do
            {
                for (int i = cstart; i <= starCount; i++)
                {
                    Star s = Stars[i];
                    if (s.computed == false)
                    {
                        s.computed = true;
                        Vec6 Star = new Vec6(s.pos, s.vel);
                        Vec6 KA = dt * f(Star, s.id);
                        Vec6 FF = dt * f(KA, s.id);
                        Vec6 KB = dt * f(Star + (1.0 / 3.0) * KA + 1.0 / 18.0 * FF, s.id);
                        Vec6 KC = dt * f(Star - 1.216 * KA + (252.0 / 125.0) * KB - (44.0 / 125.0) * FF, s.id);
                        Vec6 KD = dt * f(Star + 9.5 * KA - (72.0 / 7.0) * KB + (25.0 / 14.0) * KC + (44.0 / 125.0) * FF, s.id);
                        Vec6 F = ((5.0 / 48.0 * KA) + (27.0 / 56.0 * KB) + (125.0 / 336.0 * KC) + (1.0 / 24.0 * KD));
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
                if (id!=j)
                    acc.add(calcacc(Star.ToVector(0), Stars[j]));

            return new Vec6(Star.ToVector(1),acc);
        }

        private Vector calcacc(Vector a,Star b)
        {
            Vector tempDirection;

            tempDirection = a - b.pos; //direction vector to the other star

            double D = 1 / tempDirection.distance();

            double temp = b.getMass() * Gravitation * (double)Math.Pow((double)D, 3);
            Vector Acc = b.pos-a;
            Acc.mult(temp);
            return Acc;
        }

        

        public void export(List<Star> data, int step, string table)
        {
            foreach (Star s in data)
            {
                while (SQL.addRow(s, step, table) == false) ;
                //Console.WriteLine(s.id);
            }
                //System.IO.File.WriteAllText(@"C:\Users\Dennis\Documents\Clustersim1\file" + i + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
                //Console.WriteLine(i + "\n" + Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
            
            
        }
    }

    public static class CloneClass
    {
        /// <summary>
        /// Clones a object via shallow copy
        /// </summary>
        /// <typeparam name="T">Object Type to Clone</typeparam>
        /// <param name="obj">Object to Clone</param>
        /// <returns>New Object reference</returns>
        public static T CloneObject<T>(this T obj) where T : class
        {
            if (obj == null) return null;
            System.Reflection.MethodInfo inst = obj.GetType().GetMethod("MemberwiseClone",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (inst != null)
                return (T)inst.Invoke(obj, null);
            else
                return null;
        }
    }
}
