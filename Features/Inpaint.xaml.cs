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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APO_Projekt.Features
{
    /// <summary>
    /// Logika interakcji dla klasy Inpaint.xaml
    /// </summary>
    public partial class Inpaint : Window
    {
        public Inpaint(List<Operations> imagesList)
        {
            InitializeComponent();
            foreach (var image in imagesList)
            {
                firstImg_cmb.Items.Add(image.Title);
                secondImg_cmb.Items.Add(image.Title);
            }
        }

        public int FirstImgIndex, SecondImgIndex;

        private void ButtonInpaint_Click(object sender, RoutedEventArgs e)
        {
            FirstImgIndex = firstImg_cmb.SelectedIndex;
            SecondImgIndex = secondImg_cmb.SelectedIndex;

            this.DialogResult = true;
        }
    }
}
