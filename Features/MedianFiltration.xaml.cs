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
    /// Logika interakcji dla klasy MedianFiltration.xaml
    /// </summary>
    public partial class MedianFiltration : Window
    {
        public MedianFiltration()
        {
            InitializeComponent();
        }

        public int Mask;
        private void ButtonMedianFiltration_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbMask.SelectedItem;
            int.TryParse(typeItem.Content.ToString().Split('x')[0], out Mask);

            this.DialogResult = true;
        }
    }
}
