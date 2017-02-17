﻿using System;
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
            MassVAns.Text = "10^" + MassVBar.Value + " SM Variance";
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Start.Enabled = false;
            StarCount.Enabled = false;
            PosBar.Enabled = false;
            VelBar.Enabled = false;
            MassVBar.Enabled = false;
            progressBar.Maximum = (int)StarCount.Value;

            for (int i = 0; i < StarCount.Value; i++)
            {
                while (SQL.addRow(Misc.randomize(Math.Pow(10, PosBar.Value), Math.Pow(10, VelBar.Value),
                    Math.Pow(10, MassVBar.Value), Math.Pow(10, MassMBar.Value), i), 0, table) == false) ;
                progressBar.Increment(1);
            }

            StarCluster rand = new StarCluster(table,table,0,1);

            progressBar.Value =0;

            for (int i = 0; i < StarCount.Value; i++)
            {
                rand.initialvel(i);
                progressBar.Increment(1);
            }
            SQL.dropTable(table);
            SQL.addTable(table);
            foreach (Star s in rand.Stars)
                SQL.addRow(s,0,table);
            
            this.Close();
        }

        private void MassMBar_Scroll(object sender, EventArgs e)
        {
            MassMAns.Text = "10^" + MassMBar.Value + " SM Mean";
        }
    }
}
