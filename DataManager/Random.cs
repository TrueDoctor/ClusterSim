using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClusterLib;

namespace DataManager
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
            MassAns.Text = "10^" + MassBar.Value + " SM";
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Start.Enabled = false;
            StarCount.Enabled = false;
            PosBar.Enabled = false;
            VelBar.Enabled = false;
            MassBar.Enabled = false;
            progressBar.Maximum = (int)StarCount.Value;

            for (int i = 0; i < StarCount.Value; i++)
            {
                while (SQL.addRow(Misc.randomize(Math.Pow(10, PosBar.Value), Math.Pow(10, VelBar.Value), Math.Pow(10, MassBar.Value), i), 0, table) == false) ;
                progressBar.Increment(1);
            }
            this.Close();
        }
    }
}
