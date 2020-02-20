using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Simple
{
    [Serializable]
    public abstract class SimpleBase<T> : Object, ICloneable
    {
       
        private string _validationResultsString = string.Empty;       
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
