using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;

namespace ClusterLib
{
    public class Star : ICloneable
    {

        //declare fields/members
        public Vector pos = new Vector();
        public Vector vel = new Vector();
        private decimal dt;
        private decimal mass;
        public int id;
        private static int classId=0;

        public Star() { }

        public Star(int id) { this.id = id; pos.init();vel.init(); mass = 0; }
        public Star(decimal[] pos, decimal[] vel,   //constructor
            decimal mass,int id=-1, decimal dt = 2)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;            
            this.dt = dt;
            this.mass = mass;
            if (id == -1)
                this.id = classId++;
            else this.id = id;
        }

        public Star(Vector pos, Vector vel,   //constructor
            decimal mass, int id, decimal dt = 2)
        {
            this.pos = pos;
            this.vel = vel;
            this.dt = dt;
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

        public Star(Vec6 vec,decimal mass,int id=-1)
        {
            this.pos = new Vector(vec,0);
            this.vel = new Vector(vec,1);
            this.id = id;
            this.mass = mass;
        }

        
        public decimal getMass() //get mass
        {
            return mass;
        }

        

        
        

        public void print() //print all fields of the star in the console
        {
            Console.WriteLine(this.id);
            Console.WriteLine("pos: " + pos.toString());
            Console.WriteLine("vel: " + vel.toString());
            Console.WriteLine("mass: " + mass);
        }

        public object Clone()
        {
            //throw new NotImplementedException();
            var clone = this.MemberwiseClone();
            
            return clone;
        }

        ~Star() { } //destructor
    }
}
