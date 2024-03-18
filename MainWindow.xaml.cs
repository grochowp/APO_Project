using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Win32;

namespace APO_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public Mat? imageMat;
        public WindowImg? windowImg;
        public MainWindow()
        {
            InitializeComponent();
            WindowImg.WindowImgFocused += UpdateFocus;
            WindowImg.WindowImgClosed += ClearFocus;
        }


        private void UpdateFocus(Mat mat, WindowImg windowImg)
        {
            this.imageMat = mat;
            this.windowImg = windowImg;
        }
        private void ClearFocus()
        {
            this.imageMat = null;
            this.windowImg = null;
        }
        private void ImportMono_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

           // Image.DefaultExt = ".bmp";
           // Image.Filter = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";    
            if (Image.ShowDialog() == true)    
                
            {              
                string fileName = Image.FileName;    
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Grayscale);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImg imgWindow = new WindowImg(ImageOpened, bitmapSource);
                imgWindow.Show();             
            }
        }
        private void ImportColor_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

            // Image.DefaultExt = ".bmp";
            // Image.Filter = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";  
            if (Image.ShowDialog() == true)

            {
                string fileName = Image.FileName;
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Color);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImg image = new WindowImg(ImageOpened, bitmapSource);
                image.Show();
            }
        }
        private void ColorToGray_Click(object sender, RoutedEventArgs e)
        {
            if(this.imageMat == null || this.windowImg == null) MessageBox.Show("no obrazek?");
            else if (this.imageMat.NumberOfChannels == 1) MessageBox.Show("?"); 
            else
            {
            Mat mat = new Mat();
            CvInvoke.CvtColor(this.imageMat, mat, ColorConversion.Bgr2Gray);
            this.imageMat = mat;
            this.windowImg.mat = mat;
            this.windowImg.img.Source = Imaging.CreateBitmapSourceFromHBitmap(mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            
        }
        private void Negation_Click(object obj, RoutedEventArgs e)
        {
            //if (this.imageMat.NumberOfChannels != 1) this.ColorToGray_Click(obj, e);
            if (this.imageMat == null || this.windowImg == null) MessageBox.Show("Can`t call negation sry");
            else if (this.imageMat.NumberOfChannels != 1) MessageBox.Show("Convert to gray");
            else
            {
                Mat negatedImg = this.imageMat.Clone();
                Image<Gray, byte> grayImg = negatedImg.ToImage<Gray, byte>();
                for (int i = 0; i < grayImg.Rows; i++)
                {
                    for (int j = 0; j < grayImg.Cols; j++)
                    {
                        byte pixelVal = grayImg.Data[i, j, 0];
                        grayImg.Data[i, j, 0] = (byte)(255 - pixelVal);
                    }

                }
                this.imageMat = grayImg.Mat;
                this.windowImg.mat = grayImg.Mat;
                this.windowImg.img.Source = Imaging.CreateBitmapSourceFromHBitmap(grayImg.Mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.windowImg.HistogramUpdate();
            }
        }
        private void Histogram_Click(object sender, RoutedEventArgs e)
        {
            this.windowImg?.HistogramShow();
        }
    }
}
