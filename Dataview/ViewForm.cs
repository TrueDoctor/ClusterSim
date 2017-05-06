using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using AviFile;
using ClusterLib;


namespace Dataview
{
    public partial class ViewForm : Form
    {
        string table = "copy";
        int step = 0;
        List<List<Star>> Steps = new List<List<Star>>();
        Bitmap Canvas;
        //bool three = false;
        bool trace = false;
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
                        this.table = s;
                }

            }
            Canvas = new Bitmap(Box.Width, Box.Height);
            
        }

        private void Tasterdruck(object sender, KeyEventArgs e)
        {
            if (Box.Cursor!=Cursors.WaitCursor)
            switch(e.KeyCode)
            {
                case Keys.Right: step++;
                    break;

                case Keys.Left: step--;
                    break;

                case Keys.Up:
                        zoom*=2;
                        break;

                 case Keys.Down:
                        zoom/=2;
                        break;
                 case Keys.Enter:
                        export(2500);
                        break;

                    case Keys.T:
                        trace= !trace;
                        break;
                }
            draw();

        }

        private List<Star> import(int i)
        {
            try
            {
                //if (Misc.CountFiles(@"C:\Users\Dennis\Documents\Clustersim\")<=i&&i>=0);
                if (SQL.lastStep(table) <= i && i >= 0) ;
                List<Star> temp = SQL.readStars(table,i);//JsonConvert.DeserializeObject<List<Star>>(System.IO.File.ReadAllText(@"C:\Users\Dennis\Documents\Clustersim\file" + i + ".json"));
                return temp;
                
            }
            catch { }
            return null;
        }

        private void draw()
        {
            Box.Cursor = Cursors.WaitCursor;
            if (trace == true)
                Canvas = new Bitmap(Canvas, Box.Width, Box.Height);
            else
                Canvas = new Bitmap(Box.Width, Box.Height);

            /*for (int x = 0; x<Box.Width;x++)
                for (int y = 0; y < Box.Height; y++)
                {
                    Canvas.SetPixel(x, y, Color.Black);
                }*/
            List<Star> Stars = import(step);
            if (Stars!=null)
                foreach (Star s in Stars)
                {
                    Point p;
                    /*if (three == true)
                        p = mapAngle(s.pos.polar());      //Color new = (Color)ColorConverter.ConnvertFormString("#FFFFF");
                    else*/                
                        p = MapKoord(s.pos);

                    if (p != new Point(-1, -1))
                        Canvas.SetPixel(p.X, p.Y, Color.White);
                }
            Box.Image = Canvas;
            Box.Refresh();
            Box.Cursor = Cursors.Cross;
            this.Text = "Schritt: "+step;
            //Canvas.Save(@"C:\Users\Dennis\Documents\Clustersim\Pictures\file" + step + zoom+Canvas.Width+"x"+Canvas.Height + ".jpg",System.Drawing.Imaging.ImageFormat.Png);
        }

        private Point mapAngle(Vector a)
        {
            Point output = new Point(-1,-1);
            if (-80 < a.vec[1] && a.vec[1] < 80 && a.vec[2] < 80 && -80 < a.vec[2])
            {
                output.X = (int)((Box.Width/160.0) * a.vec[1] + Box.Width / 2);
                output.Y = (int)((Box.Height / 160.0) * a.vec[2] + Box.Height / 2);
            }
            
            return  output;

        }

        private Point MapKoord(Vector a)
        {
            Point output = new Point(-1, -1);
            
            output.X =(int) Math.Round(zoom * a.vec[0] + Box.Width / 2);
            output.Y = (int)Math.Round(zoom * a.vec[1] + Box.Height / 2);

            if (output.X > 0 && output.X < Box.Width && output.Y > 0 && output.Y < Box.Height)
                return output;
            else return new Point(-1,-1);


        }
        private void export(int frames)
        {
            Bitmap bitmap = (Bitmap)new Bitmap(Canvas);
            AviManager aviManager =
                new AviManager(@"..\..\testdata\new.avi", false);
            VideoStream aviStream =
                aviManager.AddVideoStream(true, 30, bitmap);

            try
            {
                for (int i = 1; i < frames; i++)
                {
                    step++;
                    draw();
                    Bitmap frame = (Bitmap)new Bitmap(Canvas);
                    aviStream.AddFrame(frame);
                    frame.Dispose();
                }
            }
            catch { }
            aviManager.Close();
        }

        private void Initialisierung(object sender, EventArgs e)
        {
            import(0);
            draw();
            
        }
    }
}
