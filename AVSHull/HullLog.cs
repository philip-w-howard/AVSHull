using System;
using System.Collections.Generic;
using System.Text;

namespace AVSHull
{
    class HullLog
    {
        public HullLog() 
        {
            Log = new Stack<Hull>();
        }

        public int Count {  get { return Log.Count;  } }
        private Stack<Hull> Log;

        public void Add(Hull hull)
        {
            Log.Push((Hull)hull.Clone());
        }

        private int topPermanent;

        public void Snapshot()
        {
            if (Log.Count > topPermanent)
            {
                Hull temp = Log.Pop();

                while (Log.Count > topPermanent)
                {
                    Log.Pop();
                }

                Log.Push(temp);
                topPermanent = Log.Count;
            }
        }

        public Hull Pop()
        {
            if (Log.Count == 0) return null;

            return Log.Pop();
        }
    }
}
