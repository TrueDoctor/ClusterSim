using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterLib
{
    [Serializable()]
    public class Vector
    {
        public decimal[] vec = new decimal[3]; //Field

        public Vector() { }         //blank constructor

        public Vector(decimal[] vec) //decimal[3] overload
        {
            this.vec = vec;
        }

        public Vector(decimal a,decimal b,decimal c) //decimal[3] overload
        {
            this.vec[0] = a;
            this.vec[1] = b;
            this.vec[2] = c;
        }

        public Vector(Vector vec3)  //vector overload
        {
            this.vec = vec3.vec;
        }

        public Vector(Vec6 vec,int i)  //vector overload
        {
            for(int n = 0;n<3;n++)
                this.vec[n] = vec.vec[n+i*3];
        }

        public static Vector operator+ (Vector a, Vector b)
        {
            return new Vector(new decimal[] { a.vec[0] + b.vec[0], a.vec[1] + b.vec[1], a.vec[2] + b.vec[2] });
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(new decimal[] { a.vec[0] - b.vec[0], a.vec[1] - b.vec[1], a.vec[2] - b.vec[2] });
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(new decimal[] { a.vec[0] * b.vec[0], a.vec[1] * b.vec[1], a.vec[2] * b.vec[2] });
        }

        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(new decimal[] { a.vec[0] / b.vec[0], a.vec[1] / b.vec[1], a.vec[2] / b.vec[2] });
        }

        public static Vector operator *(decimal a, Vector b)
        {
            return new Vector(new decimal[] { a * b.vec[0], a* b.vec[1], a* b.vec[2] });
        }


        public void init()
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] = 0;
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
        public void mult(decimal value) //mult this with
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] *= value;
        }
        public void div(decimal value)   //div this by
        {
            for (int i = 0; i < 3; i++)
                this.vec[i] /= value;
        }
        public decimal skalar(Vector vec)    // skalarprodukt
        {
            decimal output = 0;
            for (int i = 0; i < 3; i++)
                output += this.vec[i] * vec.vec[i];
            return output;
        }
        public decimal distance()            //magnitude of the vector
        {
            decimal hypo = 0;
            for (int i = 0; i <= 2; i++)
                hypo += this.vec[i]* this.vec[i];
            return (decimal)Math.Sqrt((Double)hypo);
        }
        public Vector direction(Vector vec2) //calc directionvector to other vector
        {
            
            return vec2 - this;
        }
        public void scale(decimal distance)   //scale magnitude to value
        {
            decimal d = distance / this.distance();
            for (int i = 0; i <= 2; i++)
                this.vec[i] = (decimal)Math.Sqrt(Math.Pow((double)this.vec[i],2)*(double)d);
        }

        public Vector random(double range=1)   //scale magnitude to value
        { 
            for (int i = 0; i <= 2; i++)
                this.vec[i] = Misc.random(range);
            return this;
        }

        /*public Vector polar()
        {
            decimal r = this.distance();
            decimal g = 0;
            decimal b = 0;
            if (this.vec[0] > 0)
                if (this.vec[1] > 0)
                    g = this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));
                else
                    g = 360 + this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));
            else if (this.vec[0] < 0)
                g = 180 - this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));

            if (r != 0)
                b = (decimal)Math.Asin((double)this.vec[2] / r);
            return new ClusterLib.Vector(new decimal[3] { r, Math.Asin(this.vec[2] / r), g });

        }*/

        public string toString()            //Translate to String
        {
            return vec[0] + "|" + vec[1] + "|" + vec[2];
        } 



    }
    public class Vec6
    {
        public decimal[] vec = new decimal[6]; //Field

        public Vec6() { }         //blank constructor

        public Vec6(decimal[] vec) //decimal[6] overload
        {
            this.vec = vec;
        }

        public Vec6(Vec6 vec6)  //Vec6 overload
        {
            this.vec = vec6.vec;
        }

        public Vec6(Vector a,Vector b)  //Vec6 overload
        {
            this.vec =  a.vec.Concat(b.vec).ToArray();
        }

        public static Vec6 operator +(Vec6 a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a.vec[i] + b.vec[i];
            return temp;
        }

        public static Vec6 operator -(Vec6 a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a.vec[i] - b.vec[i];
            return temp;
        }

        public static Vec6 operator *(Vec6 a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a.vec[i] * b.vec[i];
            return temp;
        }

        public static Vec6 operator /(Vec6 a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a.vec[i] * b.vec[i];
            return temp;
        }

        public static Vec6 operator *(decimal a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a * b.vec[i];
            return temp;
        }


        public void init()
        {
            for (int i = 0; i < 6; i++)
                this.vec[i] = 0;
        }

        public void add(Vec6 vec) //add to Vec6
        {
            for (int i = 0; i < 6; i++)
                this.vec[i] += vec.vec[i];
        }
        public void sub(Vec6 vec) //sub from Vec6
        {
            for (int i = 0; i < 6; i++)
                this.vec[i] -= vec.vec[i];
        }
        public void mult(decimal value) //mult this with
        {
            for (int i = 0; i < 6; i++)
                this.vec[i] *= value;
        }
        public void div(decimal value)   //div this by
        {
            for (int i = 0; i < 6; i++)
                this.vec[i] /= value;
        }
        public decimal skalar(Vec6 vec)    // skalarprodukt
        {
            decimal output = 0;
            for (int i = 0; i < 6; i++)
                output += this.vec[i] * vec.vec[i];
            return output;
        }
        public decimal distance()            //magnitude of the Vec6
        {
            decimal hypo = 0;
            for (int i = 0; i <= 5; i++)
                hypo += (decimal)Math.Pow((double)this.vec[i], 5);
            return (decimal)Math.Sqrt((double)hypo);
        }
        public Vec6 direction(Vec6 vec5) //calc directionVec6 to other Vec6
        {
            vec5.sub(this);
            return vec5;
        }
       

        public Vector ToVector(int i)
        {
            if (i == 0)
                return new Vector(new decimal[] { this.vec[0], this.vec[1], this.vec[2] });
            else
                return new Vector(new decimal[] { this.vec[3], this.vec[4], this.vec[5] });
        }

        public string toString()            //Translate to String
        {
            return vec[0].ToString() + "|" + vec[1].ToString() + "|" + vec[2].ToString()+"|" + vec[3].ToString()+"|" + vec[4].ToString()+ "|" + vec[2].ToString();
        }
    }
}
