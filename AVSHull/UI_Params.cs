using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    class UI_Params : INotifyPropertyChanged
    {
        private bool? _resizeExpanded = false;
        public bool? ResizeExpanded
        {
            get { return _resizeExpanded; }
            set { _resizeExpanded = value; Notify("ResizeExpanded"); }
        }

        private bool? _newBulkheadExpanded = false;
        public bool? NewBulkheadExpanded
        {
            get { return _newBulkheadExpanded; }
            set { _newBulkheadExpanded = value; Notify("NewBulkheadExpanded"); }
        }

        private bool? _changeChinesExpanded = false;
        public bool? ChangeChinesExpanded
        {
            get { return _changeChinesExpanded; }
            set { _changeChinesExpanded = value; Notify("ChangeChinesExpanded"); }
        }

        private bool? _waterlinesExpanded = false;
        public bool? WaterlinesExpanded
        {
            get { return _waterlinesExpanded; }
            set { _waterlinesExpanded = value; Notify("WaterlinesExpanded"); }
        }

        private bool? _offsetsSetupExpanded = false;
        public bool? OffsetsSetupExpanded
        {
            get { return _offsetsSetupExpanded; }
            set { _offsetsSetupExpanded = value; Notify("OffsetsSetupExpanded"); }
        }

        private bool? _layoutSetupExpanded = false;
        public bool? LayoutSetupExpanded
        {
            get { return _layoutSetupExpanded; }
            set { _layoutSetupExpanded = value; Notify("LayoutSetupExpanded"); }
        }

        private bool? _gCodeSetupExpanded = false;
        public bool? GCodeSetupExpanded
        {
            get { return _gCodeSetupExpanded; }
            set { _gCodeSetupExpanded = value; Notify("GCodeSetupExpanded"); }
        }

        private bool? _offsetSetupExpanded = false;
        public bool? OffsetSetupExpanded
        {
            get { return _offsetSetupExpanded; }
            set { _offsetSetupExpanded = value; Notify("OffsetSetupExpanded"); }
        }

        private bool? _allowBulkheadMoves = false;
        public bool? AllowBulkheadMoves
        {
            get { return _allowBulkheadMoves; }
            set { _allowBulkheadMoves = value; Notify("AllowBulkheadMoves"); }
        }

        private bool? _insertBulkhead = false;
        public bool? InsertBulkhead
        {
            get { return _insertBulkhead; }
            set { _insertBulkhead = value; Notify("InsertBulkhead"); }
        }

        private int _numChines = 5;
        public int NumChines
        {
            get { return _numChines; }
            set { _numChines = value; Notify("NumChines"); }
        }

        private double _newBulkheadLoc = 40.0;
        public double NewBulkheadLoc
        {
            get { return _newBulkheadLoc; }
            set { _newBulkheadLoc = value; Notify("NewBulkheadLoc"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    };
}
