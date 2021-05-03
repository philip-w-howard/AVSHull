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

        private int _numChines = 5;

        public UI_Params()
        {
        }

        public int NumChines
        {
            get { return _numChines; }
            set { _numChines = value; Notify("NumChines"); }
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
