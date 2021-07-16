using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AVSHull
{
    abstract class UndoRedoLog<T> where T : class
    {
        protected Stack<T> Log = new Stack<T>();

        public int Count { get { return Log.Count; } }
        private int topPermanent;

        public abstract void Add(T value);
        //{
        //    bool is_cloneable = typeof(ICloneable).IsAssignableFrom(typeof(T));
        //    bool is_enumerable = typeof(IEnumerable<T>).IsAssignableFrom(typeof(T));
        //    if (is_cloneable)
        //        Log.Push((T)((ICloneable)value).Clone());
        //    else if (is_enumerable)
        //    {
        //        T copy = new T();
        //        IEnumerable collection = value as IEnumerable;

        //    }
        //    else
        //    {
        //        Log.Push(value);
        //    }

        //}
        public void Snapshot()
        {
            if (Log.Count > topPermanent)
            {
                T temp = Log.Pop();

                while (Log.Count > topPermanent)
                {
                    Log.Pop();
                }

                Log.Push(temp);
                topPermanent = Log.Count;
            }
        }

        public T Pop()
        {
            if (Log.Count == 0) return null;

            return Log.Pop();
        }
        public T Peek()
        {
            if (Log.Count == 0) return null;

            return Log.Peek();
        }
        public void Clear()
        {
            Log.Clear();
        }
    }

    class CloneableLog<U> : UndoRedoLog<U> where U : class, ICloneable
    {
        public override void Add(U value)
        {
            Log.Push((U)value.Clone());
        }
    }

    class ListLog<U,V> : UndoRedoLog<U> where V: class, new() where U : List<V>, new()
    {
        public override void Add(U value)
        {
            U list = new U();
            foreach (V item in value)
            {
                list.Add(item);
            }

            Log.Push(list);
        }
    }

    class HullLog : CloneableLog<Hull>
    { }

    class PanelLog : ListLog<List<Panel>, Panel>
    { }

}
