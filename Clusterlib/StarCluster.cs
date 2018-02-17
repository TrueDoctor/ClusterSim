

//using Newtonsoft.Json.Converters; 
namespace ClusterSim.ClusterLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class StarCluster
    {
        private const int BoxLevels = 4;
        private double boxCoefficient;

        // fields
        public const double Gravitation = 0.0002959122083
            ; // 0.0000000000667384;//gravitation constant in AE^3 /Sunmass*Day^2

        public List<Thread> Savethreads = new List<Thread>();

        public List<Star> Stars; // Array of Stars

        private List<Box> boxes = new List<Box>();

        private double boxSize;

        public double Dt; // delta time

        private List<int>[] instructions;

        private readonly List<IMassive> massLayer = new List<IMassive>();

        private List<Star> oldStars; // While calculating a step the same values have to be used

        private readonly string rtable; // table to read from

        private int starCount;

        private int start; // starting step

        private readonly List<List<Star>> steps = new List<List<Star>>(); // list of Clusters stored in ram

        private readonly string wtable; // table to write at

        public StarCluster(string rtable, string wtable, int start, double dt = 1, double coe = 0.4)
        {
            // constructor 
            this.starCount = SQL.starsCount(rtable);
            this.boxCoefficient = coe;
            this.rtable = rtable;
            this.wtable = wtable;
            this.start = start;
            this.Dt = dt;
            this.Stars = new List<Star>();

            this.Stars = SQL.readStars(rtable, start); // initialize
            this.boxSize = (this.Stars.Max(x => x.Pos.vec.Max()) - this.Stars.Min(x => x.Pos.vec.Min()))
                           / Math.Pow(2, BoxLevels); // calc low level box sizes
        }

        public StarCluster(int count, double dt = 1, double coe = 0.4)
        {
            // constructor 
            this.Dt = dt;
            this.boxCoefficient = coe;
            this.Stars = new List<Star>(count);

            //this.Stars = null; // initialize
        }
        
        public Star[] DoStep(int step, int min, int max, Misc.Method m)
        {
            this.CalcBoxes();
            this.instructions = new List<int>[this.Stars.Count];
            this.starCount = this.Stars.Count;

            //Console.WriteLine("Calc Forces");

            if (this.Stars == null) return null;
            foreach (var s in this.Stars) // reset computation status
                s.computed = false;


            int processors = Environment.ProcessorCount; // get number of processors
            int perCore = (max - min + 1) / processors; // divide the cluster in equal parts
            int left = (max - min + 1) % processors; // calc remainder
            var threads = new List<Thread>();

            this.oldStars = new List<Star>(); // save the current values
            foreach (var s in this.Stars) // clone each to prevent shallow copys
                this.oldStars.Add(s.Clone());


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

                Console.WriteLine($"start: {this.start}");

                threads.Add(
                    new Thread(
                        delegate()
                            {
                                this.Integrate(start, this.starCount - 1, m);
                            })); 

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

        public void Export(List<Star> data, int step, string table)
        {
            // XDMessagingClient client = new XDMessagingClient(); //https://github.com/TheCodeKing/XDMessaging.Net
            // IXDBroadcaster broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
            int i = step;
            foreach (var s in data) while (SQL.addRow(s, i, table) == false) ; // do until succesfull

            // Console.WriteLine("Step: "+i+" id: "+s.id);
            // broadcaster.SendToChannel("steps", "i"+i);

            // }
        }

        public void Initialvel(int id, double mass)
        {
            var rand = new Random();
            var acc = new Vector();

            foreach (var s in this.Stars)
                if (s.id != this.Stars[id].id)
                    acc.add(this.Calcacc(this.Stars[id].Pos, s, id)); // add all acceleration vectors

            double bacc = acc.distance(); // magnitude|x| of the vector
            double v = Math.Sqrt(
                bacc * Math.Sqrt(
                    Gravitation * this.Stars[id].GetMass()
                    / bacc)); // Velocity=Squareroot(|acc|*r) r=Squareroot((G*m)/|acc|)

            double x1 = 2 * rand.NextDouble() - 1; // x1 = random -1,1
            double x2 = 2 * rand.NextDouble() - 1; // x2 = random -1,1
            double
                x3 = (x1 * acc.vec[0] + x2 * acc.vec[1])
                     / -acc.vec[2]; // x3= (x1*acc1)/-acc3 + (x2*acc2)/acc3 generate orhogonal vector by using the dot product

            var vel = new Vector(x1, x2, x3);
            double skalar = vel.skalar(acc);
            this.Stars[id].Vel.add(vel.scale(v)); // scale vector to match the V magnitude and add to the random variance
        }

        private void Integrate(int cstart, int end, Misc.Method method)
        {
            // as RK4
            //Thread.CurrentThread.Name = "working";
            /*int i = 0;
            try
            {
                bool ready;
                //do
                //{*/
            // while (!(cstart < Stars.Count && cstart < OldStars.Count))
            // cstart = 0;
            Console.WriteLine($"cstart: {cstart}");

            for (int i = cstart; i <= end; i++)
            {
                if (end == this.Stars.Count) return;

                var s = this.Stars[i];
                if (!s.computed)
                {
                    s.computed = true;
                    if (!s.dead)
                        try
                        {
                            var tempInst = new ConcurrentStack<int>();
                            GetInstruction(s.Pos, s.id, this.boxes[0], ref tempInst);
                            this.instructions[s.id] = tempInst.ToList();

                            switch (method)
                            {
                                case Misc.Method.Rk4:
                                    this.Rk4(ref s);
                                    break;

                                case Misc.Method.Rk5:
                                    this.Rk5(ref s);
                                    break;
                            }

                            s.Print();
                        }
                        catch (DivideByZeroException)
                        {
                            s.dead = true;
                        }
                }
            }
        }

        private Star Rk4(ref Star s)
        {
            var star = new Vec6(this.oldStars[s.id].Pos, this.oldStars[s.id].Vel); // convert star to vec6
            var ka = this.F(star, s.id); // calculate help values
            var kb = this.F(star + this.Dt / 2 * ka, s.id);
            var kc = this.F(star + this.Dt / 2 * kb, s.id);
            var kd = this.F(star + kc, s.id);
            var f = this.Dt / 6 * (ka + 2 * kb + 2 * kc + kd); // calculate resulting Vector
            s.Pos = s.Pos + f.ToVector(0);
            s.Vel = s.Vel + f.ToVector(1);
            s.Print();
            return s;
        }

        private Star Rk5(ref Star s)
        {
            var star = new Vec6(this.oldStars[s.id].Pos, this.oldStars[s.id].Vel);
            var ka = this.Dt * this.F(star, s.id);
            var ff = this.Dt * this.F(ka, s.id);
            var kb = this.Dt * this.F(star + 1.0 / 3 * ka + 1.0 / 18 * ff, s.id);
            var kc = this.Dt * this.F(star - 1.216 * ka + 252.0 / 125 * kb - 44.0 / 125 * ff, s.id);
            var kd = this.Dt * this.F(
                         star + 9.5 * ka - 72.0 / 7 * kb + 25.0 / 14 * kc + 44.0 / 125 * ff,
                         s.id);

            var f = 5.0 / 48 * ka + 27.0 / 56 * kb + 125.0 / 336 * kc + 1.0 / 24 * kd;

            s.Pos += f.ToVector(0);
            s.Vel += f.ToVector(1);
            return s;
        }

        private Star Euler(ref Star s)
        {
            var star = new Vec6(this.oldStars[s.id].Pos, this.oldStars[s.id].Vel);
            var f = this.Dt * this.F(star, s.id);
            s.Pos += f.ToVector(0);
            s.Vel += f.ToVector(1);
            return s;
        }

        private Vec6 F(Vec6 star, int id)
        {
            var acc = new Vector();
            /*for (int j = 0; j < Stars.Count; j++)
                if (id != Stars[j].id && !Stars[j].dead)//no self intersection to prevent dividing by 0
                    acc.add(calcacc(Star.ToVector(0), Stars[j], id));//add all acceleration vectors
                                                                     //*/
            for (var j = 0; j < this.instructions[id].Count; j++)
            {
                int temp = this.instructions[id][j];
                if (!this.massLayer[temp].dead && !this.massLayer[id].dead && this.massLayer[temp].mass != 0
                    && temp != id)
                {
                    // no self intersection to prevent dividing by 0
                    acc.add(this.Calcacc(star.ToVector(0), this.massLayer[temp], id, temp)); // add all acceleration vectors
                }
                else if (this.massLayer[temp].mass != 0)
                {
                } //m67s tow layers up, get added to instructions
            }//*/

            /*                                                         for (int j = 0; j < Boxes.Count; j++)
                                                                         if (id != Boxes[j].id)//no self intersection to prevent dividing by 0
                                                                             acc.add(calcacc(Star.ToVector(0), Boxes[j], id));//add all acceleration vectors
                                                                             //*/
            return new Vec6(star.ToVector(1), acc);
        }

        public Vector Calcacc(Vector a, IMassive b, int id = -1, int bid = -1, double mass = 0)
        {
            var tempDirection = a - b.pos;

            if (a == b.pos)
            {
                /*foreach (Box bs in Boxes.Where(x=Y))
                    if (bs.ids.Contains(4548))
                        Console.WriteLine(bs.id);*/
                throw new DivideByZeroException();
            }

            double d = 1 / tempDirection.distance(); // Sterne und Weltraum Grundlagen der Himmelsmechanik S.91

            double acceleration = b.mass * Gravitation * Math.Pow(d, 3);
            if (mass == 0)
            {
                mass = this.Stars[id].GetMass();
            }

            acceleration = b.mass / mass * acceleration;
            var accVec = b.pos - a;
            /*if (Double.IsNaN(acceleration))
                ;*/
            accVec.mult(acceleration);
            return accVec;
        }

        private void CalcBoxes()
        {
            //Console.WriteLine("Calc Boxes");
            this.boxSize = (this.Stars.Max(x => x.Pos.vec.Max()) - this.Stars.Min(x => x.Pos.vec.Min()));

            this.massLayer.Clear();
            this.boxes.Clear();
            foreach (var s in this.Stars) this.massLayer.Add(s);
            int id = this.Stars.Count;

            this.AddBox(ref this.boxes, ref id, new Vector().init(-this.boxSize / 2), this.boxSize, new List<IMassive>(this.Stars));

            //this.Boxes.OrderBy(x => x.id);

            this.boxes = this.boxes.OrderBy(x => x.id).ToList();
            this.massLayer.AddRange(this.boxes);

            //this.MassLayer.OrderBy(x => x.id);
        }

        private int AddBox(ref List<Box> boxes, ref int boxId, Vector pos, double size, IEnumerable<IMassive> stars)
        {
            stars = stars.ToList();
            if (stars.Count() == 1)
            {
                int tempId = boxId++;
                boxes.Add(new Box(tempId, pos / size, pos, size, stars, new List<int>() { stars.First().id }, true));
                return tempId;
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
                            {
                                tbox.ids.Add(this.AddBox(ref boxes, ref boxId, pos + size / 2 * new Vector(i, j, k), size / 2, z));
                            }

                            z = y.Except(z).ToList();
                        }
                        y = x.Except(y).ToList();
                    }
                    x = stars.Except(x).ToList();
                }
                var temp = (from box in boxes
                            join id in tbox.ids on box.id equals id
                            select box as IMassive);
                tbox.objects = temp.ToList();
                //boxes.Join(tbox.ids,v=>v.id,g=>g,(v,g)=>v);
                tbox.Calc();
                boxes.Add(tbox);
                return tbox.id;
            }
        }

        private void GetInstruction(Vector sPos, int sid, Box box, ref ConcurrentStack<int> ids)
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
                    ids.Push(box.id);
                    return;
                }


            if (box.size * box.size / (sPos - box.pos).distance2() < this.boxCoefficient || box.root)
            {
                ids.Push(box.id);
                return;
            }

            foreach (int id in box.ids)
            {
                if (this.boxes[id - this.Stars.Count].mass != 0)
                    GetInstruction(sPos, sid, this.boxes[id - this.Stars.Count], ref ids);
            }
        }
    }
}