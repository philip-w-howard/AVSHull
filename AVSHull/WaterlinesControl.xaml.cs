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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for WaterlinesControl.xaml
    /// </summary>
    public class WaterlineControlDatax : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _showWaterlines;
        private double _deltaLength;
        private double _deltaHeight;

        public WaterlineControlDatax() { }

        public bool ShowWaterlines
        {
            get { return _showWaterlines; }
            set
            {
                _showWaterlines = value;
                Notify("ShowWaterlines");
            }
        }
        public double DeltaLength
        {
            get { return _deltaLength; }
            set { _deltaLength = value; Notify("DeltaLength"); }
        }

        public double DeltaHeight
        {
            get { return _deltaHeight; }
            set { _deltaHeight = value; Notify("DeltaHeight"); }
        }
        private void Notify(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }


    public partial class WaterlinesControl : UserControl
    {
        public WaterlinesControl()
        {
            InitializeComponent();
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.WaterlinesExpanded = false;

        }
    }
}
