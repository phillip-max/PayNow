using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
//using Microsoft.Practices.EnterpriseLibrary.Validation;

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
                //ValidationResults _validationResults = null;
                //Validator v = ValidationFactory.CreateValidator(this.GetType());
                //_validationResults = v.Validate(this);

                //if (_validationResults.IsValid)
                //{
                //    foreach (T item in this)
                //    {
                //        if (!item.IsValid)
                //            return false;
                //    }
                //}
                //else
                //    return false;

                return true;
            }
        }
    }
}
