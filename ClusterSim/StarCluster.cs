using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim
{
    class StarCluster
    {
        //fields
        private const double gravitation = 0.0000000000667384;
        private int StarCount;
        private List <Star> Stars;
        private Vector range = new Vector(new double[] { 10000000000000000000, 10000000000000000000, 10000000000000000000 });
        double mean = 0;
        double stdDev = 100;

        public StarCluster(int StarCount)   //constructor takes Starcount
        {
            this.StarCount = StarCount;
            Stars = new List<Star>(0);
            randomize();                   //initialize
        }



        private void initialize()   //download newest Data
        {
            Stars = new SQL().readStars("[Initial Stars]");            
        }

        public void randomize(Vector range = null)
        {
            if (range == null)
                this.range = range;
            SQL client = new SQL();

            for (int i = 0; i < StarCount; i++)
            {

                client.writeStar(i, new Star(new double[] {random(), random(), random()}, new double[] { random(), random(), random() }, new double[] { random(), random(), random()}, random(), 2), "[Initial Stars]");
                initialize();
            }    
        }


        public void calcForce()     //calculete Force based on gravitation

        {
            Vector tempForce;
            Vector tempDirection;

            for (int i = 0; i < Stars.Capacity - 1; i++) //G between all stars
            {
                tempForce = new Vector();
                           
                for (int j = 0; j < Stars.Capacity-1; j++)
                {
                    if (i != j)         //no self interaction
                    {

                        tempDirection = Stars[i].getPos().direction(Stars[j].getPos()); //direction vector to the other star

                        double force = gravitation * ((Stars[i].getMass()                  //calculate the force    
                            * Stars[j].getMass()) / Math.Pow(tempDirection.distance(), 2));//

                        tempDirection.scale(force);     //scale the Direction vector's magnitude to match the total force  
                        tempForce.add(tempDirection);   //add forces
                    }
                }
                Stars[i].calcAcc(tempForce);    //save Force to the star and calculate all values
            }          
        }

        private double random()
        {
            Random rand = new Random(); //reuse this if you are generating many
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

    }
}
