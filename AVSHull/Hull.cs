using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class Hull : INotifyPropertyChanged, ICloneable
    {
        public List<Bulkhead> bulkheads;

        public Hull()
        {
            bulkheads = new List<Bulkhead>();
        }

        public void LoadFromHullFile(string filename)
        {
            bulkheads = new List<Bulkhead>();

            using (StreamReader file = File.OpenText(filename))
            {
                string line;
                int num_chines;
                int numBulkheads = 5;

                line = file.ReadLine();
                if (!int.TryParse(line, out num_chines)) throw new Exception("Invalid HUL file format");

                Bulkhead bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.BOW);
                bulkheads.Add(bulkhead);

                for (int ii = 1; ii < numBulkheads - 1; ii++)
                {
                    bulkhead = new Bulkhead();
                    bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.VERTICAL);
                    bulkheads.Add(bulkhead);
                }

                bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.TRANSOM);
                bulkheads.Add(bulkhead);
            }
            RepositionToZero();

            Notify("HullData");
        }

        private void RepositionToZero()
        {
            Point3D zero = GetMin();

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in bulkheads)
            {
                bulk.ShiftBy(zeroVect);
            }
        }


        private Point3D GetMin()
        {
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;

            foreach (Bulkhead bulk in bulkheads)
            {
                for (int ii = 0; ii < bulk.NumChines; ii++)
                {
                    Point3D point = bulk.GetPoint(ii);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                    min_z = Math.Min(min_z, point.Z);
                }

            }

            return new Point3D(min_x, min_y, min_z);
        }

        //*********************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        //***********************************************
        // IClonable implementation
        public object Clone()
        {
            Hull copy = new Hull();

            foreach (Bulkhead bulkhead in bulkheads)
            {
                bulkheads.Add((Bulkhead)bulkhead.Clone());
            }

            return copy;
        }
    }
}
