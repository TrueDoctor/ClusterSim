using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Analysis
{
    using System.Runtime.CompilerServices;
    using System.Security.Policy;

    using Microsoft.Win32.SafeHandles;

    public static class ViewPlot
    {
        public static void SPlot(IEnumerable<Star> stars, Parameters param, string name)
        {
            double min = Math.Log(Statistics.GetMin(stars, param));
            double max = Math.Log(Statistics.GetMax(stars, param));

            GnuPlot.Set($"cbrange[{(int)min}:{(int)max}]");
            GnuPlot.SPlot(
                stars.Select(x => x.pos.vec[0]).ToArray(),
                stars.Select(x => x.pos.vec[1]).ToArray(),
                stars.Select(x => x.pos.vec[2]).ToArray(),
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

            foreach (Parameters parameter in Enum.GetValues(typeof(Parameters)))
            {
                var data = Statistics.RadialAverage(stars, parameter, stepSize, steps);
                
                GnuPlot.HPlot(data.ToArray(), $"title '{parameter.ToString()}' w linespoints ls {(int)parameter + 1}");
            }
            GnuPlot.Plot();
            GnuPlot.HoldOff();
        }

        public static void PlotXY(IEnumerable<Star> stars, string range)
        {
            GnuPlot.HoldOn();
            GnuPlot.Set("logscale y 100", "yrange ");//+ range);
            foreach (Parameters parameter in Enum.GetValues(typeof(Parameters)))
            {
                var x = new List<double>();
                var y = new List<double>();

                Statistics.RadialCloud(x, y, stars, parameter);

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

        public static void PlotXY(IEnumerable<Star> stars, string range, string path)
        {
            GnuPlot.WriteLine("set terminal pngcairo size 1920,1080 enhanced font 'Verdana,10'");
            GnuPlot.Set("logscale y 10", "yrange " + range);

            GnuPlot.Set($@"output '{path}.png'");
            PlotXY(stars, range);

            GnuPlot.Set("output");
        }
    }
}
