using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    class Matrix
    {
        public static void Multiply(double[,] left, double[,] right, out double[,] returnMatrix)
        {
            double[,] result;       // temp array so we can compute in place
            result = new double[left.GetLength(0), left.GetLength(1)];

            for (int arow = 0; arow < left.GetLength(0); arow++)
            {
                for (int bcol = 0; bcol < right.GetLength(1); bcol++)
                {
                    result[arow, bcol] = 0;
                    for (int acol = 0; acol < left.GetLength(1); acol++)
                    {
                        result[arow, bcol] += left[arow, acol] * right[acol, bcol];
                    }
                }
            }

            returnMatrix = result;
        }

        public static void Multiply(Point3DCollection left, double[,] right, out Point3DCollection returnMatrix)
        {
            Point3DCollection result = new Point3DCollection(left.Count);   // temp array so we can compute in place

            for (int ii = 0; ii < left.Count; ii++)
            {
                Point3D point = new Point3D();
                point.X = left[ii].X * right[0, 0] + left[ii].Y * right[1, 0] + left[ii].Z * right[2, 0];
                point.Y = left[ii].X * right[0, 1] + left[ii].Y * right[1, 1] + left[ii].Z * right[2, 1];
                point.Z = left[ii].X * right[0, 2] + left[ii].Y * right[1, 2] + left[ii].Z * right[2, 2];

                result.Add(point);
            }

            returnMatrix = result;
        }
        public static void Multiply(List<Rect> left, double[,] right, out List<Rect> returnMatrix)
        {
            List<Rect> result = new List<Rect>(left.Count);   // temp array so we can compute in place

            foreach (Rect rect in left)
            {
                Point point = new Point();
                point.X = rect.Location.X * right[0, 0] + rect.Location.Y * right[1, 0];
                point.Y = rect.Location.X * right[0, 1] + rect.Location.Y * right[1, 1];

                result.Add(new Rect(point, rect.Size));
            }

            returnMatrix = result;
        }

        public static void Multiply(PointCollection left, double[,] right, out PointCollection returnMatrix)
        {
            PointCollection result = new PointCollection(left.Count);   // temp array so we can compute in place

            for (int ii = 0; ii < left.Count; ii++)
            {
                Point point = new Point();
                point.X = left[ii].X * right[0, 0] + left[ii].Y * right[1, 0];
                point.Y = left[ii].X * right[0, 1] + left[ii].Y * right[1, 1];

                result.Add(point);
            }

            returnMatrix = result;
        }
    }
}
