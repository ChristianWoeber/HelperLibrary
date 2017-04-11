using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsOlderThanMinutes(this DateTime dt, int minutes)
        {
            if (dt.AddMinutes(minutes) < DateTime.Now)
                return true;

            return false;
        }
    }
}
