

//using Newtonsoft.Json.Converters; 
namespace ClusterSim.ClusterLib.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Analysis;
    using ClusterSim.ClusterLib.Calculation;

    public static class CenterEnumerableExtension
    {
        public static Vector GetCenter(this IEnumerable<IMassive> list)
        {
            double mass = 0;
            Vector center = new Vector().init();

            if (list == null) throw new ArgumentNullException();

            double radius = list.GetRadius();

            list = list/*.Where(x=>x.pos.distance2() < radius * radius)*/.ToList();

            if (list.Count() != 0)
            {
                foreach (IMassive x in list)
                {
                    mass += x.mass;
                }

                foreach (IMassive m in list)
                {
                    if (m.mass == 0)
                        continue;
                    center += (m.mass / mass) * (m.pos);
                }
                return center;
                //if (mass!=0)
                //  pos.div(mass);
            }
            return null;
        }

        public static IEnumerable<IMassive> MoveCenter(this IEnumerable<IMassive> list, Vector pos)
        {
            
            list = list.ToList();

            if (list.Count() != 0)
            {
                foreach (IMassive x in list)
                {
                    x.pos -= pos;
                }
                return list;
            }
            return null;
        }
    } 
}