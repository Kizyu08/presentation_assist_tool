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


//test

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {        
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        IplImage image = new IplImage();
        Timer t;
        Timer t1;
        //Timer test;
        public int sx1, sy1;                          // 始点
        int aiu;
        CvCapture capture = new CvCapture(0);
        private Bitmap capuutu;
        IplImage runruntattta;

        System.Diagnostics.Stopwatch MyStopWatch
        {
            set;
            get;
        }

        Point PastPoint
        {
            set;
            get;
        }

        void writeLog(String logText)
        {
            textBox_Log.SelectionStart = textBox_Log.Text.Length;
            textBox_Log.SelectionLength = 0;
            textBox_Log.SelectedText = "[" + System.DateTime.Now.ToString() + "]" + logText + "\r\n";
        }

        public Form1()
        {
            MyStopWatch = new System.Diagnostics.Stopwatch();

            this.WindowState = FormWindowState.Maximized;
            //pictureBox1.Size = new Size(960, 720);
            PastPoint = new Point();
            
            //撮影サイズ
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameWidth, 960);
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameHeight, 720);

            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.5;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "プレゼンテーション|*.pptx;ppt|すべてのファイル|*.*";
            ofd.Title = "使用するスライドを選択してください";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき
                //選択されたスライドで開始する
                Process.Start(ofd.FileName);
                writeLog("スライドを開始しました");
            }

            t = new Timer();
            t.Interval = 2500;                  //修正:500→5000
            t.Tick += new EventHandler(un);
            t.Start();
            writeLog("タイマーt開始");
            
        }

        private void un(object sender, EventArgs e)
        {
            //t.Stop();   

            writeLog("F5キーを送信します...");
            SendKeys.Send("{F5}");
            TopMost = true;
            //this.Focus();
           
            t1 = new Timer();
            t1.Tick += new EventHandler(uun);
            t1.Interval = 3000; // ミリ秒単位で指定 修正:300→3000
            t1.Start();
            writeLog("タイマーt1開始");
        }

        private void uun(object sender, EventArgs e)
        {
            t.Stop();
            writeLog("タイマーt停止");
            t1.Stop();
            writeLog("タイマーt1停止");

            //using (var cap = Cv.CreateCameraCapture(0)) // カメラのキャプチャ
            //{
                //IplImage im = new IplImage();           // カメラ画像格納用の変数
                //while (Cv.WaitKey(1) == -1)             // 任意のキーが入力されるまでカメラ映像を表示
                //{
                //    im = Cv.QueryFrame(capture);            // カメラからフレーム(画像)を取得
                //    Cv.ShowImage("Test", im);           //  カメラ映像の表示
                //    //Bitmap iplim = BitmapConverter.ToBitmap(im);
                //    //pictureBox2.Image = iplim;
                //}
            //}

            //this.Focus();

            image = Cv.QueryFrame(capture);		//撮影
            Cv.ReleaseImage(image);
            image = Cv.QueryFrame(capture);
            writeLog("撮影し、imageに格納しました");
            

            timer.Start();
            writeLog("タイマーtimer開始");
            
            Bitmap myBitmap = BitmapConverter.ToBitmap(image);
            pictureBox2.Image = myBitmap;
            writeLog("imageをBitmap化、右上のボックスに表示します");

            CvMemStorage storage = new CvMemStorage(0);
            
            IplImage srcImg = (OpenCvSharp.IplImage)BitmapConverter.ToIplImage(myBitmap);
            IplImage srcImgGray = new IplImage(srcImg.Size, BitDepth.U8, 1);
            IplImage tmpImg = new IplImage(srcImg.Size, BitDepth.U8, 1);
            {
                Cv.CvtColor(srcImg, srcImgGray, ColorConversion.BgrToGray);

                // (1)画像の二値化と輪郭の検出
                Cv.Threshold(srcImgGray, tmpImg, 90, 255, ThresholdType.Binary);
                // Cv.Threshold(srcImgGray,tmpImg, 0, 255, ThresholdType.Otsu);

                CvSeq<CvPoint> contours;
                Cv.FindContours
                    (tmpImg, storage, out contours,
                    CvContour.SizeOf,
                    ContourRetrieval.Tree,
                    ContourChain.ApproxSimple
                    );
                contours = Cv.ApproxPoly(contours, CvContour.SizeOf, storage, CvConst.CV_POLY_APPROX_DP, 10, true);
                // (2)ツリーノードイテレータの初期化
                writeLog("ツリーノードイテレータを初期化しています...");
                CvTreeNodeIterator<CvSeq<CvPoint>> it = new CvTreeNodeIterator<CvSeq<CvPoint>>(contours, 1);
                    
                foreach (CvSeq<CvPoint> contour in it)
                {
                    //fs.StartWriteStruct("contour", NodeType.Seq);
                    // (4)輪郭を構成する頂点座標を取得
                    CvPoint tmp = contour[-1].Value;
                    int vertex_count = contours.Total;
                    writeLog("頂点が" + vertex_count + "個検出されました");
                    for (int i = 0; i < contour.Total; i++)
                    {
                        int count = i + 1 ;
                        writeLog(count + "回目のトライです");
                        writeLog("キャリブレーションを試みます...");
                        if (contour.Total == 4 && contours.Total ==1)
                        {
                            writeLog("4つの頂点と1の矩形が検出されました");
                            writeLog("射影変換処理を開始します...");
                            CvPoint point = contour[i].Value;
                            srcImg.Line(tmp, point, CvColor.Blue, 2);

                            tmp = point;

                            CvPoint a = contour[1].Value;
                            CvPoint b = contour[2].Value;
                            CvPoint c = contour[3].Value;
                            CvPoint d = contour[4].Value;
                            
                            //src画像用の座標
                            double[] A = new double[]{  a.X, a.Y,
                                                        b.X, b.Y,
                                                        c.X, c.Y,
                                                        d.X, d.Y
                                                        };

                            // 変換先の頂点の座標
                            double[] B = new double[]{    0,   0,
                                                          0, 720,
                                                        960, 720,
                                                        960,   0
                                                        };
                            //行列として管理
                            CvMat Mata = new CvMat(4, 2, MatrixType.F64C1, A);
                            CvMat Matb = new CvMat(4, 2, MatrixType.F64C1, B);

                            IplImage dst = Cv.CreateImage(Cv.Size(960,720), srcImg.Depth, srcImg.NChannels);

                            //ホモグラフィ行列
                            CvMat homography = new CvMat(3, 3, MatrixType.F64C1);

                            //射影変換
                            Cv.FindHomography(Mata, Matb, homography);
                            writeLog("射影変換しています...");

                            //透視投影
                            Cv.WarpPerspective(srcImg, dst, homography, Interpolation.Cubic);
                            writeLog("透視投影しています...");

                        }
                        else
                        {
                            writeLog("条件を満たさなかったため、射影変換をスキップします");
                        }

                        
                        timer.Stop();
                        writeLog("タイマーtimerを停止");

                        label1.Text = timer.Elapsed.ToString();

                        IplImage imgTracking = Cv.CreateImage(new CvSize(image.Width, image.Height), BitDepth.U8, 3);
                        
                        Cv.Zero(imgTracking);
                        
                        IplImage imgHSV = Cv.CreateImage(new CvSize(image.Width, image.Height), BitDepth.U8, 3);
                        
                        Cv.CvtColor(image, imgHSV, ColorConversion.BgrToHsv);
                        
                        IplImage imgTo = Cv.CreateImage(new CvSize(imgHSV.Width, imgHSV.Height), BitDepth.U8, 1);
                        //色指定
                        Cv.InRangeS(imgHSV, new CvScalar(5, 50, 50), new CvScalar(15, 255, 255), imgTo);
                            
                        CvMoments moments = new CvMoments(imgTo, false);
                        
                        //指定色の面積取得
                        double area = Cv.GetCentralMoment(moments, 0, 0);

                        double moments01 = Cv.GetSpatialMoment(moments, 0, 1);
                        double moments10 = Cv.GetSpatialMoment(moments, 1, 0);

                                             
                        {
                            //重心座標取得
                            int posX = (int)(moments10 / area);
                            int posY = (int)(moments01 / area);

                            if ((PastPoint.X != posX) || (PastPoint.Y != posY))
                            {
                                if (MyStopWatch.IsRunning == true)
                                {
                                    MyStopWatch.Stop();
                                }

                                float nowX = Math.Abs((float)(posX - PastPoint.X)); //X成分の距離    //移動距離
                                float nowY = Math.Abs((float)(posY - PastPoint.Y)); //Y成分の距離

                                float nowXY = (float)Math.Sqrt((double)((nowX * nowX) + (nowY * nowY)));

                                float MouseSpeed = (nowXY / (float)(MyStopWatch.ElapsedMilliseconds));
                                
                                PastPoint = new Point(posX, posY);

                                label2.Text = "移動距離X" + nowX.ToString();
                                label3.Text = "移動距離Y" + nowY.ToString();
                                label4.Text = "面積" + area.ToString();
                                label5.Text = "重心X" + posX.ToString();
                                label6.Text = "重心Y" + posY.ToString();
                                label7.Text = "カーソル速度" + MouseSpeed.ToString();
                                        
                                MyStopWatch.Restart();
                                
                                //TopMost = true;
                                        
                                //位置が0でなく、移動速度が0.4～1.6pixel/msec        
                                if (PastPoint.X >= 0 && PastPoint.Y >= 0 && posX >= 0 && posY >= 0　&& MouseSpeed > 0.4 && MouseSpeed < 1.6) //0.4-1.6
                                {
                                    Graphics g = pictureBox1.CreateGraphics();　　//描画定義
                                           
                                    
                                    Pen pen = new Pen(Color.Aqua);  //ペン色
                                    pen.Width = 5;                  //ペン太さ
                                    g.DrawLine(pen, sx1, sy1, posX,posY);
                                    sx1 = posX;
                                    sy1 = posY;
                                    PastPoint = new Point(posX, posY);
                                    g.Dispose();
                                    aiu = 0;
                                    Invalidate();

                                }

                                if (posY > 720 && MouseSpeed > 2.0 && aiu == 0)  //2.0-
                                {
                                    //this.Hide();
                                    SendKeys.Send("{RIGHT}");
                                    aiu = 1;
                                    Invalidate(); 
                                }

                                if (posY < 240 && MouseSpeed > 2.0 && aiu == 0)
                                {
                                    //this.Hide();
                                    SendKeys.Send("{LEFT}");
                                    aiu = 1;
                                    Invalidate();
                                }
                                
                                sx1 = posX;
                                sy1 = posY;
                         
                            }
                        }
                            
                            
                        Bitmap hhh= BitmapConverter.ToBitmap(imgTo);
                        writeLog("指定色を抽出した画像を右下に表示します");
                        pictureBox3.Image = hhh;


                        
                    }
                }
                Cv.ReleaseImage(image);
                
                writeLog("imageに使用していたメモリを開放します");

            }
        }

        private void ppp(object sender, PaintEventArgs e)
        {
            Graphics pb1g = pictureBox1.CreateGraphics();
        }

        //差分
        private void button1_Click(object sender, EventArgs e)
        {
            //撮影
            runruntattta = Cv.QueryFrame(capture);

            //撮影画像を8bit化しhyyとheeyに格納
            IplImage hyy = Cv.CreateImage(runruntattta.Size, BitDepth.U8, 1);
            IplImage heey = Cv.Clone(hyy);

            //撮影画像を8bit化しimgHSVに格納
            IplImage imgHSV = Cv.CreateImage(new CvSize(runruntattta.Width, runruntattta.Height), BitDepth.U8, 3);

            //撮影画像をBGRからHSV化してimgHSVに格納
            Cv.CvtColor(runruntattta, imgHSV, ColorConversion.BgrToHsv);        //ERROR!!

            //imgHSVを8bit化してimgToに格納
            IplImage imgTo = Cv.CreateImage(new CvSize(imgHSV.Width, imgHSV.Height), BitDepth.U8, 1);

            //imgHSVから指定範囲の色を抽出しimgToに格納
            Cv.InRangeS(imgHSV, new CvScalar(5, 50, 50), new CvScalar(15, 255, 255), imgTo);

            //imgToをBitmap化してhgとし、pictureBox3に表示
            Bitmap hg = BitmapConverter.ToBitmap(imgTo);
            pictureBox3.Image = hg;

            //撮影画像をグレースケール化してhyyに格納、heyにBitmap化、pictureBox2に表示
            Cv.CvtColor(runruntattta, hyy, ColorConversion.RgbToGray);
            Bitmap hey = BitmapConverter.ToBitmap(hyy);
            pictureBox2.Image = hey;

            //スクリーンショットを撮影、capuutuに格納後pictureBox5のサイズにリサイズ
            capuutu = Win32APICall.GetDesktop();
            capuutu = ResizeImage(capuutu, pictureBox5.Width, pictureBox5.Height);

            //スクショをIpl化しtommに格納
            IplImage tomm = BitmapConverter.ToIplImage(capuutu);

            //スクショを8Bit化してtoomに格納
            IplImage toom = Cv.CreateImage(tomm.Size, BitDepth.U8, 1);

            //tommをグレースケール化してtoomに格納
            Cv.CvtColor(tomm, toom, ColorConversion.RgbToGray);

            //toom(グレースケール化したスクショ)をBitmap化し、pictureBox5に描画
            Bitmap tom = BitmapConverter.ToBitmap(toom);
            pictureBox5.Image = tom;

            //heey(8bit化した撮影画像)をone-fiveにコピー
            IplImage one = Cv.Clone(heey);
            IplImage two = Cv.Clone(one);
            IplImage three = Cv.Clone(one);
            IplImage four = Cv.Clone(one);
            IplImage five = Cv.Clone(one);

            // |toom-hyy| = one
            Cv.AbsDiff(toom, hyy, one);
            
            //  one+imgTo = five (imgTo:撮影画像から指定色を抽出したもの)
            Cv.Add(one, imgTo, five);
            
            // |one-five| = three
            Cv.AbsDiff(one, five, three);

            //threeを二値化しfourに格納する
            Cv.Threshold(three, four, 0, 255, ThresholdType.Otsu);
            //fourの差分結果を塗りつぶす
            Cv.Set(four, new CvScalar(255, 0, 0));
            //fourをpictureBox6に表示
            Bitmap sandai = BitmapConverter.ToBitmap(four);
            pictureBox6.Image = sandai;

            //oneを二値化しtwoに格納する
            Cv.Threshold(one, two, 0, 255, ThresholdType.Otsu);
            //twoをpictureBxo4に表示
            Bitmap haan = BitmapConverter.ToBitmap(two);
            pictureBox4.Image = haan;


        }

        public static Bitmap ResizeImage (Bitmap image, double dw, double dh)
        {
            double imagew = image.Width;
            double imageh = image.Height;
            int w = (int)((imagew * 2) / 3);
            int h = (int)((imageh * 2) / 3);
            Bitmap result = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, result.Width, result.Height);
            return result;
        }

        public class Win32APICall
        {
            [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
            public static extern IntPtr DeleteDC(IntPtr hdc);
 
            [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
            public static extern IntPtr DeleteObject(IntPtr hObject);
 
            [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
            public static extern bool BitBlt(IntPtr hdcDest, int nXDest,
                int nYDest, int nWidth, int nHeight, IntPtr hdcSrc,
                int nXSrc, int nYSrc, int dwRop);
 
            [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,
                int nWidth, int nHeight);
 
            [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
 
            [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobjBmp);
 
            [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
            public static extern IntPtr GetDesktopWindow();
 
            [DllImport("user32.dll", EntryPoint = "GetDC")]
            public static extern IntPtr GetDC(IntPtr hWnd);
 
            [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
            public static extern int GetSystemMetrics(int nIndex);
 
            [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            public static Bitmap GetDesktop()
            {
                int screenX;
                int screenY;
                IntPtr hBmp;
                IntPtr hdcScreen = GetDC(GetDesktopWindow());
                IntPtr hdcCompatible = CreateCompatibleDC(hdcScreen);

                screenX = GetSystemMetrics(0);
                screenY = GetSystemMetrics(1);
                hBmp = CreateCompatibleBitmap(hdcScreen, screenX, screenY);

                if (hBmp != IntPtr.Zero)
                {
                    IntPtr hOldBmp = (IntPtr)SelectObject(hdcCompatible, hBmp);
                    BitBlt(hdcCompatible, 0, 0, screenX, screenY, hdcScreen, 0, 0, 13369376);

                    SelectObject(hdcCompatible, hOldBmp);
                    DeleteDC(hdcCompatible);
                    ReleaseDC(GetDesktopWindow(), hdcScreen);

                    Bitmap bmp = System.Drawing.Image.FromHbitmap(hBmp);

                    DeleteObject(hBmp);
                    GC.Collect();

                    return bmp;
                }
                return null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename;
            DateTime dt = new DateTime();
            dt = System.DateTime.Now;
            filename = dt.ToString("yyyy.MM.dd_hh.mm.ss") + ".txt";

            System.IO.StreamWriter sw = new System.IO.StreamWriter(@filename, false, System.Text.Encoding.GetEncoding("shift_jis"));
            foreach(string line in textBox_Log.Lines)
            {
                sw.WriteLine(line);
            }
            sw.Close();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            settings set = new settings();
            set.ShowDialog(this);

        }       
    }
}
