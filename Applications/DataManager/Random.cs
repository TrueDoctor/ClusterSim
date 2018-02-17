using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClusterSim.ClusterLib;

namespace ClusterSim.DataManager
{
    public partial class Random : Form
    {
        private string table;
        public Random(string table)
        {
            this.table = table;
            InitializeComponent();
        }

        private void PosChange(object sender, EventArgs e)
        {
            PosAns.Text = "10^" + PosBar.Value + " AE";
        }

        private void VelChange(object sender, EventArgs e)
        {
            VelAns.Text = "10^" + VelBar.Value + " AE/D";
        }

        private void MassChange(object sender, EventArgs e)
        {
            MassVAns.Text = "10^" + MassVBar.Value + " SM Variance";
        }

        private void MassMBar_Scroll(object sender, EventArgs e)
        {
            MassMAns.Text = "10^" + MassMBar.Value + " SM Mean";
        }

        private void Start_Click(object sender, EventArgs e)
        {
            List<double> masses = new List<double>();
            Start.Enabled = false;
            StarCount.Enabled = false;
            PosBar.Enabled = false;
            VelBar.Enabled = false;
            MassVBar.Enabled = false;
            progressBar.Maximum = (int)StarCount.Value;

            BarAns.Visible = true;
            BarAns.Text = "Generiere Zufallsparameter";


            if (Kroupa.Checked)
            {
                System.Random randm = new System.Random();
                for (int i = 0; i < (int)StarCount.Value; i++)
                {
                    double r = randm.NextDouble();

                    if (r < 0.371572)
                        masses.Add((double)Math.Pow(0.352183 * r + 0.039811, 10 / 7.0));

                    else if (r < 0.849794)
                        masses.Add((double)Math.Pow(2.834447 - 1.886697 * r, -10 / 3.0));

                    else
                        masses.Add((double)Math.Pow(16.357585 - 16.351370 * r, -10 / 13.0));

                }
            }



            var Stars = new List<Star>();

            for (int i = 0; i < (int)StarCount.Value; i++)
            {
                Application.DoEvents();

                if (Kroupa.Checked)
                    Stars.Add(
                        new Star(
                            new Vector().random(Math.Pow(10, PosBar.Value)),
                            new Vector().random(Math.Pow(10, VelBar.Value)),
                            masses[i], i));

                else
                    Stars.Add(
                               Misc.randomize(
                                   Math.Pow(10, PosBar.Value), /*Math.Pow(10, VelBar.Value)*/
                                   0,
                                   Math.Pow(10, MassVBar.Value),
                                   Math.Pow(10, MassMBar.Value),
                                   i)); //add new row of stars

                progressBar.Increment(1);
            }

            StarCluster cluster = new StarCluster(0);
            cluster.Stars = Stars;
            cluster.Stars.MoveCenter(cluster.Stars.GetCenter());

            SQL.addRows(cluster.Stars, 0, table);

            StarCluster rand = new StarCluster(table, table, 0, 1);

            progressBar.Value = 0;
            BarAns.Text = "Berechne Initialgeschwindigkeiten";

            for (int i = 0; i < StarCount.Value; i++)
            {
                Application.DoEvents();
                rand.initialvel(i,0);//initial velocity for each star
                progressBar.Increment(1);
            }
            SQL.dropTable(table);
            SQL.addTable(table);//clear table 
            SQL.addRows(rand.Stars, 0, table);//export data

            Close();//close form
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Kroupa.Checked)
            {
                MassMBar.Enabled = false;
                MassMAns.Enabled = false;
                MassVBar.Enabled = false;
                MassVAns.Enabled = false;
            }
            else
            {
                MassMBar.Enabled = true;
                MassMAns.Enabled = true;
                MassVBar.Enabled = true;
                MassVAns.Enabled = true;
            }
        }

        private void calc_Tick(object sender, EventArgs e)
        {
            double mass;
            double n = (int)StarCount.Value;
            double r3 = Math.Pow(10, PosBar.Value * 3);
            if (Kroupa.Checked)
                mass = (double)StarCount.Value * 0.36;
            else
                mass = (double)StarCount.Value * Math.Pow(10, MassMBar.Value * 3);
            double relax = 800000 * Math.Sqrt((n * r3) / mass) * (1 / (Math.Log(n) - 1));
            Relaxation.Text = String.Format("Relaxationszeit: {0} Jahre", Math.Round(relax, 1));
        }
    }
}
