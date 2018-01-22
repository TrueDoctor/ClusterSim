using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClusterSim.ClusterLib.Analysis
{
    using System.Threading.Tasks;

    public partial class Analysis : Form
    {
        public Analysis(string table)
        {
            this.InitializeComponent();

            this.TableName = table;
            this.Steps = SQL.lastStep(table);
            //this.Steps = 1500;

            this.ClusterName.Text = $@"Analyse für: {table}";
        }

        public string TableName { get; set; }

        public int Steps { get; set; }

        private void EnergyAnalysis(object sender, EventArgs e)
        {
            var data = new List<double>[3];

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = new List<double>();
            }
            
            for (int i = 0; i < 200; i++)
            {
                Application.DoEvents();

                var stars = SQL.readStars(this.TableName, i * (this.Steps / 200));

                if (stars == null)
                {
                    continue;
                }

                stars.MoveCenter(stars.GetCenter());
                stars = stars.Where(s => s.pos.distance() < 4 * stars.GetRadius()).ToList();

                var mass = stars.Sum(x => x.mass);
                
                data[0].Add(stars.Sum(x => x.GetMetric(mass, Parameters.Kinetic)));
                if (i>0&&data[0][i] > 10*data[0][i-1])
                    data[0][i] = data[0][i-1]*1.7; 

                data[1].Add(stars.Sum(x => x.GetMetric(mass, Parameters.Potential)));
                data[2].Add(data[0][i] + data[1][i]);
            }

            GnuPlot.HoldOn();
            Statistics.SetLineStyles();

            for (int i = 0; i < 3; i++)
            {
                GnuPlot.HPlot(data[i].ToArray(), $"title '{i}' w linespoints ls {i + 1}");
            }

            GnuPlot.Plot();
            GnuPlot.HoldOff();
        }

        private void DensityAnalysis(object sender, EventArgs e)
        {
            var data = new double[200];
            for (int i = 0; i < 200; i++)
            {
                Application.DoEvents();

                var stars = SQL.readStars(this.TableName, i * (this.Steps / 200));

                if (stars == null)
                {
                    continue;
                }

                var center = stars.GetCenter();
                if (center != new Vector().init())
                {
                    stars.MoveCenter(center);
                }

                data[i] = stars.GetRadius();
            }
            
            Statistics.SetLineStyles();
            
            GnuPlot.Plot(data, "title 'Dichte' w linespoints");
        }

        private void RelaxationTime(object sender, EventArgs e)
        {
            var data = new double[200];
            for (int i = 0; i < 200; i++)
            {
                Application.DoEvents();

                var stars = SQL.readStars(this.TableName, i * (this.Steps / 200));

                if (stars == null)
                {
                    continue;
                }

                var center = stars.GetCenter();
                if (center != new Vector().init())
                {
                    stars.MoveCenter(center);
                }

                data[i] = stars.GetRelax();
            }

            Statistics.SetLineStyles();

            GnuPlot.Plot(data, "title 'Relaxationszeit' w linespoints");
        }

        private async void EfficiencyAnalysis(object sender, EventArgs e)
        {
            Statistics.SetLineStyles();
            GnuPlot.HoldOn();

            GnuPlot.Plot(await Task.Run(() => this.GetTimes(0)), "title 'Rechenzeit n^2' w linespoints");
            GnuPlot.Plot(await Task.Run(() => this.GetTimes(0.5)), "title 'Rechenzeit nlog(n)' w linespoints");

            GnuPlot.HoldOff();
        }

        private double[] GetTimes(double coe)
        {
            var times = new List<double>();
            var watch = new System.Diagnostics.Stopwatch();

            for (int i = 2; i < 800; i++)
            {
                //Application.DoEvents();
                var cluster = new StarCluster(i, 1, coe);

                for (int j = 0; j < i; j++)
                {
                    cluster.Stars.Add(Misc.randomize(10, 10, 10, 10, j));
                }
                watch.Start();
                cluster.doStep(1, 0, i-1, Misc.Method.RK5);
                watch.Stop();
                times.Add(watch.ElapsedMilliseconds);
                watch.Reset();
            }
            return times.ToArray();
        }
    }
}
