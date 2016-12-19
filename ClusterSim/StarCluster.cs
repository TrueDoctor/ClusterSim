using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim
{
    class StarCluster
    {
        private const double gravitation = 0.0000000000667384;
        private int StarCount;
        private List <Star> Stars;


        public StarCluster(int StarCount)
        {
            this.StarCount = StarCount;    
        }

        private void initialize()
        {
            double[] zero = { 0, 0, 0 };
            for (int i = 0; i < StarCount; i++)
            {
                Stars.Add(new Star(zero,zero,zero,1,1));
            }
        }

        public void calcForce()
        {
            Vector tempForce;
            Vector tempDirection;

            foreach (Star s in Stars)
            {
                tempForce = new Vector();

                for (int i = 0; i < StarCount; i++)
                {
                    tempDirection = s.getPos().direction(Stars[i].getPos());

                    double force = gravitation * ((s.getMass() 
                        * Stars[i].getMass()) / Math.Pow(tempDirection.distance(),2));

                    tempDirection.scale(force);
                    tempForce.add(tempDirection);
                }
            }
        }
 
    }
}
