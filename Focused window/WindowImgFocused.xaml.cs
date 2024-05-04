using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using APO_Projekt.Features;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace APO_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy Window.xaml
    /// </summary>
    public partial class WindowImgFocused : Window
    {
        public Mat? mat;
        public Histogram? hist;

        public static event Action<Mat, WindowImgFocused>? windowImgFocused;
        public static event Action? windowImgClosed;
        public WindowImgFocused(Mat mat, BitmapSource image, string windowName)
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

        public void StretchHistogram(Mat mat)
        {
            this.StretchContrast(0, 255, 0, 255);
        }

        public void StretchContrast(int p1, int p2, int q3, int q4)
        {
            if (mat == null) return;
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
            WindowImgFocused imgWindow = new WindowImgFocused(result, bitmapSource, "Posterize");
            imgWindow.Show();
        }

        public void ApplyLinearSharpening(float[,] mask, BorderType selectedBorderType, string title)
        {
            ConvolutionKernelF matrixKernel = new ConvolutionKernelF(mask);

            try
            {
                Mat sharpenedMat = new Mat();
                System.Drawing.Point point = new System.Drawing.Point(-1, -1);

                if (mat.Size != sharpenedMat.Size)
                {
                    sharpenedMat = mat.Clone();
                }

                CvInvoke.Filter2D(mat, sharpenedMat, matrixKernel, point, 0, selectedBorderType);

                mat = sharpenedMat;
                img.Source = Imaging.CreateBitmapSourceFromHBitmap(sharpenedMat.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Title = title;
                HistogramUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying linear sharpening: " + ex.Message);
                Console.WriteLine("Exception details: " + ex.ToString());
            }
        }
    }
}










