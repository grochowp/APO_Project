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
    /// Logika interakcji dla klasy StretchData.xaml
    /// </summary>
    public partial class StretchData : Window
    {
        public StretchData()
        {
            InitializeComponent();
        }

        public int P1, P2, Q3, Q4;

        private void Stretch_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(p1_text.Text, out P1);
            int.TryParse(p2_text.Text, out P2);
            int.TryParse(q3_text.Text, out Q3);
            int.TryParse(q4_text.Text, out Q4);

            this.DialogResult = true;
        }
    }
}
