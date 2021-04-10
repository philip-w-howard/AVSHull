using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AVSHull
{
     /// <summary>
    /// Interaction logic for PanelLayoutSetup.xaml
    /// </summary>
    public partial class PanelLayoutSetup : Window
    {
        public bool OK { get; private set; }
        public PanelLayoutSetup()
        {
            InitializeComponent();
            OK = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            OK = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OK = false;
            Close();
        }
    }
}
