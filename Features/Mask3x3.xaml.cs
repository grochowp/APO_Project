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
    /// Logika interakcji dla klasy Mask3x3.xaml
    /// </summary>
    public partial class Mask3x3 : Window
    {
        public Mask3x3()
        {
            InitializeComponent();
        }

        public int M1,M2,M3,M4,M5,M6,M7,M8,M9;

        private void Button3x3_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(m1.Text, out M1);
            int.TryParse(m2.Text, out M2);
            int.TryParse(m3.Text, out M3);
            int.TryParse(m4.Text, out M4);
            int.TryParse(m5.Text, out M5);
            int.TryParse(m6.Text, out M6);
            int.TryParse(m7.Text, out M7);
            int.TryParse(m8.Text, out M8);
            int.TryParse(m9.Text, out M9);

            this.DialogResult = true;
        }
        
    }
}
