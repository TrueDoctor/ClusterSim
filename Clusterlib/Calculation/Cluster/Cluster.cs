namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using global::ClusterSim.ClusterLib.Utility;

    public class Cluster
    {
        // fields
        public const double Gravitation = 0.0002959122083; // 0.0000000000667384;//gravitation constant in AE^3 / Sun-mass * Day^2

        protected readonly List<IMassive> MassLayer = new List<IMassive>();

        public Cluster(List<Star> stars, double dt = 1) : this(dt)
        {
            this.Stars = stars;

            if (this.Stars == null)
            {
                throw new ArgumentException("SternListe null");
            }
        }

        public Cluster(double dt = 1)
        {
            // constructor 
            this.Dt = dt;
        }

        public List<int>[] Instructions { get; set; }

        public List<Star> Stars { get; set; }

        public double Dt { get; set; } // delta time

        public Star[] DoStep(int min, int max, Misc.Method m)
        {
            if (this.Stars == null)
            {
                return null;
            }

            this.Instructions = new List<int>[this.Stars.Count];

            this.MassLayer.Clear();
            foreach (var s in this.Stars)
            {
                this.MassLayer.Add(s);
                s.Computed = false;
            }
            
            this.CalcBoxes();

            this.Integrate(min, max, m);
            
            return this.Stars.Where(x => x.Computed && x.id >= min && x.id <= max)
                .OrderBy(x => x.id).ToArray();
        }
        
        public Vector CalcAcc(Vector a, IMassive b, int id = -1, double mass = 0)
        {
            var tempDirection = a - b.pos;

            if (a == b.pos)
            {
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

            accVec.mult(acceleration);
            return accVec;
        }

        protected virtual void Integrate(int cStart, int end, Misc.Method method)
        {
            for (int i = cStart; i <= end; i++)
            {
                var s = this.Stars[i];

                if (s.Computed)
                {
                    continue;
                }

                s.Computed = true;
                if (s.dead)
                {
                    continue;
                }

                try
                {
                    this.GetInstruction(s);

                    var oldAcc = s.Acc;

                    switch (method)
                    {
                        case Misc.Method.Rk4:
                            this.Rk4(ref s);
                            break;

                        case Misc.Method.Rk5:
                            this.Rk5(ref s);
                            break;
                        case Misc.Method.Euler:
                            this.Euler(ref s);
                            break;
                    }

                    if (!oldAcc.IsNull())
                    {
                        var change = (oldAcc - s.Acc).distance();
                        var old = s.Acc.distance();
                        s.DAcc = change / old;
                    }
                }
                catch (DivideByZeroException)
                {
                    s.dead = true;
                }
            }
        }

        protected virtual void GetInstruction(Star s)
        {
            this.Instructions[s.id] = new List<int>();
            for (int i = 0; i < this.Stars.Count; i++)
            {
                if (i != s.id)
                {
                    this.Instructions[s.id].Add(i);
                }
            }
        }

        protected virtual void CalcBoxes()
        {
        }
        
        protected void Rk4(ref Star s)
        {
            var star = new Vec6(s.Pos, s.Vel); // convert star to vec6
            var ka = this.F(star, s.id); // calculate help values
            var kb = this.F(star + this.Dt / 2 * ka, s.id);
            var kc = this.F(star + this.Dt / 2 * kb, s.id);
            var kd = this.F(star + kc, s.id);
            var f = this.Dt / 6 * (ka + 2 * kb + 2 * kc + kd); // calculate resulting Vector
            s.Pos = s.Pos + f.ToVector(0);
            s.Vel = s.Vel + f.ToVector(1);
            s.Acc = ka.ToVector(1) / this.Dt;
            s.Print();
        }

        protected void Rk5(ref Star s)
        {
            var star = new Vec6(s.Pos, s.Vel);
            var ka = this.Dt * this.F(star, s.id);
            var ff = this.Dt * this.F(ka, s.id);
            var kb = this.Dt * this.F(star + 1.0 / 3 * ka + 1.0 / 18 * ff, s.id);
            var kc = this.Dt * this.F(star - 1.216 * ka + 252.0 / 125 * kb - 44.0 / 125 * ff, s.id);
            var kd = this.Dt * this.F(
                         star + 9.5 * ka - 72.0 / 7 * kb + 25.0 / 14 * kc + 44.0 / 125 * ff,
                         s.id);

            var f = 5.0 / 48 * ka + 27.0 / 56 * kb + 125.0 / 336 * kc + 1.0 / 24 * kd;
            s.Acc = ka.ToVector(1) / this.Dt;
            s.Pos += f.ToVector(0);
            s.Vel += f.ToVector(1);
        }

        protected void Euler(ref Star s)
        {
            var star = new Vec6(s.Pos, s.Vel);
            var f = this.Dt * this.F(star, s.id);
            s.Acc = f.ToVector(1) / this.Dt;
            s.Pos += f.ToVector(0);
            s.Vel += f.ToVector(1);
        }

        private Vec6 F(Vec6 star, int id)
        {
            var acc = new Vector();
            for (var j = 0; j < this.Instructions[id].Count; j++)
            {
                int temp = this.Instructions[id][j];
                if (!this.MassLayer[temp].dead && !this.MassLayer[id].dead && temp != id)
                {
                    acc.add(this.CalcAcc(star.ToVector(0), this.MassLayer[temp], id, temp)); // add all acceleration vectors
                }
            }

            return new Vec6(star.ToVector(1), acc);
        }
    }
}