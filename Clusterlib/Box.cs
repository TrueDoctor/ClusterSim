using System.Collections.Generic;

namespace ClusterSim.ClusterLib
{
    class Box : IMassive
    {
        Vector Position,  AvgPos;
        public Vector PosId, Dimension;
        List<IMassive> objects = new List<IMassive>();
        public List<int> Calcids;
        public List<Box> Neighbours { get; set; }= new List<Box>();
        public List<int> ids;
        public int id { get; set; }
        public bool dead { get; set; } = false;
        public bool root;
        public double mass { get; set; }
        public double size { get; set; }
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
            this.size = Dim;
            this.ids = ids;
            //pos.init();
            Calc();
            this.root = root;
        }
        public void refresh(List<IMassive> Objects)
        {
            objects.Clear();
            foreach (int i in this.ids)
                objects.Add(Objects.Find(x => x.id == i));
        }

        public void Calc()
        {
            pos = Position + (Dimension / 2);
            var temp = new Vector();
            mass = 0;

            #region Avg 
            if (ids.Count != 0)
            {
                 
                objects.ForEach(x => mass += x.mass);
                
                foreach (IMassive m in objects)
                {
                    if (m.mass == 0)
                        continue;
                    temp += (m.mass/mass) * (m.pos-pos);
                }
                pos += temp;
                //if (mass!=0)
                  //  pos.div(mass);
            }
            #endregion
        }
    }

    
}
