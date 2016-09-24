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
        Timer ext1;
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

        //指示棒座標系
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
            if(Properties.Settings.Default.autoopen == true)
            {
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
            else 
            {
                t = new Timer();
                t.Interval = 2500;                  //修正:500→5000
                t.Tick += new EventHandler(uun);
                t.Start();
                writeLog("タイマーt開始");

                //最小化状態で表示するようにする
                this.WindowState = FormWindowState.Minimized;

                t1 = new Timer();
                t1.Start();
            }

        }

        private void un(object sender, EventArgs e)
        {
            //t.Stop();   

            writeLog("F5キーを送信します...");
            SendKeys.Send("{F5}");
            TopMost = true;
            this.WindowState = FormWindowState.Maximized;
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


            IplImage srcImg = BitmapConverter.ToIplImage(myBitmap);
            IplImage srcImgGray = new IplImage(srcImg.Size, BitDepth.U8, 1);
            IplImage tmpImg = new IplImage(srcImg.Size, BitDepth.U8, 1);

            
            if(Properties.Settings.Default.autoc == true)
            {
                CvMemStorage storage = new CvMemStorage(0);

                Cv.CvtColor(srcImg, srcImgGray, ColorConversion.BgrToGray);

                //画像の二値化と輪郭の検出
                Cv.Threshold(srcImgGray, tmpImg, 100, 255, ThresholdType.Binary);
                // Cv.Threshold(srcImgGray,tmpImg, 0, 255, ThresholdType.Otsu);
                Bitmap srcimageG = BitmapConverter.ToBitmap(tmpImg);
                pictureBox4.Image = srcimageG;

                CvSeq<CvPoint> contours; //輪郭シーケンスを定義

                Cv.FindContours(tmpImg, storage, out contours,CvContour.SizeOf,ContourRetrieval.Tree,ContourChain.ApproxSimple);

                contours = Cv.ApproxPoly(contours, CvContour.SizeOf, storage, CvConst.CV_POLY_APPROX_DP, 10, true);

                //ツリーノードイテレータの初期化
                writeLog("ツリーノードイテレータを初期化しています...");
                CvTreeNodeIterator<CvSeq<CvPoint>> it = new CvTreeNodeIterator<CvSeq<CvPoint>>(contours, 1);
                    
                foreach (CvSeq<CvPoint> contour in it)
                {
                    // 輪郭を構成する頂点座標を取得
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

                            Bitmap tmpLine = BitmapConverter.ToBitmap(srcImg);
                            pictureBox4.Image = tmpLine;

                            tmp = point;

                            CvPoint a = contour[0].Value;
                            CvPoint b = contour[1].Value;
                            CvPoint c = contour[2].Value;
                            CvPoint d = contour[3].Value;
                            
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

                            Properties.Settings.Default.X1 = A[0];
                            Properties.Settings.Default.Y1 = A[1];
                            Properties.Settings.Default.X2 = A[2];
                            Properties.Settings.Default.Y2 = A[3];
                            Properties.Settings.Default.X3 = A[4];
                            Properties.Settings.Default.Y3 = A[5];
                            Properties.Settings.Default.X4 = A[6];
                            Properties.Settings.Default.Y4 = A[7];

                            Properties.Settings.Default.Save();
                            
                            homography hom = new homography();
                            IplImage dst = hom.hom(srcImg, A, B);
                            Bitmap homed = BitmapConverter.ToBitmap(dst);
                            pictureBox5.Image = homed;



                        }
                        else
                        {

                            writeLog("条件を満たさなかったため、射影変換をスキップします");
                            

                        }

                        
                        timer.Stop();
                        writeLog("タイマーtimerを停止");

                        
                         
                            
                        //Bitmap hhh= BitmapConverter.ToBitmap(imgTo);
                        //writeLog("指定色を抽出した画像を右下に表示します");
                        //pictureBox3.Image = hhh;


                        
                    }
                }
                Cv.ReleaseImage(image);
                
                writeLog("imageに使用していたメモリを開放します");

            }
            else
            {
                writeLog("自動キャリブレーションはスキップされました");
                writeLog("マニュアルキャリブレーションを実行します");
                Manual ma = new Manual();

                writeLog("設定画面を開きます");

                DialogResult result = ma.ShowDialog(this);
                if (!result.Equals(DialogResult.OK))
                {
                    // 設定内容を読み込みなおす
                    Properties.Settings.Default.Reload();
                    writeLog("propertiesをリロードしました");
                    return;
                }

                // 設定内容を保存する
                Properties.Settings.Default.Save();
                writeLog("propertiesを保存しました");

                double[] M = new double[]{  
                                            Properties.Settings.Default.X1, Properties.Settings.Default.Y1,
                                            Properties.Settings.Default.X2, Properties.Settings.Default.Y2,
                                            Properties.Settings.Default.X3, Properties.Settings.Default.Y3,
                                            Properties.Settings.Default.X4, Properties.Settings.Default.Y4
                                            };
                // 変換先の頂点の座標
                double[] B = new double[]{    0,   0,
                                              0, 720,
                                            960, 720,
                                            960,   0
                                            };

                homography hom1 = new homography();
                IplImage dst = Cv.CreateImage(new CvSize(image.Width, image.Height), BitDepth.U8, 3);
                dst = hom1.hom(srcImg, M, B);
                Bitmap homed = BitmapConverter.ToBitmap(dst);
                pictureBox5.Image = homed;

                
            }

            if (Properties.Settings.Default.MousePaint == true) 
            {
                ext1 = new Timer();
                ext1.Interval = 100;
                ext1.Tick += new EventHandler(MPloop);
                ext1.Start();
            }
            else
            {
                ext1 = new Timer();
                ext1.Interval = 100;
                ext1.Tick += new EventHandler(homloop);
                ext1.Start();
            }
            
        }

        public void homloop(object sender, EventArgs e)
        {
            image = Cv.QueryFrame(capture);

            homography homl = new homography();

            double[] A = new double[]{ 
                                        Properties.Settings.Default.X1, Properties.Settings.Default.Y1,
                                        Properties.Settings.Default.X2, Properties.Settings.Default.Y2,
                                        Properties.Settings.Default.X3, Properties.Settings.Default.Y3,
                                        Properties.Settings.Default.X4, Properties.Settings.Default.Y4
                                        };
            // 変換先の頂点の座標
            double[] B = new double[]{    0,   0,
                                                          0, 720,
                                                        960, 720,
                                                        960,   0
                                                        };

            IplImage homed1 = Cv.CreateImage(new CvSize(image.Width, image.Height), BitDepth.U8, 3);
            homed1 = homl.hom(image, A, B);
            Bitmap homedb = BitmapConverter.ToBitmap(homed1);
            pictureBox5.Image = homedb;

            double H1 = Decimal.ToDouble(Properties.Settings.Default.ext_H1);
            double S1 = Decimal.ToDouble(Properties.Settings.Default.ext_S1);
            double V1 = Decimal.ToDouble(Properties.Settings.Default.ext_V1);

            double H2 = Decimal.ToDouble(Properties.Settings.Default.ext_H2);
            double S2 = Decimal.ToDouble(Properties.Settings.Default.ext_S2);
            double V2 = Decimal.ToDouble(Properties.Settings.Default.ext_V2);

            label1.Text = timer.Elapsed.ToString();

            IplImage imgTracking = Cv.CreateImage(new CvSize(image.Width, image.Height), BitDepth.U8, 3);

            Cv.Zero(imgTracking);

            IplImage imgHSV = Cv.CreateImage(homed1.Size, BitDepth.U8, 3);

            Cv.CvtColor(homed1, imgHSV, ColorConversion.BgrToHsv);

            IplImage imgTo = Cv.CreateImage(new CvSize(imgHSV.Width, imgHSV.Height), BitDepth.U8, 1);
            //指定色を抽出
            Cv.InRangeS(imgHSV, new CvScalar(H1, S1, V1), new CvScalar(H2, S2, V2), imgTo);

            if(Properties.Settings.Default.Filter1 == true)
            {
                Cv.Smooth(imgTo, imgTo, SmoothType.Median, 19, 19);
            }
            
            //ディスプレイの高さ
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            //ディスプレイの幅
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            
            IplImage imgDispray = Cv.CreateImage(new CvSize(w, h), BitDepth.U8, 1);
            Cv.Resize(imgTo, imgDispray);

            CvMoments moments = new CvMoments(imgDispray, false);

            Bitmap imgToB = BitmapConverter.ToBitmap(imgDispray);
            pictureBox3.Image = imgToB;

            //指定色の面積取得
            double area = Cv.GetCentralMoment(moments, 0, 0);

            double moments01 = Cv.GetSpatialMoment(moments, 0, 1);
            double moments10 = Cv.GetSpatialMoment(moments, 1, 0);


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
                if(posX < 0)
                {
                    posX = 480;
                }

                if(posY < 0)
                {
                    posY = 360;
                }


                //位置が0でなく、移動速度が0.4～1.6pixel/msec        
                if (PastPoint.X >= 0 && PastPoint.Y >= 0 && posX >= 0 && posY >= 0 && MouseSpeed > 0.4 && MouseSpeed < 1.6) //0.4-1.6
                {
                    Graphics g = pictureBox1.CreateGraphics();　　//描画定義
                    writeLog("線を描画しています");

                    Pen pen = new Pen(Color.Aqua);  //ペン色
                    pen.Width = 5;                  //ペン太さ
                    g.DrawLine(pen, sx1, sy1, posX, posY);
                    sx1 = posX;
                    sy1 = posY;
                    PastPoint = new Point(posX, posY);
                    //ug.Dispose();
                    aiu = 0;
                    Invalidate();

                }

                if (posY > 720 && MouseSpeed < 2.0 && aiu == 0)  //2.0-
                {
                    writeLog("ページを送ります");
                    //this.Hide();
                    SendKeys.Send("{RIGHT}");
                    aiu = 1;
                    Invalidate();
                }

                if (posY < 240 && MouseSpeed < 2.0 && aiu == 0)
                {
                    writeLog("ページを戻します");
                    //this.Hide();
                    SendKeys.Send("{LEFT}");
                    aiu = 1;
                    Invalidate();
                }

                sx1 = posX;
                sy1 = posY;

            }

        }

        public void MPloop(object sender, EventArgs e)
        {

            //画面座標でマウスポインタの位置を取得する
            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            //画面座標をクライアント座標に変換する
            System.Drawing.Point cp = pictureBox1.PointToClient(sp);


            int posX = cp.X;
            int posY = cp.Y;

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
                label4.Text = " ";
                label5.Text = "重心X" + posX.ToString();
                label6.Text = "重心Y" + posY.ToString();
                label7.Text = "カーソル速度" + MouseSpeed.ToString();

                MyStopWatch.Restart();

                //TopMost = true;
                if (posX < 0)
                {
                    posX = 480;
                }

                if (posY < 0)
                {
                    posY = 360;
                }


                //位置が0でなく、移動速度が0.4～1.6pixel/msec        
                if (PastPoint.X >= 0 && PastPoint.Y >= 0 && posX >= 0 && posY >= 0 && MouseSpeed > 0.4 && MouseSpeed < 1.6) //0.4-1.6
                {
                    Graphics g = pictureBox1.CreateGraphics();　　//描画定義
                    writeLog("線を描画しています");

                    Pen pen = new Pen(Color.Aqua);  //ペン色
                    pen.Width = 5;                  //ペン太さ
                    g.DrawLine(pen, sx1, sy1, posX, posY);
                    sx1 = posX;
                    sy1 = posY;
                    PastPoint = new Point(posX, posY);
                    //ug.Dispose();
                    aiu = 0;
                    Invalidate();

                }

                if (posY > 720 && MouseSpeed < 2.0 && aiu == 0)  //2.0-
                {
                    writeLog("ページを送ります");
                    //this.Hide();
                    SendKeys.Send("{RIGHT}");
                    aiu = 1;
                    Invalidate();
                }

                if (posY < 240 && MouseSpeed < 2.0 && aiu == 0)
                {
                    writeLog("ページを戻します");
                    //this.Hide();
                    SendKeys.Send("{LEFT}");
                    aiu = 1;
                    Invalidate();
                }

                sx1 = posX;
                sy1 = posY;

            }

        }
        private void picturebox1Paint(object sender, PaintEventArgs e)
        {
            Graphics pb1g = pictureBox1.CreateGraphics();
        }

        //差分
        private void button1_Click(object sender, EventArgs e)
        {
            //撮影
            runruntattta = Cv.QueryFrame(capture);

            //8bit3chの画像hyy,heeyを作成
            IplImage hyy = Cv.CreateImage(runruntattta.Size, BitDepth.U8, 1);
            IplImage heey = Cv.Clone(hyy);

            //8bit3chの画像imgHSVを作成
            IplImage imgHSV = Cv.CreateImage(new CvSize(runruntattta.Width, runruntattta.Height), BitDepth.U8, 3);

            //撮影画像をBGRからHSV化してimgHSVに格納
            Cv.CvtColor(runruntattta, imgHSV, ColorConversion.BgrToHsv);        //ERROR!!

            //8bit3chの画像imgToを作成
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

            //スクリーンショットを撮影、capuutuに格納後 pictureBox5のサイズ→hyyのサイズ にリサイズ
            capuutu = Win32APICall.GetDesktop();
            capuutu = ResizeImage(capuutu, hyy.Width, hyy.Height);

            //スクショをIpl化しtommに格納
            IplImage tomm = BitmapConverter.ToIplImage(capuutu);

            //8bit3chの画像toomを作成
            IplImage toom = Cv.CreateImage(tomm.Size, BitDepth.U8, 1);

            //tommをグレースケール化してtoomに格納
            Cv.CvtColor(tomm, toom, ColorConversion.RgbToGray);

            //toom(グレースケール化したスクショ)をBitmap化し、pictureBox5に描画
            Bitmap tom = BitmapConverter.ToBitmap(toom);
            pictureBox5.Image = tom;

            //heey(8bit3ch空の画像)をone-fiveにコピー
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
          //pictureBox6.Image = sandai;

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

        class homography
        {
            public IplImage hom(IplImage srcImg, double[] A, double[] B)
            {
                //行列として管理
                CvMat Mata = new CvMat(4, 2, MatrixType.F64C1, A);
                CvMat Matb = new CvMat(4, 2, MatrixType.F64C1, B);

                IplImage dst = Cv.CreateImage(Cv.Size(960, 720), srcImg.Depth, srcImg.NChannels);

                //ホモグラフィ行列
                CvMat homography = new CvMat(3, 3, MatrixType.F64C1);

                //射影変換
                Cv.FindHomography(Mata, Matb, homography);

                

                //透視投影
                Cv.WarpPerspective(srcImg, dst, homography, Interpolation.Cubic);

                return dst;

            }
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
            //set.ShowDialog(this);
            writeLog("設定画面を開きます");

            DialogResult result = set.ShowDialog(this);
            if (!result.Equals(DialogResult.OK))
            {
                // 設定内容を読み込みなおす
                Properties.Settings.Default.Reload();
                writeLog("propertiesをリロードしました");
                return;
            }

            // 設定内容を保存する
            Properties.Settings.Default.Save();
            writeLog("propertiesを保存しました");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            t = new Timer();
            t.Interval = 2500;            
            t.Start();
            writeLog("タイマーt開始");

            t1 = new Timer();
            t1.Tick += new EventHandler(uun);
            t1.Interval = 3000; 
            t1.Start();
            writeLog("タイマーt1開始");
            
        }



        private void button6_Click(object sender, EventArgs e)
        {
            ext1 = new Timer();
            ext1.Interval = 100;                  
            ext1.Tick += new EventHandler(homloop);
            ext1.Start();
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Manual mc = new Manual();
            mc.ShowDialog(this);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            bool MPmode = !Properties.Settings.Default.MousePaint;
            Properties.Settings.Default.MousePaint = MPmode;
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            pictureBox1.SendToBack();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            pictureBox1.BringToFront();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            double x1 = Properties.Settings.Default.X1;
            double y1 = Properties.Settings.Default.Y1;
            double x2 = Properties.Settings.Default.X2;
            double y2 = Properties.Settings.Default.Y2;
            double x3 = Properties.Settings.Default.X3;
            double y3 = Properties.Settings.Default.Y3;
            double x4 = Properties.Settings.Default.X4;
            double y4 = Properties.Settings.Default.Y4;

            Properties.Settings.Default.X1 = x4;
            Properties.Settings.Default.Y1 = y4;
            Properties.Settings.Default.X2 = x1;
            Properties.Settings.Default.Y2 = y1;
            Properties.Settings.Default.X3 = x2;
            Properties.Settings.Default.Y3 = y2;
            Properties.Settings.Default.X4 = x3;
            Properties.Settings.Default.Y4 = y3;

            Properties.Settings.Default.Save();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ext1 = new Timer();
            ext1.Interval = 100;
            ext1.Tick += new EventHandler(MPloop);
            ext1.Start();
        }       
    }
}
