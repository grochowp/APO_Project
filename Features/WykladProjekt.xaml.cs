using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
    /// Logika interakcji dla klasy WykladProjekt.xaml
    /// </summary>
    public partial class WykladProjekt : Window
    {
        public WykladProjekt(List<Operations> imagesList)
        {
            InitializeComponent();
            foreach (var image in imagesList)
            {
                firstImg_cmb.Items.Add(image.Title);
            }
        }


        public string Operation, BorderType;
        public int FirstImgIndex;

        private void operation_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem operation = (ComboBoxItem)operation_cmb.SelectedItem;
            Operation = operation.Content.ToString();
            ComboBoxItem border = (ComboBoxItem)borderType_cmb.SelectedItem;
            BorderType = border.Content.ToString();
            FirstImgIndex = firstImg_cmb.SelectedIndex;
  
            this.DialogResult = true;
        }

    }
}
