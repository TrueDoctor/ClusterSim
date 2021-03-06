﻿namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using global::ClusterSim.ClusterLib.Utility;

    public class Cluster : ICluster
    {
        // fields
        public const double Gravitation = 0.0002959122083; // 0.0000000000667384;//gravitation constant in AE^3 / Sun-mass * Day^2

        public Cluster(List<Star> stars, double dt = 1) : this(dt)
        {
            this.Stars = stars;
            this.ComputeCount = this.GetComputeCount();

            if (this.Stars == null)
            {
                throw new ArgumentException("SternListe null");
            }
        }

        public Cluster(double dt = 1)
        {
            // constructor 
            this.Dt = dt;
            this.ParentDt = dt;
            if (this.Stars == null)
            {
                this.Stars = new List<Star>();
            }
        }

        public List<int>[] Instructions { get; set; } 

        public List<Star> Stars { get; set; }

        public double Dt { get; set; } // delta time

        public double ParentDt { get; set; }

        public double CalculationComplexity { get; set; }

        public int ComputeCount { get; set; }

        public int RecursionLevel { get; set; } = 0;

        public List<IMassive> MassLayer { get; set; } = new List<IMassive>();

        protected bool SkipInstructionRefresh { get; set; } = false;

        public Star[] DoStep(Misc.Method m, bool multiThreading, int min, int max)
        {
            max = max == -1 ? this.Stars.Count - 1 : max;
            
            for (int i = min; i < max + 1; i++)
            {
                this.Stars[i].ToCompute = true;
            }

            return this.DoStep(m, multiThreading);
        }

        public virtual Star[] DoStep(Misc.Method m, bool multiThreading, bool forceCluster = true)
        {
            var ids = (from star in this.Stars where star.ToCompute select star.id).ToList();
            
            this.ComputeCount = ids.Count;

            this.Instructions = new List<int>[this.Stars.Count];
            
            this.Dt = this.Dt < this.ParentDt ? this.Dt : this.ParentDt;

            this.SkipInstructionRefresh = false;

            for (double time = 0; this.ParentDt > time; time += this.Dt)
            {
                this.MassLayer.Clear();

                foreach (var s in this.Stars)
                {
                    s.Computed = false; // reset computation status
                    this.MassLayer.Add(s);
                }
                
                this.CalcBoxes();
                if (multiThreading)
                {
                    Parallel.ForEach(ids, i => this.Integrate(i, Misc.Method.Rk5));
                }
                else
                {
                    foreach (int id in ids)
                    {
                        this.Integrate(id, Misc.Method.Rk5);
                    }
                }

                if (!this.SkipInstructionRefresh && this.ComputeCount < BoxCluster.BoxPivot)
                {
                    this.ReplaceInstructions();
                    this.SkipInstructionRefresh = true;
                }

                this.Dt = this.GetNewDt();
                if (time + this.Dt > this.ParentDt)
                {
                    this.Dt = this.ParentDt - time;
                }
            }

            this.Dt = this.GetNewDt();

            this.SkipInstructionRefresh = false;
            if (this.Stars.Exists(x => (x.ToCompute && !x.Computed) || (!x.ToCompute && x.Computed)))
            {
                throw new Exception("nicht alle Sterne berechnet");
            }

            this.Stars = this.Stars.OrderBy(star => star.id).ToList();
            return this.Stars.Where(x => x.Computed && x.ToCompute).ToArray();
        }

        public Vector CalcAcc(Vector a, IMassive b, double mass)
        {
            var tempDirection = a - b.pos;

            /*if (a == b.pos)
            {
                throw new DivideByZeroException();
            }*/

            double d = 1 / tempDirection.distance(); // Sterne und Weltraum Grundlagen der Himmelsmechanik S.91

            double acceleration = b.mass * Gravitation * Math.Pow(d, 3);

            acceleration = b.mass / mass * acceleration;
            var accVec = b.pos - a;

            accVec.mult(acceleration);
            return accVec;
        }

        protected double GetNewDt()
        {
            var maxDAcc = this.Stars.Where(x => x.ToCompute).OrderBy(x => x.Dt / x.DAcc).First();
            double dt = this.Dt;
            if (!(maxDAcc.DAcc > 0))
            {
                return dt;
            }

            dt = SubCluster.CalcRequiredDt(maxDAcc);

            if (dt > this.ParentDt)
            {
                dt = this.ParentDt;
            }

            return dt;
        }

        protected virtual void Integrate(int id, Misc.Method method)
        {
            var s = this.Stars[id];

            try
            {
                this.GetInstruction(s);

                s.Computed = true;

                var oldAcc = new Vector(s.Acc);

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

                s.Dt = this.Dt;

                if (!oldAcc.IsNull())
                {
                    var change = (oldAcc - s.Acc).distance();
                    var old = oldAcc.distance();
                    s.DAcc = change / old;
                }
            }
            catch (DivideByZeroException)
            {
                s.dead = true;
            }
        }
        
        protected virtual void GetInstruction(Star s, bool forceBox = false)
        {
            if (this.SkipInstructionRefresh)
            {
                return;
            }

            var tempInst = new List<int>();
            for (int i = 0; i < this.Stars.Count; i++)
            {
                if (i != s.id)
                {
                    tempInst.Add(i);
                }
            }

            this.Instructions[s.id] = tempInst.ToList();
        }

        protected virtual void CalcBoxes(bool forceBoxes = false)
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

        protected int GetComputeCount()
        {
            return this.Stars.Count(x => x.ToCompute);
        }

        protected virtual void ReplaceInstructions()
        {
        }
        
        private Vec6 F(Vec6 star, int id)
        {
            var acc = new Vector();
            for (var j = 0; j < this.Instructions[id].Count; j++)
            {
                int temp = this.Instructions[id][j];
                if (!this.MassLayer[temp].dead && !this.MassLayer[id].dead && temp != id)
                {
                    acc.add(this.CalcAcc(star.ToVector(0), this.MassLayer[temp], this.MassLayer[id].mass)); // add all acceleration vectors
                }
            }

            return new Vec6(star.ToVector(1), acc);
        }
    }
}