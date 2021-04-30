using System;
using System.Collections.Generic;
using System.Text;

namespace AVSHull
{
    interface ILayoutWriter
    {
        PanelLayout Layout { get; set; }

        bool? SaveLayout();
    }
}
