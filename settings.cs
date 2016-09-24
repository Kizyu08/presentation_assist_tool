using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
namespace WindowsFormsApplication1
{
    public partial class settings : Form
    {
        Bitmap bmp1;
        Bitmap bmp2;
        Mat mat1;
        Mat mat2;


        public settings()
        {
            InitializeComponent();
        }

        

        private void settings_Load(object sender, EventArgs e)
        {
            color1();
            color2();

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private IplImage fnc(Mat mat,CvScalar scalar)
        {
            mat = new Mat(new CvSize(50, 50), MatrixType.U8C3, scalar);
            IplImage ipl = mat.ToIplImage();
            return ipl;
        }

        public void color1()
        {
            Double H1 = decimal.ToDouble(numericUpDown1.Value);
            Double S1 = decimal.ToDouble(numericUpDown2.Value);
            Double V1 = decimal.ToDouble(numericUpDown3.Value);

            int[] rgb1 = HSVtoRGB((int)H1, (int)S1, (int)V1);

            CvScalar hsv1 = new CvScalar(rgb1[2], rgb1[1], rgb1[0]);
            IplImage ipl1 =  fnc(mat1, hsv1);

            Bitmap bmp1 = BitmapConverter.ToBitmap(ipl1);
            pictureBox1.Image = bmp1;
        }

        public void color2()
        {
            Double H2 = decimal.ToDouble(numericUpDown4.Value);
            Double S2 = decimal.ToDouble(numericUpDown5.Value);
            Double V2 = decimal.ToDouble(numericUpDown6.Value);

            int[] rgb2 = HSVtoRGB((int)H2, (int)S2, (int)V2);
            
            CvScalar hsv2 = new CvScalar(rgb2[2], rgb2[1], rgb2[0]);
            IplImage ipl2 = fnc(mat2, hsv2);
            
            Bitmap bmp2 = BitmapConverter.ToBitmap(ipl2);
            pictureBox2.Image = bmp2;
        }

        public int[] HSVtoRGB(int h, int s, int v)
        {
            float f;
            int i, p, q, t;
            int[] rgb = new int[3];

            i = (int)Math.Floor(h / 60.0f) % 6;
            f = (float)(h / 60.0f) - (float)Math.Floor(h / 60.0f);
            p = (int)Math.Round(v * (1.0f - (s / 255.0f)));
            q = (int)Math.Round(v * (1.0f - (s / 255.0f) * f));
            t = (int)Math.Round(v * (1.0f - (s / 255.0f) * (1.0f - f)));

            switch (i)
            {
                case 0: rgb[0] = v; rgb[1] = t; rgb[2] = p; break;
                case 1: rgb[0] = q; rgb[1] = v; rgb[2] = p; break;
                case 2: rgb[0] = p; rgb[1] = v; rgb[2] = t; break;
                case 3: rgb[0] = p; rgb[1] = q; rgb[2] = v; break;
                case 4: rgb[0] = t; rgb[1] = p; rgb[2] = v; break;
                case 5: rgb[0] = v; rgb[1] = p; rgb[2] = q; break;
            }

            return rgb;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            color1();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            color1();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            color1();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            color2();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            color2();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            color2();
        }
        
    }
}
