using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace AVSHull
{
    public class EditableHull : Hull, INotifyPropertyChanged

    {
        private const int POINTS_PER_CHINE = 50;
        public static int NOT_SELECTED = -1;

        public List<Point3DCollection> Chines;
        private int m_SelectedBulkhead;
        private Hull m_BaseHull;

        public int SelectedBulkhead
        {
            get { return m_SelectedBulkhead; }
            set { m_SelectedBulkhead = value; }
        }

        public Hull BaseHull 
        { 
            get { return m_BaseHull; } 
            set 
            { 
                m_BaseHull = value;
                Bulkheads = new List<Bulkhead>();

                foreach (Bulkhead bulkhead in m_BaseHull.Bulkheads)
                {
                    Bulkheads.Add((Bulkhead)bulkhead.Clone());
                }

                PrepareChines(POINTS_PER_CHINE);

                Notify("BaseHull");
            }
        }

        public EditableHull(Hull baseHull)
        {
            m_SelectedBulkhead = NOT_SELECTED;
            BaseHull = baseHull;
        }

        public void Rotate(double x, double y, double z)
        {
            double[,] rotate;

            rotate = GeometryOperations.CreateRotateMatrix(x, y, z);
            UpdateWithMatrix(rotate);

            RepositionToZero();
        }

        public Geometry GetBulkheadGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            foreach (Bulkhead bulk in Bulkheads)
            {
                for (int chine = 0; chine < bulk.NumChines - 1; chine++)
                {
                    Point p1 = new Point(bulk.Points[chine].X, bulk.Points[chine].Y);
                    Point p2 = new Point(bulk.Points[chine + 1].X, bulk.Points[chine + 1].Y);

                    geom.Children.Add(new LineGeometry(p1, p2));
                }
            }

            return geom;
        }
        public Geometry GetChineGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            foreach (Point3DCollection chine in Chines)
            {
                // FIXTHIS: use a foreach and simply remember the previous point
                for (int point = 0; point < chine.Count - 1; point++)
                {
                    Point p1 = new Point(chine[point].X, chine[point].Y);
                    Point p2 = new Point(chine[point + 1].X, chine[point + 1].Y);

                    geom.Children.Add(new LineGeometry(p1, p2));
                }
            }

            return geom;
        }
    
        private void UpdateWithMatrix(double[,] matrix)
        {
            for (int ii = 0; ii < Bulkheads.Count; ii++)
            {
                Bulkheads[ii].UpdateWithMatrix(matrix);
            }

            if (Chines != null)
            {
                for (int ii = 0; ii < Chines.Count; ii++)
                {
                    Point3DCollection newChine;
                    Matrix.Multiply(Chines[ii], matrix, out newChine);
                    Chines[ii] = newChine;
                }
            }
        }
        protected Point3D GetMin()
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

            if (Chines != null)
            {
                foreach (Point3DCollection chine in Chines)
                {
                    foreach (Point3D point in chine)
                    {
                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);
                    }
                }
            }

            return new Point3D(min_x, min_y, min_z);
        }

        public Size3D GetSize()
        {
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;
            double max_z = double.MinValue;

            foreach (Bulkhead bulk in Bulkheads)
            {
                foreach (Point3D point in bulk.Points)
                {
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    max_z = Math.Max(max_z, point.Z);

                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                    min_z = Math.Min(min_z, point.Z);
                }

            }
            if (Chines != null)
            {
                foreach (Point3DCollection chine in Chines)
                {
                    foreach (Point3D point in chine)
                    {
                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);

                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);
                    }
                }
            }

            return new Size3D(max_x - min_x, max_y - min_y, max_z - min_z);
        }

        private void RepositionToZero()
        {
            Point3D zero = GetMin();

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in Bulkheads)
            {
                bulk.ShiftBy(zeroVect);
            }

            if (Chines != null)
            {
                for (int ii = 0; ii < Chines.Count; ii++)
                {
                    Point3DCollection newChine = new Point3DCollection(Chines.Count);
                    foreach (Point3D point in Chines[ii])
                    {
                        newChine.Add(point + zeroVect);
                    }

                    Chines[ii] = newChine;
                }
            }
        }

        private List<Point3DCollection> PrepareChines(Hull hull, int points_per_chine)
        {
            int nChines = hull.Bulkheads[0].NumChines;
            List<Point3DCollection> chines = new List<Point3DCollection>();

            for (int chine = 0; chine < nChines; chine++)
            {
                Point3DCollection chine_data = new Point3DCollection(Bulkheads.Count);

                for (int bulkhead = 0; bulkhead < hull.Bulkheads.Count; bulkhead++)
                {
                    chine_data.Add(hull.Bulkheads[bulkhead].Points[chine]);
                }
                Splines spline = new Splines(chine_data, Splines.RELAXED);
                Point3DCollection newChine = spline.GetPoints(points_per_chine);
                chines.Add(newChine);
            }

            return chines;
        }

        private void PrepareChines(int points_per_chine)
        {
            Chines = PrepareChines(this, points_per_chine);
        }

        public void UpdateBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            if (chine < 0 && m_BaseHull.Bulkheads[bulkhead].Type != Bulkhead.BulkheadType.BOW)
            {
                // assume we are shifting the bulkhead in the Z direction
                m_BaseHull.Bulkheads[bulkhead].MoveZ(z);
            }
            else
                m_BaseHull.Bulkheads[bulkhead].UpdateBulkheadPoint(chine, x, y, z);
        }

        public void DeleteBulkhead(int index)
        {
            Bulkheads.RemoveAt(index);
            m_BaseHull.Bulkheads.RemoveAt(index);

            m_BaseHull.Notify("HullData");
        }

        public void InsertBulkhead(double Z)
        {
            int num_chines = Bulkheads[0].NumChines;
            if (num_chines % 2 == 1)
                num_chines = num_chines / 2 + 1;
            else
                num_chines /= 2;

            // get points for new bulkhead
            // First, create chines for base hull
            List<Point3DCollection> chines = PrepareChines(m_BaseHull, POINTS_PER_CHINE);
            Point3DCollection points = new Point3DCollection();
            for (int ii=num_chines-1; ii>=0; ii--)
            {
                Point3D point = GeometryOperations.InterpolateFromZ(chines[ii], Z);
                points.Add(point);
            }

            // figure out where it goes
            int index = 0;
            for (int ii = 0; ii < m_BaseHull.Bulkheads.Count; ii++)
            {
                if (m_BaseHull.Bulkheads[ii].Points[0].Z > Z)
                {
                    index = ii;
                    break;
                }
            }

            m_BaseHull.Bulkheads.Insert(index, new Bulkhead(points, Bulkhead.BulkheadType.VERTICAL));
            m_BaseHull.Notify("HullData");
        }
        //m_selectedBulkhead, m_draggingHandle, x, y, z);
        //*************************************************************
        // INotifyPropertyChanged implementation
        // Handled by base class
    }
}
