using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    class SVGWriter : ILayoutWriter
    {
        private System.IO.StreamWriter svgFile;

        public SVGWriter()
        { }

        private void Open(string filename)
        {
            svgFile = new System.IO.StreamWriter(filename);
            svgFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            svgFile.WriteLine("<!-- Generator: AVS Hull 0.0.0, SVGWriter V0.0.0  -->");
            svgFile.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            svgFile.WriteLine("<svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"");
            svgFile.WriteLine("     x=\"0px\" y=\"0px\"");
            svgFile.WriteLine("     Width=\"96in\" Height=\"48in\" xml:space=\"preserve\">");
        }

        private void Close()
        {
            if (svgFile != null)
            {
                svgFile.WriteLine("</svg>");
                svgFile.Close();
            }
        }


        private void Write(Panel panel)
        {
            PointCollection points = panel.Points;
            svgFile.Write("<polyline points=\"");
            foreach (Point p in points)
            {
                svgFile.Write(" " + p.X + "," + p.Y + " ");
            }

            svgFile.WriteLine("\" style=\"fill:none;stroke:black;stroke-width:1\" />");
        }
        public PanelLayoutControl Layout { get; set; }

        public bool? SaveLayout()
        {
            if (Layout == null) return false;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "SVG files (*.svg)|*.svg|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                Open(saveDlg.FileName);
                foreach (Panel panel in Layout.Panels)
                {
                    Write(panel);
                }

                Close();
            }

            return result;
        }

    }
}
