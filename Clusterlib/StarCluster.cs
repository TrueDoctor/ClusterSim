

//using Newtonsoft.Json.Converters; 
namespace ClusterSim.ClusterLib
{
    using System;
    using System.Collections.Generic;
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

        private readonly Box[][,,] Boxes = new Box[BoxLevels + 1][,,];

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
            if (this.Boxes[0] == null) this.CalcBoxes();
            this.RefreshBoxes();
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
                            var Star = new Vec6(this.OldStars[i].pos, this.OldStars[i].vel);
                            var KA = this.dt * this.f(Star, s.id);
                            var FF = this.dt * this.f(KA, s.id);
                            var KB = this.dt * this.f(Star + 1.0 / 3 * KA + 1.0 / 18 * FF, s.id);
                            var KC = this.dt * this.f(Star - 1.216 * KA + 252.0 / 125 * KB - 44.0 / 125 * FF, s.id);
                            var KD = this.dt * this.f(
                                         Star + 9.5 * KA - 72.0 / 7 * KB + 25.0 / 14 * KC + 44.0 / 125 * FF,
                                         s.id);
                            var F = 5.0 / 48 * KA + 27.0 / 56 * KB + 125.0 / 336 * KC + 1.0 / 24 * KD;

                            s.pos = s.pos + F.ToVector(0);
                            s.vel = s.vel + F.ToVector(1);
                            s.print();
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
                foreach (Box bs in Boxes[2])
                    if (bs.ids.Contains(4548))
                        Console.WriteLine(bs.id);
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
            this.BoxSize = (this.Stars.Max(x => x.pos.vec.Max()) - this.Stars.Min(x => x.pos.vec.Min()))
                           / Math.Pow(2, BoxLevels); // calc low level box sizes
            var halfcount = (int)Math.Pow(2, BoxLevels);
            var temp = new List<IMassive>();
            foreach (var s in this.Stars) this.MassLayer.Add(s);
            int i = this.Stars.Count;

            // for (int level = 0; level <= BoxLevels; level++)
            // {
            var level = 0;
            this.Boxes[level] = new Box[halfcount, halfcount, halfcount];
            for (var x = 0; x < halfcount; x++)
                for (var y = 0; y < halfcount; y++)
                    for (var z = 0; z < halfcount; z++)
                    {
                        var boxpos = new Vector(
                            (x - halfcount / 2) * this.BoxSize,
                            (y - halfcount / 2) * this.BoxSize,
                            (z - halfcount / 2) * this.BoxSize);
                        var dim = new Vector().init(this.BoxSize);

                        var boxStars = this.Stars.Where(
                            s => (x == 0 ? true : s.pos.vec[0] > boxpos.vec[0])
                                 && (x == halfcount - 1 ? true : s.pos.vec[0] <= boxpos.vec[0] + this.BoxSize)
                                 && (y == 0 ? true : s.pos.vec[1] > boxpos.vec[1])
                                 && (y == halfcount - 1 ? true : s.pos.vec[1] <= boxpos.vec[1] + this.BoxSize) && !s.dead
                                 && (z == 0 ? true : s.pos.vec[2] > boxpos.vec[2])
                                 && (z == halfcount - 1 ? true : s.pos.vec[2] <= boxpos.vec[2] + this.BoxSize)).ToList();
                        var ids = new List<int>();//boxStars.Select(p => p.id).ToList();

                        var thisBox = new Box(i++, boxpos, new Vector(x, y, z), this.BoxSize, this.MassLayer, ids, true);
                        this.Boxes[level][x, y, z] = thisBox;
                        this.MassLayer.Add(thisBox);
                    }

            for (level = 0; level < BoxLevels; level++)
            {
                var max = (int)Math.Pow(2, BoxLevels - level - 1);
                this.Boxes[level + 1] = new Box[max, max, max];
                for (var x = 0; x < max; x++)
                    for (var y = 0; y < max; y++)
                        for (var z = 0; z < max; z++)
                        {
                            var boxpos = new Vector(
                                (x - max / 2) * this.BoxSize * Math.Pow(2, level),
                                (y - max / 2) * this.BoxSize * Math.Pow(2, level),
                                (z - max / 2) * this.BoxSize * Math.Pow(2, level));
                            var vecEqual = new Vector(x, y, z);
                            var ids = new List<int>();
                            foreach (var b in this.Boxes[level]) if ((b.PosId / 2).Floor() == vecEqual) ids.Add(b.id);
                            var tempbox = new Box(
                                i++,
                                boxpos,
                                new Vector(x, y, z),
                                this.BoxSize * Math.Pow(2, level),
                                this.MassLayer,
                                ids);
                            this.Boxes[level + 1][x, y, z] = tempbox;
                            this.MassLayer.Add(tempbox);
                        }
            }

            this.GenerateInstructions();
        }

        

        private void GenerateInstructions()
        {
            Console.WriteLine("Generate Instructions");
            this.Instructions = new List<int>[this.Stars.Count];
            var temp = new List<Box>();
            var UpperTemp = new List<Box>();
            foreach (var b in this.Boxes[0])
            {
                // For each level 0 Box
                //if (b.ids.Count == 0) // Skip empty Boxes
                    //continue;
                temp = new List<Box>(); // initialize temp
                UpperTemp = new List<Box>(); // initialize Upper temp

                var Boxids = new List<int>[BoxLevels + 1]; // create box instruction array
                Boxids[0] = new List<int>();

                temp.Add(b); // Add Box as initial seed
                for (var level = 0; level < BoxLevels; level++)
                {
                    // for all levels
                    // doesn't work
                    Boxids[level + 1] = new List<int>(); // initialize id array
                    if (level != 0 || true)
                    {
                        var oldtemp = new List<Box>(temp); // dublicate list  to prevent enumeration failures
                        temp.Clear(); // clear Temp
                        foreach (var c in oldtemp) // add level-1 ids of surrounding level
                            foreach (var t in this.Boxes[level]) // foreach Box in same layer
                                if (!temp.Exists(x => x.id == t.id) && !oldtemp.Exists(x => x.id == t.id) && t.mass != 0
                                ) // if boxes dont already exst an are Neighbors
                                    if (t.PosId.IsNeighbour(c.PosId))
                                    {
                                        if (!c.Neighbours.Contains(t))
                                            c.Neighbours.Add(t);
                                        Boxids[level].AddRange(t.ids); // add ids of surrounding Boxes
                                        temp.Add(t); // add box to current layer selected
                                    }
                    }

                    // propably working
                    foreach (var t in temp)
                    {
                        // add level-1 ids to completet level+1 of existing
                        var pos = (t.PosId / 2).Floor();
                        var Upper = this.Boxes[level + 1][(int)pos.vec[0], (int)pos.vec[1], (int)pos.vec[2]];
                        Boxids[level + 1].AddRange(
                            Upper.ids.Where(x => !temp.Exists(y => y.id == x) && !Boxids[level + 1].Contains(x)));

                        // if (!UpperTemp.Exists(x=>x.id==Upper.id))
                        UpperTemp.Add(Upper);
                    }

                    temp.Clear();
                    temp.AddRange(UpperTemp);
                    UpperTemp.Clear();
                }

                var topLevelCalcIds = new List<int>();

                for (int i = 1; i < Boxes.Length; i++)
                {
                    topLevelCalcIds.AddRange(new List<int>(Boxids[i]));
                }

                b.Calcids = topLevelCalcIds;
                if (b.root)
                {
                    foreach (int id in b.ids)
                    {
                        this.Instructions[id] = new List<int>();
                        foreach (var list in Boxids)
                            this.Instructions[id].AddRange(list.Where(x => x != id && x != b.id && MassLayer[x].mass != 0));
                        Instructions[id].Sort();
                        // Instructions[id].RemoveAll(x=>x==b.id);
                    }
                }
            }
        }

        private void RefreshInstruction()
        {

            Console.WriteLine("Refresh Instructions");

            foreach (List<int> i in Instructions)
                i.Clear();

            foreach (Box b in Boxes[0])
            {
                if (b.ids.Count == 0)
                {
                    continue;
                }
                foreach (int id in b.ids)
                {
                    var temp = new List<int>();

                    temp.AddRange(b.ids);

                    foreach (var n in b.Neighbours)
                    {
                        foreach (var ids in n.ids)
                            if(ids!=b.id)
                                temp.Add(ids); //(n.Neighbours.Select(x=>x.ids));
                    }

                    temp.AddRange(b.Calcids);

                    Instructions[id].AddRange(temp.Except(new int[] { id,b.id }).Where(x=>MassLayer[x].mass!=0));
                }
            }
        }

        private void RefreshBoxes()
        {
            Console.WriteLine("Refresh Boxes");
            this.MassLayer.Clear();
            this.MassLayer.AddRange(this.Stars);
            var halfcount = (int)Math.Pow(2, BoxLevels);
            Vector temp;

            foreach (var b in Boxes[0])
            {
                b.ids.Clear();
            }

            foreach (var s in this.Stars)
            {
                temp = (s.pos / this.BoxSize).Floor() + halfcount / 2;
                this.Boxes[0][temp.vec[0] < 0 ? 0 : temp.vec[0] > halfcount - 1 ? halfcount - 1 : (int)temp.vec[0],
                    temp.vec[1] < 0 ? 0 : temp.vec[1] > halfcount - 1 ? halfcount - 1 : (int)temp.vec[1],
                    temp.vec[2] < 0 ? 0 : temp.vec[2] > halfcount - 1 ? halfcount - 1 : (int)temp.vec[2]].ids.Add(s.id);
            }

            for (var j = 0; j < this.Boxes.Length; j++)
                foreach (var b in this.Boxes[j])
                {
                    b.refresh(MassLayer);
                    b.Calc();
                    this.MassLayer.Add(b);
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
            this.MassLayer.OrderBy(x => x.id);

            if(Instructions.Contains(null))
                GenerateInstructions();
            else
                RefreshInstruction();
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