using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class Hull : INotifyPropertyChanged, ICloneable
    {
        private List<Bulkhead> m_Bulkheads;

        public List<Bulkhead> Bulkheads
        {
            get { return m_Bulkheads; }
            set { m_Bulkheads = value; SetBulkheadHandler(); Notify("Bulkheads"); }
        }

        private DateTime _timestamp;
        public DateTime Timestamp 
        { 
            get { return _timestamp; } 
            set { _timestamp = value; Notify("Timestamp"); }
        }

        public Hull()
        {
            Bulkheads = new List<Bulkhead>();
        }

        public Hull(CreateHullData setup)
        {
            Point3DCollection points = new Point3DCollection();
            double height = 0;
            double width = 0;
            double Z = 0;

            Bulkheads = new List<Bulkhead>();

            Timestamp = DateTime.Now;

            if (setup.IncludeBow)
            {
                Z = 0;
                width = 0;
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    points.Add(new Point3D(width, height, Z));
                    height += setup.Height/setup.NumChines;
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.BOW));
            }

            // Vertical bulkheads
            while (Bulkheads.Count < setup.NumBulkheads - 1)
            {
                points.Clear();
                Z = Bulkheads.Count * setup.Length / setup.NumBulkheads;
                width = 0;
                height = 0;
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    points.Add(new Point3D(width, height, Z));
                    width += setup.Width / setup.NumChines;
                    height += setup.Height / setup.NumChines;
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.VERTICAL));
            }

            // Transom
            points.Clear();
            Z = Bulkheads.Count * setup.Length / setup.NumBulkheads;
            width = 0;
            height = 0;
            for (int ii = 0; ii < setup.NumChines+1; ii++)
            {
                points.Add(new Point3D(width, height, Z + height*Math.Cos(Math.PI / 180 * setup.TransomAngle)));
                width += setup.Width / setup.NumChines;
                height += setup.Height / setup.NumChines;
            }
            Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.TRANSOM));
            CheckTransom();
            SetBulkheadHandler();
        }

        public Hull(string filename)
        {
            Bulkheads = new List<Bulkhead>();

            Timestamp = DateTime.Now;

            using (StreamReader file = File.OpenText(filename))
            {
                string line;
                int num_chines;
                int numBulkheads = 5;

                line = file.ReadLine();
                if (!int.TryParse(line, out num_chines)) throw new Exception("Invalid HUL file format");

                Bulkhead bulkhead = new Bulkhead(file, num_chines, Bulkhead.BulkheadType.BOW);
                Bulkheads.Add(bulkhead);

                for (int ii = 1; ii < numBulkheads - 1; ii++)
                {
                    bulkhead = new Bulkhead(file, num_chines, Bulkhead.BulkheadType.VERTICAL);
                    Bulkheads.Add(bulkhead);
                }

                bulkhead = new Bulkhead(file, num_chines, Bulkhead.BulkheadType.TRANSOM);
                Bulkheads.Add(bulkhead);
            }
            RepositionToZero();
            CheckTransom();
            SetBulkheadHandler();
        }

        private void SetBulkheadHandler()
        {
            foreach (Bulkhead bulk in Bulkheads)
            {
                bulk.PropertyChanged += bulkhead_PropertyChanged;
            }
        }
        private void RepositionToZero()
        {
            Point3D min = GetMin();
            Point3D max = GetMax();
            Point3D zero = min;

            zero.X = (max.X + min.X) / 2; // move centerline to zero

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in Bulkheads)
            {
                bulk.ShiftBy(zeroVect);
            }
        }


        private Point3D GetMin()
        {
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;

            foreach (Bulkhead bulk in Bulkheads)
            {
                for (int ii = 0; ii < bulk.NumChines; ii++)
                {
                    Point3D point = bulk.Points[ii];
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                    min_z = Math.Min(min_z, point.Z);
                }

            }

            return new Point3D(min_x, min_y, min_z);
        }

        private Point3D GetMax()
        {
            double min_x = double.MinValue;
            double min_y = double.MinValue;
            double min_z = double.MinValue;

            foreach (Bulkhead bulk in Bulkheads)
            {
                for (int ii = 0; ii < bulk.NumChines; ii++)
                {
                    Point3D point = bulk.Points[ii];
                    min_x = Math.Max(min_x, point.X);
                    min_y = Math.Max(min_y, point.Y);
                    min_z = Math.Max(min_z, point.Z);
                }

            }

            return new Point3D(min_x, min_y, min_z);
        }

        public void Scale(double x, double y, double z)
        {
            double[,] scale = new double[3, 3];

            scale[0, 0] = x;
            scale[1, 1] = y;
            scale[2, 2] = z;

            UpdateWithMatrix(scale);

            RepositionToZero();
            Notify("HullScale");
        }

        private void UpdateWithMatrix(double[,] matrix)
        {
            Timestamp = DateTime.Now;

            for (int ii = 0; ii < Bulkheads.Count; ii++)
            {
                Bulkheads[ii].UpdateWithMatrix(matrix);
            }
        }

        public virtual void ChangeChines(int numChines)
        {
            Timestamp = DateTime.Now;

            for (int ii = 0; ii < Bulkheads.Count; ii++)
            {
                Bulkheads[ii] = new Bulkhead(Bulkheads[ii], numChines);
            }

            Notify("HullData");
        }

        private void CheckTransom()
        {
            for (int ii = 0; ii < Bulkheads.Count; ii++)
            {
                Bulkheads[ii].CheckTransomAngle();
            }

        }
        //*********************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void bulkhead_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Hull:Bulkhead PropertyChanged: " + e.PropertyName);
            Timestamp = DateTime.Now;

            Notify(e.PropertyName);
        }

        //***********************************************
        // IClonable implementation
        public object Clone()
        {
            Hull copy = new Hull();

            foreach (Bulkhead bulkhead in Bulkheads)
            {
                copy.Bulkheads.Add((Bulkhead)bulkhead.Clone());
            }

            copy.Timestamp = Timestamp;

            return copy;
        }
    }
}
