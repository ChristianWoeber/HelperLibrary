using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperLibrary.Util.Atrributes;

namespace HelperLibrary.Database.Models
{
    public class YahooDataRecord
    {
        [InputMapping(KeyWords = new string[] { "date", "per date", "as of" })]
        [Column(Storage = "ASOF")]
        public DateTime Asof { get; set; }

        [InputMapping(KeyWords = new string[] { "close" })]
        [Column(Storage = "CLOSE_PRICE")]
        public decimal ClosePrice { get; set; }

        [InputMapping(KeyWords = new string[] { "id", "secId" })]
        [Column(Storage = "SECURITY_ID")]
        public int SecurityId { get; set; }

        [InputMapping(KeyWords = new string[] { "adj close" })]
        [Column(Storage = "ADJUSTED_CLOSE_PRICE")]
        public decimal AdjClosePrice { get; set; }


        public string Name { get;  set; }
    }
}
