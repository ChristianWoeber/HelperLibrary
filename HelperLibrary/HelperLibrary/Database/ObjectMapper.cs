using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;

namespace HelperLibrary.Database
{
    internal class ObjectMapper<T>
    {
        internal static T Create(IDataReader rd)
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var item in typeof(T).GetProperties())
            {
                var lstAttributes = (IList<CustomAttributeData>)item.CustomAttributes;
                if (lstAttributes.Count > 0)
                {
                    var attr = lstAttributes[0].NamedArguments[0].TypedValue.Value.ToString();
                    var dbValue = rd[attr];
                    var propertyValue = TypeConversion(dbValue, item.PropertyType);
                    item.SetValue(obj, propertyValue);
                }
            }
            return obj;
        }

        private static object TypeConversion(object dbValue, Type propertyType)
        {
            if (propertyType == typeof(int))
            {
                return Convert.ToInt32(dbValue);
            }
            else if (propertyType == typeof(int?))
            {
                if (propertyType == null)
                    return null;

                return Convert.ToInt32(dbValue);
            }

            else if (propertyType == typeof(string))
            {
                var tmp = dbValue.ToString();
                return tmp;
            }
            else if (propertyType == typeof(DateTime))
            {
                var tmp = (DateTime)dbValue;
                return tmp;
            }
            else if (propertyType == typeof(decimal))
            {
                var tmp = (decimal)dbValue;
                return tmp;
            }
            else if (propertyType == typeof(double))
            {
                var tmp = (double)dbValue;
                return tmp;
            }

            return null;
        }
    }
}