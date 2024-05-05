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
    /// Logika interakcji dla klasy Morphology.xaml
    /// </summary>
    public partial class Morphology : Window
    {
        public Morphology()
        {
            InitializeComponent();
        }

        public int Size;
        public string Operation, Element;
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem size = (ComboBoxItem)cmb_Size.SelectedItem;
            ComboBoxItem operation = (ComboBoxItem)cmb_Operation.SelectedItem;
            ComboBoxItem element = (ComboBoxItem)cmb_Element.SelectedItem;

            int.TryParse(size.Content.ToString().Split('x')[0], out Size);
            Operation = operation.Content.ToString();
            Element = element.Content.ToString();
            this.DialogResult = true;
        }
    }
}
