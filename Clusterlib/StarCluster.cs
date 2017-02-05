using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


using Newtonsoft.Json.Converters; 


namespace ClusterLib
{
    public class StarCluster
    {
        //fields
        private const double Gravitation = 0.0002959122083;  //0.0000000000667384;
        private int StarCount;
        private int saves = 0;
        private List <Star> Stars;
        private List<List<Star>> Steps = new List<List<Star>>();
        
        private Vector range = new Vector(new double[] { 10000000000000000000, 10000000000000000000, 10000000000000000000 });
        double mean = 0;
        Random rand = new Random();
        double dt=2;

        public StarCluster(int StarCount)   //constructor takes Starcount
        {
            this.StarCount = StarCount;
            Stars = new List<Star>(0);
            //randomize();
            initialize();                   //initialize
            //Console.WriteLine(calcacc(new Vector(new double[3] { 1, 2, 3 }), new Star(new double[] { 3, 2, 1 },new double[3],new double[3],10)).toString());
        }



        private void initialize()   //download newest Data
        {
            Stars = new SQL().readStars("[Initial Stars]");
                       
        }

        public void randomize(Vector range = null)
        {
            if (range == null)
                this.range = range;
            SQL client = new SQL();

            for (int i = 0; i < StarCount; i++)
            {
                client.writeStar(i, new Star(new double[] {random(10000000000), random(10000000000), random(10000000000) }, new double[] { random(), random(), random() },
                    new double[] { random(), random(), random() }, Math.Abs(random(10000000)), 2), "[Initial Stars]");          
            }
        }


        public void doStep(int step)    
        {         
            foreach(Star s in Stars)
            {
                Vec6 Star = new Vec6(s.pos, s.vel);
                Vec6 KA = f(Star,s.id);
                Vec6 KB = f(Star + ((dt / 2) * KA), s.id);
                Vec6 KC = f(Star + ((dt / 2) * KB), s.id);
                Vec6 KD = f(Star + KC,s.id);
                Vec6 F  = ((dt/6) * (KA + (2 * KB) + (2 * KC) + KD));
                s.pos = s.pos + F.ToVector(0);
                s.vel = s.vel + F.ToVector(1);
                s.print();
            }

            /*List<Star> temp = new List<Star>();
            foreach (Star s in Stars)
                temp.Add(new Star(s));
*/

            //Steps.Add(CloneClass.CloneObject<List<Star>> (Stars));
            // temp.Clear();
            //temp = null;

            // if (Steps.Count == 100)
            // {
            List<Star> temp = new List<Star>(Stars);
                //Thread save = new Thread(delegate () { export(saves++,temp); });

                //save.Priority = ThreadPriority.BelowNormal;
                //save.Start();
            export(saves++,temp);
            //}
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

            double temp = b.getMass() * Gravitation * Math.Pow(D, 3);
            Vector Acc = b.pos-a;
            Acc.mult(temp);
            return Acc;
        }

        private double random(double stdDev=1)
        {
             //reuse this if you are generating many
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public void export(int i,List<Star> data)
        {
            
                System.IO.File.WriteAllText(@"C:\Users\Dennis\Documents\Clustersim\file" + i + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
                Console.WriteLine(i + "\n" + Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
            
            
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
