using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HelperLibrary.Database.Models;
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
                                        break;
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
                                            break;
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
        // n=name, o=open, p = previous close, s = symbol// 
        public static YahooDataRecord GetSingleYahooLineHcMapping(string data)
        {
            var dataArray = data.Split(',', ';');

            if (dataArray[2].Contains("N/A") || dataArray[4].Contains("N/A"))
                return null;

            var name = Normalize(dataArray[0]);
            var close = ParseDecimal(dataArray[2]);
            var asof = ParseDateTime(dataArray[4]);

            var rec = new YahooDataRecord
            {
                Name = name ?? "",
                AdjClosePrice = close ?? Decimal.MinValue,
                ClosePrice = close ?? Decimal.MinValue,
                Asof = asof ?? DateTime.MinValue
            };
            return rec;
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
            var regex = new Regex(@"\d\/\d\d?\/\d\d\d\d");

            if (regex.IsMatch(input))
            {
                foreach (Match match in Regex.Matches(input, @"\d\/\d\d?\/\d\d\d\d"))
                {
                    return DateTime.Parse(match.Value.ToString(),CultureInfo.InvariantCulture);
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

    public class InputMapper : Tuple<string, PropertyInfo>
    {
        public InputMapper(string propertyName, PropertyInfo propertyInfo) : base(item1: propertyName, item2: propertyInfo)
        {

        }
        public string PropertyName => Item1;
        public PropertyInfo PropertyInfo => Item2;
    }
}
