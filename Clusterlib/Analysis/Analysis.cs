using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ClusterSim.ClusterLib.Analysis
{
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public partial class Analysis : Form
    {
        public Analysis(string table)
        {
            this.InitializeComponent();

            this.TableName = table;
            this.Steps = SQL.lastStep(table);

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
                var stars = SQL.readStars(this.TableName, i * (this.Steps / 200));

                if (stars == null)
                    continue;

                stars.MoveCenter(stars.GetCenter());
                stars = stars.Where(s => s.pos.distance() < 4 * stars.GetRadius()).ToList();

                var mass = stars.Sum(x => x.mass);
                
                data[0].Add(stars.Sum(x => x.GetMetric(mass, Parameters.Kinetic)));
                data[1].Add(stars.Sum(x => x.GetMetric(mass, Parameters.Potential)));
                data[2].Add(data[0][i] - data[1][i]);
            }

            GnuPlot.HoldOn();
            Statistics.SetLineStyles();

            for (int i = 0; i < 3; i++)
            {
                GnuPlot.HPlot(data[i].ToArray(), $"title '{i}' w linespoints ls {i + 1}");
            }

            GnuPlot.Plot();
        }
    }
}
