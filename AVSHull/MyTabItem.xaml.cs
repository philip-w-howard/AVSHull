using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for MyTabItem.xaml
    /// </summary>
    public partial class MyTabItem : TabItem
    {
        public String TabName { get; set; }

        public MyTabItem()
        {
            InitializeComponent();
        }

        protected override void OnSelected(System.Windows.RoutedEventArgs e)
        {
            base.OnSelected(e);
            Debug.WriteLine("Selected {0} {1}", TabName, TabName.Length);
        }
        protected override void OnUnselected(System.Windows.RoutedEventArgs e)
        {
            base.OnUnselected(e);
            Debug.WriteLine("UnSelected {0} {1}", TabName, TabName.Length);
        }
    }
}

