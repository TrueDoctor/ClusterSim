using System;
using System.Linq;

namespace ClusterSim.ClusterLib
{
    [Serializable]
    public class Vector
    {
        public double[] Vec = new double[3];
        public double[] vec  //Field
        {
            get
            {
                return Vec;
            }
            set
            {
                // if (double.IsNaN(value[0]))
                //   throw new DivideByZeroException();
                //else
                Vec = value;
            }
        }

        public Vector()
        {
            this.Vec.Initialize();
        }         //blank constructor

        public Vector(double[] vec) //double[3] overload
        {
            this.vec = vec;
        }

        public Vector(double a, double b, double c) //double[3] overload
        {
            vec[0] = a;
            vec[1] = b;
            vec[2] = c;
        }

        public Vector(Vector vec3)  //vector overload
        {
            vec = vec3.vec;
        }

        public Vector(byte[] input)  //vector overload
        {
            Deserialize(input);
        }

        public Vector(Vec6 vec, int i)  //vec6 overload split into a single vec3
        {
            for (int n = 0; n < 3; n++)
                this.vec[n] = vec.vec[n + i * 3];
        }

        public static Vector Abs(Vector a)
        {
            return new Vector(a.vec.Select(x => Math.Abs(x)).ToArray());
        }

        public static Vector operator +(Vector a, Vector b)//+ operator overload
        {
            return new Vector(new[] { a.vec[0] + b.vec[0], a.vec[1] + b.vec[1], a.vec[2] + b.vec[2] });
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(new[] { a.vec[0] - b.vec[0], a.vec[1] - b.vec[1], a.vec[2] - b.vec[2] });
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(new[] { a.vec[0] * b.vec[0], a.vec[1] * b.vec[1], a.vec[2] * b.vec[2] });
        }


        public static Vector operator /(Vector a, Vector b)
        {
            if (b.vec.Contains(0))
                throw new DivideByZeroException();
            return new Vector(new[] { a.vec[0] / b.vec[0], a.vec[1] / b.vec[1], a.vec[2] / b.vec[2] });
        }

        public static Vector operator *(double a, Vector b)
        {
            return new Vector(new[] { a * b.vec[0], a * b.vec[1], a * b.vec[2] });
        }

        public static Vector operator /(Vector b, double a)
        {
            return new Vector(new[] { b.vec[0] / a, b.vec[1] / a, b.vec[2] / a });
        }

        public static Vector operator +(Vector b, double a)
        {
            return new Vector(new[] { b.vec[0] + a, b.vec[1] + a, b.vec[2] + a });
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return (Math.Abs(a.vec[0] - b.vec[0]) < 0.000000001 && Math.Abs(a.vec[1] - b.vec[1]) < 0.000000001 && Math.Abs(a.vec[2] - b.vec[2]) < 0.000000001) ? true : false;
        }

        public override bool Equals(System.Object obj)
        {
            var a = obj as Vector;
            if (a == null)
            {
                return false;
            }

            return this == a;
        }

        protected bool Equals(Vector other)
        {
            return Equals(this.Vec, other.Vec);
        }

        public override int GetHashCode()
        {
            return (this.Vec != null ? this.Vec.GetHashCode() : 0);
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return (a.vec[0] != b.vec[0] && a.vec[1] != b.vec[1] && a.vec[2] != b.vec[2]) ? true : false;
        }

        public static bool operator <(Vector a, Vector b)
        {
            return a.vec[0] < b.vec[0] && a.vec[1] < b.vec[1] && a.vec[2] < b.vec[2] ? true : false;
        }
        public static bool operator >(Vector a, Vector b)
        {
            return a.vec[0] > b.vec[0] && a.vec[1] > b.vec[1] && a.vec[2] > b.vec[2] ? true : false;
        }

        public Vector init(double n = 0)//initialize vector
        {
            for (int i = 0; i < 3; i++)
                vec[i] = n;
            return this;
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
        public void mult(double value) //mult this with value
        {
            for (int i = 0; i < 3; i++)
                vec[i] *= value;
        }
        public Vector div(double value)   //div this by value
        {
            if (value == 0 || double.IsNaN(value) || double.IsInfinity(value))
                throw new DivideByZeroException();
            for (int i = 0; i < 3; i++)
                vec[i] /= value;
            return this;
        }
        public double skalar(Vector vec)    // dot product(skalarprodukt)
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
                hypo += vec[i] * vec[i];
            return Math.Sqrt(hypo);
        }

        public double distance2()            //magnitude of the vector squared
        {
            double hypo = 0;
            for (int i = 0; i <= 2; i++)
                hypo += vec[i] * vec[i];
            return hypo;
        }

        public Vector direction(Vector vec2) //calc directionvector to other vector
        {

            return vec2 - this;
        }
        public Vector scale(double distance)   //scale magnitude to value
        {
            div(this.distance());
            mult(distance);
            return this;
        }

        public Vector random(double range = 1)   //generate random vector
        {
            for (int i = 0; i <= 2; i++)
                vec[i] = Misc.random(range);
            return this;
        }
        public Vector Floor()   //generate random vector
        {
            for (int i = 0; i <= 2; i++)
                vec[i] = Math.Floor(vec[i]);
            return this;
        }

        public bool IsNeighbour(Vector b)
        {
            /* for (int x = -1; x <= 1; x++)
                 for (int y = -1; y <= 1; y++)
                     for (int z = -1; z <= 1; z++)
                     {
                         if (vec[0] == b.Vec[0] && vec[1] == b.vec[1] && vec[2] == b.vec[2])
                             return true;
                     }*/
            for (int i = 0; i <= 2; i++)
                if (Math.Abs(vec[i] - b.vec[i]) > 1)
                    return false;
            return true;

        }

        /*public Vector polar()//convert to polar coordinates
        {
            double r = this.distance();
            double g = 0;
            double b = 0;
            if (this.vec[0] > 0)
                if (this.vec[1] > 0)
                    g = this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));
                else
                    g = 360 + this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));
            else if (this.vec[0] < 0)
                g = 180 - this.vec[1] / (Math.Abs(this.vec[0]) + Math.Sqrt(Math.Pow(this.vec[0], 2) + Math.Pow(this.vec[1], 2)));

            if (r != 0)
                b = (double)Math.Asin((double)this.vec[2] / r);
            return new ClusterLib.Vector(new double[3] { r, Math.Asin(this.vec[2] / r), g });

        }*/

        public string toString()            //Translate to String
        {
            return vec[0] + "|" + vec[1] + "|" + vec[2];
        }
        public byte[] Serialize()
        {
            var temp = new byte[24];
            for (int i = 0; i < 3; i++)
                Array.Copy(BitConverter.GetBytes(vec[i]), 0, temp, i * 8, 8);
            return temp;
        }
        public void Deserialize(byte[] input)
        {
            for (int i = 0; i < 3; i++)
                vec[i] = BitConverter.ToDouble(input, i * 8);
        }

        public bool IsNull()
        {
            return this.Vec.All(d => !(Math.Abs(d) < 0.000000000001));
        }


    }
    public class Vec6//6 dimensional vector propertys according to Vector
    {
        public double[] vec = new double[6]; //Field

        public Vec6() { }         //blank constructor

        public Vec6(double[] vec) //double[6] overload
        {
            this.vec = vec;
        }

        public Vec6(Vec6 vec6)  //Vec6 overload
        {
            vec = vec6.vec;
        }

        public Vec6(Vector a, Vector b)  //Vec6 overload
        {
            vec = a.vec.Concat(b.vec).ToArray();
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
                if (b.vec[i] != 0 || !double.IsNaN(b.vec[i]) || !double.IsInfinity(b.vec[i]))
                    temp.vec[i] = a.vec[i] / b.vec[i];
                else throw new DivideByZeroException();
            return temp;
        }

        public static Vec6 operator *(double a, Vec6 b)
        {
            Vec6 temp = new Vec6();
            for (int i = 0; i < 6; i++)
                temp.vec[i] = a * b.vec[i];
            return temp;
        }


        public void init()
        {
            for (int i = 0; i < 6; i++)
                vec[i] = 0;
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
        public void mult(double value) //mult this with
        {
            for (int i = 0; i < 6; i++)
                vec[i] *= value;
        }
        public void div(double value)   //div this by
        {
            if (value == 0 || double.IsNaN(value) || double.IsInfinity(value))
                throw new DivideByZeroException();
            for (int i = 0; i < 6; i++)
                vec[i] /= value;
        }
        public double skalar(Vec6 vec)    // skalarprodukt
        {
            double output = 0;
            for (int i = 0; i < 6; i++)
                output += this.vec[i] * vec.vec[i];
            return output;
        }
        public double distance()            //magnitude of the Vec6
        {
            double hypo = 0;
            for (int i = 0; i <= 5; i++)
                hypo += Math.Pow(vec[i], 5);
            return Math.Sqrt(hypo);
        }
        public Vec6 direction(Vec6 vec5) //calc directionVec6 to other Vec6
        {
            vec5.sub(this);
            return vec5;
        }


        public Vector ToVector(int i)//split Vec6 into tow seperate vectors
        {
            if (i == 0)
                return new Vector(new[] { vec[0], vec[1], vec[2] });
            return new Vector(new[] { vec[3], vec[4], vec[5] });
        }

        public string toString()            //Translate to String
        {
            return vec[0] + "|" + vec[1] + "|" + vec[2] + "|" + vec[3] + "|" + vec[4] + "|" + vec[2];
        }
    }
}
