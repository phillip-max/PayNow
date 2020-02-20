using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Simple
{
    [Serializable]
    public abstract class SimpleCollection<T> : CollectionBase where T: SimpleBase<T>
    {   
        public T Add(T item)
        {
            List.Add(item);
            return item;
        }

        public void Remove(T item)
        {
            List.Remove(item);
        }

        public bool Contains(T item)
        {

            return List.Contains(item);
        }

        public T this[int index]
        {
            get
            {
                return ((T)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        public bool IsValid
        {
            get
            {   
                return true;
            }
        }
    }
}
