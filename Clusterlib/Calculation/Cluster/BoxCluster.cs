namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    
    public class BoxCluster : Cluster, ICluster
    {
        // fields
        protected const int BoxPivot = 110;

        protected readonly double BoxCoefficient;

        private List<Box> boxes = new List<Box>();

        public BoxCluster(List<Star> stars, double dt = 1, double coe = 0.4) : this(dt, coe)
        {
            this.Stars = stars;

            if (this.Stars == null)
            {
                throw new ArgumentException("SternListe null");
            }
        }

        public BoxCluster(double boxCoefficient = 0.4, double dt = 1) : base(dt)
        {
            this.BoxCoefficient = boxCoefficient;
        }
        
        protected double BoxSize { get; set; }

        protected List<Box> Boxes { get => this.boxes; set => this.boxes = value; }

        protected override void CalcBoxes()
        {
            if (this.Stars.Count < BoxPivot)
            {
                base.CalcBoxes();
                return;
            }

            this.BoxSize = this.Stars.Max(x => x.Pos.vec.Max()) - this.Stars.Min(x => x.Pos.vec.Min());

            this.Boxes.Clear();

            int id = this.Stars.Count;

            this.AddBox(
                ref this.boxes,
                ref id,
                new Vector().init(-this.BoxSize / 2),
                this.BoxSize,
                new List<IMassive>(this.Stars));

            this.Boxes = this.Boxes.OrderBy(x => x.id).ToList();
            this.MassLayer.AddRange(this.Boxes);
        }
        
        protected override void GetInstruction(Star s)
        {
            if (this.Stars.Count < BoxPivot)
            {
                base.GetInstruction(s);
                return;
            }

            var tempInst = new ConcurrentBag<int>();
            this.GenerateInstruction(s.Pos, s.id, this.Boxes[0], ref tempInst);
            this.Instructions[s.id] = tempInst.ToList();
        }

        private int AddBox(ref List<Box> workBoxes, ref int boxId, Vector pos, double size, IEnumerable<IMassive> stars)
        {
            stars = stars.ToList();
            if (stars.Count() == 1)
            {
                int tempId = boxId++;
                workBoxes.Add(new Box(tempId, pos / size, pos, size, stars, new List<int> { stars.First().id }, true));
                return tempId;
            }

            var tBox = new Box(boxId++, pos, pos / size, size, new List<IMassive>(), new List<int>());
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
                            tBox.ids.Add(this.AddBox(ref workBoxes, ref boxId, pos + size / 2 * new Vector(new double[] { i, j, k }), size / 2, z));
                        }

                        z = y.Except(z).ToList();
                    }

                    y = x.Except(y).ToList();
                }

                x = stars.Except(x).ToList();
            }

            var temp = from box in workBoxes
                       join id in tBox.ids on box.id equals id
                       select box as IMassive;
            tBox.objects = temp.ToList();
            tBox.Calc();
            workBoxes.Add(tBox);
            return tBox.id;
        }

        private void GenerateInstruction(Vector sPos, int sid, Box box, ref ConcurrentBag<int> ids)
        {
            switch (box.ids.Count)
            {
                case 0:
                    return;
                case 1:
                    if (box.ids.Contains(sid) || Math.Abs(box.mass) < 1e-2000)
                    {
                        return;
                    }

                    if (box.root)
                    {
                        ids.Add(box.id);
                        return;
                    }

                    break;
            }

            if (box.size * box.size / (sPos - box.pos).distance2() < this.BoxCoefficient || box.root)
            {
                ids.Add(box.id);
                return;
            }

            foreach (int id in box.ids)
            {
                if (Math.Abs(this.Boxes[id - this.Stars.Count].mass) > 1e-320)
                {
                    this.GenerateInstruction(sPos, sid, this.Boxes[id - this.Stars.Count], ref ids);
                }
            }
        }
    }
}