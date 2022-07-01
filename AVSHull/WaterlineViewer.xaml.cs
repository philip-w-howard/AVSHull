using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public class WaterlineControlData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _deltaLength;
        private double _deltaHeight;
        private double _weight;
        private double _waterDensity;
        bool _showAllWaterlines;

        public WaterlineControlData() { }

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
        public double Weight
        {
            get { return _weight; }
            set { _weight = value; Notify("Weight"); }
        }
        public double WaterDensity
        {
            get { return _waterDensity; }
            set { _waterDensity = value; Notify("WaterDensity"); }
        }
        public bool ShowAllWaterlines
        {
            get { return _showAllWaterlines; }
            set { _showAllWaterlines = value; Notify("ShowAllWaterlines"); }
        }
        private void Notify(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }

    /// <summary>
    /// Interaction logic for WaterlineViewer.xaml
    /// </summary>
    public partial class WaterlineViewer : UserControl
    {
        public WaterlineViewer()
        {
            InitializeComponent();
        }

        private void GenerateWaterlines()
        {
            WaterlineControlData values = (WaterlineControlData)this.FindResource("WaterlineData");

            Waterlines.CreateHull();
            Waterlines.IsEditable = false;
            Waterlines.IsRotatable = true;

            Waterlines.GenerateWaterlines(values.DeltaHeight, values.DeltaLength);
            Waterlines.InvalidateVisual();
            Waterlines.Rotate(0, 90, 90);
        }
        private void RedrawClick(object sender, RoutedEventArgs e)
        {
            GenerateWaterlines();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (BaseHull.Instance() != null && BaseHull.Instance().Bulkheads.Count != 0)
            {
                GenerateWaterlines();
            }
        }
    }
}
