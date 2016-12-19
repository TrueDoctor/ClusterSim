using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim
{
    public class Vector
    {
        public double[] vec = new double[3];

        public Vector()
        {

        }
        public Vector(double[] vec)
        {
            this.vec = vec;
        }

        public Vector(Vector vec3)
        {
            this.vec = vec3.vec;
        }

        public void add(Vector vec)
        {           
            for (int i = 0; i < 3; i++)
                this.vec[i] += vec.vec[i];           
        }
        public void sub(Vector vec)
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] -= vec.vec[i];
        }
        public void mult(double value)
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] *= value;
        }
        public void div(double value)
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] /= value;
        }
        public double skalar(Vector vec)
        {
            double output = 0;
            for (int i = 0; i < 3; i++)
                output += this.vec[i] * vec.vec[i];
            return output;
        }
        public double distance()
        {
            double hypo = 0;
            for (int i = 0; i <= 2; i++)
                hypo += Math.Pow(this.vec[i],2);
            return Math.Sqrt(hypo);
        }
        public Vector direction(Vector vec2)
        {
            vec2.sub(this);
            return vec2;
        }
        public void scale(double distance)
        {
            double d = distance / this.distance();
            for (int i = 0; i <= 2; i++)
                this.vec[i] = Math.Sqrt(Math.Pow(this.vec[i],2)*d);
        }



    }
}
