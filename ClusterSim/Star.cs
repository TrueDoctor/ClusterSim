using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ClusterSim
{
    class Star
    {
        //declare fields
        private Vector pos = new Vector();
        private Vector vel = new Vector();
        private Vector acc = new Vector();
        private double dt;
        private double mass;

        public Star(double[] pos, double[] vel, double[] acc,   //constructor
            double mass, double dt = 2)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;
            this.acc.vec = acc;
            this.dt = dt;
            this.mass = mass;
        }   
        
        public Vector getPos() //get position
        {
            return pos;
        }
        public Vector getVel() //get Velocity
        {
            return vel;
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
            Console.WriteLine("pos: " + pos.Tostring());
            Console.WriteLine("vel: " + vel.Tostring());
            Console.WriteLine("acc: " + acc.Tostring());
            Console.WriteLine("mass: " + mass);
        }

        ~Star() { } //destructor
    }
}
