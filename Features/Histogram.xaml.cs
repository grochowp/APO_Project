using Emgu.CV;
using Emgu.CV.Structure;
using LiveCharts;
using LiveCharts.Wpf;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace APO_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy Histogram.xaml
    /// </summary>
    public partial class Histogram : Window
    {
        public Histogram()
        {
            InitializeComponent();
        }

        public void HistogramShow(Mat mat)
        {
            int[] data = new int[256];
            Image<Gray, byte> image = mat.ToImage<Gray, byte>();

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    byte intens = image.Data[i, j, 0];
                    data[intens]++;
                }
            }

            List<TabHist> tableData = new List<TabHist>();

            for(int i = 0; i< data.Length; i++)
            {
                tableData.Add(new TabHist{ intensity = i, count = data[i] });
            }


            ColumnSeries<int> histSeries = new ColumnSeries<int>
            {
                Values = data,
                Fill = new SolidColorPaint(SKColors.SkyBlue),
                Padding = 0,
                YToolTipLabelFormatter = (point) => point.Coordinate.ToString(),
            };

            HistogramName.Series = new ISeries[] { histSeries };
            DataName.ItemsSource = tableData;

            this.Show();
        }
       
    }

    public class TabHist
    {
        public int intensity { get; set; }
        public int count { get; set; }
    }
}
