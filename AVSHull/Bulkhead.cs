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

        public double m_transomAngle;
        public double TransomAngle { get { return m_transomAngle; } }

        private Point3DCollection m_points;
        public Point3DCollection Points { get { return m_points; } }

        public Point3D GetPoint(int index)
        {
            return m_points[index];
        }

        public Bulkhead()
        {
            m_points = new Point3DCollection();
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
                
                if (type == BulkheadType.BOW && Math.Abs(value) < NEAR_ZERO) value = 0;
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
                for (int ii = temp_points.Count - 1; ii > 0; ii--)
                {
                    m_points.Add(temp_points[ii]);
                }

                // Force center to zero
                m_points.Add(new Point3D(0, temp_points[0].Y, temp_points[0].Z));

                // Insert other half of hull
                for (int ii = 1; ii < temp_points.Count; ii++)
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

        public void UpdateBulkheadPoint(int chine, double x, double y, double z)
        {
            Point3D basePoint = m_points[0];
            Point3D point = m_points[chine];
            int otherChine = (m_points.Count - 1) - chine;
            Point3D otherPoint = m_points[otherChine];

            switch (type)
            {
                case BulkheadType.BOW:
                    point.X += x;
                    point.Y += y;
                    point.Z += 0;                    // force all points to be on the Z axix
                    otherPoint.X -= x;
                    otherPoint.Y += y;
                    otherPoint.Z += 0;               // force all points to be on the Z axix
                    break;
                case BulkheadType.VERTICAL:
                    point.X += x;
                    point.Y += y;
                    point.Z = basePoint.Z;          // force all points to be vertical relative to base point
                    otherPoint.X -= x;
                    otherPoint.Y += y;
                    otherPoint.Z = basePoint.Z;          // force all points to be vertical relative to base point
                    break;
                case BulkheadType.TRANSOM:
                    if (x == 0 && y == 0 && z == 0)
                    {
                        // Simply force Z to be on the plane of the transom
                        if (chine != 0)
                        {
                            point.Z = NewZPoint(basePoint, point);
                            otherPoint.Z = NewZPoint(basePoint, point);
                        }
                    }
                    else if (x == 0)
                    {
                        // assume updating from side view
                        // Believe the user's y coordinate and then compute Z to be on the plane.
                        point.Y += y;
                        point.Z = NewZPoint(basePoint, point);
                        otherPoint.Y += y;
                        otherPoint.Z = NewZPoint(basePoint, otherPoint);
                    }
                    else if (y == 0)
                    {
                        // assume updating from top view
                        // Can't update Z or Y from top view
                        point.X += x;
                        otherPoint.X -= x;
                    }
                    else if (z == 0)
                    {
                        // assume updating from front view
                        // can update both x and y
                        point.X += x;
                        point.Y += y;
                        point.Z = NewZPoint(basePoint, point);
                        otherPoint.X -= x;
                        otherPoint.Y += y;
                        otherPoint.Z = NewZPoint(basePoint, otherPoint);
                    }
                    else
                    {
                        throw new Exception("Perspective updates not implemented");
                    }
                    break;
            }

            m_points[chine] = point;

            if (chine != otherChine)
            {
                m_points[otherChine] = otherPoint;
            }

            Notify("Bulkhead");
        }

        private double NewZPoint(Point3D basePoint, Point3D newPoint)
        {
            return basePoint.Z + (newPoint.Y - basePoint.Y) * Math.Cos(m_transomAngle) / Math.Sin(m_transomAngle);
        }
    }
 }
