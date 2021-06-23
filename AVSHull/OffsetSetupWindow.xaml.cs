using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for OffsetSetupWindow.xaml
    /// </summary>
    public partial class OffsetSetupWindow : UserControl
    {
        public OffsetSetupWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.OffsetsSetupExpanded = false;
        }

        //public String OutputType
        //{
        //    get { return OutputType_Input.Text; }
        //}
        //public String SpacingStyle
        //{
        //    get { return SpacingStyle_Input.Text; }
        //}
        //public String Spacing
        //{
        //    get { return Spacing_Input.Text; }
        //}

        private void OutputTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            switch (box.SelectedItem.ToString())
            {
                case "Eighths":
                    break;
                case "Sixteenths":
                    break;
                case "Thirtysecondths":
                    break;
                case "Decimal 2-digits":
                    break;
                case "Decimal 3-digits":
                    break;
                case "Decimal 4-digits":
                    break;
                default:
                    break;
            }
        }
    }
}
