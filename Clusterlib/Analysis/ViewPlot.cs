namespace ClusterSim.ClusterLib.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterSim.ClusterLib.Calculation;
    using ClusterSim.ClusterLib.Utility;

    public static class ViewPlot
    {
        public static void SPlot(IEnumerable<Star> stars, Parameters param, string name)
        {
            double min = Math.Log(Statistics.GetMin(stars, param));
            double max = Math.Log(Statistics.GetMax(stars, param));

            GnuPlot.Set($"cbrange[{(int)min}:{(int)max}]");
            GnuPlot.SPlot(
                stars.Select(x => x.Pos.vec[0]).ToArray(),
                stars.Select(x => x.Pos.vec[1]).ToArray(),
                stars.Select(x => x.Pos.vec[2]).ToArray(),
                stars.Select(x => Math.Log(x.GetMetric(param))).ToArray(),
                name,
                "with points palette pt 7");
        }

        public static void Plot(IEnumerable<Star> stars, string range, double stepSize, int steps)
        {
            GnuPlot.HoldOn();
            Statistics.SetLineStyles();
            GnuPlot.Set("logscale y 10");

            if (!range.Equals(string.Empty))
            {
                GnuPlot.Set("yrange " + range);
            }
            GnuPlot.Set("ylabel 'Radius in Pc', font ',5'", "set tics font ', 10'");
            foreach (Parameters parameter in Enum.GetValues(typeof(Parameters)))
            {
                var data = Statistics.RadialAverage(stars, parameter, stepSize, steps);
                
                GnuPlot.HPlot(data.ToArray(), $"title '{parameter.ToString()}' w linespoints ls {(int)parameter + 1}");
            }
            GnuPlot.Plot();
            GnuPlot.HoldOff();
        }

        public static void PlotXY(IEnumerable<Star> stars, string range, double radius = 0)
        {
            GnuPlot.HoldOn();
            if (radius == 0) radius = stars.GetRadius() * 4;

            GnuPlot.Set("logscale y 100", "yrange "+ range, $"xrange [0:{(int)radius}]");

            stars.MoveCenter(stars.GetCenter());
            //stars = stars.Where(s => s.pos.distance() < 4 * stars.GetRadius()).ToList();
            var mass = stars.Sum(x => x.Mass);

            GnuPlot.Set("xlabel 'Abstand vom Zentrum in AE' font ',15'", "tics font ', 12'");

            foreach (Parameters parameter in Enum.GetValues(typeof(Parameters)))
            {
                var x = new List<double>();
                var y = new List<double>();

                Statistics.RadialCloud(x, y, stars, parameter, mass);

                GnuPlot.HPlot(x.ToArray(), y.ToArray(), $"title '{parameter.ToString()}' ");//w linespoints ls {(int)parameter + 1}
            }
            GnuPlot.Plot();
            GnuPlot.HoldOff();
        }

        public static void Plot(IEnumerable<Star> stars,string range, double stepSize, int steps, string path)
        {
            GnuPlot.WriteLine("set terminal pngcairo size 1920,1080 enhanced font 'Verdana,10'");
            GnuPlot.Set("logscale y 10", "yrange " + range);

            GnuPlot.Set($@"output '{path}.png'");
            Plot(stars, range, stepSize, steps);
            
            GnuPlot.Set("output");
        }

        public static void PlotXY(IEnumerable<Star> stars, string range, double radius, string path)
        {
            GnuPlot.WriteLine("set terminal pngcairo size 1920,1080 enhanced font 'Verdana,10'");
            GnuPlot.Set("logscale y 10", "yrange " + range);

            GnuPlot.Set($@"output '{path}.png'");
            PlotXY(stars, range, radius);

            GnuPlot.Set("output");
        }
    }
}
