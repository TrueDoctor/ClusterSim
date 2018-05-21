using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.Cluster
{
    using ClusterSim.ClusterLib.Calculation.Gpu;
    using ClusterSim.ClusterLib.Utility;

    public class GpuCluster : Cluster, ICluster
    {
        public int Burst { get; set; } = 2;

        public new Star[] Stars;

        public GpuCluster(List<Star> stars)
        {
            this.Stars = stars.ToArray();
            MinPrecision = ClusterLib.Properties.Settings.Default.MinPrecision;
        }

        public Star[] DoStep(Misc.Method m, double distanceFromGalaxy)
        {
            if (m != Misc.Method.Rk5)
            {
                return base.DoStep(m, true, false, distanceFromGalaxy);
            }
            
            for (double time = 0; this.ParentDt > time;)
            {
                this.Burst = (int)((this.ParentDt / this.Dt) * 0.1) / 2 * 2;
                
                this.Burst = this.Burst < 2 ? 2 : this.Burst;

                if (time + this.Dt * this.Burst > this.ParentDt)
                {
                    this.Dt = (this.ParentDt - time) / this.Burst;
                }

                ComputeWorker.DoStep(this.Stars, this.Dt, this.Burst - 2, distanceFromGalaxy);
                
                ComputeWorker.DoStep(this.Stars, this.Dt, 2, distanceFromGalaxy);

                time += this.Dt * this.Burst;

                this.Dt = this.GetNewDt();
                
            }

            this.Dt = this.GetNewDt(); // * this.Burst;

            return this.Stars;
        }

        protected override double GetNewDt()
        {
            var maxDAcc = this.Stars.OrderBy(x => x.Dt / x.DAcc).First();
            double dt = this.Dt;
            if (!(maxDAcc.DAcc > 0))
            {
                return dt;
            }

            dt = SubCluster.CalcRequiredDt(maxDAcc);

            if (dt > this.ParentDt)
            {
                dt = this.ParentDt;
            }

            return dt;
        }
    }
}
