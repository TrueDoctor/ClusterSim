namespace ClusterSim.ClusterLib.Calculation
{
    using System;

    using ClusterSim.ClusterLib.Utility;

    [Serializable]
    public class Star : IMassive
    {
        private Vector acc = new Vector();

        private int Id;

        // declare fields/members
        private Vector pos = new Vector();

        private Vector vel = new Vector();

        public Star()
        {
        } // empty constructor

        public Star(int id)
        {
            this.id = id;
            this.Pos.init();
            this.Vel.init();
            this.Mass = 0;
        } // constructor Create a initialized star from id

        public Star(
            double[] pos,
            double[] vel, // constructor 
            double mass,
            int id = -1,
            bool dead = false)
        {
            this.Pos.vec = pos;
            this.Vel.vec = vel;
            this.Mass = mass;
            this.id = id;
            this.dead = dead;
        }

        public Star(
            Vector pos,
            Vector vel, // constructor
            double mass,
            int id,
            bool dead = false,
            double dt = 2)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Mass = mass;
            this.id = id;
            this.dead = dead;
        }

        public Star(Star s)
        {
            // constructor from given Star
            this.Pos = s.Pos;
            this.Vel = s.Vel;
            this.id = s.id;
            this.Mass = s.Mass;
        }

        public Star(Vec6 vec, double mass, int id = -1)
        {
            // create Star from Vec6
            this.Pos = new Vector(vec, 0);
            this.Vel = new Vector(vec, 1);
            this.id = id;
            this.Mass = mass;
        }

        public bool dead { get; set; }

        public bool toClaculate

        public bool Computed { get; set; } = false;

        public Vector Acc { get; set; } = new Vector();

        public double DAcc { get; set; } = 0;

        public int id
        {
            get => this.Id;
            set
            {
                if (value < 0)
                {
                    this.dead = true;
                    this.Id = -(value + 1);
                }
                else
                {
                    this.Id = value;
                }
            }
        }

        double IMassive.mass
        {
            get => this.Mass;
            set => this.Mass = value;
        }

        Vector IMassive.pos
        {
            get => this.Pos;
            set => this.Pos = value;
        }

        public double Mass { get; set; }

        public Vector Pos
        {
            get => this.pos;
            set => this.pos = value;
        }

        public Vector Vel
        {
            get => this.vel;
            set => this.vel = value;
        }

        public Star Clone()
        {
            // clone method to prevent shallow copys
            var clone = new Star(this.Pos, this.Vel, this.Mass, this.id, this.dead) { DAcc = this.DAcc, Acc = this.Acc };

            return clone;
        }

        public double GetMass()
        {
            // return mass
            return this.Mass;
        }

        public double GetRelativeMass()
        {
            // return mass
            try
            {
                return Math.Sqrt(Convert.ToDouble(this.Mass) / (1.0 - Math.Pow(this.Vel.distance() / Misc.c, 2)));
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "\n\n\n\n\n\nGeschwingkeit von Stern {0} größer als Lichtgeschwindigkeit.\n kleinere Zeitschritte wählen!\n\n\n\n\n\n\n{1}\n\n",
                    this.id,
                    e.Message);
                return 7922816251426433;
            }
        }

        public void Print()
        {
            // print all fields of the star in the console
            Console.Write(this.id + ", ");

            // Console.WriteLine("pos: " + pos.toString());
            // Console.WriteLine("vel: " + vel.toString());
            // Console.WriteLine("mass: " + mass);
        }

        public byte[] Serialize()
        {
            var temp = new byte[92];
            Array.Copy(this.Pos.Serialize(), 0, temp, 0, 24);
            Array.Copy(this.Vel.Serialize(), 0, temp, 24, 24);
            Array.Copy(this.Acc.Serialize(), 0, temp, 48, 24);
            Array.Copy(BitConverter.GetBytes(this.Mass), 0, temp, 72, 8);
            Array.Copy(BitConverter.GetBytes(this.DAcc), 0, temp, 80, 8);
            Array.Copy(BitConverter.GetBytes(this.dead ? -(this.id + 1) : this.id), 0, temp, 88, 4);
            return temp;
        }

        public Star Deserialize(byte[] input)
        {
            var vec = new byte[24];
            Array.Copy(input, vec, 24);
            this.Pos.Deserialize(vec);
            Array.Copy(input, 24, vec, 0, 24);
            this.Vel.Deserialize(vec);
            Array.Copy(input, 48, vec, 0, 24);
            this.Vel.Deserialize(vec);
            this.Mass = BitConverter.ToDouble(input, 72);
            this.DAcc = BitConverter.ToDouble(input, 80);
            this.id = BitConverter.ToInt32(input, 88);
            return this;
        }

        public string ToCsv()
        {
            // print all fields of the star in the console
            return this.Pos.vec[0] + ";" + this.Pos.vec[1] + ";" + this.Pos.vec[2];
        }

        public string ToTsv()
        {
            // print all fields of the star in the console
            return this.Pos.vec[0] + "   " + this.Pos.vec[1] + "  " + this.Pos.vec[2];
        }
    }
}