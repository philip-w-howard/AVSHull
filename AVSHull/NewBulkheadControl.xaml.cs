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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for NewBulkheadControl.xaml
    /// </summary>
    public partial class NewBulkheadControl : UserControl
    {
        public NewBulkheadControl()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.NewBulkheadExpanded = false;

            HullView editableHull = new HullView();
            editableHull.InsertBulkhead(values.NewBulkheadLoc);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.NewBulkheadExpanded = false;
        }
    }
}
