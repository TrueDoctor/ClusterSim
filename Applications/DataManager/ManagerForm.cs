using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using ClusterSim.ClusterLib.Calculation;

using XDMessaging;

namespace ClusterSim.DataManager
{
    using ClusterSim.ClusterLib.Analysis;
    using ClusterSim.ClusterLib.Utility;

    public partial class DataManager : Form
    {
        string table; // current selected table

        int index = 0;

        public DataManager()
        {
            InitializeComponent();
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private static DialogResult
            ShowInputDialog(
                ref string input)
        {
            // http://stackoverflow.com/questions/97097/what-is-the-c-sharp-version-of-vb-nets-inputdialog
            Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

        private bool tableWorking(string name)
        {
            progressBar.Maximum = SQL.lastStep(name);
            progressBar.Visible = true;
            int count = SQL.starsCount(name);

            for (int i = SQL.firstStep(name); i <= SQL.lastStep(name); i++) // check if every step has the same amount of stars
            {
                if (count != SQL.starsCount(name, i))
                    return false;
                else
                    progressBar.Value = i;
                Application.DoEvents();
            }
            progressBar.Visible = false;
            return true;
        }

        private void ListRefresh_Tick(object sender, EventArgs e)
        {
            // show tables
            List<string> list = SQL.readTables();
            
            if (list != null)
            {
                list.Sort();
                ServerList.Items.Clear();
                foreach (string s in list.OrderBy(x=>x)) ServerList.Items.Add(s);
            }
            

            ServerList.SetSelected(index, true);
        }

        private void ListIndexChange(object sender, EventArgs e)
        {
            // display detailed information on selection
            try
            {
                index = ServerList.SelectedIndex;
                table = ServerList.SelectedItem.ToString();
                int s = SQL.lastStep(table);
                if (s == -1) SchritteAns.Text = "Liste ist leer";
                else SchritteAns.Text = "Schritte: " + Convert.ToString(s + 1);

                int i = SQL.starsCount(table);
                if (i == -1) SterneAns.Text = "Liste ist leer";
                else SterneAns.Text = "Sterne: " + Convert.ToString(i);
                newTableName.Text = table;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ServerList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    string messageBoxText = string.Format("Wollen sie die Tabelle {0} unwiederruflich löschen?", table);
                    string caption = "Tabelle löschen";
                    MessageBoxButtons button = MessageBoxButtons.YesNo;
                    MessageBoxIcon icon = MessageBoxIcon.Warning;
                    DialogResult res = MessageBox.Show(messageBoxText, caption, button, icon);

                    if (res == DialogResult.Yes) SQL.dropTable(table);
                    if (ServerList.SelectedIndex > 0) ServerList.SelectedIndex--;
                    ListRefresh_Tick(new object(), new EventArgs());
                    break;

                case Keys.D:
                    SQL.deleteStep(table, SQL.lastStep(table));
                    ListRefresh_Tick(new object(), new EventArgs());
                    break;
            }
        }

        private void AddTable_Click(object sender, EventArgs e)
        {
            string name = newTableName.Text;

            if (SQL.readTables().Contains(name))
            {
                string messageBoxText = string.Format("Sie sind im Begriff die Tabelle {0} zu überschreiben", name);
                string caption = "Tabelle Überschreiben";
                MessageBoxButtons button = MessageBoxButtons.YesNo;
                MessageBoxIcon icon = MessageBoxIcon.Warning;
                DialogResult res = MessageBox.Show(messageBoxText, caption, button, icon);

                if (res == DialogResult.Yes)
                {
                    SQL.dropTable(name);
                    SQL.addTable(name);

                    ListRefresh_Tick(new object(), new EventArgs());
                }
            }

            SQL.addTable(newTableName.Text);
            ListRefresh_Tick(new object(), new EventArgs());
            ServerList.SelectedItem = newTableName.Text;
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            // refresh icon
            ListRefresh_Tick(new object(), new EventArgs());
        }

        private void DataView_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"..\..\..\Dataview\bin\Debug\DataView.exe", table); // start Dataview with table as parameter
        }

        private void randomTable_Click(object sender, EventArgs e)
        {
            string name = newTableName.Text;

            // if (ServerList.Items.Contains(newTableName.Text))//check if input name already existes
            // name = ServerList.SelectedItem.ToString();
            /*else */
            if (SQL.readTables().Contains(name))
            {
                string messageBoxText = string.Format("Sie sind im Begriff die Tabelle {0} zu überschreiben", name);
                string caption = "Tabelle Überschreiben";
                MessageBoxButtons button = MessageBoxButtons.YesNo;
                MessageBoxIcon icon = MessageBoxIcon.Warning;
                DialogResult res = MessageBox.Show(messageBoxText, caption, button, icon);

                if (res == DialogResult.Yes)
                {
                    SQL.dropTable(name);
                    SQL.addTable(name);

                    Random form = new Random(name);
                    form.Show(); // start random Form and pass name as argument
                    ListRefresh_Tick(new object(), new EventArgs());
                }
            }
            else
            {
                name = newTableName.Text; // else crate new table
                SQL.addTable(name);

                Random form = new Random(name);
                form.Show(); // start random Form and pass name as argument
                ListRefresh_Tick(new object(), new EventArgs());
            }

            ServerList.SelectedItem = name;
        }

        private void ClusterSim_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            progressBar.Value = 0;

            XDMessagingClient client = new XDMessagingClient(); // https://github.com/TheCodeKing/XDMessaging.Net
            IXDListener listener = client.Listeners.GetListenerForMode(XDTransportMode.HighPerformanceUI);
            listener.RegisterChannel("steps");

            listener.MessageReceived += (o, ep) =>
                {
                    if (ep.DataGram.Channel == "steps")
                    {
                        // select channel
                        switch (ep.DataGram.Message.First().ToString())
                        {
                            case "s":
                                int n = Convert.ToInt32(
                                    ep.DataGram.Message.Remove(0, 1)); // check wether message is max or current value

                                progressBar.Maximum = n;
                                break;
                            case "i":

                                int m = Convert.ToInt32(ep.DataGram.Message.Remove(0, 1));
                                progressBar.Value = m;

                                break;
                            case "a":
                                if (ep.DataGram.Message == "abort") progressBar.Visible = false;
                                break;
                        }
                    }

                    if (!(progressBar.Value < progressBar.Maximum - 1))
                        progressBar.Visible = false; // make invisible after execution 
                };
            System.Diagnostics.Process.Start(@"ClusterSim.exe", table);
        }
        
        private void Check_Click(object sender, EventArgs e)
        {
            if (tableWorking(table)) MessageBox.Show("Fehlerfrei");
            else MessageBox.Show("Tabelle fehlerhaft");
        }

        private void Download_Click(object sender, EventArgs e)
        {
            if (table != null)
            {
                int von = 0, bis = 0;
                string inputvon = "Von";
                if (ShowInputDialog(ref inputvon) == DialogResult.OK) von = Convert.ToInt32(inputvon);
                string inputbis = "Bis";
                if (ShowInputDialog(ref inputbis) == DialogResult.OK) bis = Convert.ToInt32(inputbis);

                SaveFileDialog saveFileDialog1 = new SaveFileDialog(); // savefile dialouge
                saveFileDialog1.InitialDirectory = @"C:\";
                saveFileDialog1.FileName = table;
                saveFileDialog1.Title = "Save table as File";
                saveFileDialog1.CheckPathExists = true;
                saveFileDialog1.DefaultExt = "tsv";
                saveFileDialog1.Filter = "Tsv files (*.tsv)|*.tsv|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                string path = null;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = saveFileDialog1.FileName;
                    List<Star> list = new List<Star>();
                    progressBar.Visible = true;
                    progressBar.Maximum = SQL.lastStep(table);
                    for (int i = von; i <= bis; i++)
                    {
                        list.AddRange(SQL.readStars(table, i));
                        progressBar.Value = i;
                        Application.DoEvents();
                    }

                    progressBar.Value = 0;
                    progressBar.Maximum = list.Count;
                    double min = Math.Log(list.Min(x => x.Mass));
                    double max = Math.Log(list.Max(x => x.Mass));

                    List<string> lines = new List<string>();
                    /*List<int> colors = new List<int>();
                    System.Random int255 = new System.Random();
                    for (int i = 0; i < list.Count; i++)
                        colors.Add(65536 * int255.Next(255) + 256 * int255.Next(255) + int255.Next(255));*/

                    foreach (Star s in list)
                    {
                        lines.Add(s.ToTsv().Replace(',', '.') + "    " + 255*((Math.Log(s.Mass)-min)/(max-min)));
                        progressBar.Increment(1);
                        Application.DoEvents();
                    }

                    System.IO.File.WriteAllLines(path, lines);
                    progressBar.Value = 0;
                    progressBar.Visible = false;
                }
            }
        }
        
        private void ClusterAnalysis(object sender, EventArgs e)
        {
            var ana = new Analysis(this.table);
            ana.Show();
        }
    }
}