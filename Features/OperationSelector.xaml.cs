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
        public OperationSelector()
        {
            InitializeComponent();
        }

        public int Operation;

        private void ButtonMedianFiltration_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbOperation.SelectedItem;
            

            this.DialogResult = true;
        }
    }
}
