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
        private Vector acc = new Vector();
        private double dt;
        private double mass;
        public int id;
        private static int classId=0;

        public Star() { }
        public Star(double[] pos, double[] vel, double[] acc,   //constructor
            double mass,int id=-1, double dt = 2)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;
            this.acc.vec = acc;
            this.dt = dt;
            this.mass = mass;
            if (id == -1)
                this.id = classId++;
        }

        public Star(Star s)
        {
            this.pos = s.pos;
            this.vel = s.vel;
            this.id = s.id;
            this.mass = s.mass;
        }

        public Vector getAcc() //get acceleration
        {
            return acc;
        }

        public double getMass() //get mass
        {
            return mass;
        }

        public void calcAcc(Vector acc) //calc acceleration based on force and mass
        {
            this.acc = acc;
            this.acc.div(mass);
            calcVel();
        }

        private void calcVel()  //calc velocety based on acceleration and dt
        {
            Vector tempVel = this.acc;
            tempVel.mult(dt);
            vel.add(tempVel);
            calcPos();
        }

        private void calcPos()  //calc position based on Vel
        {
            Vector tempPos = this.vel;
            tempPos.mult(dt);
            pos.add(tempPos);
            this.print();
        }

        public void print() //print all fields of the star in the console
        {
            Console.WriteLine("pos: " + pos.toString());
            Console.WriteLine("vel: " + vel.toString());
            Console.WriteLine("acc: " + acc.toString());
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
