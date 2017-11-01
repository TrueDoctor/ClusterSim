namespace ClusterSim.Net.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib;

    public class SendEventArgs : EventArgs
    {
        private readonly int step;

        private readonly List<Star> dStars;

        private readonly List<int[]> orders;

        public int dt {  get; private set; }

        #region get set


        public int Step
        {
            get { return this.step; }
        }

        public List<int[]> Orders
        {
            get { return this.orders; }
        }

        public List<Star> Stars
        {
            get { return this.dStars; }
        }

        #endregion

        /// <inheritdoc />
        // ReSharper disable once StyleCop.SA1201
        public SendEventArgs(int step,int dt, List<int[]> orders, Star[] dStars)
        {
            this.step = step;
            this.dStars = dStars.ToList();
            this.orders = orders;
            this.dt = dt;
        }
    }
}