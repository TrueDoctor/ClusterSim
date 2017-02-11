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


namespace DataManager
{
    public partial class DataManager : Form
    {
        public DataManager()
        {
            InitializeComponent();
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void ListRefresh_Tick(object sender, EventArgs e)
        {
            List<String> list = SQL.readTables();
            if (list != null)
            {
                ServerList.Items.Clear();
                foreach (string s in list)
                    ServerList.Items.Add(s);
            }
        }

        private void ListIndexChange(object sender, EventArgs e)
        {
            int s = SQL.lastStep((string)ServerList.SelectedItem);
            if (s == -1)
                SchritteAns.Text = "Liste ist leer";
            else
                SchritteAns.Text = "Schritte: "+Convert.ToString(s+1);
            
            
            int i = SQL.starsCount((string)ServerList.SelectedItem);
            if (i == -1)
                SterneAns.Text = "Liste ist leer";
            else
                SterneAns.Text = "Sterne: "+Convert.ToString(i+1);
        }

        private void ServerList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    string messageBoxText = String.Format("Wollen sie die Tabelle {0} unwiederruflich löschen?",(string)ServerList.SelectedItem);
                    string caption = "Tabelle löschen";
                    MessageBoxButtons button = MessageBoxButtons.YesNo;
                    MessageBoxIcon icon = MessageBoxIcon.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                    SQL.dropTable((string)ServerList.SelectedItem);
                    ListRefresh_Tick(new object(), new EventArgs());
                    break;
            }
        }

        private void AddTable_Click(object sender, EventArgs e)
        {
            SQL.addTable(newTableName.Text);
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void DataView_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\Users\Dennis\Documents\Visual Studio 2015\Projects\ClusterSim\Dataview\bin\Debug\DataView.exe", (string)ServerList.SelectedItem);
        }

        private void randomTable_Click(object sender, EventArgs e)
        {
            Random form = new Random((string)ServerList.SelectedItem);
            form.Show();
        }

        private void ClusterSim_Click(object sender, EventArgs e)
        {
            
            XDMessagingClient client = new XDMessagingClient();
            IXDListener listener = client.Listeners.GetListenerForMode(XDTransportMode.HighPerformanceUI);
            listener.RegisterChannel("steps");

            listener.MessageReceived += (o, ep) =>
            {
                if (ep.DataGram.Channel == "steps")
                {
                    MessageBox.Show(ep.DataGram.Message);
                    switch (ep.DataGram.Message.First())
                    {
                        case 's':
                            int n = Convert.ToInt32(ep.DataGram.Message.Remove(0, 1));
                            progressBar.Maximum = n;
                            break;
                        case 'i':
                            MessageBox.Show(ep.DataGram.Message);
                            int m = Convert.ToInt32(ep.DataGram.Message.Remove(0, 1));
                            progressBar.Value = m;

                            break;
                    }
                }
            };
            System.Diagnostics.Process.Start(@"C:\Users\Dennis\Documents\Visual Studio 2015\Projects\ClusterSim\ClusterSim\bin\Debug\ClusterSim.exe", (string)ServerList.SelectedItem);
        }
    }
}
