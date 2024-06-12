using Emgu.CV;
using Emgu.CV.Structure;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Windows;

namespace APO_Projekt.Features
{
    /// <summary>
    /// Logika interakcji dla klasy Histogram2D.xaml
    /// </summary>
    public partial class Histogram2D : Window
    {
        public Histogram2D()
        {
            InitializeComponent();
        }

        public void Histogram2DShow(Mat mat1, Mat mat2)
        {

            try
            {
                double[,] data = new double[256, 256];
                Image<Gray, byte> image1 = mat1.ToImage<Gray, byte>();
                Image<Gray, byte> image2 = mat2.ToImage<Gray, byte>();

                for (int i = 0; i < image1.Height; i++)
                  {
                      for (int j = 0; j < image1.Width; j++)
                      {
                          byte intensity1 = image1.Data[i, j, 0];
                          byte intensity2 = image2.Data[i, j, 0];
                        // MessageBox.Show(intensity1.ToString() + " | " + intensity2.ToString());
                        data[intensity1, intensity2]++;
                      }
                  }
               
                var model = new PlotModel { Title = "Histogram 2D" };

              

                var colorAxis = new LinearColorAxis
                {
                    Position = AxisPosition.Right,
                    Palette = OxyPalettes.Jet(200) 
                };

                model.Axes.Add(colorAxis);

                var heatMapSeries = new HeatMapSeries
                {
                    X0 = 0,
                    X1 = 255,
                    Y0 = 0,
                    Y1 = 255,
                    Interpolate = false,
                    RenderMethod = HeatMapRenderMethod.Bitmap,
                    Data = data,
                    LabelFontSize = 0
                };
                model.Series.Add(heatMapSeries);

                var plotView = new OxyPlot.Wpf.PlotView
                {
                    Model = model,
                    Width = 600,
                    Height = 400
                };

                var newWindow = new Histogram2D
                {
                    Content = plotView,
                    Width = 600,
                    Height = 400
                };
                newWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas wyświetlania histogramu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
