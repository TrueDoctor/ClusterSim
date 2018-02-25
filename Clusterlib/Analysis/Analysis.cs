namespace ClusterSim.ClusterLib.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using ClusterSim.ClusterLib.Calculation.Cluster;
    using ClusterSim.ClusterLib.Utility;

    public partial class Analysis : Form
    {
        public Analysis(string table)
        {
            this.InitializeComponent();

            this.TableName = table;
            this.Steps = SQL.lastStep(table);
            this.DataPoints = 200;
            this.progressBar.Maximum = this.DataPoints;

            this.ClusterName.Text = $@"Analyse für: {table}";
        }

        public string TableName { get; set; }

        public int Steps { get; set; }

        public int DataPoints { get; set; }

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
                this.progressBar.Increment(1);

                var stars = SQL.readStars(this.TableName, i * (this.Steps / 200));

                if (stars == null)
                {
                    continue;
                }

                stars.MoveCenter(stars.GetCenter());
                //stars = stars.Where(s => s.Pos.distance() < 4 * stars.GetRadius()).ToList();

                var mass = stars.Sum(x => x.Mass);
                
                data[0].Add(stars.Sum(x => x.GetMetric(mass, Parameters.Kinetic)));
                if (i>0&&data[0][i] > 2*data[0][i-1])
                    data[0][i] = data[0][i-1]*1.4; 

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

            this.progressBar.Value = 0;
        }

        private void DensityAnalysis(object sender, EventArgs e)
        {
            var data = new double[200];
            for (int i = 0; i < 200; i++)
            {
                Application.DoEvents();
                this.progressBar.Increment(1);

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

                data[i] = stars.GetRadius() * 4.84814e-6 ;
            }
            
            Statistics.SetLineStyles();
            GnuPlot.Set("ylabel 'Radius in Pc', font ',5'", "set tics font ', 10'");
            GnuPlot.Plot(data, "title 'Halb-Masse-Radius' w linespoints");
            this.progressBar.Value = 0;
        }

        private void RelaxationTime(object sender, EventArgs e)
        {
            var data = new double[200];
            for (int i = 0; i < 200; i++)
            {
                Application.DoEvents();
                this.progressBar.Increment(1);

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

                data[i] = /*stars.GetPercentEscaped() / (i * 2000 * (this.Steps / 200));*/ stars.GetRelax();
            }

            Statistics.SetLineStyles();

            GnuPlot.Set("yrange [0:100]");

            GnuPlot.Plot(data, "title 'Relaxationszeit' w linespoints");
            this.progressBar.Value = 0;
        }

        public void EfficiencyAnalysis(object sender, EventArgs e)
        {
            Statistics.SetLineStyles();
            GnuPlot.HoldOn();
            GnuPlot.Set("key top left", "xlabel 'Sternzahl'", "ylabel '\\frac{Rechenzeit}/{x} im ms'");
            //GnuPlot.Plot(this.GetTimes(new Cluster(), false), "title 'normal ' w linespoints");
            //GnuPlot.Plot(this.GetTimes(new Cluster(), true), "title 'normal 4 cores' w linespoints");
            GnuPlot.Plot(this.GetTimes(new BoxCluster(), false), "title 'Box' w linespoints");
            //GnuPlot.Plot(this.GetTimes(new BoxCluster(), true), "title 'Box 4 cores' w linespoints");
            /*
            GnuPlot.Plot(await Task.Run(() => this.GetTimes(new Cluster(), false)), "title 'Rechenzeit normal 1 core' w linespoints");
            GnuPlot.Plot(await Task.Run(() => this.GetTimes(new Cluster(), true)), "title 'Rechenzeit normal 4 core' w linespoints");
            GnuPlot.Plot(await Task.Run(() => this.GetTimes(new BoxCluster(), false)), "title 'Rechenzeit Box 1 core' w linespoints");
            GnuPlot.Plot(await Task.Run(() => this.GetTimes(new BoxCluster(), true)), "title 'Rechenzeit Box 4 core' w linespoints");*/

            GnuPlot.HoldOff();
        }

        private double[] GetTimes(ICluster cluster, bool multithreading)
        {
            var times = new List<double>();
            var watch = new System.Diagnostics.Stopwatch();
            cluster.Stars.Add(Misc.randomize(10, 10, 10, 10, 0));

            for (int i = 1; i < 800; i++)
            {
                cluster.Stars.Add(Misc.randomize(10, 10, 10, 10, i));

                watch.Start();

                for (int j = 0; j < 10; j++)
                {
                    cluster.DoStep(Misc.Method.Rk5, multithreading);
                }
                
                watch.Stop();
                times.Add(Math.Exp(watch.ElapsedMilliseconds / (10.0 * (i + 1))));
                watch.Reset();
                Application.DoEvents();
            }

            times[0] = times[1];
            return times.ToArray();
        }

        private void CalcLivetime(object sender, EventArgs e)
        {
            var lifetime = Statistics.GetLifeTime(this.TableName);
            this.ClusterLifetime.Text += lifetime;
        }
    }
}
