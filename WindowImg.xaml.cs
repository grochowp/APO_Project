﻿using System;
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
using Microsoft.Win32;

namespace APO_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy Window.xaml
    /// </summary>
    public partial class WindowImg : Window
    {
        public Mat? mat;
        public Histogram? hist;

        public static event Action<Mat, WindowImg>? WindowImgFocused;
        public static event Action? WindowImgClosed;
        public WindowImg(Mat mat, BitmapSource image)
        {
            InitializeComponent();
            this.mat = mat;
            this.img.Source = image;

            Activated += (s, e) => WindowImgFocused?.Invoke(mat, this);
            Closing += ImgClosed;

        }

        public void ImgClosed(object? sender, System.ComponentModel.CancelEventArgs e) {
            WindowImgClosed?.Invoke();
            this.mat = null;
            this.hist?.Close();
        }

        public void HistogramUpdate()
        {
            if(this.mat!= null)
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
    }
}










