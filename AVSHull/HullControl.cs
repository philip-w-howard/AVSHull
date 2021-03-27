using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class HullControl : Control, INotifyPropertyChanged
    {
        public enum PerspectiveType { FRONT, TOP, SIDE, PERSPECTIVE };
        private const double CLICK_WIDTH = 5.0;
        private const int HANDLE_SIZE = 5;
        private const int NOT_SELECTED = -1;

        private EditableHull m_editableHull;
        private double m_scale = 1.0;
        public bool IsEditable = false;
        public PerspectiveType perspective = PerspectiveType.PERSPECTIVE;

        private List<Geometry> m_bulkheadGeometry;
        private List<RectangleGeometry> m_handles;

        private int m_selectedBulkhead = NOT_SELECTED;
        private int m_draggingHandle = NOT_SELECTED;
        private bool m_dragging = false;
        private Point m_startDrag = new Point(0, 0);
        private Point m_lastDrag = new Point(0, 0);

        public HullControl()
        {
            m_editableHull = null;
            m_bulkheadGeometry = new List<Geometry>();
            m_handles = new List<RectangleGeometry>();
        }

        public EditableHull editableHull
        {
            get { return m_editableHull; }
            set { m_editableHull = value; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (m_editableHull != null)
            {
                Geometry chines = m_editableHull.GetChineGeometry();
                Rect bounds = chines.Bounds;

                double scale_x = 0.9 * finalSize.Width / bounds.Width;
                double scale_y = 0.9 * finalSize.Height / bounds.Height;

                m_scale = Math.Min(scale_x, scale_y);

                return new Size(m_scale * bounds.Width, m_scale * bounds.Height);
            }
            else
            {
                return finalSize;
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            if (m_editableHull == null) return;

            ScaleTransform scaleXform = new ScaleTransform(m_scale, m_scale);

            m_bulkheadGeometry.Clear();
            foreach (Bulkhead bulk in m_editableHull.bulkheads)
            {
                Geometry bulkGeom = bulk.GetGeometry();
                bulkGeom.Transform = scaleXform;
                m_bulkheadGeometry.Add(bulkGeom);
            }

            Geometry chines = m_editableHull.GetChineGeometry();
                       
            chines.Transform = scaleXform;

            Pen bulkheadPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            Pen chinePen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

            foreach (Geometry geom in m_bulkheadGeometry)
            {
                drawingContext.DrawGeometry(null, bulkheadPen, geom);
            }
            
            drawingContext.DrawGeometry(null, chinePen, chines);

            if (IsEditable && m_selectedBulkhead != NOT_SELECTED)
            {
                foreach (Geometry geom in m_handles)
                {
                    drawingContext.DrawGeometry(null, bulkheadPen, geom);
                }
            }
        }

        private void CreateHandles()
        {
            m_handles.Clear();
            if (IsEditable && m_selectedBulkhead != NOT_SELECTED)
            {
                ScaleTransform xform = new ScaleTransform(m_scale, m_scale);

                Bulkhead bulk = m_editableHull.bulkheads[m_selectedBulkhead];
                foreach (Point3D point in bulk.Points)
                {
                    Rect rect = new Rect();
                    rect.Height = HANDLE_SIZE/m_scale;
                    rect.Width = HANDLE_SIZE/m_scale;
                    rect.X = point.X - HANDLE_SIZE / m_scale / 2;
                    rect.Y = point.Y - HANDLE_SIZE / m_scale / 2;
                    RectangleGeometry geom = new RectangleGeometry(rect);
                    geom.Transform = xform;
                    m_handles.Add(geom);
                }
            }
        }
        private int BulkheadClicked(Point loc)
        {
            Pen pen = new Pen(Brushes.Black, CLICK_WIDTH);
            for (int index=0; index< m_bulkheadGeometry.Count; index++)
            {
                if (m_bulkheadGeometry[index].StrokeContains(pen, loc))
                {
                    return index;
                }
            }
            return NOT_SELECTED;
        }

        private int HandleClicked(Point loc)
        {
            for (int index = 0; index < m_handles.Count; index++)
            {
                if (m_handles[index].FillContains(loc))
                {
                    return index;
                }
            }

            return NOT_SELECTED;
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (IsEditable)
            {
                m_draggingHandle = HandleClicked(loc);
                if (m_draggingHandle != NOT_SELECTED)
                {
                    m_dragging = true;
                    m_startDrag = loc;
                    m_lastDrag = loc;

                    Debug.WriteLine("clicked handle {0}", m_draggingHandle);
                }
                else
                {
                    m_dragging = false;
                    m_selectedBulkhead = BulkheadClicked(loc);
                    if (m_selectedBulkhead != NOT_SELECTED)
                    {
                        CreateHandles();
                        InvalidateVisual();
                    }
                }

                e.Handled = true;
            }
        }
        //protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        //{
        //    UIElement content = (UIElement)this.Content;
        //    Point loc = e.GetPosition(content);

        //    Debug.WriteLine("dropped handle {0} {1}", m_DraggingHandle, m_Dragging);
        //    if (m_Dragging)
        //    {
        //        double x, y, z;

        //        if (perspective == PerspectiveType.FRONT)
        //        {
        //            // Front
        //            x = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
        //            y = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
        //            z = 0;
        //        }
        //        else if (perspective == PerspectiveType.SIDE)
        //        {
        //            // Side
        //            x = 0;
        //            y = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
        //            z = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
        //        }
        //        else if (perspective == PerspectiveType.TOP)
        //        {
        //            // Top
        //            x = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
        //            y = 0;
        //            z = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
        //        }
        //        else
        //        {
        //            x = 0;
        //            y = 0;
        //            z = 0;
        //        }

        //        m_Hull.UpdateMirroredBulkheadPoint(m_SelectedBulkhead, m_DraggingHandle, x, y, z);

        //        m_Dragging = false;

        //        //FIXTHIS: need to recompute chines?
        //        Draw();
        //    }
        //}

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point loc = e.GetPosition(this);

                if (m_dragging)
                {
                    ScaleTransform xform = new ScaleTransform(m_scale, m_scale);

                    Rect rect = m_handles[m_draggingHandle].Rect;
                    rect.X += (loc.X - m_lastDrag.X) / m_scale;
                    rect.Y += (loc.Y - m_lastDrag.Y) / m_scale;

                    m_lastDrag = loc;
                    RectangleGeometry geom = new RectangleGeometry(rect);
                    geom.Transform = xform;

                    m_handles[m_draggingHandle] = geom;
                    InvalidateVisual();
                }
            }
        }
        //*******************************************************
        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
