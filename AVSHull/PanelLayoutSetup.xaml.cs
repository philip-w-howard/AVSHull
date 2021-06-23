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
    public partial class PanelLayoutSetup : UserControl
    {
        public PanelLayoutSetup()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.LayoutSetupExpanded = false;
        }
    }
}