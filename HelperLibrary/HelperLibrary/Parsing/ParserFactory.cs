
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using HelperLibrary.Database.Models;
using HelperLibrary.Util.Atrributes;
using HelperLibrary.Extensions;
using System;

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

        public static List<T> GetListOfTypeFromFilePath<T>(string path)
        {
            if (File.Exists(path))
                return GetListOfType<T>(File.ReadAllText(path));

            return null;
        }


        public static List<T> GetListOfType<T>(string data)
        {
            //Action<T, object> setterFunc = null;
            var lsReturn = new List<T>();

            if (string.IsNullOrWhiteSpace(data))
                return new List<T>();

            //initialize an empty HashSet with Key PropertyName and Value the input Mapping (the search keywords of the generic Type T
            var keywords = new HashSet<InputMapper<T>>();

            //initialize an empty Dictionary with Key PropertyName and Value Mapping
            var dicInputMapping = new Dictionary<string, TextReaderInputRecordMapping>(StringComparer.OrdinalIgnoreCase);
            //Get Keywords vis Reflection for Mapping//


            foreach (var item in typeof(T).GetProperties())
            {
                var attr = item.GetCustomAttributes(typeof(InputMapping), false);
                if (attr.Length > 0)
                {
                    var mappingAttr = (InputMapping[])attr;
                    foreach (var key in mappingAttr[0].KeyWords)
                    {
                        keywords.Add(new InputMapper<T>(key, item));
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
                                if (keyword.PropertyName.ContainsIC(field))
                                {
                                    if (!dicInputMapping.ContainsKey(field))
                                    {

                                        var mapping = new TextReaderInputRecordMapping(field, i);
                                        dicInputMapping.Add(field, mapping);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {

                            // Create Obj //
                            var obj = Activator.CreateInstance<T>();

                            // Set Value of Mapped Properties
                            var insertToList = true;
                            foreach (var item in keywords)
                            {
                                if (dicInputMapping.ContainsKey(item.PropertyName))
                                {
                                    var value = fields[dicInputMapping[item.PropertyName].ArrayIndex];

                                    if (value == "null" || string.IsNullOrEmpty(value))
                                    {
                                        insertToList = false;
                                        break;
                                    }

                                    item.SetterFunc(obj, Convert.ChangeType(value, item.PropertyInfo.PropertyType, CultureInfo.InvariantCulture));


                                    //item.PropertyInfo.SetValue(obj, Convert.ChangeType(prop, item.PropertyInfo.PropertyType, CultureInfo.InvariantCulture));
                                }
                            }
                            if (insertToList)
                                lsReturn.Add(obj);
                            break;
                        }
                    }

                    isFirst = false;
                }
                return lsReturn;
            }
        }
        // n=name, o=open, p = previous close, s = symbol// 
        public static YahooDataRecordExtended GetSingleYahooLineHcMapping(string data)
        {
            var dataArray = data.Split(',', ';');

            if (dataArray[2].Contains("N/A") || dataArray[4].Contains("N/A"))
                return null;

            var name = Normalize(dataArray[0]);
            var close = ParseDecimal(dataArray[2]);
            var asof = ParseDateTime(dataArray[4]);

            return new YahooDataRecordExtended
            {
                Name = name,
                AdjustedPrice = close ?? Decimal.MinValue,
                Price = close ?? Decimal.MinValue,
                Asof = asof ?? DateTime.MinValue
            };
        }

        private static string Normalize(string input)
        {
            return input.Trim('\\', '"');
        }

        private static decimal? ParseDecimal(string input)
        {
            decimal d;
            if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                return d;

            return null;

        }

        private static DateTime? ParseDateTime(string input)
        {
            var regex = new Regex(@"\d\d?\/\d\d?\/\d\d\d\d");

            if (regex.IsMatch(input))
            {
                foreach (Match match in Regex.Matches(input, @"\d\d?\/\d\d?\/\d\d\d\d"))
                {
                    if (DateTime.TryParse(match.Value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
                    {
                        return date;
                    }

                    throw new ArgumentException("Achtung das DateTime konnte nicht geparsed werden!");

                }
            }

            return null;
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

    public class InputMapper<T> : Tuple<string, PropertyInfo>
    {
        public InputMapper(string propertyName, PropertyInfo propertyInfo) : base(item1: propertyName, item2: propertyInfo)
        {

        }
        public string PropertyName => Item1;
        public PropertyInfo PropertyInfo => Item2;
        public Action<T, object> SetterFunc => PropertyInfo.CreateSetter<T>();
    }
}
