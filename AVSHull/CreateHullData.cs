using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    public class CreateHullData : INotifyPropertyChanged
    {
        private bool _includeBow = true;
        public bool IncludeBow
        {
            get { return _includeBow; }
            set { _includeBow = value; Notify("IncludeBow"); }
        }

        private bool _flatBottom = true;
        public bool FlatBottom
        {
            get { return _flatBottom; }
            set { _flatBottom = value; Notify("FlatBottom"); }
        }

        private int _numBulkheads = 4;
        public int NumBulkheads
        {
            get { return _numBulkheads; }
            set { _numBulkheads = value; Notify("NumBulkheads"); }
        }

        private int _numChines = 5;
        public int NumChines
        {
            get { return _numChines; }
            set { _numChines = value; Notify("NumChines"); }
        }

        private double _length = 80.0;
        public double Length
        {
            get { return _length; }
            set { _length = value; Notify("Length"); }
        }

        private double _width = 80.0;
        public double Width
        {
            get { return _width; }
            set { _width = value; Notify("Width"); }
        }

        private double _height = 80.0;
        public double Height
        {
            get { return _height; }
            set { _height = value; Notify("Height"); }
        }

        private double _transomAngle = 90.0;
        public double TransomAngle
        {
            get { return _transomAngle; }
            set { _transomAngle = value; Notify("TransomAngle"); }
        }

        //******************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
