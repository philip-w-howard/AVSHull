using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for PanelLayoutControl.xaml
    /// </summary>
    public partial class PanelLayoutControl : UserControl
    {
        private const double CLICK_WIDTH = 1.0;
        private const int NOT_SELECTED = -1;
        private int m_selectedPanel = NOT_SELECTED;

        public PanelLayoutControl()
        {
            InitializeComponent();
            m_panels = new List<Panel>();
            SheetWidth = 96;
            SheetHeight = 48;
            SheetsWide = 1;
            SheetsHigh = 1;
            MouseWheel += OnMouseWheel;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseUp += OnPreviewMouseUp;

        }

        public int SheetsWide { get; set; }
        public int SheetsHigh { get; set; }
        public double SheetWidth { get; set; }
        public double SheetHeight { get; set; }

        private List<Panel> m_panels;

        private double m_scale = 1.0;
        public double Scale
        {
            get { return m_scale; }
            set 
            { 
                m_scale = value;
                InvalidateMeasure();
                InvalidateVisual(); 
            }
        }
        public void AddPanel(Panel p)
        {
            m_panels.Add(p);
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine("PanelLayout.MeasureOverride");
            //if (Double.IsInfinity(availableSize.Width) || Double.IsNaN(availableSize.Width)) availableSize.Width = 0;
            //if (Double.IsInfinity(availableSize.Height) || Double.IsNaN(availableSize.Height)) availableSize.Height = 0;

            //double width = Math.Max(availableSize.Width, SheetsWide * SheetWidth * m_scale * 1.05);
            //double height = Math.Max(availableSize.Height, SheetsHigh * SheetHeight * m_scale * 1.05);
            double width = SheetsWide * SheetWidth * m_scale;
            double height = SheetsHigh * SheetHeight * m_scale;

            return new Size(width, height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine("PanelLayoutArrangeOverride");
            //if (m_editableHull != null)
            //{
            //    Geometry chines = m_editableHull.GetChineGeometry();
            //    Rect bounds = chines.Bounds;

            //    double scale_x = 0.9 * finalSize.Width / bounds.Width;
            //    double scale_y = 0.9 * finalSize.Height / bounds.Height;

            //    m_scale = Math.Min(scale_x, scale_y);

            //    ScaleTransform scaleXform = new ScaleTransform(m_scale, m_scale);
            //    double xlate_x = (DesiredSize.Width - bounds.Width * m_scale) / 2;
            //    double xlate_y = (DesiredSize.Height - bounds.Height * m_scale) / 2;

            //    TranslateTransform xlateXform = new TranslateTransform(xlate_x, xlate_y);
            //    m_xform = new TransformGroup();
            //    m_xform.Children.Add(scaleXform);
            //    m_xform.Children.Add(xlateXform);

            //    // If scale changed, need to recreate handles.
            //    //if (m_scale != newScale) CreateHandles();
            //    if (m_RecreateHandles) CreateHandles();
            //    m_RecreateHandles = false;


            return finalSize;
            //return new Size(newScale * bounds.Width, newScale * bounds.Height);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            ScaleTransform scale = new ScaleTransform(m_scale, m_scale);

            Pen sheetPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            for (int row=0; row<SheetsWide; row++)
            {
                for (int col=0; col<SheetsHigh; col++)
                {
                    double x = row * SheetWidth;
                    double y = col * SheetHeight;
                    Rect rect = new Rect(new Point(x, y), new Point(x + SheetWidth, y + SheetHeight));
                    RectangleGeometry sheet = new RectangleGeometry(rect);
                    sheet.Transform = scale;
                    drawingContext.DrawGeometry(this.Background, sheetPen, sheet);
                }
            }

            Pen panelPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            Pen selectedPen = new Pen(System.Windows.Media.Brushes.Blue, 2.0);

            for (int index=0; index < m_panels.Count; index++)
            {
                Geometry geom = m_panels[index].GetGeometry();
                geom.Transform = scale;
                if (index == m_selectedPanel)
                    drawingContext.DrawGeometry(null, selectedPen, geom);
                else
                    drawingContext.DrawGeometry(null, panelPen, geom);

            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Scale *= 1.1;
            else if (e.Delta < 0)
                Scale /= 1.1;
            Debug.WriteLine("Delta: {0} Scale: {1}", e.Delta, m_scale);
        }

        private int PanelClicked(Point loc)
        {
            //Pen pen = new Pen(Brushes.Black, CLICK_WIDTH);
            ScaleTransform scale = new ScaleTransform(m_scale, m_scale);

            for (int index=m_panels.Count-1; index >= 0; index--)
            {
                Geometry geom = m_panels[index].GetGeometry();
                geom.Transform = scale;

                Debug.WriteLine("Panel {0}: {1} loc: {2}", index, geom.Bounds, loc);
                if (geom.FillContains(loc))
                {
                    return index;
                }
            }
            return NOT_SELECTED;
        }
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            int panel = PanelClicked(loc);
            if (panel != NOT_SELECTED)
            {
                m_selectedPanel = panel;
                InvalidateVisual();
            }
            Debug.WriteLine("Layout.MouseDown: {0} {1}", loc, m_selectedPanel);
        }
        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Point loc = e.GetPosition(canvas);

            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    if (m_selectedPanel != null && m_dragging)
            //    {
            //        // handle dragging
            //        m_selectedPanel.X += loc.X - m_dragLoc.X;
            //        m_selectedPanel.Y += loc.Y - m_dragLoc.Y;
            //        m_dragLoc = loc;
            //        Canvas.SetLeft(m_selectedPanel, m_selectedPanel.X);
            //        Canvas.SetTop(m_selectedPanel, m_selectedPanel.Y);
            //        ResizeCanvas();

            //    }
            //    else if (m_selectedPanel != null && m_rotating)
            //    {
            //        // Handle rotations
            //        double distance = loc.X - m_dragLoc.X;
            //        Debug.WriteLine("Rotate: {0}", distance);

            //        if (Math.Abs(distance) > MIN_ROTATE_DRAG)
            //        {
            //            m_dragLoc = loc;

            //            if (distance > 0)
            //                m_selectedPanel.Rotate(ROTATE_STEP);

            //            else
            //                m_selectedPanel.Rotate(-ROTATE_STEP);

            //        }
            //        ResizeCanvas();

            //    }
            //}
        }


    }
}
