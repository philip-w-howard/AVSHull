using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for HullControl.xaml
    /// </summary>
    public partial class HullControl : UserControl
    {
        public enum PerspectiveType { FRONT, TOP, SIDE, PERSPECTIVE };
        private const double CLICK_WIDTH = 5.0;
        private const int HANDLE_SIZE = 5;
        private const int NOT_SELECTED = -1;

        private EditableHull m_editableHull;
        private double m_scale = 1.0;
        private TransformGroup m_xform = new TransformGroup();              // Transform applied to geometry to scale and center drawings
        private bool IsEditable = false;
        private PerspectiveType _perspective = PerspectiveType.PERSPECTIVE;
        public PerspectiveType Perspective
        {
            get { return _perspective; }
            set
            {
                m_editableHull = new EditableHull();
                _perspective = value;
                switch (_perspective)
                {
                    case PerspectiveType.FRONT:
                        m_editableHull.Rotate(0, 0, 180);
                        IsEditable = true;
                        break;
                    case PerspectiveType.TOP:
                        m_editableHull.Rotate(0, 90, 90);
                        IsEditable = true;
                        break;
                    case PerspectiveType.SIDE:
                        m_editableHull.Rotate(0, 90, 180);
                        IsEditable = true;
                        break;
                    case PerspectiveType.PERSPECTIVE:
                        m_editableHull.Rotate(10, 30, 190);
                        IsEditable = false;
                        break;
                }
            }
        }
        private bool m_RecreateHandles = false;
        private bool m_InsertBulkhead = false;

        private List<Geometry> m_bulkheadGeometry;
        private List<RectangleGeometry> m_handles;

        private int m_selectedBulkhead = NOT_SELECTED;
        private int m_draggingHandle = NOT_SELECTED;
        private bool m_dragging = false;
        private Point m_startDrag = new Point(0, 0);
        private Point m_lastDrag = new Point(0, 0);
        private bool m_movingBulkhead = false;

        private NotifyPoint3D m_mouseLoc;
        static int SequenceCounter = 0;
        private int sequence;

        public HullControl()
        {
            InitializeComponent();

            sequence = ++SequenceCounter;

            m_editableHull = null;
            m_bulkheadGeometry = new List<Geometry>();
            m_handles = new List<RectangleGeometry>();

            m_mouseLoc = (NotifyPoint3D)FindResource("HullMouseLocation");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // attempt to clip output. FIX THIS: Does not work as expected
            RectangleGeometry clip = new RectangleGeometry();
            clip.Rect = new Rect(availableSize);
            Clip = clip;

            m_RecreateHandles = true;
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

                ScaleTransform scaleXform = new ScaleTransform(m_scale, m_scale);
                double xlate_x = (DesiredSize.Width - bounds.Width * m_scale) / 2;
                double xlate_y = (DesiredSize.Height - bounds.Height * m_scale) / 2;

                TranslateTransform xlateXform = new TranslateTransform(xlate_x, xlate_y);
                m_xform = new TransformGroup();
                m_xform.Children.Add(scaleXform);
                m_xform.Children.Add(xlateXform);

                // If scale changed, need to recreate handles.
                //if (m_scale != newScale) CreateHandles();
                if (m_RecreateHandles) CreateHandles();
                m_RecreateHandles = false;


                return finalSize;
                //return new Size(newScale * bounds.Width, newScale * bounds.Height);
            }
            else
            {
                return finalSize;
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Debug.WriteLine("HullControl.Render {0}:{1}", Perspective, sequence);
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            if (m_editableHull == null) return;

            Pen bulkheadPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            Pen chinePen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

            m_bulkheadGeometry = m_editableHull.GetBulkheadGeometry();
            foreach (Geometry geom in m_bulkheadGeometry)
            {
                geom.Transform = m_xform;
                drawingContext.DrawGeometry(null, bulkheadPen, geom);
            }


            Geometry chines = m_editableHull.GetChineGeometry();
            chines.Transform = m_xform;
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
                Bulkhead bulk = m_editableHull.Bulkheads[m_selectedBulkhead];
                foreach (Point3D point in bulk.Points)
                {
                    Rect rect = new Rect();
                    rect.Height = HANDLE_SIZE / m_scale;
                    rect.Width = HANDLE_SIZE / m_scale;
                    rect.X = point.X - HANDLE_SIZE / m_scale / 2;
                    rect.Y = point.Y - HANDLE_SIZE / m_scale / 2;
                    RectangleGeometry geom = new RectangleGeometry(rect);
                    geom.Transform = m_xform;
                    m_handles.Add(geom);
                }
            }
        }
        public void Rotate(double x, double y, double z)
        {
            m_editableHull.Rotate(x, y, z);
            _perspective = PerspectiveType.PERSPECTIVE;
            IsEditable = false;
        }

        private int BulkheadClicked(Point loc)
        {
            Pen pen = new Pen(Brushes.Black, CLICK_WIDTH);
            for (int index = 0; index < m_bulkheadGeometry.Count; index++)
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
            if (m_selectedBulkhead != NOT_SELECTED)
            {
                for (int index = 0; index < m_handles.Count; index++)
                {
                    if (m_handles[index].FillContains(loc))
                    {
                        return index;
                    }
                }
            }

            return NOT_SELECTED;
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            m_RecreateHandles = false;
            m_movingBulkhead = false;

            Point loc = e.GetPosition(this);

            if (IsEditable)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    m_draggingHandle = HandleClicked(loc);
                    if (m_InsertBulkhead)
                    {
                        if (Perspective == PerspectiveType.SIDE || Perspective == PerspectiveType.TOP)
                        {
                            double Z = (loc.X - m_editableHull.Bulkheads[0].Points[0].Z) / m_scale;
                            m_editableHull.InsertBulkhead(Z);
                        }
                    }
                    else if (m_draggingHandle != NOT_SELECTED)
                    {
                        m_dragging = true;
                        m_startDrag = loc;
                        m_lastDrag = loc;
                    }
                    else
                    {
                        m_dragging = false;
                        int bulk = BulkheadClicked(loc);
                        if (bulk != m_selectedBulkhead)
                        {
                            m_selectedBulkhead = bulk;
                            m_handles.Clear();
                        }
                        else
                        {
                            UI_Params setup = (UI_Params)this.FindResource("Curr_UI_Params");
                            bool? allowMoves = setup.AllowBulkheadMoves;

                            if (allowMoves == true)
                            {
                                m_movingBulkhead = true;
                                m_startDrag = loc;
                                m_lastDrag = loc;
                            }
                        }

                        if (m_selectedBulkhead != NOT_SELECTED)
                        {
                            CreateHandles();
                            InvalidateVisual();
                        }
                    }
                }
            }
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            m_RecreateHandles = false;

            Point loc = e.GetPosition(this);

            if (m_dragging && m_draggingHandle != NOT_SELECTED)
            {
                double x, y, z;

                if (Perspective == PerspectiveType.FRONT)
                {
                    // Front
                    x = (m_startDrag.X - loc.X) / m_scale;
                    y = (m_startDrag.Y - loc.Y) / m_scale;
                    z = 0;

                    // Can't change X coordinate on front view of BOW.
                    if (m_editableHull.Bulkheads[m_selectedBulkhead].Type == Bulkhead.BulkheadType.BOW) x = 0;
                }
                else if (Perspective == PerspectiveType.SIDE)
                {
                    // Side
                    x = 0;
                    y = (m_startDrag.Y - loc.Y) / m_scale;
                    z = -(m_startDrag.X - loc.X) / m_scale;
                }
                else if (Perspective == PerspectiveType.TOP)
                {
                    // Top
                    x = -(m_startDrag.Y - loc.Y) / m_scale;
                    y = 0;
                    z = -(m_startDrag.X - loc.X) / m_scale;
                }
                else
                {
                    x = 0;
                    y = 0;
                    z = 0;
                }


                m_editableHull.UpdateBulkheadPoint(m_selectedBulkhead, m_draggingHandle, x, y, z);

                m_dragging = false;
                m_draggingHandle = NOT_SELECTED;
                m_RecreateHandles = true;

                //FIXTHIS: need to recompute chines?
                InvalidateVisual();
            }

            HullLog undoLog = (HullLog)this.FindResource("UndoLog");
            undoLog.Snapshot();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            m_RecreateHandles = false;
            Point loc = e.GetPosition(this);

            m_mouseLoc.X = 0;
            m_mouseLoc.Y = 0;
            m_mouseLoc.Z = 0;

            Rect bounds = new Rect(new Size(0, 0));
            if (m_bulkheadGeometry.Count > 0)
            {
                bounds = m_bulkheadGeometry[0].Bounds;

                foreach (Geometry geom in m_bulkheadGeometry)
                {
                    Rect bulkBounds = geom.Bounds;
                    Point topLeft = new Point(0, 0);
                    topLeft.X = Math.Min(bulkBounds.Left, bounds.Left);
                    topLeft.Y = Math.Min(bulkBounds.Top, bounds.Top);

                    double right = Math.Max(bulkBounds.Right, bounds.Right);
                    double bottom = Math.Max(bulkBounds.Bottom, bounds.Bottom);
                    Size size = new Size(right - topLeft.X, bottom - topLeft.Y);

                    bounds.Size = size;
                    bounds.Location = topLeft;

                    double X = loc.X - bounds.Left;
                    double Y = loc.Y - bounds.Top;

                    Size3D hullSize = m_editableHull.GetSize();
                    double scale_X = hullSize.X / bounds.Width;
                    double scale_Y = hullSize.Y / bounds.Height;

                    X *= scale_X;
                    Y *= scale_Y;

                    switch (Perspective)
                    {
                        case PerspectiveType.FRONT:
                            m_mouseLoc.X = X - hullSize.X / 2;
                            m_mouseLoc.Y = hullSize.Y - Y;
                            break;
                        case PerspectiveType.SIDE:
                            m_mouseLoc.Y = hullSize.Y - Y;
                            m_mouseLoc.Z = X;
                            break;
                        case PerspectiveType.TOP:
                            m_mouseLoc.X = Y - hullSize.Y / 2;
                            m_mouseLoc.Z = X;
                            break;
                    }
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (m_dragging)
                {
                    Rect rect = m_handles[m_draggingHandle].Rect;
                    double deltaX = (loc.X - m_lastDrag.X) / m_scale;
                    double deltaY = (loc.Y - m_lastDrag.Y) / m_scale;
                    rect.X += deltaX;
                    rect.Y += deltaY;

                    m_lastDrag = loc;

                    RectangleGeometry geom = new RectangleGeometry(rect);
                    geom.Transform = m_xform;

                    m_handles[m_draggingHandle] = geom;
                    InvalidateVisual();
                }
                else if (m_movingBulkhead && m_selectedBulkhead != NOT_SELECTED && m_editableHull.Bulkheads[m_selectedBulkhead].Type != Bulkhead.BulkheadType.BOW &&
                    (Perspective == PerspectiveType.TOP || Perspective == PerspectiveType.SIDE))
                {
                    double deltaX = (loc.X - m_lastDrag.X) / m_scale;
                    double deltaY = (loc.Y - m_lastDrag.Y) / m_scale;
                    m_lastDrag = loc;
                    m_editableHull.UpdateBulkheadPoint(m_selectedBulkhead, NOT_SELECTED, 0, 0, deltaX);

                    InvalidateVisual();
                }
            }
        }
        private void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("HullControl.PropertyChanged: " + e.PropertyName);
            if (e.PropertyName == "Bulkhead" || e.PropertyName == "HullData")
            {
                CreateHandles();
                InvalidateVisual();
            }
        }

        private void InsertClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = this.FindResource("Curr_UI_Params") as UI_Params;
            m_editableHull.InsertBulkhead(values.NewBulkheadLoc);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedBulkhead != NOT_SELECTED)
            {
                m_editableHull.DeleteBulkhead(m_selectedBulkhead);
                CreateHandles();
                InvalidateVisual();
            }
        }

        private void ChinesClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = this.FindResource("Curr_UI_Params") as UI_Params;
            m_editableHull.ChangeChines(values.NumChines);
        }
    }
}
