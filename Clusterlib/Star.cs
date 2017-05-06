using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;

namespace ClusterLib
{
    [Serializable()]
    public class Star 
    {

        //declare fields/members
        public Vector pos = new Vector();
        public Vector vel = new Vector();
        private double mass;
        public int id;
        private static int classId=0;
        public bool computed = false;

        public Star() { }

        public Star(int id) { this.id = id; pos.init();vel.init(); mass = 0; }
        public Star(double[] pos, double[] vel,   //constructor
            double mass,int id=-1)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;            
            this.mass = mass;
            if (id == -1)
                this.id = classId++;
            else this.id = id;
        }

        public Star(Vector pos, Vector vel,   //constructor
            double mass, int id, double dt = 2)
        {
            this.pos = pos;
            this.vel = vel;
            this.mass = mass;
            this.id = id;
        }

        public Star(Star s)
        {
            this.pos = s.pos;
            this.vel = s.vel;
            this.id = s.id;
            this.mass = s.mass;
        }

        public Star(Vec6 vec,double mass,int id=-1)
        {
            this.pos = new Vector(vec,0);
            this.vel = new Vector(vec,1);
            this.id = id;
            this.mass = mass;
        }

        
        public double getMass() //get mass
        {
            return mass;
        }

        

        
        

        public void print() //print all fields of the star in the console
        {
            Console.WriteLine(this.id+"\n");
            //Console.WriteLine("pos: " + pos.toString());
            ///Console.WriteLine("vel: " + vel.toString());
            //Console.WriteLine("mass: " + mass);
        }

        public Star Clone()
        {

            Star clone = new Star(this.pos, this.vel, this.mass, this.id);
            
            return clone;
        }

        ~Star() { } //destructor
    }
}
