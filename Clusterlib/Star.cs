using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;

namespace ClusterLib
{
    public class Star 
    {

        //declare fields/members
        public Vector pos = new Vector();
        public Vector vel = new Vector();
        private decimal mass;
        public int id;
        public bool computed = false;

        public Star() { }//empty constructor

        public Star(int id) { this.id = id; pos.init();vel.init(); mass = 0; } //constructor Create a initialized star from id

        public Star(decimal[] pos, decimal[] vel,   //constructor 
            decimal mass,int id=-1)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;            
            this.mass = mass;
            this.id = id;
        }

        public Star(Vector pos, Vector vel,   //constructor
            decimal mass, int id, decimal dt = 2)
        {
            this.pos = pos;
            this.vel = vel;
            this.mass = mass;
            this.id = id;
        }

        public Star(Star s)//constructor from given Star
        {
            this.pos = s.pos;
            this.vel = s.vel;
            this.id = s.id;
            this.mass = s.mass;
        }

        public Star(Vec6 vec,decimal mass,int id=-1)//create Star from Vec6
        {
            this.pos = new Vector(vec,0);
            this.vel = new Vector(vec,1);
            this.id = id;
            this.mass = mass;
        }

        
        public decimal getMass() //return mass
        {
            return mass;
        }

        public decimal getRelativMass() //return mass
        {
            try
            {
                return (decimal)Math.Sqrt(Convert.ToDouble(mass) / (1.0 - Math.Pow((double)(this.vel.distance() / Misc.c), 2)));
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

            Star clone = new Star(this.pos, this.vel, this.mass, this.id);
            
            return clone;
        }

        ~Star() { } //destructor
    }
}
