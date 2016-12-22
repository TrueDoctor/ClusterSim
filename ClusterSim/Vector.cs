using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim
{
    public class Vector
    {
        public double[] vec = new double[3]; //Field

        public Vector() { }         //blank constructor

        public Vector(double[] vec) //double[3] overload
        {
            this.vec = vec;
        }

        public Vector(Vector vec3)  //vector overload
        {
            this.vec = vec3.vec;
        }

        public void add(Vector vec) //add to vector
        {           
            for (int i = 0; i < 3; i++)
                this.vec[i] += vec.vec[i];           
        }
        public void sub(Vector vec) //sub from vector
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] -= vec.vec[i];
        }
        public void mult(double value) //mult this with
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] *= value;
        }
        public void div(double value)   //div this by
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] /= value;
        }
        public double skalar(Vector vec)    // skalarprodukt
        {
            double output = 0;
            for (int i = 0; i < 3; i++)
                output += this.vec[i] * vec.vec[i];
            return output;
        }
        public double distance()            //magnitude of the vector
        {
            double hypo = 0;
            for (int i = 0; i <= 2; i++)
                hypo += Math.Pow(this.vec[i],2);
            return Math.Sqrt(hypo);
        }
        public Vector direction(Vector vec2) //calc directionvector to other vector
        {
            vec2.sub(this);
            return vec2;
        }
        public void scale(double distance)   //scale magnitude to value
        {
            double d = distance / this.distance();
            for (int i = 0; i <= 2; i++)
                this.vec[i] = Math.Sqrt(Math.Pow(this.vec[i],2)*d);
        }

        public string Tostring()            //Translate to String
        {
            return vec[0].ToString() + "," + vec[1].ToString() + "," + vec[2].ToString();
        } 



    }
}
