using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using Emgu.CV.Util;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using APO_Projekt.Features;
using System.Windows.Controls;
using System.Drawing;
using static SkiaSharp.HarfBuzz.SKShaper;
using System.Security.Policy;

namespace APO_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public Mat? displayedImage;
        public WindowImgFocused? windowImgFocused;
        private BorderType selectedBorderType = BorderType.Isolated;

        private List<WindowImgFocused> imagesList = new List<WindowImgFocused>();


        public MainWindow()
        {
            InitializeComponent();
            WindowImgFocused.windowImgFocused += UpdateFocus;
            WindowImgFocused.windowImgClosed += ClearFocus;
        }

        static double[,] Convolve(double[,] matrix1, double[,] matrix2)
        {
            int size = 5;
            double[,] kernel = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            int x = i + k - 1;
                            int y = j + l - 1;

                            if (x >= 0 && x < 3 && y >= 0 && y < 3)
                                sum += matrix1[k, l] * matrix2[x, y];
                        }
                    }
                    kernel[i, j] = sum;
                }
            }

            return kernel;
        }
        private void BorderType_Isolated_Click(object sender, RoutedEventArgs e)
        {

            selectedBorderType = BorderType.Isolated;
        }

        private void BorderType_Replicate_Click(object sender, RoutedEventArgs e)
        {

            selectedBorderType = BorderType.Replicate;
        }

        private void BorderType_Reflect_Click(object sender, RoutedEventArgs e)
        {

            selectedBorderType = BorderType.Reflect;
        }


        private void UpdateFocus(Mat mat, WindowImgFocused windowImg)
        {
            this.displayedImage = mat;
            this.windowImgFocused = windowImg;
        }
        private void ClearFocus()
        {
            this.displayedImage = null;
            this.windowImgFocused = null;
        }

        private void ImportMono_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

            Image.DefaultExt = ".bmp";
            Image.Filter = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            if (Image.ShowDialog() == true)

            {
                string fileName = Image.FileName;
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Grayscale);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused imgWindow = new WindowImgFocused(ImageOpened, bitmapSource, "Mono " + fileName);
                imgWindow.Show();
                imagesList.Add(imgWindow);
            }
        }
        private void ImportColor_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

            Image.DefaultExt = ".bmp";
            Image.Filter = "JPG/PNG Files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp|PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg";
            if (Image.ShowDialog() == true)

            {
                string fileName = Image.FileName;
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Color);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused image = new WindowImgFocused(ImageOpened, bitmapSource, "Color " + fileName);
                image.Show();
            }
        }
        private void ColorToGray_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.windowImgFocused == null) MessageBox.Show("no obrazek?");
            else if (this.displayedImage.NumberOfChannels == 1) MessageBox.Show("?");
            else
            {
                Mat mat = new Mat();
                CvInvoke.CvtColor(this.displayedImage, mat, ColorConversion.Bgr2Gray);
                this.displayedImage = mat;
                this.windowImgFocused.mat = mat;
                windowImgFocused.Title = "Mono";
                this.windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                imagesList.Add(windowImgFocused);

            }

        }
        private void Negation_Click(object obj, RoutedEventArgs e)
        {
            //if (this.imageMat.NumberOfChannels != 1) this.ColorToGray_Click(obj, e);
            if (this.displayedImage == null || this.windowImgFocused == null) MessageBox.Show("Can`t call negation sry");
            else if (this.displayedImage.NumberOfChannels != 1) MessageBox.Show("Convert to gray");
            else
            {
                Mat negatedImg = this.displayedImage.Clone();
                Image<Gray, byte> grayImg = negatedImg.ToImage<Gray, byte>();
                for (int i = 0; i < grayImg.Rows; i++)
                {
                    for (int j = 0; j < grayImg.Cols; j++)
                    {
                        byte pixelVal = grayImg.Data[i, j, 0];
                        grayImg.Data[i, j, 0] = (byte)(255 - pixelVal);
                    }

                }
                this.displayedImage = grayImg.Mat;
                this.windowImgFocused.mat = grayImg.Mat;
                this.windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(grayImg.Mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                windowImgFocused.Title = "Negation ";
                this.windowImgFocused.HistogramUpdate();
            }
        }
        private void Histogram_Click(object sender, RoutedEventArgs e)
        {
            this.windowImgFocused?.HistogramShow();
        }
        private void ImageSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.displayedImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is grayscale");
                return;
            }
            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(this.windowImgFocused?.mat, vector);
            for (int i = 0; i < vector.Size; ++i)
            {
                Mat vect = vector[i];
                Mat vectClone = vect.Clone();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(vectClone.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused imgWindow = new WindowImgFocused(vectClone, bitmapSource, "Channel " + (i + 1));
                imgWindow.Show();
            }

        }
        private void HSV_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.displayedImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is grayscale");
                return;
            }
            if (this.windowImgFocused?.mat == null) return;
            Mat initial = this.windowImgFocused.mat;
            Mat result = new Mat();
            CvInvoke.CvtColor(initial, result, ColorConversion.Bgr2Hsv);
            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(result, vector);

            string[] channelNames = { "Hue", "Saturation", "Value" };

            for (int i = 0; i < vector.Size; ++i)
            {
                Mat vect = vector[i];
                Mat vectClone = vect.Clone();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(vectClone.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused imgWindow = new WindowImgFocused(vectClone, bitmapSource, "(HSV) " + channelNames[i]);
                imgWindow.Show();
            }

        }
        private void Lab_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.displayedImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is grayscale");
                return;
            }
            if (this.windowImgFocused?.mat == null) return;
            Mat initial = this.windowImgFocused.mat;
            Mat result = new Mat();
            CvInvoke.CvtColor(initial, result, ColorConversion.Bgr2Lab);
            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(result, vector);

            string[] channelNames = { "Luminance", "a", "b" };

            for (int i = 0; i < vector.Size; ++i)
            {
                Mat vect = vector[i];
                Mat vectClone = vect.Clone();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(vectClone.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused imgWindow = new WindowImgFocused(vectClone, bitmapSource, "(Lab) " + channelNames[i]);
                imgWindow.Show();
            }

        }

        private void StretchHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.displayedImage.NumberOfChannels != 1)
            {
                MessageBox.Show("Cannot Stretch Histogram.");
                return;
            }

            this.windowImgFocused?.StretchHistogram(this.displayedImage);
            this.windowImgFocused?.HistogramUpdate();
        }

        private void StretchContrast_Click(object sender, RoutedEventArgs e)
        {

            StretchData stretchData = new StretchData();
            if (stretchData.ShowDialog() == true)
            {
                int p1 = stretchData.P1;
                int p2 = stretchData.P2;
                int q3 = stretchData.Q3;
                int q4 = stretchData.Q4;


                if (this.displayedImage == null || this.displayedImage.NumberOfChannels != 1)
                {
                    MessageBox.Show("Cannot Stretch Contrast.");
                    return;
                }
                this.windowImgFocused?.StretchContrast(p1, p2, q3, q4);
                this.windowImgFocused?.HistogramUpdate();

            }
        }

        private void Equalize_Click(object sender, RoutedEventArgs e)
        {
            {
                windowImgFocused?.EqualizeHistogram();
                windowImgFocused?.HistogramUpdate();
            }
        }


        private void Posterize_Click(object sender, RoutedEventArgs e)
        {

            Canals canals = new Canals();
            if (canals.ShowDialog() == true)
            {
                int level = canals.level;

                if (this.displayedImage == null || this.displayedImage.NumberOfChannels != 1)
                {
                    MessageBox.Show("Cannot perform posterization on a non-grayscale image.");
                    return;
                }
                this.windowImgFocused?.Posterize(this.displayedImage, level);
                this.windowImgFocused?.HistogramUpdate();
            }
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat smoothedMat = new Mat();
            System.Drawing.Size kernelSize = new System.Drawing.Size((int)3, (int)3);
            System.Drawing.Point point = new System.Drawing.Point(-1, -1);

            CvInvoke.Blur(displayedImage, smoothedMat, kernelSize, point, BorderType.Default);

            displayedImage = smoothedMat;
            windowImgFocused.mat = smoothedMat;
            windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(smoothedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            windowImgFocused?.HistogramUpdate();
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat smoothedMat = new Mat();
            System.Drawing.Size kernelSize = new System.Drawing.Size((int)3, (int)3);

            CvInvoke.GaussianBlur(displayedImage, smoothedMat, kernelSize, 0);
            displayedImage = smoothedMat;
            windowImgFocused.mat = smoothedMat;
            windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(smoothedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            windowImgFocused?.HistogramUpdate();
        }

        private void SobelEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();

            CvInvoke.Sobel(displayedImage, edgesMat, DepthType.Cv8U, 1, 1, 3, 1, 1, selectedBorderType);
            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            displayedImage = edgesMat;
            windowImgFocused.mat = edgesMat;
            windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            windowImgFocused.Title = "(Sobel) " + selectedBorderType;
            windowImgFocused?.HistogramUpdate();
        }

        private void LaplacianEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();

            CvInvoke.Laplacian(displayedImage, edgesMat, DepthType.Cv8U, 1, 1, 0, selectedBorderType);
            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            //CvInvoke.Threshold(edgesMat, edgesMat, 50, 255, ThresholdType.Binary);

            displayedImage = edgesMat;
            windowImgFocused.mat = edgesMat;
            windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            windowImgFocused.Title = "(Laplacian) " + selectedBorderType;
            windowImgFocused?.HistogramUpdate();
        }

        private void CannyEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();
            CvInvoke.Canny(displayedImage, edgesMat, 100, 200);

            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            displayedImage = edgesMat;
            windowImgFocused.mat = edgesMat;
            windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            windowImgFocused.Title = "(Canny) " + selectedBorderType;
            windowImgFocused?.HistogramUpdate();
        }

        private void LinearLaplacianSharpening1_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
            { 0, -1, 0 },
            { -1, 5, -1 },
            { 0, -1, 0 }
        };
            ApplyLinearLaplacianSharpening(laplacianMask, "(mask 1) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening2_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
                { 1, 1, 1 },
                {  1, -9, 1},
                { 1, 1, 1 }
            };
            ApplyLinearLaplacianSharpening(laplacianMask, "(mask 2) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening3_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
                { 1, -2, 1 },
                { -2, 5, -2 },
                { 1, -2, 1 }
            };
            ApplyLinearLaplacianSharpening(laplacianMask, "(mask 3) Linear Laplacian Sharpening");
        }


        private void PrewittEdgeDetectionH_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                {-1, 0, 1 },
                {-1, 0, 1 },
                {-1, 0, 1 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt H");
        }
        private void PrewittEdgeDetectionV_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                { -1, -1, -1},
                {0, 0, 0 },
                {1, 1, 1 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt V");
        }
        private void PrewittEdgeDetectionNE_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 0, 1, 1 },
              {-1, 0, 1 },
              {-1, -1, 0 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt NE");
        }
        private void PrewittEdgeDetectionSE_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
               { -1, 0, 1 },
               {-1, 0, 1 },
               {0, -1, 1 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt SE");
        }
        private void PrewittEdgeDetectionNW_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                { 1, 1, 0 },
                {1, 0, -1 },
                {0, -1, -1 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt NW");
        }
        private void PrewittEdgeDetectionSW_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
               { 0, -1, -1 },
               {1, 0, -1 },
               {1, 1, 0 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt SW");
        }
        private void PrewittEdgeDetectionIH_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 1, 0, -1 },
              {1, 0, -1 },
              {1, 0, -1 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt IH");
        }
        private void PrewittEdgeDetectionIV_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 0, -1, 0 },
              {-1, 0, -1 },
              {0, 1, 0 }
            };
            ApplyLinearLaplacianSharpening(prewittMask, "Prewitt IV");
        }

        private void ApplyLinearLaplacianSharpening(float[,] mask, string title)
        {

            if (displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            if (mask.GetLength(0) != 3 || mask.GetLength(1) != 3)
            {
                MessageBox.Show("Invalid mask size. Must be 3x3.");
                return;
            }
            ConvolutionKernelF matrixKernel = new ConvolutionKernelF(mask);

            try
            {
                Mat sharpenedMat = new Mat();
                System.Drawing.Point point = new System.Drawing.Point(-1, -1);

                if (displayedImage.Size != sharpenedMat.Size)
                {
                    sharpenedMat = displayedImage.Clone();
                }


                CvInvoke.Filter2D(displayedImage, sharpenedMat, matrixKernel, point, 0, selectedBorderType);

                displayedImage = sharpenedMat;
                windowImgFocused.mat = sharpenedMat;
                windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                windowImgFocused.Title = title;
                windowImgFocused?.HistogramUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying filter2D: " + ex.Message);
                Console.WriteLine("Exception details: " + ex.ToString());
            }
        }
        private void CustomLinearOperation_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            Mask3x3 mask = new Mask3x3();
            if (mask.ShowDialog() == true)
            {
                int m1 = mask.M1;
                int m2 = mask.M2;
                int m3 = mask.M3;
                int m4 = mask.M4;
                int m5 = mask.M5;
                int m6 = mask.M6;
                int m7 = mask.M7;
                int m8 = mask.M8;
                int m9 = mask.M9;

                /* float[,] customMast = {
                 { m1, m2, m3 },
                 {m4, m5, m6 },
                 {m7, m8, m9 }
             };
                 ApplyLinearLaplacianSharpening(customMast, "Custom Linear Operation");*/


                Mat kernel = new Mat(3, 3, DepthType.Cv32F, 1);
                float[] kernelData = new float[] { m1, m2, m3, m4, m5, m6, m7, m8, m9 };
                kernel.SetTo(kernelData);

                System.Drawing.Point point = new System.Drawing.Point(-1, -1);
                Mat sharpenedMat = new Mat();
                if (displayedImage.Size != sharpenedMat.Size)
                {
                    sharpenedMat = displayedImage.Clone();
                }
                CvInvoke.Filter2D(displayedImage, sharpenedMat, kernel, point, 0, selectedBorderType);

                displayedImage = sharpenedMat;
                windowImgFocused.mat = sharpenedMat;
                windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                windowImgFocused.Title = "Custom Linear Operation";
                windowImgFocused?.HistogramUpdate();


            }
        }

        private void MedianFiltration_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            MedianFiltration mask = new MedianFiltration();
            if (mask.ShowDialog() == true)
            {



                int kernelSize = mask.Mask;
                int borderSize = kernelSize / 2;

                Mat sharpenedMat = new Mat();
                if (displayedImage.Size != sharpenedMat.Size)
                {
                    sharpenedMat = displayedImage.Clone();
                }

                CvInvoke.CopyMakeBorder(displayedImage, sharpenedMat, borderSize, borderSize, borderSize, borderSize, selectedBorderType);
                CvInvoke.MedianBlur(displayedImage, sharpenedMat, kernelSize);

                displayedImage = sharpenedMat;
                windowImgFocused.mat = sharpenedMat;
                windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                windowImgFocused.Title = "Median Filtration";
                windowImgFocused?.HistogramUpdate();


            }
        }

        private void LinearFiltering_Click(object sender, RoutedEventArgs e)
        {

            if (this.displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mask3x3 maskSmooth = new Mask3x3();
            if (maskSmooth.ShowDialog() == true)
            {
                int m1 = maskSmooth.M1;
                int m2 = maskSmooth.M2;
                int m3 = maskSmooth.M3;
                int m4 = maskSmooth.M4;
                int m5 = maskSmooth.M5;
                int m6 = maskSmooth.M6;
                int m7 = maskSmooth.M7;
                int m8 = maskSmooth.M8;
                int m9 = maskSmooth.M9;


                Mask3x3 maskSharp = new Mask3x3();
                if (maskSharp.ShowDialog() == true)
                {

                    int m1sharp = maskSharp.M1;
                    int m2sharp = maskSharp.M2;
                    int m3sharp = maskSharp.M3;
                    int m4sharp = maskSharp.M4;
                    int m5sharp = maskSharp.M5;
                    int m6sharp = maskSharp.M6;
                    int m7sharp = maskSharp.M7;
                    int m8sharp = maskSharp.M8;
                    int m9sharp = maskSharp.M9;
                    double[,] kernel1 = new double[,] { { m1, m2, m3 }, { m4, m5, m6 }, { m7, m8, m9 } };
                    double[,] kernel2 = new double[,] { { m1sharp, m2sharp, m3sharp }, { m4sharp, m5sharp, m6sharp }, { m7sharp, m8sharp, m9sharp } };

                    Mat kernelSmooth = new Mat(3, 3, DepthType.Cv32F, 1);
                    double[] smoothedKernelData = new double[] { m1, m2, m3, m4, m5, m6, m7, m8, m9 };
                    kernelSmooth.SetTo(smoothedKernelData);

                    Mat kernelSharp = new Mat(3, 3, DepthType.Cv32F, 1);
                    double[] sharpedKernelData = new double[] { m1sharp, m2sharp, m3sharp, m4sharp, m5sharp, m6sharp, m7sharp, m8sharp, m9sharp };
                    kernelSharp.SetTo(sharpedKernelData);

                    // Wygładzanie
                    Mat smoothedMat = displayedImage.Clone();
                    CvInvoke.Filter2D(displayedImage, smoothedMat, kernelSmooth, new System.Drawing.Point(-1, -1), 0, selectedBorderType);

                    // Wyostrzanie
                    Mat sharpenedMat = displayedImage.Clone();
                    CvInvoke.Filter2D(smoothedMat, sharpenedMat, kernelSharp, new System.Drawing.Point(-1, -1), 0, selectedBorderType);


                    Mat result5x5 = displayedImage.Clone();

                    double[,] result = Convolve(kernel1, kernel2);

                    double[] kernelData = new double[5 * 5];
                    int index = 0;

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            kernelData[index++] = result[i, j];
                        }
                    }


                    Mat kernelMat = new Mat(5, 5, DepthType.Cv32F, 1);
                    kernelMat.SetTo(kernelData);

                    CvInvoke.Filter2D(displayedImage, result5x5, kernelMat, new System.Drawing.Point(-1, -1), 0, selectedBorderType);


                    displayedImage = sharpenedMat;
                    windowImgFocused.mat = sharpenedMat;
                    windowImgFocused.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    windowImgFocused.Title = "3x3 masks operation";
                    windowImgFocused?.HistogramUpdate();

                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(result5x5.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    WindowImgFocused imgWindow = new WindowImgFocused(result5x5, bitmapSource, "5x5 mask operation");
                    imgWindow.Show();

                }
            }
        }

        private void PointOperation_Click(object sender, RoutedEventArgs e)
        {


            if (this.displayedImage == null || windowImgFocused == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            OperationSelector operation = new OperationSelector(imagesList);

            if (operation.ShowDialog() == true)
            {
                WindowImgFocused firstImage = imagesList[operation.FirstImgIndex];
                WindowImgFocused secondImage = imagesList[operation.SecondImgIndex];

                Mat resultImage = new Mat();
                string selectedOperation = operation.Operation;
                double blend = operation.Blend;
                if (firstImage.mat?.Size != secondImage.mat?.Size)
                {
                    MessageBox.Show("Images must have the same size for this operation.");
                    return;
                }

                switch (selectedOperation)
                {
                    case "Add":
                        CvInvoke.Add(firstImage.mat, secondImage.mat, resultImage);
                        break;
                    case "Substract":
                        CvInvoke.Subtract(firstImage.mat, secondImage.mat, resultImage);
                        break;
                    case "Blend":
                        CvInvoke.AddWeighted(firstImage.mat, blend, secondImage.mat, 1 - blend, 0, resultImage);
                        break;
                    case "AND":
                        CvInvoke.BitwiseAnd(firstImage.mat, secondImage.mat, resultImage);
                        break;
                    case "OR":
                        CvInvoke.BitwiseOr(firstImage.mat, secondImage.mat, resultImage);
                        break;
                    case "NOT":
                        CvInvoke.BitwiseNot(firstImage.mat, resultImage);
                        break;
                    case "XOR":
                        CvInvoke.BitwiseXor(firstImage.mat, secondImage.mat, resultImage);
                        break;
                    default:
                        break;
                }

                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(resultImage.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                WindowImgFocused imgWindow = new WindowImgFocused(resultImage, bitmapSource, selectedOperation);
                imgWindow.Show();
                imagesList.Add(imgWindow);
            }
        }

    }
}


