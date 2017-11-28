

//using Newtonsoft.Json.Converters; 
namespace ClusterSim.ClusterLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Linq;
    using System.Threading;

    public class StarCluster
    {
        private const int BoxLevels = 4;

        // fields
        private const double Gravitation = 0.0002959122083
            ; // 0.0000000000667384;//gravitation constant in AE^3 /Sunmass*Day^2

        public List<Thread> savethreads = new List<Thread>();

        public List<Star> Stars; // Array of Stars

        private List<Box> Boxes = new List<Box>();

        private double BoxSize;

        public double dt; // delta time

        private List<int>[] Instructions;

        private readonly List<IMassive> MassLayer = new List<IMassive>();

        private List<Star> OldStars; // While calculating a step the same values have to be used

        private readonly string rtable; // table to read from

        private int starCount;

        private int start; // starting step

        private readonly List<List<Star>> Steps = new List<List<Star>>(); // list of Clusters stored in ram

        private readonly string wtable; // table to write at

        public StarCluster(string rtable, string wtable, int start, double dt = 1)
        {
            // constructor 
            this.starCount = SQL.starsCount(rtable);
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.dt = dt;
            this.Stars = new List<Star>();

            this.Stars = SQL.readStars(rtable, start); // initialize
            this.BoxSize = (this.Stars.Max(x => x.pos.vec.Max()) - this.Stars.Min(x => x.pos.vec.Min()))
                           / Math.Pow(2, BoxLevels); // calc low level box sizes
        }

        public StarCluster(int count, double dt = 1)
        {
            // constructor 
            this.dt = dt;
            this.Stars = new List<Star>();

            this.Stars = null; // initialize
        }

        public Star[] doStep(int step, Misc.Method m)
        {
            if (this.Stars == null) return null;
            foreach (var s in this.Stars) // reset computation status
                s.computed = false;

            int processors = Environment.ProcessorCount; // get number of processors
            int perCore = this.starCount / processors; // divide the cluster in equal parts
            int left = this.starCount % processors; // calc remainder
            var threads = new List<Thread>();

            this.OldStars = new List<Star>(); // save the current values
            foreach (var s in this.Stars) // clone each to prevent shallow copys
                this.OldStars.Add(s.Clone());

            for (var i = 0; i < processors; i++)
            {
                int start = i * perCore;

                if (left > 0)
                {
                    start++;
                    left--;
                }

                switch (m)
                {
                    // case(Method) of...
                    case Misc.Method.RK4:
                        threads.Add(
                            new Thread(
                                delegate ()
                                    {
                                        this.RK4(step, start, this.starCount - 1);
                                    })); // new Thread(start step,steps to process)
                        break;
                    case Misc.Method.RK5:
                        threads.Add(
                            new Thread(
                                delegate ()
                                    {
                                        this.RK5(step, start, this.starCount - 1);
                                    })); // new Thread(start step,steps to process)
                        break;
                }
                threads.Last().Priority = ThreadPriority.Highest;
                threads.Last().Start();
            }

            while (threads.Exists(x => x.IsAlive)) Thread.Sleep(10);

            var temp = new List<Star>();
            foreach (var s in this.Stars) // prevent shallow copys
                temp.Add(s.Clone());

            if (this.rtable != string.Empty && this.wtable != string.Empty)
            {
                this.Steps.Add(new List<Star>(temp.OrderBy(x => x.id))); // add step to steps array

                var save = new Thread(
                    delegate () { this.export(new List<Star>(this.Steps.Last()), step + this.start, this.wtable); });
                save.Priority = ThreadPriority.Highest;
                save.Start();
                save.Name = "save" + step;
                this.savethreads.Add(save);

                // export(new List<Star>(Steps.Last()), step + start, wtable);//currently disabled create new save Thread;
                return null;
            }

            return new List<Star>(temp.OrderBy(x => x.id)).ToArray();
        }

        public Star[] doStep(int step, int min, int max, Misc.Method m)
        {
            this.CalcBoxes();
            //this.GenerateInstructions();
            this.Instructions = new List<int>[this.Stars.Count];
            this.starCount = this.Stars.Count;

            if (this.Stars == null) return null;
            foreach (var s in this.Stars) // reset computation status
                s.computed = false;


            int processors = Environment.ProcessorCount; // get number of processors
            int perCore = (max - min + 1) / processors; // divide the cluster in equal parts
            int left = (max - min + 1) % processors; // calc remainder
            var threads = new List<Thread>();

            this.OldStars = new List<Star>(); // save the current values
            foreach (var s in this.Stars) // clone each to prevent shallow copys
                this.OldStars.Add(s.Clone());


            int end;
            this.start = min;
            for (var i = 0; i < (processors < max - min ? processors : max - min + 1); i++)
            {
                end = this.start + perCore;
                if (left > 0)
                {
                    end++;
                    left--;
                }

                int test = this.start;

                switch (m)
                {
                    // case(Method) of...
                    case Misc.Method.RK4:
                        threads.Add(
                            new Thread(
                                delegate ()
                                    {
                                        this.RK4(step, test, end - 1);
                                    })); // new Thread(start step,steps to process)
                        break;
                    case Misc.Method.RK5:
                        threads.Add(
                            new Thread(
                                delegate ()
                                    {
                                        this.RK5(step, test, end - 1);
                                    })); // new Thread(start step,steps to process)
                        break;
                }
                threads.Last().Priority = ThreadPriority.Highest;
                threads.Last().Name = string.Format("Calc{0}-{1}", this.start, end - 1);
                threads.Last().Start();

                // while (true) { }
                this.start = end;
            }

            // Merge Results
            while (threads.Exists(x => x.IsAlive) || this.Stars.Exists(x => !x.computed && x.id >= min && x.id <= max))
                Thread.Sleep(10);

            //foreach (var s in this.Stars.Where(c => c.computed))
            //if (this.OldStars.Exists(x => x.pos == s.pos && s.id != x.id)) throw new NotImplementedException();

            return this.Stars.Where(x => x.computed && x.id >= min && x.id <= max)
                .OrderBy(x => x.id).ToArray();
        }

        public void export(List<Star> data, int step, string table)
        {
            // XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            // IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            int i = step;

            // System.IO.File.WriteAllText(@"A:\Dennis\Clustersim\file" + step + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(Steps.ToArray(), Newtonsoft.Json.Formatting.Indented));//export as json file
            // Console.WriteLine("SQL speichern ");
            // for (int i = 0; i < Steps.Count; i++)
            // {
            foreach (var s in data) while (SQL.addRow(s, i, table) == false) ; // do until succesfull

            // Console.WriteLine("Step: "+i+" id: "+s.id);
            // broadcaster.SendToChannel("steps", "i"+i);

            // }
        }

        public void initialvel(int id)
        {
            var rand = new Random();
            var acc = new Vector();

            foreach (var s in this.Stars)
                if (s.id != this.Stars[id].id)
                    acc.add(this.Calcacc(this.Stars[id].pos, s, id)); // add all acceleration vectors

            double Bacc = acc.distance(); // magnitude|x| of the vector
            double V = Math.Sqrt(
                Bacc * Math.Sqrt(
                    Gravitation * this.Stars[id].getMass()
                    / Bacc)); // Velocity=Squareroot(|acc|*r) r=Squareroot((G*m)/|acc|)

            double x1 = 2 * rand.NextDouble() - 1; // x1 = random -1,1
            double x2 = 2 * rand.NextDouble() - 1; // x2 = random -1,1
            double
                x3 = (x1 * acc.vec[0] + x2 * acc.vec[1])
                     / -acc.vec[2]; // x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orhogonal vector by using the dot product

            var vel = new Vector(x1, x2, x3);
            double skalar = vel.skalar(acc);
            this.Stars[id].vel.add(vel.scale(V)); // scale vector to match the V magnitude and add to the random variance
        }

        public void RK5(int step, int cstart, int end)
        {
            // as RK4
            // Thread.CurrentThread.Name = "working";
            /*int i = 0;
            try
            {
                bool ready;
                //do
                //{*/
            // while (!(cstart < Stars.Count && cstart < OldStars.Count))
            // cstart = 0;
            for (int i = cstart; i <= end; i++)
            {
                var s = this.Stars[i];
                if (!s.computed)
                {
                    s.computed = true;
                    if (!s.dead)
                        try
                        {
                            Instructions[s.id] = new List<int>();
                           GetInstruction(s.pos, s.id, this.Boxes[0],ref this.Instructions[s.id]);

                            var Star = new Vec6(this.OldStars[i].pos, this.OldStars[i].vel);
                            var KA = this.dt * this.f(Star, s.id);
                            var FF = this.dt * this.f(KA, s.id);
                            var KB = this.dt * this.f(Star + 1.0 / 3 * KA + 1.0 / 18 * FF, s.id);
                            var KC = this.dt * this.f(Star - 1.216 * KA + 252.0 / 125 * KB - 44.0 / 125 * FF, s.id);
                            var KD = this.dt * this.f(
                                         Star + 9.5 * KA - 72.0 / 7 * KB + 25.0 / 14 * KC + 44.0 / 125 * FF,
                                         s.id);
                            var F = 5.0 / 48 * KA + 27.0 / 56 * KB + 125.0 / 336 * KC + 1.0 / 24 * KD;

                            s.pos += F.ToVector(0);
                            s.vel += F.ToVector(1);
                            //s.print();
                        }
                        catch (DivideByZeroException)
                        {
                            s.dead = true;
                        }
                }
            }
        }

        private Vec6 f(Vec6 Star, int id)
        {
            var acc = new Vector();
            /*for (int j = 0; j < Stars.Count; j++)
                if (id != Stars[j].id && !Stars[j].dead)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), Stars[j], id));//add all acceleration vectors
                                                                     //*/
            for (var j = 0; j < this.Instructions[id].Count; j++)
            {
                int temp = this.Instructions[id][j];
                if (!this.MassLayer[temp].dead && !this.MassLayer[id].dead && this.MassLayer[temp].mass != 0
                    && temp != id)
                {
                    // no self intersection to prevent dividing by 0
                    acc.add(this.Calcacc(Star.ToVector(0), this.MassLayer[temp], id, temp)); // add all acceleration vectors
                }
                else if (this.MassLayer[temp].mass != 0)
                {
                } //m67s tow layers up, get added to instructions
            }//*/

            /*                                                         for (int j = 0; j < Boxes.Count; j++)
                                                                         if (id != Boxes[j].id)//no self intersection to prevent dividing by 0
                                                                             acc.add(calcacc(Star.ToVector(0), Boxes[j], id));//add all acceleration vectors
                                                                             //*/
            return new Vec6(Star.ToVector(1), acc);
        }

        private Vector Calcacc(Vector a, IMassive b, int id = -1, int bid = -1)
        {
            Vector tempDirection;

            tempDirection = a - b.pos; // direction vector to the other star

            if (a == b.pos)
            {
                /*foreach (Box bs in Boxes.Where(x=Y))
                    if (bs.ids.Contains(4548))
                        Console.WriteLine(bs.id);*/
                throw new DivideByZeroException();
            }

            double D = 1 / tempDirection.distance(); // Sterne und Weltraum Grundlagen der Himmelsmechanik S.91

            double acceleration = b.mass * Gravitation * Math.Pow(D, 3);
            acceleration = b.mass / this.Stars[id].mass * acceleration;
            var AccVec = b.pos - a;
            /*if (Double.IsNaN(acceleration))
                ;*/
            AccVec.mult(acceleration);
            return AccVec;
        }

        private void CalcBoxes()
        {
            Console.WriteLine("Calc Boxes");
            this.BoxSize = (this.Stars.Max(x => x.pos.vec.Max()) - this.Stars.Min(x => x.pos.vec.Min()));

            this.MassLayer.Clear();
            this.Boxes.Clear();
            foreach (var s in this.Stars) this.MassLayer.Add(s);
            int id = this.Stars.Count;

            this.AddBox(ref this.Boxes, ref id, new Vector().init(-this.BoxSize / 2), this.BoxSize, new List<IMassive>(this.Stars));

            //this.Boxes.OrderBy(x => x.id);

            this.Boxes = this.Boxes.OrderBy(x => x.id).ToList();
            this.MassLayer.AddRange(this.Boxes);

            //this.MassLayer.OrderBy(x => x.id);
        }

        private int AddBox(ref List<Box> boxes, ref int boxId, Vector pos, double size, List<IMassive> stars)
        {
            if (stars.Count == 1)
            {
                boxes.Add(new Box(boxId++, pos / size, pos, size, stars, new List<int>() { stars[0].id }, true));
                return boxId - 1; //warning, this might not work when multithreated
            }
            else
            {
                var tbox = new Box(boxId++, pos, pos / size, size, new List<IMassive>(), new List<int>(), false);
                var x = stars.Where(a => a.pos.vec[0] < pos.vec[0] + size / 2);
                for (int i = 0; i < 2; i++)
                {
                    var y = x.Where(a => a.pos.vec[1] < pos.vec[1] + size / 2);
                    for (int j = 0; j < 2 && x.Count() != 0; j++)
                    {
                        var z = y.Where(a => a.pos.vec[2] < pos.vec[2] + size / 2);
                        for (int k = 0; k < 2 && y.Count() != 0; k++)
                        {
                            if (z.Count() != 0)
                                tbox.ids.Add(this.AddBox(ref boxes, ref boxId, pos + size / 2 * new Vector(i, j, k), size / 2, z.ToList()));
                            z = y.Except(z).ToList();
                        }
                        y = x.Except(y).ToList();
                    }
                    x = stars.Except(x).ToList();
                }
                tbox.refresh(boxes.Select(v => v as IMassive).ToList());
                tbox.Calc();
                boxes.Add(tbox);
                return tbox.id;
            }


        }

        private void GenerateInstructions()
        {
            Console.WriteLine("Generate Instructions");
            this.Instructions = new List<int>[this.Stars.Count];
            foreach (Star s in this.Stars)
            {
                //this.Instructions[s.id] = GetInstruction(s.pos, s.id, this.Boxes[0]);
            }

        }

        private void GetInstruction(Vector sPos, int sid, Box box, ref List<int> ids) 
        {
            if (box.ids.Count == 0)
            {
                return;
            }
            if (box.ids.Count == 1)
                if (box.ids.Contains(sid) || box.mass == 0)
                    return;
                else if (box.root)
                {
                    ids.Add(box.id);
                    return;
                }
                    

            if (box.size * box.size / (sPos - box.pos).distance2() < 0.4)
            {
                ids.Add(box.id);
                return;
            }
            foreach (int id in box.ids)
            {
                if (this.Boxes[id - this.Stars.Count].mass != 0)
                    GetInstruction(sPos, sid, this.Boxes[id - this.Stars.Count],ref ids);
            }
            return;


        }


        private void RK4(int step, int cstart, int end)
        {
            var i = 0;

            // try
            // {
            // bool ready;
            // do
            // {
            while (!(cstart < this.Stars.Count && cstart < this.OldStars.Count)) cstart = 0;
            for (i = cstart; i <= end; i++)
            {
                // for seqence of Stars[]
                var s = this.Stars[i];
                if (s.computed == false)
                {
                    s.computed = true;

                    GetInstruction(s.pos, s.id, this.Boxes[0],ref this.Instructions[s.id]);

                    var Star = new Vec6(this.OldStars[i].pos, this.OldStars[i].vel); // convert star to vec6
                    var KA = this.f(Star, s.id); // calculate help values
                    var KB = this.f(Star + this.dt / 2 * KA, s.id);
                    var KC = this.f(Star + this.dt / 2 * KB, s.id);
                    var KD = this.f(Star + KC, s.id);
                    var F = this.dt / 6 * (KA + 2 * KB + 2 * KC + KD); // calculate resulting Vector
                    s.pos = s.pos + F.ToVector(0);
                    s.vel = s.vel + F.ToVector(1);
                    s.print();
                }
                else
                {
                    Console.WriteLine("\n!!!Konflikt!!!\n");
                }
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
    }
}