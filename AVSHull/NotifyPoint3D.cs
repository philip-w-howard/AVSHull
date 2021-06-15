using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    class NotifyPoint3D : INotifyPropertyChanged
    {
        private Point3D loc = new Point3D();
        public double X
        {
            get { return loc.X; }
            set { loc.X = value; Notify("X"); }
        }
        public double Y
        {
            get { return loc.Y; }
            set { loc.Y = value; Notify("Y"); }
        }
        public double Z
        {
            get { return loc.Z; }
            set { loc.Z = value; Notify("Z"); }
        }

        public override string ToString()
        {
            return loc.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

    }
}
