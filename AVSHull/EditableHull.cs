using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class EditableHull : Hull, INotifyPropertyChanged

    {
        private const int POINTS_PER_CHINE = 50;
        private const int HANDLE_SIZE = 5;
        public static int NOT_SELECTED = -1;

        private List<Point3DCollection> m_chines;
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
                bulkheads = new List<Bulkhead>();

                foreach (Bulkhead bulkhead in m_BaseHull.bulkheads)
                {
                    bulkheads.Add((Bulkhead)bulkhead.Clone());
                }

                PrepareChines(POINTS_PER_CHINE);

                Notify("BaseHull");
            }
        }

        public EditableHull()
        {
            m_SelectedBulkhead = NOT_SELECTED;
            m_BaseHull = null;
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

            foreach (Bulkhead bulk in bulkheads)
            {
                for (int chine = 0; chine < bulk.NumChines - 1; chine++)
                {
                    Point p1 = new Point(bulk.GetPoint(chine).X, bulk.GetPoint(chine).Y);
                    Point p2 = new Point(bulk.GetPoint(chine + 1).X, bulk.GetPoint(chine + 1).Y);

                    geom.Children.Add(new LineGeometry(p1, p2));
                }
            }

            return geom;
        }
        public Geometry GetChineGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            foreach (Point3DCollection chine in m_chines)
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
            for (int ii = 0; ii < bulkheads.Count; ii++)
            {
                bulkheads[ii].UpdateWithMatrix(matrix);
            }

            if (m_chines != null)
            {
                for (int ii = 0; ii < m_chines.Count; ii++)
                {
                    Point3DCollection newChine;
                    Matrix.Multiply(m_chines[ii], matrix, out newChine);
                    m_chines[ii] = newChine;
                }
            }
        }
        protected Point3D GetMin()
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

            if (m_chines != null)
            {
                foreach (Point3DCollection chine in m_chines)
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
        
        private void RepositionToZero()
        {
            Point3D zero = GetMin();

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in bulkheads)
            {
                bulk.ShiftBy(zeroVect);
            }

            if (m_chines != null)
            {
                for (int ii = 0; ii < m_chines.Count; ii++)
                {
                    Point3DCollection newChine = new Point3DCollection(m_chines.Count);
                    foreach (Point3D point in m_chines[ii])
                    {
                        newChine.Add(point + zeroVect);
                    }

                    m_chines[ii] = newChine;
                }
            }
        }

        private void PrepareChines(int points_per_chine)
        {
            int nChines = bulkheads[0].NumChines;

            m_chines = new List<Point3DCollection>();

            for (int chine = 0; chine < nChines; chine++)
            {
                Point3DCollection newChine = new Point3DCollection(points_per_chine);
                Point3DCollection chine_data = new Point3DCollection(bulkheads.Count);

                for (int bulkhead = 0; bulkhead < bulkheads.Count; bulkhead++)
                {
                    chine_data.Add(bulkheads[bulkhead].GetPoint(chine));
                }
                Splines spline = new Splines(chine_data, Splines.RELAXED);
                spline.GetPoints(points_per_chine, newChine);
                m_chines.Add(newChine);
            }
        }

        //*************************************************************
        // INotifyPropertyChanged implementation
        // Handled by base class
    }
}
