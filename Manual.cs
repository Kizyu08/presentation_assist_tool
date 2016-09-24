using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using OpenCvSharp.CPlusPlus;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using OpenCvSharp.Extensions;

namespace WindowsFormsApplication1
{
    public partial class Manual : Form
    {

        // 描画用Graphicsオブジェクト  
        private Graphics g = null;
        // クリック位置の描画用座標
        private Point[] point = new Point[4];
        private int clickcnt = 0;

        IplImage image;
        CvCapture capture = new CvCapture(0);



        public Manual()
        {

            InitializeComponent();

            image = Cv.QueryFrame(capture);		//撮影
            Cv.ReleaseImage(image);
            image = Cv.QueryFrame(capture);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            image = Cv.QueryFrame(capture);		//撮影
            Cv.ReleaseImage(image);
            image = Cv.QueryFrame(capture);

            Bitmap tomo = BitmapConverter.ToBitmap(image);
            pictureBox1.Image = tomo;
            Invalidate();
        }

        public void button2_Click(object sender, EventArgs e)
        {
            // クリックで取得した座標
            double[] a = new double[]{
                                point[0].X,point[0].Y,
                                point[1].X,point[1].Y,
                                point[2].X,point[2].Y,
                                point[3].X,point[3].Y
                                };

            Properties.Settings.Default.X1 = a[0];
            Properties.Settings.Default.Y1 = a[1];
            Properties.Settings.Default.X2 = a[2];
            Properties.Settings.Default.Y2 = a[3];
            Properties.Settings.Default.X3 = a[4];
            Properties.Settings.Default.Y3 = a[5];
            Properties.Settings.Default.X4 = a[6];
            Properties.Settings.Default.Y4 = a[7];

            DialogResult = DialogResult.OK;
            Close();

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            // クリックした座標を取得
            point[clickcnt].X = e.X;
            point[clickcnt].Y = e.Y;

            //System.Diagnostics.Debug.WriteLine("({0},{1}) {2}", point[clickcnt].X, point[clickcnt].Y, clickcnt);

            // クリックした位置に点を描画
            g = pictureBox1.CreateGraphics();
            g.FillEllipse(Brushes.Aqua, point[clickcnt].X, point[clickcnt].Y, 10, 10);

            // 点の間でLineを引く
            if (clickcnt != 0)
            {
                Pen p = new Pen(Color.Aqua, 3);
                g.DrawLine(p, point[clickcnt], point[clickcnt - 1]);
                p.Dispose();
            }
            if (clickcnt == point.Length - 1)
            {
                Pen p = new Pen(Color.Aqua, 3);
                g.DrawLine(p, point[clickcnt], point[(clickcnt + 1) % point.Length]);
                p.Dispose();
            }

            g.Dispose();

            clickcnt = (clickcnt + 1) % point.Length;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Manual_Load(object sender, EventArgs e)
        {
           
        }
    }
}
