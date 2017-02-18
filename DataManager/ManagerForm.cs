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
using XDMessaging;
using System.Threading;
using System.IO;


namespace DataManager
{
    public partial class DataManager : Form
    {
        string table;//current selected table
        public DataManager()
        {
            InitializeComponent();
            ListRefresh_Tick(new object(), new EventArgs());
            
        }

        private void ListRefresh_Tick(object sender, EventArgs e)//show tables
        {
            List<String> list = SQL.readTables();
            if (list != null)
            {
                ServerList.Items.Clear();
                foreach (string s in list)
                    ServerList.Items.Add(s);
            }
        }

        private void ListIndexChange(object sender, EventArgs e)//display detailed information on selection
        {
            table = ServerList.SelectedItem.ToString();
            int s = SQL.lastStep(table);
            if (s == -1)
                SchritteAns.Text = "Liste ist leer";
            else
                SchritteAns.Text = "Schritte: "+Convert.ToString(s+1);
            
            
            int i = SQL.starsCount(table);
            if (i == -1)
                SterneAns.Text = "Liste ist leer";
            else
                SterneAns.Text = "Sterne: "+Convert.ToString(i+1);
            newTableName.Text = table;
            
        }

        private void ServerList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    string messageBoxText = String.Format("Wollen sie die Tabelle {0} unwiederruflich löschen?",table);
                    string caption = "Tabelle löschen";
                    MessageBoxButtons button = MessageBoxButtons.YesNo;
                    MessageBoxIcon icon = MessageBoxIcon.Warning;
                    DialogResult res= MessageBox.Show(messageBoxText, caption, button, icon) ;

                    if (res == DialogResult.Yes)
                        SQL.dropTable(table);
                    
                    ListRefresh_Tick(new object(), new EventArgs());
                    break;

                case Keys.D:
                    SQL.deleteStep(table,SQL.lastStep(table));
                    ListRefresh_Tick(new object(), new EventArgs());
                    break;
            }
        }

        private void AddTable_Click(object sender, EventArgs e)
        {
            SQL.addTable(newTableName.Text);
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void Refresh_Click(object sender, EventArgs e)//refresh icon
        {
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void DataView_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"..\..\..\Dataview\bin\Debug\DataView.exe", table);//start Dataview with table as parameter
        }

        private void randomTable_Click(object sender, EventArgs e)
        {
            string name;
            if (ServerList.Items.Contains(newTableName.Text))//check if input name already existes
                name = ServerList.SelectedItem.ToString();
            else if (!SQL.readTables().Contains(newTableName.Text))
            {
                name = newTableName.Text;//else crate new table
                SQL.addTable(name);
            }
            else
                name = newTableName.Text;

            Random form = new Random(name);
            form.Show();//start random Form and pass name as argument
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void ClusterSim_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            progressBar.Value = 0;

            XDMessagingClient client = new XDMessagingClient(); https://github.com/TheCodeKing/XDMessaging.Net
            IXDListener listener = client.Listeners.GetListenerForMode(XDTransportMode.HighPerformanceUI);
            listener.RegisterChannel("steps");

            listener.MessageReceived += (o, ep) =>
            {
                if (ep.DataGram.Channel == "steps")//select channel
                {
                    switch (ep.DataGram.Message.First().ToString())
                    {
                        case "s":
                            int n = Convert.ToInt32(ep.DataGram.Message.Remove(0, 1)); //check wether message is max or current value
                            
                            progressBar.Maximum = n;
                            break;
                        case "i":
                            
                            int m = Convert.ToInt32(ep.DataGram.Message.Remove(0, 1));
                            progressBar.Value = m;

                            break;
                    }
                }
                if (!(progressBar.Value < progressBar.Maximum - 1))
                    progressBar.Visible = false;//make invisible after execution 
            };
            System.Diagnostics.Process.Start(@"..\..\..\ClusterSim\bin\Debug\ClusterSim.exe", (string)ServerList.SelectedItem);
            
        }


        private bool tableWorking(string name)
        {
            progressBar.Maximum = SQL.lastStep(name);
            progressBar.Visible= true;
            int count = SQL.starsCount(name);
            for (int i = SQL.firstStep(name); i <= SQL.lastStep(name); i++)//check if every step has the same amount of stars
                if (count != SQL.starsCount(name, i))
                    return false;
                else
                    progressBar.Value = i;
            progressBar.Visible = false;
            return true;
        }

        private void Check_Click(object sender, EventArgs e)
        {
            if (tableWorking(table))
                MessageBox.Show("Fehlerfrei");
            else
                MessageBox.Show("Tabelle fehlerhaft");
        }

        private void Download_Click(object sender, EventArgs e)
        {
            if (table!=null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();//savefile dialouge
                saveFileDialog1.InitialDirectory = @"C:\";
                saveFileDialog1.Title = "Save table as File";
                saveFileDialog1.CheckPathExists = true;
                saveFileDialog1.DefaultExt = "tsv";
                saveFileDialog1.Filter = "Tsv files (*.tsv)|*.avi|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                string path = null;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = saveFileDialog1.FileName;
                    List<Star> list = new List<Star>();
                    progressBar.Visible = true;
                    progressBar.Maximum = SQL.lastStep(table);
                    for (int i = SQL.firstStep(table); i <= SQL.lastStep(table); i++)
                    {
                        list.AddRange(SQL.readStars(table, i));
                        progressBar.Value = i;
                    }
                    progressBar.Value = 0;
                    progressBar.Maximum = list.Count;
                    List<string> lines = new List<string>();
                    foreach (Star s in list)
                    {
                        lines.Add(s.toTsv().Replace(',', '.'));
                        progressBar.Increment(1);
                    }
                    System.IO.File.WriteAllLines(path,lines);
                    progressBar.Value = 0;
                    progressBar.Visible = false;
                }
            }
        }
    }
}
