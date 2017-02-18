using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AviFile; //https://www.codeproject.com/Articles/7388/A-Simple-C-Wrapper-for-the-AviFile-Library
using ClusterLib;


namespace Dataview
{
    public partial class ViewForm : Form
    {
        string table =null;
        int step = 1;
        //List<List<Star>> Steps = new List<List<Star>>();
        List<Star> Stars = new List<ClusterLib.Star>();
        Bitmap Canvas;
        //bool three = false;
        bool trace = false;//trace the star position
        decimal zoom = 10;
        public ViewForm()
        {
            InitializeComponent();
            List<String> list = SQL.readTables();
            if (list != null)
            {
                String[] parms = Environment.GetCommandLineArgs();

                foreach (string s in parms)
                {
                    //MessageBox.Show(s);
                    if (list.Contains(s))
                        this.table = s;//check if argument is valid
                }

            }
            DialogResult res;
            string name = "Tabellenname";
            
            if (table == null)
            {
                do
                    res = ShowInputDialog(ref name);//open input dialouge passing string as pointer
                while (res == DialogResult.OK && !(list.Contains(name)));
                table = name;//table = result
            }
            step = SQL.firstStep(table);//initialize step
            
        }

        private void Tasterdruck(object sender, KeyEventArgs e)
        {
            if (Box.Cursor!=Cursors.WaitCursor)
            switch(e.KeyCode)
            {
                case Keys.Right: step++; import(step);
                        break;

                case Keys.Left: step--; import(step);
                        break;

                case Keys.End:
                        step += 10; import(step);
                        break;

                case Keys.RControlKey:
                        step -= 10; import(step);
                        break;

                case Keys.Up:
                        zoom*=2;
                        break;

                 case Keys.Down:
                        zoom/=2;
                        break;
                 case Keys.Enter:
                        string frames = "frames";
                        var res = ShowInputDialog(ref frames);//open input dialouge
                        if (res==DialogResult.OK)
                            export(Convert.ToInt32(frames));
                        
                        break;

                    case Keys.T:
                        trace= !trace;//switch trace state
                        break;
                    
                    
                }
            draw();

        }

        private void import(int i)
        {
            try
            {
                //if (Misc.CountFiles(@"C:\Users\Dennis\Documents\Clustersim\")<=i&&i>=0);
                if (SQL.lastStep(table) >= i && i >= 0) //if in in porpper range
                    Stars = SQL.readStars(table,i);//JsonConvert.DeserializeObject<List<Star>>(System.IO.File.ReadAllText(@"C:\Users\Dennis\Documents\Clustersim\file" + i + ".json"));
                
                
            }
            catch { }
            
        }

        private bool draw()
        {

            Box.Cursor = Cursors.WaitCursor;
            //import(step);
            if (trace == true)//if trace = false reset bitmap
                Canvas = new Bitmap(Canvas, Box.Width, Box.Height);
            else
                Canvas = new Bitmap(Box.Width, Box.Height);
            
            
            if (Stars != null)
                foreach (Star s in Stars) 
                {
                    Point p;
                    /*if (three == true)
                        p = mapAngle(s.pos.polar());      //Color new = (Color)ColorConverter.ConnvertFormString("#FFFFF");
                    else*/                
                        p = MapKoord(s.pos);//translate koordinates

                    if (p != new Point(-1, -1))//if point in range
                        Canvas.SetPixel(p.X, p.Y, Color.White);//draw point
                }
            Box.Image = Canvas;
            Box.Refresh();
            Box.Cursor = Cursors.Cross;
            this.Text = String.Format("Schritt: {0} von {1}",step, SQL.lastStep(table));//change caption

            if (Stars != null)
                return true;
            else
                return false;

            //Canvas.Save(@"C:\Users\Dennis\Documents\Clustersim\Pictures\file" + step + zoom+Canvas.Width+"x"+Canvas.Height + ".jpg",System.Drawing.Imaging.ImageFormat.Png);
        }

        /*private Point mapAngle(Vector a)//3d maping to ignore
        {
            Point output = new Point(-1,-1);
            if (-80 < a.vec[1] && a.vec[1] < 80 && a.vec[2] < 80 && -80 < a.vec[2])
            {
                output.X = (int)((Box.Width/160.0m) * a.vec[1] + Box.Width / 2);
                output.Y = (int)((Box.Height / 160.0m) * a.vec[2] + Box.Height / 2);
            }
            
            return  output;

        }*/

        private Point MapKoord(Vector a)
        {
            Point output = new Point(-1, -1);
            
            output.X =(int) Math.Round(zoom * a.vec[0] + Box.Width / 2);
            output.Y = (int)Math.Round(zoom * a.vec[1] + Box.Height / 2);

            if (output.X > 0 && output.X < Box.Width && output.Y > 0 && output.Y < Box.Height)//if output in range of Box
                return output;
            else return new Point(-1,-1);


        }
        private void export(int frames)
        {
            Bitmap bitmap = (Bitmap)new Bitmap(Canvas);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();//savefile dialouge
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Video File";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "avi";
            saveFileDialog1.Filter = "Avi files (*.avi)|*.avi|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            string path=null;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;

                AviManager aviManager =
                    new AviManager(path, false);
                VideoStream aviStream =
                    aviManager.AddVideoStream(true, 30/*framerate*/, bitmap);

                try
                {
                    for (int i = 1; i < frames; i++)//add frames
                    {
                        import(++step);
                        while (draw() == false) ;
                        Bitmap frame = (Bitmap)new Bitmap(Canvas);
                        aviStream.AddFrame(frame);
                        frame.Dispose();
                    }
                }
                catch
                {
                    MessageBox.Show("Speichern fehlgeschlagen");
                }
                aviManager.Close();
            }
            else
                MessageBox.Show("Dateipfad fehlerhaft");
        }

        private void Initialisierung(object sender, EventArgs e)
        {
            import(0);
            draw();
            
        }

        private static DialogResult ShowInputDialog(ref string input)//http://stackoverflow.com/questions/97097/what-is-the-c-sharp-version-of-vb-nets-inputdialog
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            System.Windows.Forms.TextBox textBox = new TextBox();
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
    }
}
