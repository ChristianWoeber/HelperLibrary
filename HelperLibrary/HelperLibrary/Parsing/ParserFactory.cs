using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HelperLibrary.Extensions;
using HelperLibrary.Interfaces;
using HelperLibrary.Util.Atrributes;

namespace HelperLibrary.Parsing
{
    public class SimpleTextParser
    {
        public static T GetSingleOfType<T>(string data)
        {
            foreach (var item in typeof(T).GetCustomAttributes(typeof(InputMapping), false))
            {

            }
            return default(T);
        }


        public static List<T> GetListOfType<T>(byte[] data, bool isZip = false)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var zipArchiv = new ZipArchive(ms))
                {
                    foreach (var entry in zipArchiv.Entries)
                    {
                        if (Path.GetExtension(entry.FullName).Contains("csv"))
                        {
                            using (var rd = new StreamReader(entry.Open()))
                            {
                                return GetListOfType<T>(rd.ReadToEnd());
                            }
                        }
                    }
                }
            }
            return new List<T>();
        }


        public static List<T> GetListOfType<T>(string data)
        {
            var lsReturn = new List<T>();
            if (string.IsNullOrWhiteSpace(data))
                return new List<T>();

            var keywords = new HashSet<InputMapper>();
            var dicInputMapping = new Dictionary<string, TextReaderInputRecordMapping>(StringComparer.OrdinalIgnoreCase);
            //Get Keywords vis Reflection for Mapping//
            foreach (var item in typeof(T).GetProperties())
            {
                var attr = item.GetCustomAttributes(typeof(InputMapping), false);
                if (attr != null)
                {
                    var mappingAttr = (InputMapping[])attr;
                    foreach (var key in mappingAttr[0].KeyWords)
                    {
                        keywords.Add(new InputMapper(key, item));
                    }
                }
            }

            using (var rd = new StringReader(data))
            {
                bool isFirst = true;
                string line;

                while ((line = rd.ReadLine()) != null)
                {
                    var fields = line.Split(',', ';', '|');
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i].Trim();
                        // map Header //
                        if (isFirst)
                        {
                            foreach (var keyword in keywords)
                            {
                                if (keyword.PropertyName.ContainsIc(field))
                                {
                                    if (!dicInputMapping.ContainsKey(field))
                                    {
                                        var mapping = new TextReaderInputRecordMapping(field, i);
                                        dicInputMapping.Add(field, mapping);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Create Obj //
                            var obj = Activator.CreateInstance<T>();

                            //Search for Mapped Properties
                            foreach (var propInfo in typeof(T).GetProperties())
                            {
                                var attr = propInfo.GetCustomAttributes(typeof(InputMapping), false);
                                if (attr != null)
                                {
                                    var mappingAttr = (InputMapping[])attr;
                                    foreach (var keyword in mappingAttr[0].KeyWords)
                                    {
                                        if (dicInputMapping.ContainsKey(keyword))
                                        {
                                            var prop = fields[dicInputMapping[keyword].ArrayIndex];
                                            propInfo.SetValue(obj, Convert.ChangeType(prop, propInfo.PropertyType, CultureInfo.InvariantCulture));
                                        }
                                    }
                                }
                            }
                            lsReturn.Add(obj);
                            break;
                        }
                    }

                    isFirst = false;
                }
                return lsReturn;
            }          
        }
    }

    public class TextReaderInputRecordMapping : Tuple<string, int>
    {
        public TextReaderInputRecordMapping(string propertyName, int arrayIndex) : base(item1: propertyName, item2: arrayIndex)
        {

        }
        public string PropertyName => Item1;
        public int ArrayIndex => Item2;

    }

    public class InputMapper : Tuple<string, PropertyInfo>
    {
        public InputMapper(string propertyName, PropertyInfo propertyInfo) : base(item1: propertyName, item2: propertyInfo)
        {

        }
        public string PropertyName => Item1;
        public PropertyInfo PropertyInfo => Item2;
    }
}
