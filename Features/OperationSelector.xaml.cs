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
    /// Logika interakcji dla klasy OperationSelector.xaml
    /// </summary>
    public partial class OperationSelector : Window
    {
        public OperationSelector(List<Operations> imagesList)
        {
            InitializeComponent();
            foreach (var image in imagesList)
            {
                firstImg_cmb.Items.Add(image.Title);
                secondImg_cmb.Items.Add(image.Title);
            }
        }

        public string Operation;
        public List<Operations> ImagesList;
        public int FirstImgIndex, SecondImgIndex;
        public double Blend;

        private void ButtonMedianFiltration_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbOperation.SelectedItem;
            Operation = typeItem.Content.ToString();
            FirstImgIndex = firstImg_cmb.SelectedIndex;
            SecondImgIndex = secondImg_cmb.SelectedIndex;
            double.TryParse(blendInput.Text.ToString(), out Blend);

            this.DialogResult = true;
        }
    }
}
