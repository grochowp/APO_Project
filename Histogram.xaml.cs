using Emgu.CV;
using Emgu.CV.Structure;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
                    byte intensidad = image.Data[i, j, 0];
                    data[intensidad]++;
                }
            }

            List<TabHist> tableData = new List<TabHist>();

            for(int i = 0; i< data.Length; i++)
            {
                tableData.Add(new TabHist{intensidad = i, contar = data[i] });
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
           // DataGridTextColumn column = new DataGridTextColumn();
           // column.Header = "intensidad";
           // column.Binding

            this.Show();
        }
    }

    public class TabHist
    {
        public int intensidad { get; set; }
        public int contar { get; set; }
    }
}
