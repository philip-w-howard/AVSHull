﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AVSHull
{
    class BaseHull
    {
        private static readonly Hull _hull = new Hull();

        private BaseHull() { }

        public static Hull Instance()
        {
            return _hull;
        }


    }
}
