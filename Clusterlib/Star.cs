using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;

namespace ClusterSim.ClusterLib
{
    [Serializable]
    public class Star 
    {

        //declare fields/members
        public Vector pos = new Vector();
        public Vector vel = new Vector();
        private double mass;
        private int Id;
        public bool computed = false,dead=false;
        
        public int id
        {
            get { return Id; }
            set
            {
                if (value < 0)
                { dead = true; Id = value == Int32.MinValue ? 0 : -value; }
                else
                    Id = value;

            }
        }

        public Star() { }//empty constructor

        public Star(int id) { this.id = id; pos.init();vel.init(); mass = 0; } //constructor Create a initialized star from id

        public Star(double[] pos, double[] vel,   //constructor 
            double mass,int id=-1,bool dead = false)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;            
            this.mass = mass;
            this.id = id;
            this.dead = dead;
        }

        public Star(Vector pos, Vector vel,   //constructor
            double mass, int id, bool dead = false, double dt = 2)
        {
            this.pos = pos;
            this.vel = vel;
            this.mass = mass;
            this.id = id;
            this.dead = dead;
        }

        public Star(Star s)//constructor from given Star
        {
            this.pos = s.pos;
            this.vel = s.vel;
            this.id = s.id;
            this.mass = s.mass;
        }

        public Star(Vec6 vec,double mass,int id=-1)//create Star from Vec6
        {
            this.pos = new Vector(vec,0);
            this.vel = new Vector(vec,1);
            this.id = id;
            this.mass = mass;
        }

        
        public double getMass() //return mass
        {
            return mass;
        }

        public double getRelativMass() //return mass
        {
            try
            {
                return (double)Math.Sqrt(Convert.ToDouble(mass) / (1.0 - Math.Pow((double)(this.vel.distance() / Misc.c), 2)));
            }
            catch(Exception e)
            {
                Console.WriteLine("\n\n\n\n\n\nGeschwingkeit von Stern {0} größer als Lichtgeschwindigkeit.\n kleinere Zeitschritte wählen!\n\n\n\n\n\n\n{1}\n\n",this.id,e.Message);
                return 7922816251426433;
            }
        }

        public string toCsv() //print all fields of the star in the console
        {
            return pos.vec[0] + ";" + pos.vec[1] + ";" + pos.vec[2];
        }

        public string toTsv() //print all fields of the star in the console
        {
            return pos.vec[0] + "   " + pos.vec[1] + "  " + pos.vec[2];
        }

        public void print() //print all fields of the star in the console
        {
            Console.Write(this.id+", ");
            //Console.WriteLine("pos: " + pos.toString());
            //Console.WriteLine("vel: " + vel.toString());
            //Console.WriteLine("mass: " + mass);
        }

        public Star Clone()//clone method to prevent shallow copys
        {

            Star clone = new Star(this.pos, this.vel, this.mass, this.id,this.dead);
            
            return clone;
        }

        public byte[] Serialize()
        {
            var temp = new byte[60];
            Array.Copy(pos.Serialize(), 0, temp, 0, 24);
            Array.Copy(vel.Serialize(), 0, temp, 24, 24);
            Array.Copy(BitConverter.GetBytes(mass), 0, temp, 48, 8);
            Array.Copy(BitConverter.GetBytes(dead?id==0?Int32.MinValue:-id:id), 0, temp, 56, 4);
            return temp;
        }

        public Star Deserialize(byte[] input)
        {
            var vec = new byte[24];
            Array.Copy(input, vec, 24);
            pos.Deserialize(vec);
            Array.Copy(input,24, vec,0, 24);
            vel.Deserialize(vec);
            mass = BitConverter.ToDouble(input, 48);
            id = BitConverter.ToInt32(input, 56);
            return this; 
        }
        
    }
}
