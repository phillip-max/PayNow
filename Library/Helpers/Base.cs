using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text;
//using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace Simple
{
    [Serializable]
    public abstract class SimpleBase<T> : Object, ICloneable
    {
       // private ValidationResults _validationResults = null;
        private string _validationResultsString = string.Empty;

        //public virtual bool IsValid
        //{
        //    get
        //    {
        //        Validator v = ValidationFactory.CreateValidator(this.GetType());
        //        _validationResults = v.Validate(this);
        //        _validationResultsString = string.Empty;
        //        foreach (ValidationResult result in _validationResults)
        //        {
        //            if (!string.IsNullOrEmpty(_validationResultsString))
        //                _validationResultsString += "<br/>";
        //            _validationResultsString += "* " + result.Message;
        //        }
        //        return _validationResults.IsValid;
        //    }
        //}

        //public ValidationResults ValidationResults
        //{
        //    get
        //    {
        //        return _validationResults;
        //    }
        //}

        public string ValidationResultString
        {
            get
            {
                return _validationResultsString;
            }
        }

        public T Clone()
        {
            return (T)ObjectCloner.Clone(this);
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }

    internal static class ObjectCloner
    {
        public static object Clone(object obj)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(buffer, obj);
                buffer.Position = 0;
                object temp = formatter.Deserialize(buffer);
                return temp;
            }
        }
    }
}
