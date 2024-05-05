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
using System.Drawing.Imaging;

namespace APO_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public Mat? displayedImage;
        public Operations? operations;
        private BorderType selectedBorderType = BorderType.Isolated;

        private List<Operations> imagesList = new List<Operations>();


        public MainWindow()
        {
            InitializeComponent();
            Operations.windowImgFocused += UpdateFocus;
            Operations.windowImgClosed += ClearFocus;
        }

        

        private void BorderType_Default_Click(object sender, RoutedEventArgs e)
        {
            selectedBorderType = BorderType.Default;

            MenuItem_Default.IsChecked = true;
            MenuItem_Isolated.IsChecked = false;
            MenuItem_Replicate.IsChecked = false;
            MenuItem_Reflect.IsChecked = false;
        }
        private void BorderType_Isolated_Click(object sender, RoutedEventArgs e)
        {
            selectedBorderType = BorderType.Isolated;

            MenuItem_Default.IsChecked = false;
            MenuItem_Isolated.IsChecked = true;
            MenuItem_Replicate.IsChecked = false;
            MenuItem_Reflect.IsChecked = false;
        }

        private void BorderType_Replicate_Click(object sender, RoutedEventArgs e)
        {
            selectedBorderType = BorderType.Replicate;

            MenuItem_Default.IsChecked = false;
            MenuItem_Isolated.IsChecked = false;
            MenuItem_Replicate.IsChecked = true;
            MenuItem_Reflect.IsChecked = false;
        }

        private void BorderType_Reflect_Click(object sender, RoutedEventArgs e)
        {
            selectedBorderType = BorderType.Reflect;

            MenuItem_Default.IsChecked = false;
            MenuItem_Isolated.IsChecked = false;
            MenuItem_Replicate.IsChecked = false;
            MenuItem_Reflect.IsChecked = true;
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|JPEG Image|*.jpg;*.jpeg|PNG Image|*.png";
            saveFileDialog.Title = "Save an Image File";
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;

                // Zapisanie obrazu w wybranym formacie
                switch (saveFileDialog.FilterIndex)
                {
                    case 1: // BMP
                        CvInvoke.Imwrite(fileName, displayedImage);
                        break;
                    case 2: // JPEG
                        using (Image<Bgr, byte> image = displayedImage.ToImage<Bgr, byte>())
                        {
                            Bitmap bitmap = image.ToBitmap();
                            bitmap.Save(fileName, ImageFormat.Jpeg);
                        }
                        break;
                    case 3: // PNG
                        using (Image<Bgr, byte> image = displayedImage.ToImage<Bgr, byte>())
                        {
                            Bitmap bitmap = image.ToBitmap();
                            bitmap.Save(fileName, ImageFormat.Png);
                        }
                        break;
                }
            }
        }


        //  LAB 1

        private void UpdateFocus(Mat mat, Operations windowImg)
        {
            this.displayedImage = mat;
            this.operations = windowImg;
        }
        private void ClearFocus()
        {
            this.displayedImage = null;
            this.operations = null;
        }

        private void ImportMono_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

            Image.DefaultExt = ".bmp";
            Image.Filter = "JPG/PNG Files (*.jpeg, *.jpg, *.png, *.bmp)|*.jpeg;*.jpg;*.png;*.bmp|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            if (Image.ShowDialog() == true)

            {
                string fileName = Image.FileName;
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Grayscale);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Operations imgWindow = new Operations(ImageOpened, bitmapSource, "Mono " + fileName);
                imgWindow.Show();
                imagesList.Add(imgWindow);
            }
        }
        private void ImportColor_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Image = new OpenFileDialog();

            Image.DefaultExt = ".bmp";
            Image.Filter = "JPG/PNG Files (*.jpeg, *.jpg, *.png, *.bmp)|*.jpeg;*.jpg;*.png;*.bmp|PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg";
            if (Image.ShowDialog() == true)

            {
                string fileName = Image.FileName;
                Mat ImageOpened = CvInvoke.Imread(fileName, ImreadModes.Color);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ImageOpened.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Operations image = new Operations(ImageOpened, bitmapSource, "Color " + fileName);
                image.Show();
            }
        }
        private void ColorToGray_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.operations == null) MessageBox.Show("No image selected.");
            else if (this.displayedImage.NumberOfChannels == 1) MessageBox.Show("Image can`t be grayscale.");
            else
            {
                Mat mat = new Mat();
                CvInvoke.CvtColor(this.displayedImage, mat, ColorConversion.Bgr2Gray);
                this.displayedImage = mat;
                this.operations.mat = mat;
                operations.Title = "Mono";
                this.operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                imagesList.Add(operations);

            }

        }
        private void Negation_Click(object obj, RoutedEventArgs e)
        {
            //if (this.imageMat.NumberOfChannels != 1) this.ColorToGray_Click(obj, e);
            if (this.displayedImage == null || this.operations == null) MessageBox.Show("Can`t call negation sry");
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
                this.operations.mat = grayImg.Mat;
                this.operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(grayImg.Mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                operations.Title = "Negation ";
                this.operations.HistogramUpdate();
            }
        }
        private void Histogram_Click(object sender, RoutedEventArgs e)
        {
            this.operations?.HistogramShow();
        }
        private void ImageSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || this.displayedImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is grayscale");
                return;
            }
            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(this.operations?.mat, vector);
            for (int i = 0; i < vector.Size; ++i)
            {
                Mat vect = vector[i];
                Mat vectClone = vect.Clone();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(vectClone.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Operations imgWindow = new Operations(vectClone, bitmapSource, "Channel " + (i + 1));
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
            if (this.operations?.mat == null) return;
            Mat initial = this.operations.mat;
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
                Operations imgWindow = new Operations(vectClone, bitmapSource, "(HSV) " + channelNames[i]);
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
            if (this.operations?.mat == null) return;
            Mat initial = this.operations.mat;
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
                Operations imgWindow = new Operations(vectClone, bitmapSource, "(Lab) " + channelNames[i]);
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

            this.operations?.StretchHistogram(this.displayedImage);
            this.operations?.HistogramUpdate();
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
                this.operations?.StretchContrast(p1, p2, q3, q4);
                this.operations?.HistogramUpdate();

            }
        }

        private void Equalize_Click(object sender, RoutedEventArgs e)
        {
            {
                operations?.EqualizeHistogram();
                operations?.HistogramUpdate();
            }
        }

        //  LAB 2

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
                this.operations?.Posterize(this.displayedImage, level);
                this.operations?.HistogramUpdate();
            }
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat smoothedMat = new Mat();
            System.Drawing.Size kernelSize = new System.Drawing.Size((int)3, (int)3);
            System.Drawing.Point point = new System.Drawing.Point(-1, -1);

            CvInvoke.Blur(displayedImage, smoothedMat, kernelSize, point, BorderType.Default);

            displayedImage = smoothedMat;
            operations.mat = smoothedMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(smoothedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations?.HistogramUpdate();
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat smoothedMat = new Mat();
            System.Drawing.Size kernelSize = new System.Drawing.Size((int)3, (int)3);

            CvInvoke.GaussianBlur(displayedImage, smoothedMat, kernelSize, 0);
            displayedImage = smoothedMat;
            operations.mat = smoothedMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(smoothedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations?.HistogramUpdate();
        }

        private void SobelEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();

            CvInvoke.Sobel(displayedImage, edgesMat, DepthType.Cv8U, 1, 1, 3, 1, 1, selectedBorderType);
            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            displayedImage = edgesMat;
            operations.mat = edgesMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations.Title = "(Sobel) " + selectedBorderType;
            operations?.HistogramUpdate();
        }

        private void LaplacianEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();

            CvInvoke.Laplacian(displayedImage, edgesMat, DepthType.Cv8U, 1, 1, 0, selectedBorderType);
            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            //CvInvoke.Threshold(edgesMat, edgesMat, 50, 255, ThresholdType.Binary);

            displayedImage = edgesMat;
            operations.mat = edgesMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations.Title = "(Laplacian) " + selectedBorderType;
            operations?.HistogramUpdate();
        }

        private void CannyEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            Mat edgesMat = new Mat();
            CvInvoke.Canny(displayedImage, edgesMat, 100, 200);

            CvInvoke.BitwiseNot(edgesMat, edgesMat);

            displayedImage = edgesMat;
            operations.mat = edgesMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations.Title = "(Canny) " + selectedBorderType;
            operations?.HistogramUpdate();
        }

        private void LinearLaplacianSharpening1_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
            { 0, -1, 0 },
            { -1, 5, -1 },
            { 0, -1, 0 }
        };
            ApplyLinearSharpening(laplacianMask, "(mask 1) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening2_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
                { 1, 1, 1 },
                {  1, -9, 1},
                { 1, 1, 1 }
            };
            ApplyLinearSharpening(laplacianMask, "(mask 2) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening3_Click(object sender, RoutedEventArgs e)
        {
            float[,] laplacianMask = {
                { 1, -2, 1 },
                { -2, 5, -2 },
                { 1, -2, 1 }
            };
            ApplyLinearSharpening(laplacianMask, "(mask 3) Linear Laplacian Sharpening");
        }


        private void PrewittEdgeDetectionH_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                {-1, 0, 1 },
                {-1, 0, 1 },
                {-1, 0, 1 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt H");
        }
        private void PrewittEdgeDetectionV_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                { -1, -1, -1},
                {0, 0, 0 },
                {1, 1, 1 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt V");
        }
        private void PrewittEdgeDetectionNE_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 0, 1, 1 },
              {-1, 0, 1 },
              {-1, -1, 0 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt NE");
        }
        private void PrewittEdgeDetectionSE_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
               { -1, 0, 1 },
               {-1, 0, 1 },
               {0, -1, 1 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt SE");
        }
        private void PrewittEdgeDetectionNW_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
                { 1, 1, 0 },
                {1, 0, -1 },
                {0, -1, -1 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt NW");
        }
        private void PrewittEdgeDetectionSW_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
               { 0, -1, -1 },
               {1, 0, -1 },
               {1, 1, 0 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt SW");
        }
        private void PrewittEdgeDetectionIH_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 1, 0, -1 },
              {1, 0, -1 },
              {1, 0, -1 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt IH");
        }
        private void PrewittEdgeDetectionIV_Click(object sender, RoutedEventArgs e)
        {
            float[,] prewittMask = {
              { 0, -1, 0 },
              {-1, 0, -1 },
              {0, 1, 0 }
            };
            ApplyLinearSharpening(prewittMask, "Prewitt IV");
        }

        private void ApplyLinearSharpening(float[,] mask, string title)
        {

            if (displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            if (mask.GetLength(0) != 3 || mask.GetLength(1) != 3)
            {
                MessageBox.Show("Invalid mask size. Must be 3x3.");
                return;
            }
            operations?.ApplyLinearSharpening(mask, selectedBorderType, title);

           
        }
        private void CustomLinearOperation_Click(object sender, RoutedEventArgs e)
        {
            if (this.displayedImage == null || operations == null)
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

                 float[,] customMast = {
                 { m1, m2, m3 },
                 {m4, m5, m6 },
                 {m7, m8, m9 }
             };
                 operations?.ApplyLinearSharpening(customMast,selectedBorderType, "Custom Linear Operation");


         /*   Mat kernel = new Mat(3, 3, DepthType.Cv32F, 1);
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
         */
            }
        }

        private void MedianFiltration_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
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
                operations.mat = sharpenedMat;
                operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                operations.Title = "Median Filtration";
                operations?.HistogramUpdate();


            }
        }

        private void LinearFiltering_Click(object sender, RoutedEventArgs e)
        {

            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            operations.LinearFiltering(selectedBorderType);
        }

        private void PointOperation_Click(object sender, RoutedEventArgs e)
        {


            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            operations.PointOperations(imagesList);
        }

        //  LAB 3
        private void Morphology_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            operations.Morphology(selectedBorderType);
        }

        private void PyramidUpscale_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            operations.Pyramid("Upscale");
        }

        private void PyramidDownscale_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            operations.Pyramid("Downscale");
        }

        private void Skeletonize_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            else if (this.displayedImage.NumberOfChannels != 1)
            {
                MessageBox.Show("Image need to be grayscale.");

                return;
            }
                operations.Skeletonize();
        }

        private void Hough_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            operations.Hough();
        }

        private void ProfileLine_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
             operations.ProfileLine();
        }
    }
}


