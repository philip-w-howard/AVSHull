using System;
using System.Collections.Generic;
using System.Text;

namespace AVSHull
{
    interface ILayoutWriter
    {
        PanelLayoutControl Layout { get; set; }

        bool? SaveLayout();
    }
}
