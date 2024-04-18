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

namespace APO_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy Canals.xaml
    /// </summary>
    public partial class Canals : Window
    {
        public Canals()
        {
            InitializeComponent();
        }

        public int level;
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(level_text.Text, out level);

            this.DialogResult = true;
        }
    }
}
