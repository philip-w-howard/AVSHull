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
        private double _freeboard;
        private double _centroidX;
        private double _centroidY;
        private double _centroidZ;
        private double _momentX;
        private double _heelAngle;
        private double _pitchAngle;

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

        public double Freeboard
        {
            get { return _freeboard; }
            set { _freeboard = value; Notify("Freeboard"); }
        }
        public double CentroidX
        {
            get { return _centroidX; }
            set { _centroidX = value; Notify("CentroidX"); }
        }
        public double CentroidY
        {
            get { return _centroidY; }
            set { _centroidY = value; Notify("CentroidY"); }
        }
        public double CentroidZ
        {
            get { return _centroidZ; }
            set { _centroidZ = value; Notify("CentroidZ"); }
        }
        public double MomentX
        {
            get { return _momentX; }
            set { _momentX = value; Notify("MomentX"); }
        }
        public double HeelAngle
        {
            get { return _heelAngle; }
            set { _heelAngle = value; Notify("HeelAngle"); }
        }
        public double PitchAngle
        {
            get { return _pitchAngle; }
            set { _pitchAngle = value; Notify("PitchAngle"); }
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

            WaterlineHull.CreateHull();
            WaterlineHull.IsEditable = false;
            WaterlineHull.IsRotatable = true;

            if (values.HeelAngle != 0) WaterlineHull.Rotate(0, 0, values.HeelAngle);
            if (values.PitchAngle != 0) WaterlineHull.Rotate(values.PitchAngle, 0, 0);

            WaterlineHull.Hull.GenerateWaterlines(values.DeltaHeight, values.DeltaLength, values.Weight, values.WaterDensity, values.ShowAllWaterlines);

            if (values.PitchAngle != 0) WaterlineHull.Rotate(-values.PitchAngle, 0, 0);
            if (values.HeelAngle != 0) WaterlineHull.Rotate(0, 0, -values.HeelAngle);

            values.Freeboard = WaterlineHull.Hull.Freeboard;
            values.CentroidX = WaterlineHull.Hull.Centroid.X;
            values.CentroidY = WaterlineHull.Hull.Centroid.Y;
            values.CentroidZ = WaterlineHull.Hull.Centroid.Z;

            values.MomentX = WaterlineHull.Hull.RightingMomentX;

            WaterlineHull.InvalidateVisual();
            WaterlineHull.Rotate(0, 90, 90);
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
