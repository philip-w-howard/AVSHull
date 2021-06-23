using System;
using System.Collections.Generic;
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
    /// Interaction logic for GCodeSetup.xaml
    /// </summary>
    public partial class GCodeSetup : UserControl

    {
        public GCodeSetup()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.GCodeSetupExpanded = false;
        }
    }
}
