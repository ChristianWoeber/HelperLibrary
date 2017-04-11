using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsIc(this string source, string toCeck)
        {
            return source.IndexOf(toCeck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
