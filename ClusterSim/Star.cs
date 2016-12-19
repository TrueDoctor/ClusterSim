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
        private Vector pos = new Vector();
        private Vector vel = new Vector();
        private Vector acc = new Vector();
        private double dt;
        private double mass;

        public Star(double[] pos, double[] vel, double[] acc,
            double mass, double dt = 2)
        {
            this.pos.vec = pos;
            this.vel.vec = vel;
            this.acc.vec = acc;
            this.dt = dt;
            this.mass = mass;
        }   
        
        public Vector getPos()
        {
            return pos;
        }

        public double getMass()
        {
            return mass;
        }

        public void calcAcc(Vector acc)
        {
            this.acc = acc;
            this.acc.div(mass);
            calcVel();
        }

        private void calcVel()
        {
            Vector tempVel = this.acc;
            tempVel.mult(dt);
            vel.add(tempVel);
            calcPos();
        }

        private void calcPos()
        {
            Vector tempPos = this.vel;
            tempPos.mult(dt);
            pos.add(tempPos);
        }

        ~Star() { }
    }
}
