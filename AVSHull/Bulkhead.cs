using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    // Class representing a bulkhead: A polygon that represents a slice through a hull.
    public class Bulkhead : INotifyPropertyChanged, ICloneable
    {
        private const double NEAR_ZERO = 0.02;

        public enum BulkheadType { BOW, VERTICAL, TRANSOM };
        public BulkheadType type { get; set; }

        public int NumChines { get { return m_points.Count; } }

        private double m_transomAngle;
        public double TransomAngle {  get { return m_transomAngle; } }

        private Point3DCollection m_points;
        public Point3DCollection Points {  get { return m_points; } }

        public Point3D GetPoint(int index)
        {
            return m_points[index];
        }

        public Bulkhead()
        {
            m_points = new Point3DCollection();
        }

        public Bulkhead(SerializableBulkhead bulk)
        {
            m_transomAngle = bulk.transom_angle;
            m_points = bulk.points.Clone();
            type = bulk.bulkheadType;
        }

        public void LoadFromHullFile(StreamReader file, int numChines, BulkheadType type)
        {
            this.type = type;
            m_points = new Point3DCollection();

            string line;
            Point3DCollection temp_points = new Point3DCollection();
            for (int chine = 0; chine < numChines; chine++)
            {
                Point3D point = new Point3D();
                double value;
                line = file.ReadLine();
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead X value");
                point.X = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead Y value");
                point.Y = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead Z value");
                point.Z = value;

                temp_points.Add(point);
            }


            if (Math.Abs(temp_points[0].X) < NEAR_ZERO)
            {
                // Bottom is on center-line
                for (int ii= temp_points.Count - 1; ii>0; ii--)
                {
                    m_points.Add(temp_points[ii]);
                }

                // Force center to zero
                m_points.Add(new Point3D(0, temp_points[0].Y, temp_points[0].Z));
                
                // Insert other half of hull
                for (int ii=1; ii<temp_points.Count; ii++)
                {
                    Point3D point = new Point3D(-temp_points[ii].X, temp_points[ii].Y, temp_points[ii].Z);
                    m_points.Add(point);
                }
            }
            else
            {
                // flat floor: bottom is not on center-line
                for (int ii = temp_points.Count - 1; ii >= 0; ii--)
                {
                    m_points.Add(temp_points[ii]);
                }

                // Insert other half of hull
                for (int ii = 1; ii < temp_points.Count; ii++)
                {
                    Point3D point = new Point3D(-temp_points[ii].X, temp_points[ii].Y, temp_points[ii].Z);
                    m_points.Add(point);
                }
            }

            ComputeAngle();
            StraightenBulkhead();

            Notify("Bulkhead");
        }

        protected void ComputeAngle()
        {
            m_transomAngle = 0;
            if (type == BulkheadType.TRANSOM)
            {
                double delta, max_delta;
                int max_index = 1;

                // find greatest delta_z
                max_delta = 0;
                for (int ii = 1; ii < m_points.Count; ii++)
                {
                    delta = Math.Abs(m_points[ii - 1].Z - m_points[ii].Z);
                    if (delta > max_delta)
                    {
                        delta = max_delta;
                        max_index = ii;
                    }
                }

                double delta_y = m_points[0].Y - m_points[max_index].Y;
                double delta_z = m_points[0].Z - m_points[max_index].Z;

                if (delta_z == 0)
                    type = BulkheadType.VERTICAL;
                else
                    m_transomAngle = Math.Atan2(delta_y, delta_z);
            }
        }

        protected void StraightenBulkhead()
        {
            for (int chine = 0; chine < m_points.Count; chine++)
            {
                // FIXTHIS: uncomment after UpdatePoint works with TRANSOM points. UpdatePoint(chine, 0, 0, 0);
            }
        }

        public void ShiftBy(Vector3D offset)
        {
            for (int ii = 0; ii < m_points.Count; ii++)
            {
                m_points[ii] += offset;
            }
            Notify("ShiftBy");
            
        }

        public Geometry GetGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            for (int chine = 0; chine < NumChines - 1; chine++)
            {
                Point p1 = new Point(GetPoint(chine).X, GetPoint(chine).Y);
                Point p2 = new Point(GetPoint(chine + 1).X, GetPoint(chine + 1).Y);

                geom.Children.Add(new LineGeometry(p1, p2));
            }

            return geom;
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

        //**********************************************
        // IClonable implementation
        public object Clone()
        {
            Bulkhead copy = new Bulkhead();
            copy.type = type;
            copy.m_transomAngle = TransomAngle;
            copy.m_points = m_points.Clone();

            return copy;
        }
        public void UpdateWithMatrix(double[,] matrix)
        {
            Point3DCollection result = new Point3DCollection(m_points.Count);   // temp array so we can compute in place

            foreach (Point3D point in m_points)
            {
                Point3D new_points = new Point3D();
                new_points.X = point.X * matrix[0, 0] + point.Y * matrix[1, 0] + point.Z * matrix[2, 0];
                new_points.Y = point.X * matrix[0, 1] + point.Y * matrix[1, 1] + point.Z * matrix[2, 1];
                new_points.Z = point.X * matrix[0, 2] + point.Y * matrix[1, 2] + point.Z * matrix[2, 2];

                result.Add(new_points);
            }

            m_points = result;

            Notify("Bulkhead");
        }
    }

    //**********************************************
    public class SerializableBulkhead
    {
        public double transom_angle;
        public Bulkhead.BulkheadType bulkheadType;
        public Point3DCollection points;

        public SerializableBulkhead()
        {
            points = new Point3DCollection();
        }

        public SerializableBulkhead(Bulkhead bulkhead)
        {
            transom_angle = bulkhead.TransomAngle;
            bulkheadType = bulkhead.type;
            points = bulkhead.Points.Clone();
        }
    }
}
