﻿using System.Collections.Generic;

namespace ClusterSim.ClusterLib
{
    class Box : IMassive
    {
        Vector Position, Dimension, AvgPos;
        public Vector PosId;
        List<IMassive> objects = new List<IMassive>();
        public List<int>[] Calcids;
        public List<int> ids;
        public int id { get; set; }
        public bool dead { get; set; } = false;
        public bool root;
        public double mass { get; set; }
        public Vector pos
        {
            get => AvgPos;
            set => AvgPos = value;
        }


        public Box(int id,Vector Pos,Vector PosId, double Dim, List<IMassive> Objects, List<int> ids,bool root = false)
        {
            this.id = id;
            foreach (int i in ids)
                objects.Add(Objects.Find(x=>x.id == i));
            Position = Pos;
            this.PosId = PosId;
            AvgPos = Dimension = new Vector();
            Dimension.init(Dim);
            this.ids = ids;
            pos.init();
            Calc();
            this.root = root;
        }
        public void refresh(ref List<IMassive> Objects, List<int> ids)
        {
            objects.Clear();
            foreach (int i in ids)
                objects.Add(Objects.Find(x => x.id == i));
        }

        public void Calc()
        {
            #region Avg 
            if (ids.Count != 0)
            {
                foreach (IMassive m in objects)
                {
                    pos += m.mass * m.pos;
                    mass += m.mass;
                }
                if (mass!=0)
                    pos.div(mass);
            }
            #endregion
        }
    }

    
}
