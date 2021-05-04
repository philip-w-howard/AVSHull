using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    class Splines
    {
        public const int RELAXED = 1;
        public const int CLAMPED = 2;

        public Splines(Point3DCollection points, int endCondition)
        {
            m_numPoints = points.Count;
            m_endCondition = endCondition;
            m_points = points;

            m_m_matrix = new double[m_numPoints, m_numPoints];
            m_b_matrix = new double[m_numPoints, 3];
            m_chordLength = new double[m_numPoints - 1];

            //********************************************
            ComputeChords();
            CreateMatrices();
            GaussianElimination();
        }

        private void ComputeChords()
        {
            // Use normalized chords: 0<t<1
            for (int ii = 0; ii < m_numPoints - 1; ii++)
            {
                m_chordLength[ii] = 1;
            }

            // Compute chord length for scaling
            for (int ii = 0; ii < m_numPoints - 1; ii++)
            {
                double x = m_points[ii].X - m_points[ii + 1].X;
                double y = m_points[ii].Y - m_points[ii + 1].Y;
                double z = m_points[ii].Z - m_points[ii + 1].Z;

                m_chordLength[ii] = Math.Sqrt(x * x + y * y + z * z);
            }
        }

        private double GetCoord(Point3D point, int axis)
        {
            if (axis == 0)
                return point.X;
            else if (axis == 1)
                return point.Y;
            else if (axis == 2)
                return point.Z;
            else
                return Double.NaN;
        }
        private void CreateMatrices()
        {
            m_m_matrix[0, 0] = 1;
            m_m_matrix[0, 1] = .5;

            for (int ii = 1; ii < m_numPoints - 1; ii++)
            {
                m_m_matrix[ii, ii - 1] = m_chordLength[ii];
                m_m_matrix[ii, ii] = 2 * (m_chordLength[ii - 1] + m_chordLength[ii]);
                m_m_matrix[ii, ii + 1] = m_chordLength[ii - 1];
            }

            m_m_matrix[m_numPoints - 1, m_numPoints - 2] = 2;
            m_m_matrix[m_numPoints - 1, m_numPoints - 1] = 4;

            for (int axis = 0; axis < 3; axis++)
            {
                m_b_matrix[0, axis] = 3.0 / 2.0 / m_chordLength[0] * (GetCoord(m_points[1], axis) - GetCoord(m_points[0], axis));
                m_b_matrix[m_numPoints - 1, axis] = 6.0 / m_chordLength[m_numPoints - 2] *
                    (GetCoord(m_points[m_numPoints - 1], axis) - GetCoord(m_points[m_numPoints - 2], axis));

                for (int point = 1; point < m_numPoints - 1; point++)
                {
                    double factor = 3 / m_chordLength[point - 1] / m_chordLength[point];
                    double diff1 = GetCoord(m_points[point + 1], axis) - GetCoord(m_points[point], axis);
                    double diff2 = GetCoord(m_points[point], axis) - GetCoord(m_points[point - 1], axis);
                    double scale1 = m_chordLength[point - 1] * m_chordLength[point - 1];
                    double scale2 = m_chordLength[point] * m_chordLength[point];
                    m_b_matrix[point, axis] = factor * (scale1 * diff1 + scale2 * diff2);
                }
            }
        }

        private void GaussianElimination()
        {
            double scale;
            for (int row = 0; row < m_numPoints - 1; row++)
            {
                // Normalize current row
                scale = m_m_matrix[row, row];
                for (int col = row; col < m_numPoints; col++)
                {
                    m_m_matrix[row, col] /= scale;
                }
                for (int axis = 0; axis < 3; axis++)
                {
                    m_b_matrix[row, axis] /= scale;
                }

                // Zero left of next row
                scale = m_m_matrix[row + 1, row];
                for (int col = row; col < m_numPoints; col++)
                {
                    m_m_matrix[row + 1, col] -= scale * m_m_matrix[row, col];
                }
                for (int axis = 0; axis < 3; axis++)
                {
                    m_b_matrix[row + 1, axis] -= scale * m_b_matrix[row, axis];
                }
            }

            // Normalize the last row
            scale = m_m_matrix[m_numPoints - 1, m_numPoints - 1];
            for (int col = m_numPoints - 1; col < m_numPoints; col++)
            {
                m_m_matrix[m_numPoints - 1, col] /= scale;
            }
            for (int axis = 0; axis < 3; axis++)
            {
                m_b_matrix[m_numPoints - 1, axis] /= scale;
            }

            //****************************************
            // We now have a Reduced Row Echelon Form matrix
            // Solve for the unknowns
            // NOTE: this is optimized because we know we started with a tri-diagonal matrix
            for (int row = m_numPoints - 1; row > 0; row--)
            {
                scale = m_m_matrix[row - 1, row];
                m_m_matrix[row - 1, row] = 0;
                for (int axis = 0; axis < 3; axis++)
                {
                    m_b_matrix[row - 1, axis] -= scale * m_b_matrix[row, axis];
                }
            }
        }

        public Point3DCollection GetPoints(int total_points)
        {
            Point3DCollection points = new Point3DCollection();

            // B[1-4, segment, axis]
            double[,,] B = new double[4, m_numPoints - 1, 3];

            // Compute the coefficients
            for (int seg = 0; seg < m_numPoints - 1; seg++)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    double tmax = m_chordLength[seg];
                    B[0, seg, axis] = GetCoord(m_points[seg], axis);
                    B[1, seg, axis] = m_b_matrix[seg, axis];
                    B[2, seg, axis] = 3 / (tmax * tmax) * (GetCoord(m_points[seg + 1], axis) - GetCoord(m_points[seg], axis))
                                      - 2 / tmax * m_b_matrix[seg, axis]
                                      - 1.0 / tmax * m_b_matrix[seg + 1, axis];
                    B[3, seg, axis] = 2 / (tmax * tmax * tmax) * (GetCoord(m_points[seg], axis) - GetCoord(m_points[seg + 1], axis))
                                      + 1.0 / (tmax * tmax) * m_b_matrix[seg, axis]
                                      + 1.0 / (tmax * tmax) * m_b_matrix[seg + 1, axis];
                }
            }

            int pointsPerSegment = (total_points - 1) / (m_numPoints - 1);

            int index = 0;
            for (int seg = 0; seg < m_numPoints - 1; seg++)
            {
                double tmax = m_chordLength[seg];

                for (int point = 0; point < pointsPerSegment; point++)
                {
                    double t = point * tmax / pointsPerSegment;

                    Point3D loc = new Point3D();

                    loc.X = B[0, seg, 0] +
                            B[1, seg, 0] * t +
                            B[2, seg, 0] * t * t +
                            B[3, seg, 0] * t * t * t;

                    loc.Y = B[0, seg, 1] +
                            B[1, seg, 1] * t +
                            B[2, seg, 1] * t * t +
                            B[3, seg, 1] * t * t * t;

                    loc.Z = B[0, seg, 2] +
                            B[1, seg, 2] * t +
                            B[2, seg, 2] * t * t +
                            B[3, seg, 2] * t * t * t;

                    points.Add(loc);
                    index++;
                }
            }

            // Set the end point
            points.Add(m_points[m_numPoints - 1]);

            return points;
        }

        private int m_numPoints;                // N
        private int m_endCondition;             // C1
        //private int m_pointsPerSegment;         // Z
        private Point3DCollection m_points;
        private double[,] m_m_matrix;           // N [rows, cols]
        private double[,] m_b_matrix;           // B [rows, axis]
        private double[] m_chordLength;         // L [rows]
    }
}
