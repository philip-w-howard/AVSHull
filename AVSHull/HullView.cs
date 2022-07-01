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
    // Presents a view of the Singleton hull defined by BaseHull
    public class HullView : Hull, INotifyPropertyChanged
    {
        public const int POINTS_PER_CHINE = 50;
        public static int NOT_SELECTED = -1;

        private List<Point3DCollection> Chines;
        private List<Point3DCollection> Waterlines;
        private bool m_displayWaterlines = false;
        public bool DisplayWaterlines
        { get { return m_displayWaterlines; } }

        private int m_SelectedBulkhead;
        public int SelectedBulkhead
        {
            get { return m_SelectedBulkhead; }
            set { m_SelectedBulkhead = value; }
        }

        public HullView()
        {
            Bulkheads = new List<Bulkhead>();

            foreach (Bulkhead bulkhead in BaseHull.Instance().Bulkheads)
            {
                Bulkheads.Add((Bulkhead)bulkhead.Clone());
            }

            m_SelectedBulkhead = NOT_SELECTED;
            Chines = GenerateChines(POINTS_PER_CHINE);
        }

        public void Rotate(double x, double y, double z)
        {
            double[,] rotate;

            rotate = GeometryOperations.CreateRotateMatrix(x, y, z);
            UpdateWithMatrix(rotate);

            RepositionToZero();
        }

        public List<Geometry> GetBulkheadGeometry()
        {
            List<Geometry> geom_list = new List<Geometry>();

            foreach (Bulkhead bulk in Bulkheads)
            {
                GeometryGroup geom = new GeometryGroup();

                for (int chine = 0; chine < bulk.NumChines - 1; chine++)
                {
                    Point p1 = new Point(bulk.Points[chine].X, bulk.Points[chine].Y);
                    Point p2 = new Point(bulk.Points[chine + 1].X, bulk.Points[chine + 1].Y);

                    geom.Children.Add(new LineGeometry(p1, p2));
                }
                geom_list.Add(geom);
            }

            return geom_list;
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

        public Geometry GetWaterlineGeometry()
        {
            GeometryGroup geom = new GeometryGroup();

            if (m_displayWaterlines && Waterlines != null)
            {
                foreach (Point3DCollection waterline in Waterlines)
                {
                    // FIXTHIS: use a foreach and simply remember the previous point
                    for (int point = 0; point < waterline.Count - 1; point++)
                    {
                        Point p1 = new Point(waterline[point].X, waterline[point].Y);
                        Point p2 = new Point(waterline[point + 1].X, waterline[point + 1].Y);

                        geom.Children.Add(new LineGeometry(p1, p2));
                    }
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

            if (Waterlines !=null)
                for (int ii = 0; ii < Waterlines.Count; ii++)
                {
                    Point3DCollection newWaterline;
                    Matrix.Multiply(Waterlines[ii], matrix, out newWaterline);
                    Waterlines[ii] = newWaterline;
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

            if (m_displayWaterlines && Waterlines != null)
            {
                foreach (Point3DCollection waterline in Waterlines)
                {
                    foreach (Point3D point in waterline)
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
                        max_x = Math.Max(max_x, point.X);
                        max_y = Math.Max(max_y, point.Y);
                        max_z = Math.Max(max_z, point.Z);

                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);
                    }
                }
            }

            if (m_displayWaterlines && Waterlines != null)
            {
                foreach (Point3DCollection waterline in Waterlines)
                {
                    foreach (Point3D point in waterline)
                    {
                        max_x = Math.Max(max_x, point.X);
                        max_y = Math.Max(max_y, point.Y);
                        max_z = Math.Max(max_z, point.Z);

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
 
            if (Waterlines != null)
            {
                for (int ii = 0; ii < Waterlines.Count; ii++)
                {
                    Point3DCollection newWaterline = new Point3DCollection(Waterlines.Count);
                    foreach (Point3D point in Waterlines[ii])
                    {
                        newWaterline.Add(point + zeroVect);
                    }

                    Waterlines[ii] = newWaterline;
                }
            }
        }

        public void UpdateBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            if (chine < 0 && BaseHull.Instance().Bulkheads[bulkhead].Type != Bulkhead.BulkheadType.BOW)
            {
                // assume we are shifting the bulkhead in the Z direction
                BaseHull.Instance().Bulkheads[bulkhead].MoveZ(z);
            }
            else
                BaseHull.Instance().Bulkheads[bulkhead].UpdateBulkheadPoint(chine, x, y, z);
        }

        public void DeleteBulkhead(int index)
        {
            Bulkheads.RemoveAt(index);
            BaseHull.Instance().Bulkheads.RemoveAt(index);

            BaseHull.Instance().Notify("HullData");
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
            List<Point3DCollection> chines = BaseHull.Instance().GenerateChines(POINTS_PER_CHINE);
            Point3DCollection points = new Point3DCollection();
            for (int ii=num_chines-1; ii>=0; ii--)
            {
                Point3D? point = GeometryOperations.InterpolateFromZ(chines[ii], Z);
                points.Add(point.Value);
            }

            // figure out where it goes
            int index = 0;
            for (int ii = 0; ii < BaseHull.Instance().Bulkheads.Count; ii++)
            {
                if (BaseHull.Instance().Bulkheads[ii].Points[0].Z > Z)
                {
                    index = ii;
                    break;
                }
            }

            BaseHull.Instance().Bulkheads.Insert(index, new Bulkhead(points, Bulkhead.BulkheadType.VERTICAL, BaseHull.Instance().Bulkheads[index].IsFlatBottomed, BaseHull.Instance().Bulkheads[index].HasClosedTop));
            BaseHull.Instance().Notify("HullData");
        }

        public override void ChangeChines(int numChines)
        {
            base.ChangeChines(numChines);
            BaseHull.Instance().ChangeChines(numChines);
        }

        private bool InRange(double Z, Point3DCollection chine)
        {
            Point3D lastPoint = chine[0];
            foreach (Point3D point in chine)
            {
                if (Z >= lastPoint.Z && Z <= point.Z) return true;
                if (Z <= lastPoint.Z && Z >= point.Z) return true;
            }
            return false;
        }
        // Generate a series fo waterlines.
        // NOTES:
        //    1) This is in ViewHull not Hull because it needs chines and so that it will work with a rotated hull (for heel and pitch)
        //    2) This code assumes an open top hull
        //    3) Computation will stop when the waterline allos the hull to take on water.
        public List<Point3DCollection> GenerateWaterlines(double depthInterval, double lengthInterval)
        {
            Waterlines = new List<Point3DCollection>();
            m_displayWaterlines = true;

            // Copy the chines and then add first/last bulkhead points
            List<Point3DCollection> myChines = new List<Point3DCollection>();

            int last = Bulkheads.Count - 1;

            for (int ii = 0; ii <= NumChines / 2; ii++)
            {
                myChines.Add(Chines[ii]);
                if (ii != 0)
                {
                    myChines[myChines.Count-1].Insert(0, Bulkheads[0].Points[ii - 1]);
                    myChines[myChines.Count-1].Add(Bulkheads[last].Points[ii - 1]);
                }
            }

            for (int ii = NumChines/2; ii < NumChines; ii++)
            {
                myChines.Add(Chines[ii]);
                if (ii != NumChines-1)
                {
                    myChines[myChines.Count-1].Insert(0, Bulkheads[0].Points[ii + 1]);
                    myChines[myChines.Count-1].Add(Bulkheads[last].Points[ii + 1]);
                }
            }

            Point3D min = GetMin();
            Size3D size = GetSize();

            double height;

            height = min.Y;

            bool takingOnWater = false;
            while (!takingOnWater)
            {
                Point3DCollection left = new Point3DCollection();
                Point3DCollection right = new Point3DCollection();
                bool foundLeft = false;

                //**********************************
                // debug
                int lastIndex = 0;

                int startOffset = -1;
                int end = -1;
                int increment = -1;
                //int startOffset = 0;
                //int end = myChines.Count;
                //int increment = 1;
                for (double length=min.Z; length<=min.Z + size.Z; length += lengthInterval)
                {
                    int index = myChines.Count / 2 + startOffset;
                    lastIndex = -1;
                    Point3D? lastPoint = null;
                    while (lastPoint == null && index != end-2*increment)
                    {
                        lastPoint = GeometryOperations.InterpolateFromZ(myChines[index], length);
                        lastIndex = index;
                        index += increment;
                    }

                    // If nothing is in range, go to the next point
                    if (lastPoint == null)
                    {
                        if (Waterlines.Count == 2 && length > 8 && length < 12) Debug.WriteLine("Nothing in range {0}", Waterlines.Count);
                        continue;
                    }

                    if (Waterlines.Count == 2 && length > 8 && length < 12) Debug.WriteLine("Starting lastPoint: {0} {1}", lastPoint, index - 1);
                    
                    //if (index == 1 && lastPoint.Value.Y < height) takingOnWater = true;

                    Point3D? point;
                    // FIX THIS: determine when taking on water.
                    for (int chine=index; chine!=end; chine+=increment)
                    {
                        foundLeft = false;

                        point = GeometryOperations.InterpolateFromZ(myChines[chine], length);
                        if (point != null)
                        {
                            if (chine == end-increment && point.Value.Y < height) 
                                takingOnWater = true;

                            if (Math.Min(lastPoint.Value.Y, point.Value.Y) <= height && height < Math.Max(lastPoint.Value.Y, point.Value.Y))
                            {
                                Point3D newPoint = GeometryOperations.InterpolateFromY(lastPoint.Value, point.Value, height);
                                if (Waterlines.Count == 2 && length > 8 && length < 12) Debug.WriteLine("Point: {0} from {1} to {2} indexes {3} {4}", newPoint, lastPoint, point, lastIndex, chine);
                                if (!foundLeft)
                                {
                                    left.Add(newPoint);
                                    foundLeft = true;
                                    break;  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<< TEMP <<<<<<<<<<<<<<<
                                }
                                else
                                {
                                    right.Add(newPoint);
                                    break;  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                                }
                            }

                            if (Waterlines.Count == 2 && length > 8 && length < 12) Debug.WriteLine("new lastpoint: {0} old: {1} chine: {2}", point, lastPoint, chine);
                            lastPoint = point;
                            lastIndex = chine;
                        }
                        else
                        {
                            if (Waterlines.Count == 2) Debug.WriteLine("null point {0} {1} {2}", Waterlines.Count, chine, length);
                        }
                    }
                }

                if (!takingOnWater)
                {
                    for (int ii = right.Count - 1; ii >= 0; ii--)
                    {
                        left.Add(right[ii]);
                    }
                    Waterlines.Add(left);

                    height += depthInterval;

                    // avoid an infinite loop if something goes wrong with taking on water calculation above
                    if (height > min.Y + size.Y) takingOnWater = true;
                }
            }

            return Waterlines;
        }

         //*************************************************************
        // INotifyPropertyChanged implementation
        // Handled by base class
    }
}
