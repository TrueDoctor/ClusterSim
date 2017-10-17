﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

//using Newtonsoft.Json.Converters; 


namespace ClusterSim.ClusterLib
{
    public class StarCluster
    {


        //fields
        private const double Gravitation = 0.0002959122083;  //0.0000000000667384;//gravitation constant in AE^3 /Sunmass*Day^2
        private const int BoxLevels = 3;
        private double BoxSize;
        private string rtable;//table to read from
        private string wtable;//table to write at
        private int start;//starting step
        private int starCount;
        public List<Thread> savethreads = new List<Thread>();
        public List<Star> Stars;//Array of Stars
        Box[][,,] Boxes = new Box[BoxLevels+1][,,];
        List<IMassive> MassLayer = new List<IMassive>();
        List<int>[] Instructions;
        private List<Star> OldStars;//While calculating a step the same values have to be used
        private List<List<Star>> Steps = new List<List<Star>>();//list of Clusters stored in ram


        double dt;//delta time

        public StarCluster(string rtable, string wtable, int start, double dt = 1)   //constructor 
        {
            starCount = SQL.starsCount(rtable);
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.dt = dt;
            Stars = new List<Star>();

            Stars = SQL.readStars(rtable, start);                   //initialize
            BoxSize = (Stars.Max(x => x.pos.vec.Max()) - Stars.Min(x => x.pos.vec.Min())) / Math.Pow(2, BoxLevels); //calc low level box sizes
            

        }

        public StarCluster(int count, double dt = 1)   //constructor 
        {
            this.dt = dt;
            Stars = new List<Star>();

            Stars = null;                   //initialize
        }




        public Star[] doStep(int step, Misc.Method m)
        {
            if (Stars == null)
                return null;
            foreach (Star s in Stars)//reset computation status
                s.computed = false;

            int processors = Environment.ProcessorCount;//get number of processors
            int perCore = starCount / processors;//divide the cluster in equal parts
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
                        threads.Add(new Thread(delegate () { RK4(step, start, starCount - 1); }));//new Thread(start step,steps to process)
                        break;
                    case Misc.Method.RK5:
                        threads.Add(new Thread(delegate () { RK5(step, start, starCount - 1); }));//new Thread(start step,steps to process)
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
                save.Name = "save" + step;
                savethreads.Add(save);
                //export(new List<Star>(Steps.Last()), step + start, wtable);//currently disabled create new save Thread;
                return null;
            }
            return new List<Star>(temp.OrderBy(x => x.id)).ToArray();
        }

        public Star[] doStep(int step, int min, int max, Misc.Method m)
        {
            if (Boxes[0] == null)
                CalcBoxes();
            RefreshBoxes();
            starCount = Stars.Count;
            if (Stars == null)
                return null;
            foreach (Star s in Stars)//reset computation status
                s.computed = false;

            foreach (Star c in Stars)
                if (Stars.Exists(x => x.pos == c.pos && c.id != x.id))
                    throw new NotImplementedException();

            int processors = Environment.ProcessorCount;//get number of processors
            int perCore = ((max - min) + 1) / processors;//divide the cluster in equal parts
            int left = ((max - min) + 1) % processors;//calc remainder
            List<Thread> threads = new List<Thread>();

            OldStars = new List<Star>();//save the current values
            foreach (Star s in Stars)//clone each to prevent shallow copys
                OldStars.Add(s.Clone());

#region multithread orders
            int end;
            start = min;
            for (int i = 0; i < (processors<max-min?processors:max-min); i++)
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
                        threads.Add(new Thread(delegate () { RK4(step, test, end - 1); }));//new Thread(start step,steps to process)
                        break;
                    case Misc.Method.RK5:
                        threads.Add(new Thread(delegate () { RK5(step, test, end - 1); }));//new Thread(start step,steps to process)
                        break;
                }
                threads.Last().Priority = ThreadPriority.AboveNormal;
                threads.Last().Name = String.Format("Calc{0}-{1}", start, end - 1);
                threads.Last().Start();

                //while (true) { }
                start = end;


            }
#endregion

            //Merge Results
            while (threads.Exists(x => x.IsAlive) || Stars.Exists(x => !x.computed && x.id >= min && x.id <= max))
                Thread.Sleep(10);

            foreach (Star s in Stars.Where(c => c.computed))
                if (OldStars.Exists(x => x.pos == s.pos && s.id != x.id))
                    throw new NotImplementedException();

            return Stars.Where(x => x.computed && x.id>=min&& x.id <= max&&!Double.IsNaN(x.pos.vec[0])).OrderBy(x => x.id).ToArray();
        }

        private void CalcBoxes()
        {
            Console.WriteLine("Calc Boxes");
            BoxSize = (Stars.Max(x => x.pos.vec.Max()) - Stars.Min(x => x.pos.vec.Min())) / Math.Pow(2, BoxLevels); //calc low level box sizes
            int halfcount = (int)Math.Pow(2, BoxLevels);
            List<IMassive> temp = new List<IMassive>();
            foreach (Star s in Stars)
                MassLayer.Add(s);
            int i = Stars.Count ;
            //for (int level = 0; level <= BoxLevels; level++)
            //{
            int level = 0;
            Boxes[level] = new Box[halfcount, halfcount, halfcount];
            for (int x = 0; x < halfcount; x++)
                for (int y = 0; y < halfcount; y++)
                    for (int z = 0; z < halfcount; z++)
                    {
                        var boxpos = new Vector((x-(halfcount/2)) * BoxSize, (y - (halfcount / 2)) * BoxSize, (z - (halfcount / 2)) * BoxSize);
                        var dim = new Vector().init(BoxSize);

                        var boxStars =
                            Stars.Where(s => ((x == 0 ? true : s.pos.vec[0] > boxpos.vec[0]) && (x == halfcount - 1 ? true : s.pos.vec[0] <= boxpos.vec[0] + BoxSize))
                             && ((y == 0 ? true : s.pos.vec[1] > boxpos.vec[1]) && (y == halfcount - 1 ? true : s.pos.vec[1] <= boxpos.vec[1] + BoxSize)) && (!s.dead)
                             && ((z == 0 ? true : s.pos.vec[2] > boxpos.vec[2]) && (z == halfcount - 1 ? true : s.pos.vec[2] <= boxpos.vec[2] + BoxSize))).ToList();
                        var ids = boxStars.Select(p => p.id).ToList();

                        var thisBox = new Box(i++, boxpos, new Vector(x, y, z), BoxSize, MassLayer, ids,true);
                        Boxes[level][x,y,z]=thisBox;
                        MassLayer.Add(thisBox);
                    }
            
            for (level = 0; level < BoxLevels; level++)
            {
                int max = (int)Math.Pow(2, BoxLevels - level - 1);
                Boxes[level + 1] = new Box[max,max,max];
                for (int x = 0; x < max; x++)
                    for (int y = 0; y < max; y++)
                        for (int z = 0; z < max; z++)
                        {
                            var boxpos = new Vector((x - (max / 2)) * BoxSize*Math.Pow(2,level), (y - (max / 2)) * BoxSize*Math.Pow(2, level), (z - (max / 2)) * BoxSize*Math.Pow(2, level));
                            var vecEqual = new Vector(x, y, z);
                            var ids = new List<int>();
                            foreach (Box b in Boxes[level])
                                if ((b.PosId / 2).Floor() == vecEqual)
                                    ids.Add(b.id);
                            var tempbox = new Box(i++, boxpos, new Vector(x, y, z), BoxSize* Math.Pow(2, level), MassLayer, ids);
                            Boxes[level + 1][x,y,z]=tempbox;
                            MassLayer.Add(tempbox);
                        }

            }

            GenerateInstructions();
        }

        private void RefreshBoxes()
        {
            Console.WriteLine("Refresh Boxes");
            MassLayer.Clear();
            MassLayer.AddRange(Stars);
            int halfcount = (int)Math.Pow(2, BoxLevels);
            Vector temp;
            foreach (Star s in Stars)
            {
                temp = (s.pos / BoxSize).Floor()+(halfcount/2);
                Boxes[0][temp.vec[0]<0?0:temp.vec[0]>halfcount-1?halfcount-1:(int)temp.vec[0], temp.vec[1] < 0 ? 0 : temp.vec[1] > halfcount - 1 ? halfcount - 1 : (int)temp.vec[1], temp.vec[2] < 0 ? 0 : temp.vec[2] > halfcount - 1 ? halfcount - 1 : (int)temp.vec[2]].ids.Add(s.id);
            }
            for (int j = 0; j < Boxes.Length; j++)
                foreach (Box b in Boxes[j])
                {
                    b.Calc();
                    MassLayer.Add(b);
                }
            /*
            List<IMassive> temp = new List<IMassive>();
            int i = 0;
            //MassLayer.Clear();
            //MassLayer.AddRange(Stars);
            
            for (int x = 0; x < halfcount; x++)
                for (int y = 0; y < halfcount; y++)
                    for (int z = 0; z < halfcount; z++)
                    {
                        Box b = Boxes[0][x,y,z];
                        var boxpos = b.PosId;
                        
                        var ids =
                            Stars.Where(s => ((x == 0 ? true : s.pos.vec[0] > boxpos.vec[0]) && (x == halfcount - 1 ? true : s.pos.vec[0] <= boxpos.vec[0] + BoxSize))
                             && ((y == 0 ? true : s.pos.vec[1] > boxpos.vec[1]) && (y == halfcount - 1 ? true : s.pos.vec[1] <= boxpos.vec[1] + BoxSize)) && (!s.dead)
                             && ((z == 0 ? true : s.pos.vec[2] > boxpos.vec[2]) && (z == halfcount - 1 ? true : s.pos.vec[2] <= boxpos.vec[2] + BoxSize))).Select(p => p.id).ToList();
                        
                        b.refresh(ref MassLayer, ids);
                        //MassLayer.Add(b);
                    }
            /*for (int j = 1; j < Boxes.Length; j++)
                MassLayer.AddRange(Boxes[j]);*/
            MassLayer.OrderBy(x => x.id);
        }

        private void GenerateInstructions()
        {
            Console.WriteLine("Generate Instructions");
            Instructions = new List<int>[Stars.Count];
            var temp = new List<Box>();
            var UpperTemp = new List<Box>();
            foreach (Box b in Boxes[0]) //For each level 0 Box
            {
                if (b.ids.Count == 0)   //Skip emty Boxes
                    continue;
                temp = new List<Box>(); //initialize temp
                UpperTemp = new List<Box>();//initialize Upper temp

                var Boxids = new List<int>[BoxLevels+1];  //create box instruction array
                Boxids[0] = new List<int>();

                temp.Add(b);    //Add Box as initial seed
                for (int level = 0; level < BoxLevels; level++)  //for all levels
                {
                    //doesnt work
                    Boxids[level +1] = new List<int>();    //initialize id array
                    if (level != 0||true)
                    {
                        var oldtemp = new List<Box>(temp);      //dublicate list  to prevent enumeration failures
                        temp.Clear();                           //clear Temp
                        foreach (Box c in oldtemp)                      //add level-1 ids of surrounding level
                            foreach (Box t in Boxes[level])             //foreach Box in same layer
                                if (!temp.Exists(x => x.id == t.id)&&!oldtemp.Exists(x => x.id == t.id) && t.mass != 0)    //if boxes dont already exst an are Neighbours
                                    if (t.PosId.IsNeighbour(c.PosId))   //
                                    {
                                        Boxids[level].AddRange(t.ids);  //add ids of surrounding Boxes
                                        temp.Add(t);                    //add box to current layer selected
                                    }
                    }
                    //propably working
                    foreach (Box t in temp)                         //add level-1 ids to completet level+1 of existing
                    {
                        var pos = (t.PosId / 2).Floor();
                        Box Upper = Boxes[level + 1][(int)pos.vec[0], (int)pos.vec[1], (int)pos.vec[2]];
                        Boxids[level +1].AddRange(Upper.ids.Where(x => !temp.Exists(y => y.id == x)&&!Boxids[level+1].Contains(x)));
                        //if (!UpperTemp.Exists(x=>x.id==Upper.id))
                            UpperTemp.Add(Upper);
                    }
                    temp.Clear();
                    temp.AddRange(UpperTemp);
                    UpperTemp.Clear();
                }
                b.Calcids = Boxids;
                if (b.root)
                    foreach (int id in b.ids)
                    {
                        Instructions[id] = new List<int>();
                        foreach (List<int> list in Boxids)
                            Instructions[id].AddRange(list.Where(x=>x!=id&&x!=b.id));
                        //Instructions[id].RemoveAll(x=>x==b.id);
                    }

            }
        }



        private void RK4(int step, int cstart, int end)
        {
            int i = 0;
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

        public void RK5(int step, int cstart, int end)//as RK4
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
                if (!s.computed)
                {
                    s.computed = true;
                    if (!s.dead)
                    {
                        try
                        {
                            Vec6 Star = new Vec6(OldStars[i].pos, OldStars[i].vel);
                            Vec6 KA = dt * f(Star, s.id);
                            Vec6 FF = dt * f(KA, s.id);
                            Vec6 KB = dt * f(Star + (1.0 / 3) * KA + 1.0 / 18 * FF, s.id);
                            Vec6 KC = dt * f(Star - 1.216 * KA + (252.0 / 125) * KB - (44.0 / 125) * FF, s.id);
                            Vec6 KD = dt * f(Star + 9.5 * KA - (72.0 / 7) * KB + (25.0 / 14) * KC + (44.0 / 125) * FF, s.id);
                            Vec6 F = ((5.0 / 48 * KA) + (27.0 / 56 * KB) + (125.0 / 336 * KC) + (1.0 / 24 * KD));
                            //s.pos = s.pos + F.ToVector(0);
                            //s.vel = s.vel + F.ToVector(1);
                            s.print();
                        }
                        catch (DivideByZeroException)
                        {
                            s.dead = true;
                        }
                    }
                }
            }
        }

        private Vec6 f(Vec6 Star, int id)
        {
            Vector acc = new Vector();/*
            for (int j = 0; j < Stars.Count; j++)
                if (id != Stars[j].id && !Stars[j].dead)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), Stars[j], id));//add all acceleration vectors
/*/
            for (int j = 0; j < Instructions[id].Count; j++)
            {
                int temp = Instructions[id][j];
                if (!MassLayer[temp].dead && !MassLayer[id].dead && MassLayer[temp].mass != 0&&temp!=id)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), MassLayer[temp], id));//add all acceleration vectors
                else if(MassLayer[temp].mass != 0) { }
            }                                                      /*
                                                            for (int j = 0; j < Boxes.Count; j++)
                                                                if (id != Boxes[j].id)//no self intersection to prevent dividing by 0
                                                                    acc.add(calcacc(Star.ToVector(0), Boxes[j], id));//add all acceleration vectors
                                                                    */
            return new Vec6(Star.ToVector(1), acc);
        }

        private Vector calcacc(Vector a, IMassive b, int id = -1)
        {

            Vector tempDirection;

            tempDirection = a - b.pos; //direction vector to the other star

            
            double D = 1 / tempDirection.distance();//Sterne und Weltraum Grundlagen der Himmelsmechanik S.91
            
            double acceleration = b.mass * Gravitation * Math.Pow(D, 3);
            acceleration = (b.mass / Stars[id].mass) * acceleration;
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
                    acc.add(calcacc(Stars[id].pos, s, id));//add all acceleration vectors


            double Bacc = acc.distance();//magnitude|x| of the vector
            double V = Math.Sqrt(Bacc * Math.Sqrt((Gravitation * Stars[id].getMass()) / Bacc));//Velocity=Squareroot(|acc|*r) r=Squareroot((G*m)/|acc|)

            double x1 = 2 * rand.NextDouble() - 1;//x1 = random -1,1
            double x2 = 2 * rand.NextDouble() - 1;//x2 = random -1,1
            double x3 = (x1 * acc.vec[0] + x2 * acc.vec[1]) / -(acc.vec[2]);// x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orhogonal vector by using the dot product

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
