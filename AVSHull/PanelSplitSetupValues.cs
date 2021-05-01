﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    class PanelSplitSetupValues : INotifyPropertyChanged
    {
        private double _start;
        public double Start
        {
            get { return _start; }
            set { _start = value; Notify("Start"); }
        }
        private int _numTongues;
        public int NumTongues
        {
            get { return _numTongues; }
            set { _numTongues = value; Notify("NumTongues"); }
        }
        private double _depth;
        public double Depth
        {
            get { return _depth; }
            set { _depth = value; Notify("Depth"); }
        }

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