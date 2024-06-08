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
using System.IO;
using Emgu.CV.Reg;

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


        private void AboutInfo_Click(object sender, RoutedEventArgs e)
        {
            AboutProgram ap = new AboutProgram();
            ap.Show();
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

            SaveFileDialog saveDialog = new SaveFileDialog();

            saveDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp"; // Ustaw filtry dla różnych formatów obrazów

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    Bitmap bitmap = displayedImage.ToBitmap();


                    ImageFormat format;

                    switch (saveDialog.FilterIndex)

                    {

                        case 1: // JPEG

                            format = ImageFormat.Jpeg;

                            break;

                        case 2: // PNG

                            format = ImageFormat.Png;

                            break;

                        case 3: // BMP

                            format = ImageFormat.Bmp;

                            break;

                        default:

                            throw new Exception("Nieobsługiwany format obrazu.");
                    }
                    bitmap.Save(saveDialog.FileName, format);

                    MessageBox.Show("Obraz został pomyślnie zapisany.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd podczas zapisywania obrazu: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string title = "Mono " + Path.GetFileNameWithoutExtension(fileName);
                Operations image = new Operations(ImageOpened, bitmapSource, title);
                image.Show();
                imagesList.Add(image);
                image.Closed += Image_Closed;

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
                string title = "Color " + Path.GetFileNameWithoutExtension(fileName);
                Operations image = new Operations(ImageOpened, bitmapSource, title);
                image.Show();
                imagesList.Add(image);
                image.Closed += Image_Closed;
            }
        }

        private void Image_Closed(object sender, EventArgs e)
        {
            if (sender is Operations closedImage)
            {
                imagesList.Remove(closedImage); // Usuwamy obraz z listy po zamknięciu okna
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

            displayedImage = this.operations?.StretchHistogram(this.displayedImage);
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
                displayedImage = this.operations?.StretchContrast(p1, p2, q3, q4);
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
            CvInvoke.CvtColor(displayedImage, displayedImage, ColorConversion.Bgr2Gray);

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
            CvInvoke.CvtColor(displayedImage, displayedImage, ColorConversion.Bgr2Gray);

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
            CvInvoke.CvtColor(displayedImage, displayedImage, ColorConversion.Bgr2Gray);

            displayedImage = edgesMat;
            operations.mat = edgesMat;
            operations.img.Source = Imaging.CreateBitmapSourceFromHBitmap(edgesMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            operations.Title = "(Canny) " + selectedBorderType;
            operations?.HistogramUpdate();
        }

        private void LinearLaplacianSharpening1_Click(object sender, RoutedEventArgs e)
        {

           ApplyLinearSharpening("(mask 1) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening2_Click(object sender, RoutedEventArgs e)
        {
           ApplyLinearSharpening("(mask 2) Linear Laplacian Sharpening");
        }

        private void LinearLaplacianSharpening3_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening("(mask 3) Linear Laplacian Sharpening");
        }


        private void PrewittEdgeDetectionH_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening("Prewitt H");
        }
        private void PrewittEdgeDetectionV_Click(object sender, RoutedEventArgs e)
        {

            ApplyLinearSharpening("Prewitt V");
        }
        private void PrewittEdgeDetectionNE_Click(object sender, RoutedEventArgs e)
        {

            ApplyLinearSharpening("Prewitt NE");
        }
        private void PrewittEdgeDetectionSE_Click(object sender, RoutedEventArgs e)
        {

            ApplyLinearSharpening("Prewitt SE");
        }
        private void PrewittEdgeDetectionNW_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening("Prewitt NW");
        }
        private void PrewittEdgeDetectionSW_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening("Prewitt SW");
        }
        private void PrewittEdgeDetectionIH_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening ("Prewitt IH");
        }
        private void PrewittEdgeDetectionIV_Click(object sender, RoutedEventArgs e)
        {
            ApplyLinearSharpening("Prewitt IV");
        }

        private void ApplyLinearSharpening(string title)
        {

            float[,] mask = null;

            switch (title)
            {
                case "Prewitt H":
                    mask = new float[,] {
                        { -1, 0, 1 },
                        { -1, 0, 1 },
                        { -1, 0, 1 }
                    };
                    break;

                case "Prewitt V":
                    mask = new float[,] {
                        { -1, -1, -1},
                        { 0, 0, 0 },
                        { 1, 1, 1 }
                    };
                    break;

                case "Prewitt NE":
                    mask = new float[,] {
                        { 0, 1, 1 },
                        {-1, 0, 1 },
                        {-1, -1, 0 }
                       };
                    break;

                case "Prewitt SE":
                    mask = new float[,] {
                       { -1, -1, 0 },
                       {-1, 0, 1 },
                       {0, 1, 1 }
                    };
                    break;

                case "Prewitt NW":
                    mask = new float[,] {
                        { 1, 1, 0 },
                        {1, 0, -1 },
                        {0, -1, -1 }
                       };
                    break;

                case "Prewitt SW":
                    mask = new float[,] {
                        { 0, -1, -1 },
                        {1, 0, -1 },
                        {1, 1, 0 }
                       };
                    break;

                case "Prewitt IH":
                    mask = new float[,] {
                       { 1, 0, -1 },
                       {1, 0, -1 },
                       {1, 0, -1 }
                     };
                    break;

                case "Prewitt IV":
                    mask = new float[,] {
                       { 0, -1, 0 },
                       {-1, 0, -1 },
                       {0, 1, 0 }
                      };
                    break;

                case "(mask 1) Linear Laplacian Sharpening":
                    mask = new float[,]{
                        { 0, -1, 0 },
                        { -1, 5, -1 },
                        { 0, -1, 0 }
                    };
                    break;

                case "(mask 2) Linear Laplacian Sharpening":
                    mask = new float[,] {
                        { 1, 1, 1 },
                        {  1, -9, 1},
                        { 1, 1, 1 }
                       };
                    break;

                case "(mask 3) Linear Laplacian Sharpening":
                    mask = new float[,] {
                        { 1, -2, 1 },
                        { -2, 5, -2 },
                        { 1, -2, 1 }
                    };
                    break;
            }

          

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
            displayedImage = operations?.ApplyLinearSharpening(mask, selectedBorderType, title);


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
                displayedImage = operations?.ApplyLinearSharpening(customMast, selectedBorderType, "Custom Linear Operation");


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

            displayedImage = operations.LinearFiltering(selectedBorderType);
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
            displayedImage = operations.Morphology(selectedBorderType);
        }

        private void PyramidUpscale_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            displayedImage = operations.Pyramid("Upscale");
        }

        private void PyramidDownscale_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            displayedImage = operations.Pyramid("Downscale");
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
            displayedImage = operations.Skeletonize();
        }

        private void Hough_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }
            displayedImage = operations.Hough();
        }




        //  LAB 4

        private void GrabCut_Click(object sender, EventArgs e)
        {
            if (this.displayedImage == null || this.operations == null)
            {
                MessageBox.Show("No image selected.");
                return;
            }

            displayedImage = operations.GrabCut();
        }


        private void Inpaint_Click(object sender, EventArgs e)
        {
            if (this.operations == null)
            {
                MessageBox.Show("No images imported.");
                return;
            }

            operations.Inpaint(imagesList);
        }

        // PROJEKT WYKLADOWY

        private void Wyklad_Click(object sender, EventArgs e)
        {
            if (this.operations == null)
            {
                MessageBox.Show("No images imported.");
                return;
            }

            WykladProjekt projekt = new WykladProjekt(imagesList);

            if (projekt.ShowDialog() == true)
            {
                Mat firstMat = new Mat();
                Mat secondImage = displayedImage.Clone();



                switch (projekt.BorderType)
                {
                    case "Default":
                        BorderType_Default_Click(this, null);
                        break;

                    case "Isolated":
                        BorderType_Isolated_Click(this, null);
                        break;
                    case
                        "Reflect":
                        BorderType_Reflect_Click(this, null);
                        break;
                    case
                        "Replicate":
                        BorderType_Replicate_Click(this, null);
                        break;
                }

                ApplyLinearSharpening(projekt.Operation);

                firstMat = displayedImage;


                CvInvoke.CvtColor(secondImage, secondImage, ColorConversion.Bgr2Gray);
                //  CvInvoke.CvtColor(displayedImage, displayedImage, ColorConversion.Bgr2Gray);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(secondImage.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Operations imgWindow = new Operations(secondImage, bitmapSource, "(Gray)");
                imgWindow.Show();



                operations.Show2dHistogram(firstMat, secondImage);

            }
        }

    }
}

