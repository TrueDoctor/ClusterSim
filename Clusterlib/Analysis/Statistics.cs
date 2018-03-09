namespace ClusterSim.ClusterLib.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Utility;

    public enum Parameters
    {
        Mass,
        Vel,
        Pulse,
        Kinetic,
        Potential
    }

    public static class Statistics
    {
        public static double GetMin(IEnumerable<Star> stars, Parameters param)
        {
            return stars.Min(x => x.GetMetric(param));
        }

        public static double GetMax(IEnumerable<Star> stars, Parameters param)
        {
            return stars.Max(x => x.GetMetric(param));
        }

        public static double EvauateCluster(IEnumerable<Star> stars, Parameters param)
        {
            double res = 0;
            foreach (Star star in stars)
            {
                res += star.GetMetric(param);
            }
            return res;
        }

        public static Vector MassCenter(IEnumerable<Star> stars)
        {
            stars = stars.ToList();
            var temp = new Vector();
            var mass = stars.Sum(x => x.Mass);

            return stars.Cast<IMassive>().Aggregate(temp, (current, m) => current + m.mass / mass * m.pos);

        }

        public static int GetLifeTime(string table)
        {
            var steps = SQL.lastStep(table);
            return ApproximateLifetime(table, 0, steps);
        }

        private static int ApproximateLifetime(string table, int start, int end)
        {
            int step = (end - start) / 10;

            step = step < 1 ? 1 : step; 
            
            for (int i = 0; i <= 10; i++)
            {
                var stars = SQL.readStars(table, i * step);

                stars.MoveCenter(stars.GetCenter());

                if (stars.GetPercentEscaped() <= 47)
                {
                    continue;
                }

                if (step >= 1)
                {
                    return ApproximateLifetime(table, (i - 1) * step, i * step);
                }

                return i * step;
            }

            return -1;
        }

        public static IEnumerable<double> RadialAverage(IEnumerable<Star> estars, Parameters param, double stepSize)
        {
            int processed = 0;
            IEnumerable<Star> stars = estars as IList<Star> ?? estars.ToList();
            int count = stars.Count();
            var center = MassCenter(stars);
            var values = new List<double>();
            stepSize = stars.GetRadius() * 4 / 30;
            for (double s = stepSize*2; processed < 0.90 * count; s += stepSize)
            {
                var temp = stars.Where(x => (x.Pos-center).distance() < s).ToList();
                processed += temp.Count;
                stars = stars.Except(temp).ToList();
                double res = temp.Sum(star => star.GetMetric(param));

                values.Add(res/(12.5663706*s*s)); //double.IsNaN(res / temp.Count)?0:res/ temp.Count);
            }
            return values;
        }

        public static IEnumerable<double> RadialAverage(IEnumerable<Star> estars, Parameters param, double stepSize, int steps)
        {
            int processed = 0;
            IEnumerable<Star> stars = estars as IList<Star> ?? estars.ToList();
            int count = stars.Count();
            double prevRadius = stepSize;
            var center = MassCenter(stars);
            var values = new List<double>();
            for (double s = stepSize * 2; s < steps * stepSize; s += stepSize)
            {
                var temp = stars.Where(x => (x.Pos - center).distance2() < s*s).ToList();
                processed += temp.Count;
                stars = stars.Except(temp).ToList();



                double res = temp.Sum(star => star.GetMetric(param));

                values.Add(res / (12.5663706 * s * s * s - 12.5663706 * prevRadius * prevRadius * prevRadius)); //double.IsNaN(res / temp.Count)?0:res/ temp.Count);
                prevRadius = s;
            }
            return values;
        }

        public static void RadialCloud(List<double> x, List<double> y, IEnumerable<Star> estars, Parameters param, double mass = 0)
        {
            List<Star> stars = estars.ToList();
            
            foreach (Star star in stars)
            {
                    x.Add(star.Pos.distance());
                    y.Add(star.GetMetric(mass, param));
            }
        }

        public static void PlotMetric(string table, int step, Parameters param, double stepSize = 3000000)
        {
            var stars = SQL.readStars(table, step);

            var data = Statistics.RadialAverage(stars, param, stepSize);


            GnuPlot.Set("logscale y 2");

            GnuPlot.Plot(data.ToArray(), $"title '{param.ToString()}' w linespoints ls {(int)param+1}");
        }

        public static void SetLineStyles()
        {
            GnuPlot.Set(
                "style line 1 lc rgb 'red' lt 1 lw 2 pt 7 ps 1.5",
                "style line 2 lc rgb 'cyan' lt 1 lw 2 pt 9 ps 1.5",
                "style line 3 lc rgb 'green' lt 1 lw 2 pt 11 ps 1.5",
                "style line 4 lc rgb 'magenta' lt 1 lw 2 pt 13 ps 1.5",
                "style line 5 lc rgb 'blue' lt 1 lw 2 pt 5 ps 1.5",
                "style line 6 lc rgb 'black' lt 1 lw 2 pt 6 ps 1.5");
        }

        public static void ShowPlot(IEnumerable<double> data,string options = "")
        {
            GnuPlot.Plot(data.ToArray(), options);
        }
    }
}