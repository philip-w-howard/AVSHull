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
        
        private BulkheadType _type;
        public BulkheadType Type 
        {
            get { return _type; }
            set { _type = value; Notify("Type"); }
        }

        public int NumChines { get { return m_points.Count; } }

        private double m_transomAngle;
        public double TransomAngle 
        { 
            get { return m_transomAngle; }
            set { m_transomAngle = value; Notify("TransomAngle"); }
        }

        private Point3DCollection m_points;
        public Point3DCollection Points 
        { 
            get { return m_points; } 
            set { m_points = value; Notify("Bulkhead"); }
        }

        public Bulkhead()
        {
            m_points = new Point3DCollection();
        }

        public Bulkhead(Bulkhead original, int num_chines)
        {
            const int PRECISION = 10;
            m_points = new Point3DCollection();
            Point3DCollection tempPoints = new Point3DCollection();

            if (original.NumChines % 2 == 1)
            {
                // centerline bulkhead
                int useable_chines = original.NumChines / 2 + 1;

                for (int ii = 0; ii < useable_chines; ii++)
                {
                    tempPoints.Add(original.Points[ii]);
                }

                Splines shape = new Splines(tempPoints, Splines.RELAXED);
                Point3DCollection outline = shape.GetPoints((num_chines + 1) * PRECISION);

                int index = 0;
                for (int ii = 0; ii < num_chines; ii++)
                {
                    m_points.Add(outline[index]);
                    index += PRECISION;
                }

                // Add the center point
                m_points.Add(original.Points[original.NumChines / 2]);

                // Add the other half
                index = PRECISION * num_chines;
                for (int ii = 0; ii < num_chines; ii++)
                {
                    index -= PRECISION;
                    Point3D point = outline[index];
                    point.X = -point.X;
                    m_points.Add(point);
                }
            }
            else
            {
                // flat floor bulkhead
                int useable_chines = original.NumChines / 2 ;

                for (int ii = 0; ii < useable_chines; ii++)
                {
                    tempPoints.Add(original.Points[ii]);
                }

                Splines shape = new Splines(tempPoints, Splines.RELAXED);
                Point3DCollection outline = shape.GetPoints((num_chines + 1) * PRECISION);

                int index = 0;
                for (int ii = 0; ii < num_chines; ii++)
                {
                    m_points.Add(outline[index]);
                    index += PRECISION;
                }

                // Add the center point
                m_points.Add(original.Points[original.NumChines / 2]);

                // Add the other half
                index = PRECISION * num_chines;
                for (int ii = 0; ii < num_chines; ii++)
                {
                    index -= PRECISION;
                    Point3D point = outline[index];
                    point.X = -point.X;
                    m_points.Add(point);
                }
            }
        }

        public Bulkhead(Point3DCollection points, BulkheadType type)
        {
            m_points = new Point3DCollection();
            this.Type = type;

            if (Math.Abs(points[0].X) < NEAR_ZERO)
            {
                // Bottom is on center-line
                for (int ii = points.Count - 1; ii > 0; ii--)
                {
                    m_points.Add(points[ii]);
                }

                // Force center to zero
                m_points.Add(new Point3D(0, points[0].Y, points[0].Z));

                // Insert other half of hull
                for (int ii = 1; ii < points.Count; ii++)
                {
                    Point3D point = new Point3D(-points[ii].X, points[ii].Y, points[ii].Z);
                    m_points.Add(point);
                }
            }
            else
            {
                // flat floor: bottom is not on center-line
                for (int ii = points.Count - 1; ii >= 0; ii--)
                {
                    m_points.Add(points[ii]);
                }

                // Insert other half of hull
                for (int ii = 1; ii < points.Count; ii++)
                {
                    Point3D point = new Point3D(-points[ii].X, points[ii].Y, points[ii].Z);
                    m_points.Add(point);
                }
            }

            ComputeAngle();
            StraightenBulkhead();
        }

        // Create a bulkhead from a Carlson HUL file
        public Bulkhead(StreamReader file, int numChines, BulkheadType type)
        {
            this.Type = type;
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
        }

        public void CheckTransomAngle()
        {
            if (Type == BulkheadType.TRANSOM && TransomAngle == 0)
            {
                ComputeAngle();
            }
        }
        protected void ComputeAngle()
        {
            m_transomAngle = 0;
            if (Type == BulkheadType.TRANSOM)
            {
                double delta, delta_z, delta_y;

                // find greatest delta_z
                delta_z = 0;
                delta_y = 0;
                for (int ii = 1; ii < m_points.Count; ii++)
                {
                    delta = Math.Abs(m_points[ii - 1].Z - m_points[ii].Z);
                    if (delta > delta_z)
                    {
                        delta_z = delta;
                        delta_y = Math.Abs(m_points[ii - 1].Y - m_points[ii].Y);
                    }
                }

                if (delta_z == 0)
                {
                    Type = BulkheadType.VERTICAL;
                    m_transomAngle = Math.PI / 2;
                }
                else
                    m_transomAngle = Math.Atan2(delta_y, delta_z);
            }
            else if (Type == BulkheadType.VERTICAL)
            {
                m_transomAngle = Math.PI / 2;
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
            Notify("Bulkhead");

        }

        public Geometry GetGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            for (int chine = 0; chine < NumChines - 1; chine++)
            {
                Point p1 = new Point(Points[chine].X, Points[chine].Y);
                Point p2 = new Point(Points[chine + 1].X, Points[chine + 1].Y);

                geom.Children.Add(new LineGeometry(p1, p2));
            }

            return geom;
        }

        //******************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        //**********************************************
        // IClonable implementation
        public object Clone()
        {
            Bulkhead copy = new Bulkhead
            {
                Type = Type,
                m_transomAngle = TransomAngle,
                m_points = m_points.Clone()
            };

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

            switch (Type)
            {
                case BulkheadType.BOW:
                    point.X += 0;                   // Can't shift BOW points in the X direction
                    point.Y += y;
                    point.Z += z;
                    otherPoint.X -= 0;              // Can't shift BOW points in the X direction
                    otherPoint.Y += y;
                    otherPoint.Z += z;
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

            Notify("Bulkhead.Handle");
        }

        private double NewZPoint(Point3D basePoint, Point3D newPoint)
        {
            return basePoint.Z + (newPoint.Y - basePoint.Y) * Math.Cos(m_transomAngle) / Math.Sin(m_transomAngle);
        }

        public void MoveZ(double deltaZ)
        {
            for (int ii=0; ii<m_points.Count; ii++)
            {
                Point3D point = m_points[ii];
                point.Z += deltaZ;
                m_points[ii] = point;
            }
            Notify("Bulkhead");
        }
    }
 }
