using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using ClusterSim.ClusterLib.Calculation;


namespace ClusterSim.Dataview
{
    using System.Drawing.Imaging;
    using System.IO;

    using Accord.Video.FFMPEG;

    using ClusterSim.ClusterLib;
    using ClusterSim.ClusterLib.Analysis;
    using ClusterSim.ClusterLib.Utility;
    using ClusterSim.ClusterLib.Visualization;

    using Point = System.Drawing.Point;

    public partial class ViewForm : Form
    {
        string table = null;
        int step = 1;
        //List<List<Star>> Steps = new List<List<Star>>();
        List<Star> Stars = new List<ClusterLib.Calculation.Star>();
        Bitmap Canvas;
        int c = 0;
        //bool three = false;
        bool trace, towD, threeD, fancy, refresh = true;//trace the star position
        double zoom = 10;
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
                        table = s;//check if argument is valid
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
            if (Box.Cursor != Cursors.WaitCursor)
                switch (e.KeyCode)
                {
                    case Keys.Right:
                        step++; import(step);
                        break;

                    case Keys.Left:
                        step--; import(step);
                        break;

                    case Keys.U:
                        step += 10; import(step);
                        break;

                    case Keys.Z:
                        step -= 10; import(step);
                        break;

                    case Keys.Up:
                        zoom *= 2;
                        Canvas = new Bitmap(Box.Width - this.Box.Width % 2, Box.Height - this.Box.Height % 2, PixelFormat.Format32bppPArgb);
                        break;

                    case Keys.Down:
                        Canvas = new Bitmap(Box.Width - this.Box.Width % 2, Box.Height - this.Box.Height % 2, PixelFormat.Format32bppPArgb);
                        zoom /= 2;
                        break;
                    case Keys.Enter:
                        string frames = "frames";
                        var res = ShowInputDialog(ref frames);//open input dialouge
                        if (res == DialogResult.OK)
                            export(Convert.ToInt32(frames));

                        break;

                    case Keys.T:
                        trace = !trace;  //switch trace state
                        break;

                    case Keys.F:
                        fancy = !fancy;  //switch Fancy graphic mode
                        this.towD = false;
                        break;

                    case Keys.R:
                        this.refresh = !this.refresh;  //switch refreshing of max step
                        break;

                    case Keys.D2:
                        towD = !towD;
                        break;

                    case Keys.D3:
                        threeD = !threeD;
                        break;

                    case Keys.G:
                        string inputstep = "step";
                        if (ShowInputDialog(ref inputstep) == DialogResult.OK)
                            step = Convert.ToInt32(inputstep);
                        import(step);
                        break;
                        
                    case Keys.S://speicherung eines Frames
                        SavePicture();
                        break;

                    case Keys.A:
                        var test = SQL.lastStep(this.table);
                        GnuPlot.Set("autoscale z", "size square", "view equal xyz");
                        GnuPlot.Set("yrange [] writeback", "xrange [] writeback", "zrange [] writeback");
                        ViewPlot.SPlot(this.Stars, Parameters.Mass, table);
                        //GnuPlot.Unset("autoscale");
                        var temp = new List<Star>();
                        for (int i = 0; i < test; i++)
                        {
                            //GnuPlot.Set("xrange restore", "yrange restore", "zrange restore");
                            temp.AddRange(SQL.readStars(this.table, this.step++));
                            //ViewPlot.SPlot(this.Stars, Parameters.Mass, table);
                            Application.DoEvents();
                        }
                        GnuPlot.Set("xrange restore", "yrange restore", "zrange restore");
                        ViewPlot.SPlot(temp, Parameters.Mass, table);

                        break;


                }
            draw();

        }

        private void SavePicture()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();//savefile dialouge
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Screenshot";
            saveFileDialog1.FileName = table + "s" + step + "z" + zoom + "x" + Canvas.Width + "y" + Canvas.Height + ".png";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "jpg";
            saveFileDialog1.Filter = "Jpeg files (*.jpg)|*.jpg|Png files (*.png)|*.png|Bitmap files (*.bmp)|*.bmp|All files alle Bildformate unterstüzt (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;


            if (saveFileDialog1.ShowDialog() == DialogResult.OK && GetImageFormat(System.IO.Path.GetExtension(saveFileDialog1.FileName)) != null)
            {
                string filepath = saveFileDialog1.FileName;

                System.Drawing.Imaging.ImageFormat format = GetImageFormat(System.IO.Path.GetExtension(filepath).Replace(".", ""));
                if (format != null)
                    Canvas.Save(filepath, format);
            }
        }

        private static System.Drawing.Imaging.ImageFormat GetImageFormat(string format)
        {
            System.Drawing.Imaging.ImageFormat imageFormat = null;

            try
            {
                format.Replace(".", "");
                var imageFormatConverter = new ImageFormatConverter();
                imageFormat = (System.Drawing.Imaging.ImageFormat)imageFormatConverter.ConvertFromString(format.Replace(".", ""));
            }
            catch (Exception)
            {
                MessageBox.Show("Bildformat" + format.Replace(".", "") + " nicht unterstützt");
                //throw;
            }

            return imageFormat;
        }

        private void import(int i)
        {
            try
            {
                //if (Misc.CountFiles(@"C:\Users\Dennis\Documents\Clustersim\")<=i&&i>=0);

                if (SQL.lastStep(table) >= i && i >= 0) //if in in porpper range
                    Stars = SQL.readStars(table, i);//JsonConvert.DeserializeObject<List<Star>>(System.IO.File.ReadAllText(@"C:\Users\Dennis\Documents\Clustersim\file" + i + ".json"));


            }
            catch { }

        }

        private bool draw()
        {
            Application.DoEvents();

            Box.Cursor = Cursors.WaitCursor;
            //import(step);
            if (trace == true)//if trace = false reset bitmap
                Canvas = new Bitmap(Canvas, Box.Width - this.Box.Width % 2, Box.Height - this.Box.Height % 2);
            else
                Canvas = new Bitmap((Box.Width - this.Box.Width % 2)*1, (Box.Height - this.Box.Height % 2)*1, PixelFormat.Format24bppRgb);

            var fCanvas = Graphics.FromImage(this.Canvas);


            if (Stars != null)
                foreach (Star s in Stars)
                {
                    if (fancy)
                    {
                        var bit = s.CratePic();

                        var p = this.MapKoord(s.Pos);
                         p = new Point(p.X - bit.Height / 2, p.Y - bit.Width / 2);
                        fCanvas.DrawImage(bit, p);
                    }

                    else
                    {
                        System.Drawing.Point p;
                        
                        p = MapKoord(s.Pos); //translate koordinates

                        if (p != new System.Drawing.Point(-1, -1)) //if point in range
                            Canvas.SetPixel(p.X, p.Y, Color.White); //draw point
                    }
                }
            
            Box.Image = Canvas;
            Box.Refresh();
            Box.Cursor = Cursors.Cross;
            if (Stars != null)
                if (Stars.Count <= 1000 && this.refresh)
                    c = SQL.lastStep(table);
            Text = String.Format("{0}   Schritt: {1} von {2}", table, step, c);//change caption

            if (this.towD)
            {
                //ViewPlot.Plot(this.Stars, "[0.000000000000000000000000000000000001: 0.00000000000000001]", 600000, 80);
                ViewPlot.PlotXY(this.Stars, "[0.0000000000001: 100]");
            }

            if (this.threeD)
            {
                ViewPlot.SPlot(this.Stars, Parameters.Kinetic, $"{this.table} step {this.step}");
            }
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

        private System.Drawing.Point MapKoord(Vector a)
        {
            var output = new System.Drawing.Point(-1, -1);

            output.X = (int)Math.Round(zoom * a.vec[0] + this.Canvas.Width / 2);
            output.Y = (int)Math.Round(zoom * a.vec[1] + this.Canvas.Height / 2);

            if (output.X > 0 && output.X < this.Canvas.Width && output.Y > 0 && output.Y < this.Canvas.Height)//if output in range of Box
                return output;
            else return new System.Drawing.Point(-1, -1);


        }
        private void export(int frames)
        {
            Bitmap bitmap = (Bitmap)new Bitmap(Canvas);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();//savefile dialouge
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Video File";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "avi";
            saveFileDialog1.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            string path = null;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;

                /*AviManager aviManager =
                    new AviManager(path, false);
                VideoStream aviStream =
                    aviManager.AddVideoStream(true, 30, bitmap);

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
                aviManager.Close();/*/
                //MetricsToVideo(path, step, frames, Parameters.Energy);
                FfMpeg(path, frames);
            }
            else
                MessageBox.Show("Dateipfad fehlerhaft");
        }

        private void Initialisierung(object sender, EventArgs e)
        {
            import(0);
            draw();
            c = SQL.lastStep(table);

        }

        private void FfMpeg(string path, int frames)
        {
            var x = new Accord.Video.FFMPEG.VideoFileWriter();
            if (this.towD) x.Open(path, 1920, 1080, 24, VideoCodec.H264, 7200000);
            else
            {
                x.Open(path, this.Canvas.Width, this.Canvas.Height, 24, VideoCodec.H264, 50000000);
            }

            /*try
            {*/
            var range = SQL.readStars(this.table, this.step + frames).GetRadius() * 4;


            for (int i = 0; i < frames; i++) //add frames
            {
                Application.DoEvents();
                //import(++step);
                //this.towD = true;
                this.Stars = SQL.readStars(this.table, ++step);
                this.Stars.MoveCenter(this.Stars.GetCenter());
                Bitmap frame;
                if (towD)
                {
                    var savePath = path.Replace(".mp4", string.Empty) + $@"\{step}";
                    Directory.CreateDirectory(path.Replace(".mp4", string.Empty));

                    ViewPlot.PlotXY(this.Stars, "[0.0000000000001: 100]", range, savePath);
                    //ViewPlot.Plot(this.Stars, "[0.000000000000000000000000000000000001: 0.00000000000000001]", 800000, 80, savePath);

                    do
                        try
                        {
                            frame = Bitmap.FromFile(savePath + ".png", false) as Bitmap;
                        }
                        catch
                        {
                            frame = null;
                        }
                    while (frame == null);
                }
                else
                {
                    while (draw() == false);
                    frame = (Bitmap)new Bitmap(this.Box.Image);
                }

                try
                {
                    x.WriteVideoFrame(frame);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    step--;
                }
                frame.Dispose();
            }
            /*}
            catch(Exception e)
            {
                MessageBox.Show("Speichern fehlgeschlagen\n" + e.Message);
            }*/
            x.Close();
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
