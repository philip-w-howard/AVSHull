using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    class UI_Params : INotifyPropertyChanged
    {
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
