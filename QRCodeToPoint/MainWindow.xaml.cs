using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using System.Collections;

namespace QRCodeToPoint
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string curFileName;//当前文件路径
        private Bitmap curBitmap;//当前位图
        private QRCode loadedQRCode;//当前二维码数据

        private Bitmap resultBitmap;//结果位图

        //转换参数
        private int _QRCodeSize = 200;
        private int _QRPointDistance = 10;
        private int _PointSize = 2;

        private int _BackGroundColorR = 255;
        private int _BackGroundColorG = 255;
        private int _BackGroundColorB = 255;
        private int _FrontGroundColorR = 0;
        private int _FrontGroundColorG = 0;
        private int _FrontGroundColorB = 0;

        private int _PositionAlpha = 255;

        public MainWindow()
        {
            InitializeComponent();

            lbPositionAlpha.Visibility = Visibility.Hidden;
            lbPositionAlpha.IsEnabled = false;
            sldPositionAlpha.Visibility = Visibility.Hidden;
            sldPositionAlpha.IsEnabled = false;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opnDlg = new OpenFileDialog();
            opnDlg.Filter = "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.png; *.jpg; *.gif;";
            opnDlg.Title = "打开图像文件";
            if (opnDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                curFileName = opnDlg.FileName;
                try
                {
                    curBitmap = (Bitmap)Image.FromFile(curFileName);
                }
                catch (Exception exp)
                {
                    System.Windows.MessageBox.Show(exp.Message);
                }


                BarcodeReader reader = new BarcodeReader();
                reader.Options.CharacterSet = "UTF-8";
                Result result = reader.Decode(curBitmap);
                if (result != null)
                {
                    QRResult.Content = result.Text.ToString();

                    loadedQRCode = GetQRCodeMatrix(result.Text.ToString());

                    try
                    {
                        //获取点距离和点大小
                        _QRPointDistance = Convert.ToInt32(tbDistance.Text);
                        _PointSize = Convert.ToInt32(tbPointSize.Text);

                        //计算图片尺寸
                        _QRCodeSize = loadedQRCode.Matrix.Height * _QRPointDistance;
                        tbSize.Text = _QRCodeSize.ToString();

                        //画图
                        resultBitmap = new Bitmap(_QRCodeSize, _QRCodeSize);
                        Graphics tempGraphics = Graphics.FromImage(resultBitmap);
                        DrawQRCode(0, 0, tempGraphics);
                        QRCodeImage.Source = ChangeBitmapToImageSource(resultBitmap);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            string inputText = tbInput.Text;

            QRResult.Content = inputText;

            loadedQRCode = GetQRCodeMatrix(inputText);

            try
            {
                //获取点距离和点大小
                _QRPointDistance = Convert.ToInt32(tbDistance.Text);
                _PointSize = Convert.ToInt32(tbPointSize.Text);

                //计算图片尺寸
                _QRCodeSize = loadedQRCode.Matrix.Height * _QRPointDistance;
                tbSize.Text = _QRCodeSize.ToString();

                //画图
                resultBitmap = new Bitmap(_QRCodeSize, _QRCodeSize);
                Graphics tempGraphics = Graphics.FromImage(resultBitmap);
                DrawQRCode(0, 0, tempGraphics);
                QRCodeImage.Source = ChangeBitmapToImageSource(resultBitmap);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString());
            }
        }

        private QRCode GetQRCodeMatrix(string inputText)
        {
            ErrorCorrectionLevel level = ErrorCorrectionLevel.H;
            Dictionary<EncodeHintType,object> hints = new Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "utf-8"); //编码
            hints.Add(EncodeHintType.ERROR_CORRECTION, level); //容错率
            hints.Add(EncodeHintType.MARGIN, 0);  //二维码边框宽度，这里文档说设置0-4，但是设置后没有效果，不知原因，
            //获取数据矩阵
            //BitMatrix tempBitMatrix = new MultiFormatWriter().encode(inputText, BarcodeFormat.QR_CODE, QRCodeSize, QRCodeSize, hints);
            QRCode code = new QRCode();
            code = ZXing.QrCode.Internal.Encoder.encode(inputText, level, hints);

            //for (int i = 0; i < code.Matrix.Height; i++)
            //{
            //    for (int j = 0; j < code.Matrix.Width; j++)
            //    {
            //        if (code.Matrix[i, j] == 1)
            //        {
            //            Console.Write("1 ");
            //        }
            //        else
            //        {
            //            Console.Write("0 ");
            //        }
            //    }
            //    Console.Write("\n");
            //}

            return code;
        }

        private void DrawQRCode(int x,int y, Graphics g)
        {
            //loadedQRCode.Version.VersionNumber
            for (int i = 0; i < loadedQRCode.Matrix.Height; i++)
            {
                for (int j = 0; j < loadedQRCode.Matrix.Width; j++)
                {
                    if (loadedQRCode.Matrix[i,j] == 1)
                    {
                        DrawPoint(g, new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                            _QRPointDistance * i + _QRPointDistance / 2,
                            _QRPointDistance * j + _QRPointDistance / 2,
                            _PointSize);
                        //DrawPoint(g, new SolidBrush(System.Drawing.Color.Black),
                        //    _QRPointDistance * i + _QRPointDistance / 2,
                        //    _QRPointDistance * j + _QRPointDistance / 2,
                        //    _PointSize);
                    }
                    else
                    {
                        DrawPoint(g, new SolidBrush(System.Drawing.Color.FromArgb(255, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                            _QRPointDistance * i + _QRPointDistance / 2,
                            _QRPointDistance * j + _QRPointDistance / 2,
                            _PointSize);
                        //DrawPoint(g, new SolidBrush(System.Drawing.Color.White),
                        //    _QRPointDistance * i + _QRPointDistance / 2,
                        //    _QRPointDistance * j + _QRPointDistance / 2,
                        //    _PointSize);
                    }
                }
            }

            //画基准线
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(0, 0, 0, 0)),
                6 * _QRPointDistance, 9 * _QRPointDistance, 1 * _QRPointDistance, _QRCodeSize - 16 * _QRPointDistance);

            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                6 * _QRPointDistance, 9 * _QRPointDistance, 1 * _QRPointDistance, _QRCodeSize - 16 * _QRPointDistance);

            for (int i = 8; i < loadedQRCode.Matrix.Height; i += 2)
            {
                g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                6 * _QRPointDistance, i * _QRPointDistance, 1 * _QRPointDistance, 1 * _QRPointDistance);
            }

            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                9 * _QRPointDistance, 6 * _QRPointDistance, _QRCodeSize - 16 * _QRPointDistance, 1 * _QRPointDistance);

            for (int i = 8; i < loadedQRCode.Matrix.Height; i += 2)
            {
                g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                i * _QRPointDistance, 6 * _QRPointDistance, 1 * _QRPointDistance, 1 * _QRPointDistance);
            }

            //画标志
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                0, 0, 8 * _QRPointDistance, 8 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                0, _QRCodeSize - 8 * _QRPointDistance, 8 * _QRPointDistance, 8 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                _QRCodeSize - 8 * _QRPointDistance, 0, 8 * _QRPointDistance, 8 * _QRPointDistance);

            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                0, 0, 7 * _QRPointDistance, 7 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                0, _QRCodeSize - 7 * _QRPointDistance, 7 * _QRPointDistance, 7 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                _QRCodeSize - 7 * _QRPointDistance, 0, 7 * _QRPointDistance, 7 * _QRPointDistance);

            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
               1 * _QRPointDistance, 1 * _QRPointDistance, 5 * _QRPointDistance, 5 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
               1 * _QRPointDistance, _QRCodeSize - 6 * _QRPointDistance, 5 * _QRPointDistance, 5 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
               _QRCodeSize - 6 * _QRPointDistance, 1 * _QRPointDistance, 5 * _QRPointDistance, 5 * _QRPointDistance);

            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                2 * _QRPointDistance, 2 * _QRPointDistance, 3 * _QRPointDistance, 3 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                2 * _QRPointDistance, _QRCodeSize - 5 * _QRPointDistance, 3 * _QRPointDistance, 3 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                _QRCodeSize - 5 * _QRPointDistance, 2 * _QRPointDistance, 3 * _QRPointDistance, 3 * _QRPointDistance);

            DrawSmallLocationPoint(g, loadedQRCode.Matrix.Width - 7, loadedQRCode.Matrix.Height - 7);
            //g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
            //    _QRCodeSize - 9 * _QRPointDistance, _QRCodeSize - 9 * _QRPointDistance, 5 * _QRPointDistance, 5 * _QRPointDistance);
            //g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
            //    _QRCodeSize - 8 * _QRPointDistance, _QRCodeSize - 8 * _QRPointDistance, 3 * _QRPointDistance, 3 * _QRPointDistance);
            //g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
            //    _QRCodeSize - 7 * _QRPointDistance, _QRCodeSize - 7 * _QRPointDistance, 1 * _QRPointDistance, 1 * _QRPointDistance);
        }

        private void DrawSmallLocationPoint(Graphics g, int x, int y)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                (x - 2) * _QRPointDistance, (y - 2) * _QRPointDistance, 5 * _QRPointDistance, 5 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(_PositionAlpha, _BackGroundColorR, _BackGroundColorG, _BackGroundColorB)),
                (x - 1) * _QRPointDistance, (y - 1) * _QRPointDistance, 3 * _QRPointDistance, 3 * _QRPointDistance);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, _FrontGroundColorR, _FrontGroundColorG, _FrontGroundColorB)),
                x * _QRPointDistance, y * _QRPointDistance, 1 * _QRPointDistance, 1 * _QRPointDistance);
        }

        private void DrawPoint(Graphics g, SolidBrush brush,int x,int y,int R)
        {
            g.FillEllipse(brush, x - R, y - R, 2 * R, 2 * R);
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            GC.Collect();
            return wpfBitmap;
        }

        private void sldBackGround_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldBackGroundB != null && sldBackGroundG != null && sldBackGroundR != null)
            {
                imgBackGround.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)sldBackGroundR.Value, (byte)sldBackGroundG.Value, (byte)sldBackGroundB.Value));
                lbBackGroundR.Content = "R：" + sldBackGroundR.Value.ToString();
                lbBackGroundG.Content = "G：" + sldBackGroundG.Value.ToString();
                lbBackGroundB.Content = "B：" + sldBackGroundB.Value.ToString();

                _BackGroundColorR = (int)sldBackGroundR.Value;
                _BackGroundColorG = (int)sldBackGroundG.Value;
                _BackGroundColorB = (int)sldBackGroundB.Value;

                if (resultBitmap != null)
                {
                    DrawQRCode(0,0, Graphics.FromImage(resultBitmap));
                    QRCodeImage.Source = ChangeBitmapToImageSource(resultBitmap);
                }
            }
        }

        private void sldFrontGround_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldFrontGroundB != null && sldFrontGroundG != null && sldFrontGroundR != null)
            {
                imgFrontGround.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)sldFrontGroundR.Value, (byte)sldFrontGroundG.Value, (byte)sldFrontGroundB.Value));
                lbFrontGroundR.Content = "R：" + sldFrontGroundR.Value.ToString();
                lbFrontGroundG.Content = "G：" + sldFrontGroundG.Value.ToString();
                lbFrontGroundB.Content = "B：" + sldFrontGroundB.Value.ToString();

                _FrontGroundColorR = (int)sldFrontGroundR.Value;
                _FrontGroundColorG = (int)sldFrontGroundG.Value;
                _FrontGroundColorB = (int)sldFrontGroundB.Value;

                if (resultBitmap != null)
                {
                    DrawQRCode(0, 0, Graphics.FromImage(resultBitmap));
                    QRCodeImage.Source = ChangeBitmapToImageSource(resultBitmap);
                }
            }
        }

        private void sldPositionAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldPositionAlpha != null)
            {
                lbPositionAlpha.Content = "定位白透明度：" + sldPositionAlpha.Value.ToString();

                _PositionAlpha = (int)sldPositionAlpha.Value;

                if (resultBitmap != null)
                {
                    DrawQRCode(0, 0, Graphics.FromImage(resultBitmap));
                    QRCodeImage.Source = ChangeBitmapToImageSource(resultBitmap);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (resultBitmap == null)
            {
                System.Windows.MessageBox.Show("未生成二维码！");
                return;
            }

            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Title = "保存为";
            saveDlg.OverwritePrompt = true;
            saveDlg.Filter = "PNG文件 (*.png) | *.png";
            saveDlg.ShowHelp = true;
            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = saveDlg.FileName; 
                resultBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);

                for (int i = 1; i <= 40; i++)
                {
                    BarcodeWriter writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE;
                    QrCodeEncodingOptions options = new QrCodeEncodingOptions();
                    options.DisableECI = true;
                    options.CharacterSet = "UTF-8";
                    options.Width = 100 * i;
                    options.Height = 100 * i;
                    options.Margin = 0;
                    options.QrVersion = i;

                    writer.Options = options;

                    //获取数据矩阵
                    Bitmap tempmap = new QRCodeWriter().encode("a", BarcodeFormat.QR_CODE, 100 * i, 100 * i).ToBitmap();
                }
            }
        }
    }
}
