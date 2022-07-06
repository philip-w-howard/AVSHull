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
        private string _filename;
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; Notify("Filename"); }
        }

        private string _panelsfilename;
        public string PanelsFilename
        {
            get { return _panelsfilename; }
            set { _panelsfilename = value; Notify("PanelsFilename"); }
        }

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

        private DateTime _saveTimestamp;
        public DateTime SaveTimestamp
        {
            get { return _saveTimestamp; }
            set { _saveTimestamp = value; Notify("SaveTimestamp"); }
        }

        public int NumChines 
        { 
            get 
            {
                if (m_Bulkheads.Count == 0) return 0;
                return m_Bulkheads[0].NumChines; 
            } 
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
                points.Clear();
                Z = 0;
                width = 0;
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    height = ii * setup.Height / setup.NumChines;
                    points.Add(new Point3D(width, height, Z));
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.BOW, setup.FlatBottom, setup.ClosedTop));
            }
            else
            {
                points.Clear();
                Z = setup.Height * Math.Cos(Math.PI / 180 * setup.TransomAngle);
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    if (ii == setup.NumChines && setup.ClosedTop)
                        width = 0;
                    else if (ii == 0 && setup.FlatBottom)
                        width = setup.Width / (setup.NumChines + 1);
                    else
                        width = ii * setup.Width / setup.NumChines;

                    height = ii * setup.Height / setup.NumChines;

                    points.Add(new Point3D(width, height, Z - height * Math.Cos(Math.PI / 180 * setup.TransomAngle)));
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.TRANSOM, setup.FlatBottom, setup.ClosedTop));
                CheckTransom();
            }

            // Vertical bulkheads
            while (Bulkheads.Count < setup.NumBulkheads - 1)
            {
                points.Clear();
                Z = Bulkheads.Count * setup.Length / (setup.NumBulkheads - 1);
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    if (ii == setup.NumChines && setup.ClosedTop) 
                        width = 0;
                    else if (ii == 0 && setup.FlatBottom)
                        width = setup.Width / (setup.NumChines + 1);
                    else
                        width = ii * setup.Width / setup.NumChines;

                    height = ii * setup.Height / setup.NumChines;

                    points.Add(new Point3D(width, height, Z));
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.VERTICAL, setup.FlatBottom, setup.ClosedTop));
            }

            // Transom
            if (setup.IncludeTransom)
            {
                points.Clear();
                Z = setup.Length - setup.Height * Math.Cos(Math.PI / 180 * setup.TransomAngle);
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    if (ii == setup.NumChines && setup.ClosedTop)
                        width = 0;
                    else if (ii == 0 && setup.FlatBottom)
                        width = setup.Width / (setup.NumChines + 1);
                    else
                        width = ii * setup.Width / setup.NumChines;

                    height = ii * setup.Height / setup.NumChines;

                    points.Add(new Point3D(width, height, Z + height * Math.Cos(Math.PI / 180 * setup.TransomAngle)));
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.TRANSOM, setup.FlatBottom, setup.ClosedTop));
                CheckTransom();
            }
            else
            {
                points.Clear();
                Z = setup.Length;
                width = 0;
                for (int ii = 0; ii < setup.NumChines + 1; ii++)
                {
                    height = ii * setup.Height / setup.NumChines;
                    points.Add(new Point3D(width, height, Z));
                }
                Bulkheads.Add(new Bulkhead(points, Bulkhead.BulkheadType.BOW, setup.FlatBottom, setup.ClosedTop));
            }

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

            SetBulkheadHandler();

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

            copy.SetBulkheadHandler();

            return copy;
        }
        public List<Point3DCollection> GenerateChines(int points_per_chine)
        {
            int nChines = Bulkheads[0].NumChines;
            List<Point3DCollection> chines = new List<Point3DCollection>();

            for (int chine = 0; chine < nChines; chine++)
            {
                Point3DCollection chine_data = new Point3DCollection(Bulkheads.Count);

                for (int bulkhead = 0; bulkhead < Bulkheads.Count; bulkhead++)
                {
                    chine_data.Add(Bulkheads[bulkhead].Points[chine]);
                }
                Splines spline = new Splines(chine_data, Splines.RELAXED);
                Point3DCollection newChine = spline.GetPoints(points_per_chine);
                chines.Add(newChine);
            }

            return chines;
        }


    }
}
