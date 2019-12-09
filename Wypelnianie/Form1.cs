using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Wypelnianie
{
    public partial class Form1 : Form
    {
        Color[,] constBitmap;
        Color[,] normalBitmap;
        int height = 5, width = 5, z=1;
        List<Point> points = new List<Point>();
        int movingVertex = -1, m = 5, generateColor = 0;
        bool moving = false, losowe = false, animateL = false;
        List<Triangle> grid = new List<Triangle>();
        bool nFromMap = false, paint = true, tekstura = true;
        double ks = 0.5, kd = 0.5;
        (int, int, int) light;
        double t = 1;
        int tzz;
        int wavedistance = 40;
        int constwavde = 15;
        bool torus = false;
        int xclick=0, yclick = 0;
        Random rand = new Random();
        public Form1()
        {
            InitializeComponent();
            timer2.Start();

            constBitmap = new Color[pictureBox1.Width, pictureBox1.Height];
            normalBitmap = new Color[pictureBox1.Width, pictureBox1.Height];

            using (var constbmp = new Bitmap("pattern.jpg"))
            {
                using (var normalbmp = new Bitmap("normal1.jpg"))
                {
                    for (int i = 0; i < pictureBox1.Width; i++)
                    {
                        for (int j = 0; j < pictureBox1.Height; j++)
                        {
                            constBitmap[i, j] = constbmp.GetPixel(i, j);
                            normalBitmap[i, j] = normalbmp.GetPixel(i, j);
                        }
                    }
                }
            }

            pictureBox2.BackColor = Color.White;
            pictureBox3.BackColor = Color.Yellow;

            SetPoints();
            SetGrid();
        }

        private void SetPoints()
        {
            points = new List<Point>();
            int a = (pictureBox1.Width - 80) / width;
            int b = (pictureBox1.Height - 80) / height;

            int startx = 40;
            int starty = 40;

            for (int i = 0; i <= height; i++) // set points
            {
                for (int j = 0; j <= width; j++)
                {
                    points.Add(new Point(startx, starty));
                    startx += a;
                }
                startx = 40;
                starty += b;
            }
        }

        public void SetGrid()
        {
            int k = 0;
            grid = new List<Triangle>();
            for (int i = 0; i < height; i++) //set grid
            {
                for (int j = 0; j <= width; j++)
                {
                    if (j != width)
                    {
                        Triangle t = new Triangle();
                        t.b = points[k];
                        t.a = points[k + 1];
                        t.c = points[k + width + 1];
                        t.SetEdges(rand);
                        grid.Add(t);
                    }
                    if (j != 0)
                    {
                        Triangle t = new Triangle();
                        t.b = points[k];
                        t.a = points[k + width];
                        t.c = points[k + width + 1];
                        t.SetEdges(rand);
                        grid.Add(t);
                    }
                    k++;
                }
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            movingVertex = -1;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                points[movingVertex] = new Point(e.X, e.Y);
                SetGrid();
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int i = 0;
            foreach (var p in points) //finding clicked vertex
            {
                if (e.X <= p.X + 3 && e.X >= p.X - 3 && e.Y <= p.Y + 3 && e.Y >= p.Y - 3)
                {
                    movingVertex = i;
                }
                i++;
            }
            if (movingVertex != -1)
            {
                moving = true;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (paint)
            {
                var colorPaint = FillTriangle();
                using (Bitmap actualBmp = new Bitmap(pictureBox1.Width, pictureBox1.Height))
                {
                    unsafe
                    {
                        BitmapData bmpData = actualBmp.LockBits(new Rectangle(0, 0, actualBmp.Width, actualBmp.Height), ImageLockMode.ReadWrite, actualBmp.PixelFormat);

                        byte* firstPixel = (byte*)bmpData.Scan0;

                        int heightpx = bmpData.Height;
                        int bytespx = System.Drawing.Bitmap.GetPixelFormatSize(actualBmp.PixelFormat) / 8;

                        int widthbytes = bmpData.Width * bytespx;

                        Parallel.For(0, heightpx, x =>
                        {
                            byte* currentLine = firstPixel + (x * bmpData.Stride);

                            for (int j = 0; j < widthbytes; j += bytespx)
                            {
                                currentLine[j] = colorPaint[j / 4, x].B;
                                currentLine[j + 1] = colorPaint[j / 4, x].G;
                                currentLine[j + 2] = colorPaint[j / 4, x].R;
                                currentLine[j + 3] = colorPaint[j / 4, x].A;
                            }
                        }
                        );

                        actualBmp.UnlockBits(bmpData);
                    }

                    e.Graphics.DrawImage(actualBmp, 0, 0);
                }

                int i = 0;
                foreach (var p in points)
                {
                    if ((i + 1) % (width + 1) != 0 || i == 0)
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), p, points[i + 1]);
                    }
                    if (!(i >= (((width + 1) * (height + 1)) - width - 1)) && i <= ((width + 1) * (height + 1) - 1))
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), p, points[i + width + 1]);
                    }
                    if (i > width && (i + 1) % (width + 1) != 0)
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), p, points[i - width]);
                    }
                    i++;
                }
            }
        }

        private double DlugoscOdcinka(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        private ((double, double, double), (double, double, double), (double, double, double)) CountNForVertexes(Point a, Point b, Point c)
        {
            (double, double, double) Na = (0, 0, 1);
            (double, double, double) Nb = (0, 0, 1);
            (double, double, double) Nc = (0, 0, 1);

            if (nFromMap)
            {
                Na = ((double)normalBitmap[a.X, a.Y].R, (double)normalBitmap[a.X, a.Y].G, (double)normalBitmap[a.X, a.Y].B);
                Nb = ((double)normalBitmap[b.X, b.Y].R, (double)normalBitmap[b.X, b.Y].G, (double)normalBitmap[b.X, b.Y].B);
                Nc = ((double)normalBitmap[c.X, c.Y].R, (double)normalBitmap[c.X, c.Y].G, (double)normalBitmap[c.X, c.Y].B);
            }

            return (Na, Nb, Nc);
        }
        private Color[,] FillTriangle()
        {
            Color[,] paintingColors = new Color[pictureBox1.Width, pictureBox1.Height];
            Parallel.ForEach(grid, t =>
            {
                double _ks = ks;
                double _kd = kd;
                (double, double, double) I = (1, 1, 1);
                (double, double, double) I1 = (1, 1, 1);
                (double, double, double) I2 = (1, 1, 1);
                (double, double, double) I3 = (1, 1, 1);
                ((double, double, double), (double, double, double), (double, double, double)) N = ((0, 0, 1), (0, 0, 1), (0, 0, 1));
                if (losowe)
                {
                    _kd = t.KAS;
                    _ks = t.KAD;
                }
                if (generateColor == 1) //interpolacyjne
                {
                    I1 = CountI(t.a.X, t.a.Y, _ks, _kd);
                    I2 = CountI(t.b.X, t.b.Y, _ks, _kd);
                    I3 = CountI(t.c.X, t.c.Y, _ks, _kd);
                }
                if (generateColor == 2)//hybrydowe
                {
                    I1 = CountI(t.a.X, t.a.Y, _ks, _kd);
                    I2 = CountI(t.b.X, t.b.Y, _ks, _kd);
                    I3 = CountI(t.c.X, t.c.Y, _ks, _kd);
                    N = CountNForVertexes(t.a, t.b, t.c);
                }
                int ymax = 0, ymin = pictureBox1.Height;

                foreach (var e in t.edges)
                {
                    if (e.end.Y > ymax)
                        ymax = e.end.Y;
                    if (e.start.Y < ymin)
                        ymin = e.start.Y;
                }

                List<List<Edge>> listET = new List<List<Edge>>();

                for (int i = ymin; i <= ymax; i++)
                    listET.Add(new List<Edge>());

                foreach (var e in t.edges)
                {
                    if (e.start.Y != e.end.Y) // pomijamy poziome
                        listET[e.start.Y - ymin].Add(e);
                }

                int y = ymin;
                List<Edge> listAET = new List<Edge>();
                while ((y <= ymax))
                {
                    foreach (var e in listET[y - ymin])
                    {
                        e.x = e.start.X + (double)((double)((double)(e.end.X - e.start.X) / (double)(e.end.Y - e.start.Y)) * (double)(y - e.start.Y)); //punkt przeciecia
                        listAET.Add(e);
                    }

                    listAET = listAET.OrderBy(e => e.x).ToList();
                    for (int i = 0; i < listAET.Count() - 1; i += 2)
                    {
                        for (int j = (int)Math.Floor(listAET[i].x); j <= Math.Ceiling(listAET[i + 1].x); j++)
                        {
                            if (generateColor == 0)
                            {
                                I = CountI(j, y, _ks, _kd);
                            }
                            else if (generateColor == 1)
                            {
                                double w1 = DlugoscOdcinka(t.a.X, t.a.Y, j, y);
                                double w2 = DlugoscOdcinka(t.b.X, t.b.Y, j, y);
                                double w3 = DlugoscOdcinka(t.c.X, t.c.Y, j, y);
                                I = ((I1.Item1 * w1 + I2.Item1 * w2 + I3.Item1 * w3) / (w1 + w2 + w3), (I1.Item2 * w1 + I2.Item2 * w2 + I3.Item2 * w3) / (w1 + w2 + w3), (I1.Item3 * w1 + I2.Item3 * w2 + I3.Item3 * w3) / (w1 + w2 + w3));
                                I.Item1 = CheckColor(I.Item1);
                                I.Item2 = CheckColor(I.Item2);
                                I.Item3 = CheckColor(I.Item3);
                            }
                            else if (generateColor == 2)
                            {
                                (double, double, double) colors = CountColor((double)t.a.X, (double)t.a.Y, (double)t.b.X, (double)t.b.Y, (double)t.c.X, (double)t.c.Y, (double)j, (double)y);
                                I = CountIHybrid(j, y, colors, N, I1, I2, I3, _ks, _kd);
                            }
                            paintingColors[j, y] = Color.FromArgb((int)I.Item1, (int)I.Item2, (int)I.Item3);
                        }
                    }

                    listAET.RemoveAll(e => e.end.Y == y + 1);

                    y++;

                    foreach (var e in listAET)
                    {
                        e.x += (double)((double)(e.end.X - e.start.X) / (double)(e.end.Y - e.start.Y));
                    }
                }
            });
            return paintingColors;


        }

        private (double, double, double) MultiplyVectors((double, double, double) a, (double, double, double) b)
        {
            (double, double, double) result;
            result.Item1 = a.Item2 * b.Item3 - a.Item3 * b.Item2;
            result.Item2 = a.Item3 * b.Item1 - a.Item1 * b.Item3;
            result.Item3 = a.Item1 * b.Item2 - a.Item2 * b.Item1;
            return result;
        }
        private (double, double, double) MultiplyVectorAndScalar((double, double, double) a, double b)
        {
            (double, double, double) result;
            result.Item1 = a.Item2 * b;
            result.Item2 = a.Item2 * b;
            result.Item3 = a.Item3 * b;
            return result;
        }
        private (double, double, double) AddVectors((double, double, double) a, (double, double, double) b)
        {
            (double, double, double) result;
            result.Item1 = a.Item1 + b.Item1;
            result.Item2 = a.Item2 + b.Item2;
            result.Item3 = a.Item3 + b.Item3;
            return result;
        }
        private double cosinus((double, double, double) a, (double, double, double) b)
        {
            a = Normalize(a);
            b = Normalize(b);
            return a.Item1 * b.Item1 + a.Item2 * b.Item2 + a.Item3 * b.Item3;
        }
        private double CheckColor(double c)
        {
            if (c < 0)
                return 0;
            if (c > 255)
                return 255;
            else
                return c;
        }
        private (double, double, double) CountColor(double ax, double ay, double bx, double by, double cx, double cy, double x, double y)
        {
            double alfa = (x - cx - (cy - y) / (cy - by) * bx - (y - cy) / (cy - by) * cx) / (ax + (ay - cy) / (cy - by) * bx - cx - (ay - cy) / (cy - by) * cx);
            double beta = alfa * (ay - cy) / (cy - by) + (cy - y) / (cy - by);
            double gamma = 1 - alfa - beta;
            (double, double, double) result = (alfa, beta, gamma);
            return result;
        }
        private (double, double, double) Normalize((double, double, double) v)
        {
            (double, double, double) result;
            double d = Math.Sqrt(v.Item1 * v.Item1 + v.Item2 * v.Item2 + v.Item3 * v.Item3);
            result.Item1 = v.Item1 / d;
            result.Item2 = v.Item2 / d;
            result.Item3 = v.Item3 / d;
            return result;
        }
        private (double, double, double) Converse((double, double, double) v)
        {
            (double, double, double) result = ((-127 + v.Item1) / 127, -(-127 + v.Item2) / 127, (v.Item3 / 255));
            return Normalize(result);
        }

        private (double, double, double) CountI(int x, int y, double ks, double kd)
        {
            (double, double, double) N = (0, 0, 1);
       
            if(torus && xclick!=0 && yclick!=0 && DlugoscOdcinka(xclick, yclick, x, y)>wavedistance-constwavde && DlugoscOdcinka(xclick, yclick, x, y) <wavedistance+constwavde)
            {
                double h = Math.Sqrt(Math.Pow(constwavde, 2) - Math.Pow(DlugoscOdcinka(xclick, yclick, x, y) - wavedistance, 2));

                //if (x < xclick && y > yclick)
                //    N = (-(wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), -(wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), h);

                //else if (x < xclick && y < yclick)
                //    N = (-(wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), (wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), h);
                //else if (x > xclick && y > yclick)
                //    N = ((wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), -(wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), h);

                //else
                {
                    N = ((
                        wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), (wavedistance - DlugoscOdcinka(xclick, yclick, x, y)), h);

                }
                N = Normalize(N);
            }
            if (nFromMap)
            {
                N = ((double)normalBitmap[x, y].R, (double)normalBitmap[x, y].G, (double)normalBitmap[x, y].B);
                N = Converse(N);
            }
            (double, double, double) V = (0, 0, 1);
            (double, double, double) L = (0, 0, 1);
            if (animateL)
            {
                L = (light.Item1 - x, light.Item2 - y, light.Item3);
            }
            L = Normalize(L);
            (double, double, double) Il = ((double)pictureBox2.BackColor.R / 255, (double)pictureBox2.BackColor.G / 255, (double)pictureBox2.BackColor.B / 255);
            (double, double, double) Io = ((double)constBitmap[x, y].R, (double)constBitmap[x, y].G, (double)constBitmap[x, y].B);

            if (!tekstura)
            {
                Io = ((double)pictureBox3.BackColor.R, (double)pictureBox3.BackColor.G, (double)pictureBox3.BackColor.B);
            }
            (double, double, double) R = (2 * cosinus(N, L) * N.Item1 - L.Item1, 2 * cosinus(N, L) * N.Item2 - L.Item2, 2 * cosinus(N, L) * N.Item3 - L.Item3);
            R = Normalize(R);

            (double, double, double) I = (0, 0, 0);
            I.Item1 = kd * Il.Item1 * Io.Item1 * cosinus(N, L) + ks * Il.Item1 * Io.Item1 * Math.Pow(cosinus(V, R), m);
            I.Item2 = kd * Il.Item2 * Io.Item2 * cosinus(N, L) + ks * Il.Item2 * Io.Item2 * Math.Pow(cosinus(V, R), m);
            I.Item3 = kd * Il.Item3 * Io.Item3 * cosinus(N, L) + ks * Il.Item3 * Io.Item3 * Math.Pow(cosinus(V, R), m);

            I.Item1 = CheckColor(I.Item1);
            I.Item2 = CheckColor(I.Item2);
            I.Item3 = CheckColor(I.Item3);

            return I;
        }

        private (double, double, double) CountIHybrid(int x, int y, (double, double, double) colors, ((double, double, double), (double, double, double), (double, double, double)) Nabc, (double, double, double) I1, (double, double, double) I2, (double, double, double) I3, double ks, double kd)
        {
            (double, double, double) N = (0, 0, 1);
            if (nFromMap)
            {
                N = AddVectors(AddVectors(MultiplyVectorAndScalar(Nabc.Item1, colors.Item1), MultiplyVectorAndScalar(Nabc.Item2, colors.Item2)), MultiplyVectorAndScalar(Nabc.Item3, colors.Item3));
                N = Converse(N);
            }

            (double, double, double) V = (0, 0, 1);
            (double, double, double) L = (0, 0, 1);
            if (animateL)
            {
                L = (light.Item1 - x, light.Item2 - y, light.Item3);
                L = Normalize(L);
            }
            (double, double, double) Il = (pictureBox2.BackColor.R / 255, pictureBox2.BackColor.G / 255, pictureBox2.BackColor.B / 255);

            (double, double, double) Io = ((double)pictureBox3.BackColor.R, (double)pictureBox3.BackColor.G, (double)pictureBox3.BackColor.B);
            if (tekstura)
            {
                Io.Item1 = I1.Item1 * colors.Item1 + I2.Item1 * colors.Item2 + I3.Item1 * colors.Item3;
                Io.Item2 = I1.Item2 * colors.Item1 + I2.Item2 * colors.Item2 + I3.Item2 * colors.Item3;
                Io.Item3 = I1.Item3 * colors.Item1 + I2.Item3 * colors.Item2 + I3.Item3 * colors.Item3;

            }

            (double, double, double) R = (2 * cosinus(N, L) * N.Item1 - L.Item1, 2 * cosinus(N, L) * N.Item2 - L.Item2, 2 * cosinus(N, L) * N.Item3 - L.Item3);
            R = Normalize(R);

            (double, double, double) I = (0, 0, 0);
            I.Item1 = kd * Il.Item1 * Io.Item1 * cosinus(N, L) + ks * Il.Item1 * Io.Item1 * Math.Pow(cosinus(V, R), m);
            I.Item2 = kd * Il.Item2 * Io.Item2 * cosinus(N, L) + ks * Il.Item2 * Io.Item2 * Math.Pow(cosinus(V, R), m);
            I.Item3 = kd * Il.Item3 * Io.Item3 * cosinus(N, L) + ks * Il.Item3 * Io.Item3 * Math.Pow(cosinus(V, R), m);

            I.Item1 = CheckColor(I.Item1);
            I.Item2 = CheckColor(I.Item2);
            I.Item3 = CheckColor(I.Item3);

            return I;
        }

        //buttons settings
        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            animateL = radioButton10.Checked;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            double x = pictureBox1.Width / 2.0 * Math.Sin(t + 5 * Math.PI / 2) + pictureBox1.Width / 2;
            double y = pictureBox1.Height / 2.0 * Math.Sin(4 * t) + pictureBox1.Height / 2;
            t += 0.01;
            wavedistance++;
            if (tzz > 150) z = -1;
            if (tzz < 5) z = 1;
 
            tzz += z;

            light = ((int)x, (int)y, tzz);

            pictureBox1.Invalidate();
        }

   
        private void Form1_Load(object sender, EventArgs e)
        {
            light = (pictureBox1.Width / 2, pictureBox1.Width / 2, 50);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            width = (int)numericUpDown2.Value;
            SetPoints();
            SetGrid();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                generateColor = 0; //dokladne
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                generateColor = 1; //interpolacyjne
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
    
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            xclick = e.X;
            yclick = e.Y;
            wavedistance = 40;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            torus = radioButton12.Checked;
            xclick = 0;
            yclick = 0;
            nFromMap = false;

        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {
                generateColor = 2; //hybrydowe
            }
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            SetGrid();
            losowe = radioButton8.Checked;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //paint = false;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.BackColor = colorDialog1.Color;
            }
            //paint = true;
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            m = (int)numericUpDown3.Value;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            tekstura = radioButton4.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            height = (int)numericUpDown1.Value;
            SetPoints();
            SetGrid();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            kd = trackBar1.Value / 10.0;
            label4.Text = kd.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            ks = trackBar2.Value / 10.0;
            label3.Text = ks.ToString();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            nFromMap = !radioButton1.Checked;
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.BackColor = colorDialog1.Color;
            }
        }
    }
}
