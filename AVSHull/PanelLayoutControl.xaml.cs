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
        public PanelLayoutControl()
        {
            InitializeComponent();
            m_panels = new List<Panel>();
        }

        public int SheetsWide { get; set; }
        public int SheetsHigh { get; set; }

        private List<Panel> m_panels;

        public void AddPanel(Panel p)
        {
            m_panels.Add(p);
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // attempt to clip output. FIX THIS: Does not work as expected
            RectangleGeometry clip = new RectangleGeometry();
            clip.Rect = new Rect(availableSize);
            Clip = clip;

            Debug.WriteLine("PanelLayout.MeasureOverride");
            return availableSize;
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

            Pen panelPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            foreach (Panel panel in m_panels)
            {
                Geometry geom = panel.GetGeometry();
                drawingContext.DrawGeometry(null, panelPen, geom);

            }
            //    if (m_editableHull == null) return;

            //    Pen bulkheadPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            //    Pen chinePen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

            //    m_bulkheadGeometry.Clear();
            //    foreach (Bulkhead bulk in m_editableHull.bulkheads)
            //    {
            //        Geometry bulkGeom = bulk.GetGeometry();
            //        bulkGeom.Transform = m_xform;
            //        m_bulkheadGeometry.Add(bulkGeom);
            //    }

            //    foreach (Geometry geom in m_bulkheadGeometry)
            //    {
            //        drawingContext.DrawGeometry(null, bulkheadPen, geom);
            //    }

            //    Geometry chines = m_editableHull.GetChineGeometry();
            //    chines.Transform = m_xform;
            //    drawingContext.DrawGeometry(null, chinePen, chines);

            //    if (IsEditable && m_selectedBulkhead != NOT_SELECTED)
            //    {
            //        foreach (Geometry geom in m_handles)
            //        {
            //            drawingContext.DrawGeometry(null, bulkheadPen, geom);
            //        }
            //    }
            //}

        }
    }
}
