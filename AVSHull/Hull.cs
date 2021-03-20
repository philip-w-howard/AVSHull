using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class Hull : INotifyPropertyChanged
    {
        private const int POINTS_PER_CHINE = 50;
        private List<Bulkhead> m_bulkheads;
        private List<Point3DCollection> m_chines;

        public Hull()
        {
            m_bulkheads = new List<Bulkhead>();
            m_chines = new List<Point3DCollection>();
        }

        public Hull(SerializableHull hull)
        {
            m_chines = null;
            m_bulkheads = null;
            if (hull.bulkheads != null)
            {
                m_bulkheads = new List<Bulkhead>();
                foreach (SerializableBulkhead bulk in hull.bulkheads)
                {
                    m_bulkheads.Add(new Bulkhead(bulk));
                }

                PrepareChines(POINTS_PER_CHINE);
                RepositionToZero();

                Notify("HullData");
            }
        }

        private void RepositionToZero()
        {
            Point3D zero = GetMin();

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in m_bulkheads)
            {
                bulk.Shift(zeroVect);
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
            int nChines = m_bulkheads[0].NumChines;

            m_chines = new List<Point3DCollection>();

            for (int chine = 0; chine < nChines; chine++)
            {
                Point3DCollection newChine = new Point3DCollection(points_per_chine);
                Point3DCollection chine_data = new Point3DCollection(m_bulkheads.Count);

                for (int bulkhead = 0; bulkhead < m_bulkheads.Count; bulkhead++)
                {
                    chine_data.Add(m_bulkheads[bulkhead].GetPoint(chine));
                }
                Splines spline = new Splines(chine_data, Splines.RELAXED);
                spline.GetPoints(points_per_chine, newChine);
                m_chines.Add(newChine);
            }

            RepositionToZero();
        }

        protected Point3D GetMin()
        {
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;

            foreach (Bulkhead bulk in m_bulkheads)
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

        //*********************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        
        //***************************************************
        // Serialization
        public class SerializableHull
        {
            public List<SerializableBulkhead> bulkheads;

            public SerializableHull()
            { }

            public SerializableHull(Hull hull)
            {
                bulkheads = new List<SerializableBulkhead>();

                foreach (Bulkhead bulkhead in hull.m_bulkheads)
                {
                    bulkheads.Add(new SerializableBulkhead(bulkhead));
                }
            }
        }
    }
}
