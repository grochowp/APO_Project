using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using APO_Projekt.Features;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using SkiaSharp;

namespace APO_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy Window.xaml
    /// </summary>
    public partial class Operations : Window
    {
        public Mat? mat;
        public Histogram? hist;
        public Histogram2D hist2d;

        public static event Action<Mat, Operations>? windowImgFocused;
        public static event Action? windowImgClosed;
        public Operations(Mat mat, BitmapSource image, string windowName)
        {
            InitializeComponent();
            this.mat = mat;
            this.img.Source = image;
            this.Title = windowName;

            Activated += (s, e) => windowImgFocused?.Invoke(mat, this);
            Closing += ImgClosed;

        }

        public void ImgClosed(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            windowImgClosed?.Invoke();
            this.mat = null;
            this.hist?.Close();
        }

        public void HistogramUpdate()
        {
            if (this.mat != null)
            {
                this.hist?.HistogramShow(this.mat);
            }
        }

        public void Show2dHistogram(Mat mat1, Mat mat2)
        {
           
            if (this.hist2d == null)
            {
                this.hist2d = new Histogram2D();
                this.hist2d.Histogram2DShow(mat1, mat2);
                this.hist2d.Closed += (_, _) => this.hist2d = null;
            }
            else
            {
                this.hist2d.Focus();
            }
        }

        public void HistogramShow()
        {
            if (this.mat == null || this.mat.NumberOfChannels != 1)
            {
                MessageBox.Show("Image is not grayscale");
                return;
            }
            if (this.hist == null)
            {
                this.hist = new Histogram();
                this.hist.HistogramShow(this.mat);
                this.hist.Closed += (_, _) => this.hist = null;
            }
            else
            {
                this.hist.Focus();
            }
        }

        public Mat StretchHistogram(Mat mat)
        {
           return this.StretchContrast(0, 255, 0, 255);
        }

        public Mat StretchContrast(int p1, int p2, int q3, int q4)
        {
            if (mat == null) return null;
            Mat stretchedImage = mat.Clone();

            Image<Gray, byte> grayImage = stretchedImage.ToImage<Gray, byte>();

            double minValue = 0;
            double maxValue = 255;

            for (int i = 0; i < grayImage.Rows; ++i)
            {
                for (int j = 0; j < grayImage.Cols; ++j)
                {
                    byte pixelValue = grayImage.Data[i, j, 0];
                    if (pixelValue >= p1 && pixelValue <= p2)
                    {
                        if (pixelValue < maxValue) maxValue = pixelValue;
                        if (pixelValue > minValue) minValue = pixelValue;
                    }
                }
            }

            for (int i = 0; i < grayImage.Rows; ++i)
            {
                for (int j = 0; j < grayImage.Cols; ++j)
                {
                    double pixelValue = grayImage.Data[i, j, 0];
                    if (pixelValue >= p1 && pixelValue <= p2)
                    {
                        double newValue = ((pixelValue - maxValue) / (minValue - maxValue)) * (q4 - q3) + q3;
                        byte newByteValue = (byte)Math.Round(newValue);
                        grayImage.Data[i, j, 0] = newByteValue;
                    }
                }
            }

            this.mat = grayImage.Mat;
            this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(this.mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return this.mat;
        }

        public void EqualizeHistogram()
        {
            if (mat == null || mat.NumberOfChannels != 1)
            {
                MessageBox.Show("Cannot Equalize Histogram.");
                return;
            }

            int[] histogram = new int[256];
            int[] cumulativeHistogram = new int[256];
            double[] lut = new double[256];

            byte[] data = new byte[mat.Rows * mat.Cols];
            mat.CopyTo(data);

            foreach (byte intensity in data)
            {
                histogram[intensity]++;
            }

            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];
            }

            double scale = 255.0 / (mat.Rows * mat.Cols);

            for (int i = 0; i < 256; i++)
            {
                lut[i] = cumulativeHistogram[i] * scale;
            }
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)lut[data[i]];
            }

            this.mat.SetTo(data);
            this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public void Posterize(Mat mat, int levels)
        {
            Mat result = new Mat(mat.Size, DepthType.Cv8U, 1);

            int step = (int)Math.Ceiling(256.0 / levels);

            byte[] matData = new byte[mat.Rows * mat.Cols * mat.ElementSize];
            mat.CopyTo(matData);

            byte[] resultData = new byte[result.Rows * result.Cols * result.ElementSize];

            for (int y = 0; y < mat.Rows; y++)
            {
                for (int x = 0; x < mat.Cols; x++)
                {
                    int index = y * mat.Cols + x;
                    byte pixelValue = matData[index];

                    byte newValue = (byte)(pixelValue / step * step);

                    resultData[index] = newValue;
                }
            }

            result.SetTo(resultData);

            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(result.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Operations imgWindow = new Operations(result, bitmapSource, "Posterize");
            imgWindow.Show();
        }

        public Mat ApplyLinearSharpening(float[,] mask, BorderType selectedBorderType, string title)
        {  
                Mat grayImage = new Mat();
                CvInvoke.CvtColor(this.mat, grayImage, ColorConversion.Bgr2Gray);


                ConvolutionKernelF matrixKernel = new ConvolutionKernelF(mask);

             
                Mat sharpenedMat = new Mat(grayImage.Size, grayImage.Depth, grayImage.NumberOfChannels);
                System.Drawing.Point point = new System.Drawing.Point(-1, -1);

                CvInvoke.Filter2D(grayImage, sharpenedMat, matrixKernel, point, 0, selectedBorderType);

            //  Mat colorImage = new Mat();
            //   CvInvoke.CvtColor(sharpenedMat, colorImage, ColorConversion.Gray2Bgr);
            CvInvoke.BitwiseNot(sharpenedMat, sharpenedMat);

            this.mat = sharpenedMat;
                this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Title = title;
                HistogramUpdate();

                return this.mat;
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

        public Mat LinearFiltering(BorderType selectedBorderType)
        {
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
                    Mat smoothedMat = this.mat.Clone();
                    CvInvoke.Filter2D(this.mat, smoothedMat, kernelSmooth, new System.Drawing.Point(-1, -1), 0, selectedBorderType);

                    // Wyostrzanie
                    Mat sharpenedMat = this.mat.Clone();
                    CvInvoke.Filter2D(smoothedMat, sharpenedMat, kernelSharp, new System.Drawing.Point(-1, -1), 0, selectedBorderType);


                    Mat result5x5 = this.mat.Clone();

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

                    CvInvoke.Filter2D(this.mat, result5x5, kernelMat, new System.Drawing.Point(-1, -1), 0, selectedBorderType);


                    this.mat = sharpenedMat;
                    this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    this.Title = "3x3 masks operation";
                    this?.HistogramUpdate();

                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(result5x5.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Operations imgWindow = new Operations(result5x5, bitmapSource, "5x5 mask operation");
                    imgWindow.Show();
                    return this.mat;
                }
                return null;
            }
            return null;
        }

        public void PointOperations(List<Operations> imagesList)
        {
            OperationSelector operation = new OperationSelector(imagesList);

            if (operation.ShowDialog() == true)
            {
                Operations firstImage = imagesList[operation.FirstImgIndex];
                Operations secondImage = imagesList[operation.SecondImgIndex];

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
                Operations imgWindow = new Operations(resultImage, bitmapSource, selectedOperation);
                imgWindow.Show();
                imagesList.Add(imgWindow);
            }
        }


        public Mat MorphologyHelper(int size, string element)
        {
            Mat tempMat = new Mat(size, size, DepthType.Cv8U, 1);
            tempMat.SetTo(new MCvScalar(0));
            byte[] diamond = null;

            if (element != null)
            {
                switch (element)
                {
                    case "Diamond":
                        if (size == 3)
                        {
                            diamond = new byte[] { 0, 1, 0, 1, 1, 1, 0, 1, 0 };

                        }
                        else if (size == 5)
                        {
                            diamond = new byte[] { 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
                        }
                        else if (size == 7)
                        {
                            diamond = new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
                        }
                        tempMat.SetTo(diamond);
                        break;

                    case "Square":
                        tempMat = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(size, size), new System.Drawing.Point(-1, -1));
                        break;

                    default:
                        break;
                }
            }
            return tempMat;
        }
        public Mat Morphology(BorderType selectedBorderType)
        {
            Morphology morphology = new Morphology();

            if (morphology.ShowDialog() == true)
            {
                Mat tempImage = this.mat.Clone();
                Mat resultImage = new Mat();
                Image<Gray, byte> grayImage = tempImage.ToImage<Gray, byte>();

                switch (morphology.Operation)
                {
                    case "Erode":
                        CvInvoke.Erode(grayImage, resultImage, MorphologyHelper(morphology.Size, morphology.Element), new System.Drawing.Point(-1, -1), 1, selectedBorderType, new MCvScalar(0));
                        break;
                    case "Dilate":
                        CvInvoke.Dilate(grayImage, resultImage, MorphologyHelper(morphology.Size, morphology.Element), new System.Drawing.Point(-1, -1), 1, selectedBorderType, new MCvScalar(0));
                        break;
                    case "Open":
                        CvInvoke.MorphologyEx(grayImage, resultImage, MorphOp.Open, MorphologyHelper(morphology.Size, morphology.Element), new System.Drawing.Point(-1, -1), 1, selectedBorderType, new MCvScalar(0));
                        break;
                    case "Close":
                        CvInvoke.MorphologyEx(grayImage, resultImage, MorphOp.Close, MorphologyHelper(morphology.Size, morphology.Element), new System.Drawing.Point(-1, -1), 1, selectedBorderType, new MCvScalar(0));
                        break;
                    default:
                        break;
                }

                this.mat = resultImage;
                this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(resultImage.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Title = "Mono (" + morphology.Operation + ") (" + morphology.Size + "x" + morphology.Size + ") (" + morphology.Element + ")";
                this?.HistogramUpdate();
                return this.mat;
            }
            return null;
        }

        public Mat Pyramid(string method)
        {
            Mat pyrImg = this.mat.Clone();
            Image<Gray, byte> grayImage = pyrImg.ToImage<Gray, byte>();

            Mat scaledMat = new Mat();

            if (method == "Upscale")
            {
                CvInvoke.PyrUp(grayImage, scaledMat);

            }
            else if (method == "Downscale")
            {
                CvInvoke.PyrDown(grayImage, scaledMat);
            }

            this.mat = scaledMat;
            this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(scaledMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            this.Title = $"Pyramid {method}";
            this?.HistogramUpdate();
            return this.mat;
        }

        public Mat Skeletonize()
        {
            int size = this.mat.Width * this.mat.Height;

            Mat skel = new Mat(this.mat.Size, DepthType.Cv8U, 1);
            Mat copy = this.mat.Clone();
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Cross, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));

            while (true)
            {
                Mat open = new Mat();
                CvInvoke.MorphologyEx(copy, open, MorphOp.Open, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

                Mat temp = new Mat();
                CvInvoke.Subtract(copy, open, temp);

                Mat erode = new Mat();
                CvInvoke.Erode(copy, erode, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

                CvInvoke.BitwiseOr(skel, temp, skel);

                copy = erode.Clone();

                if (CvInvoke.CountNonZero(copy) == 0)
                    break;
            }

            this.mat = skel;
            this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(skel.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            this.Title = "Skeletonized";
            this?.HistogramUpdate();
            return this.mat;
        }

        public Mat Hough()
        {
           
                Mat image = this.mat.Clone();
                Image<Gray, byte> gray = image.ToImage<Gray, byte>();

                Mat edges = new Mat();
                CvInvoke.Canny(gray, edges, 50, 150);

                LineSegment2D[] lines = CvInvoke.HoughLinesP(
                    edges,
                    1,
                    Math.PI / 180,
                    100,
                    50,
                    10
                );

                foreach (LineSegment2D line in lines)
                {
                    CvInvoke.Line(gray.Mat, line.P1, line.P2, new MCvScalar(0, 0, 255), 2);
                }

                this.mat = gray.Mat;
                this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(gray.Mat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Title = "Hough";
                this?.HistogramUpdate();
                return this.mat;
            
           
        }

        public Mat GrabCut()
        {
           
                if (this.mat == null)
                {
                    MessageBox.Show("No image loaded.");
                    return null;
                }

                Mat mask = new Mat(this.mat.Size, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(2));

                Rectangle rect = new Rectangle(50, 50, 200, 200); 

                CvInvoke.GrabCut(this.mat, mask, rect, new Mat(), new Mat(), 5, GrabcutInitType.InitWithRect);

                byte[] objectPixels = new byte[mask.Rows * mask.Cols];
                mask.CopyTo(objectPixels);

                for (int i = 0; i < objectPixels.Length; i++)
                {
                    objectPixels[i] = (byte)(objectPixels[i] == 1 ? 255 : 0);
                }
                Mat result = new Mat(mask.Size, DepthType.Cv8U, 1);
                result.SetTo(objectPixels);


                this.mat = result;
                this.img.Source = Imaging.CreateBitmapSourceFromHBitmap(result.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Title = "GrabCut Segmentation";
                this?.HistogramUpdate();
                return this.mat;
           
        }
       

        public void Inpaint(List<Operations> imagesList)
        {
            Inpaint inpaint = new Inpaint(imagesList);

            if (inpaint.ShowDialog() == true)
            {
                Mat firstImage = imagesList[inpaint.FirstImgIndex].mat; 
                Mat secondImage = imagesList[inpaint.SecondImgIndex].mat; 
                Image<Gray, byte> image1 = firstImage.ToImage<Gray, byte>();
                Image<Gray, byte> image2 = secondImage.ToImage<Gray, byte>();

                Mat inpaintedImage = new Mat();
                CvInvoke.Inpaint(image1, image2, inpaintedImage, 3, InpaintType.NS);
    
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(inpaintedImage.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Operations imgWindow = new Operations(inpaintedImage, bitmapSource, "Impainted image");
                imgWindow.Show();
                imagesList.Add(imgWindow);
            }
        }

    }
}










