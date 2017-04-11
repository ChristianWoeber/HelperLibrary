using System;
using System.Net;
using HelperLibrary.Database.Models;
using HelperLibrary.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YahooLibrary;

namespace HelperLibraryTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void GetListOfTypeYahooRecordTest()
        {

            var testdata = "Date,Open,High,Low,Close,Volume,Adj Close \r\n" +
                            "2017 - 03 - 20,12050.80957,12082.30957,12033.240234,12052.900391,107350200,12052.900391           \r\n" +
                            "2017 - 03 - 17,12039.240234,12117.900391,12018.269531,12095.240234,187435700,12095.240234         \r\n" +
                            "2017 - 03 - 16,12140.299805,12156.44043,12046.070312,12083.179688,119337500,12083.179688          \r\n" +
                            "2017 - 03 - 15,12000.44043,12027.049805,11977.320312,12009.870117,101358200,12009.870117          \r\n" +
                            "2017 - 03 - 14,11987.339844,12002.75,11930.379883,11988.790039,105152600,11988.790039             \r\n" +
                            "2017 - 03 - 13,11954.799805,12006.009766,11949.030273,11990.030273,75690500,11990.030273          \r\n" +
                            "2017 - 03 - 10,12018.480469,12067.070312,11936.80957,11963.179688,107316000,11963.179688          \r\n" +
                            "2017 - 03 - 09,11923.509766,12024.700195,11917.780273,11978.389648,102964600,11978.389648         \r\n" +
                            "2017 - 03 - 08,11922.860352,12017.280273,11922.30957,11967.30957,96140500,11967.30957             \r\n" +
                            "2017 - 03 - 07,11963.759766,11988.870117,11935.330078,11966.139648,76452900,11966.139648          \r\n" +
                            "2017 - 03 - 06,11956.80957,11998.830078,11921.570312,11958.400391,96192800,11958.400391           \r\n" +
                            "2017 - 03 - 03,11998.05957,12058.209961,11995.400391,12027.360352,100194300,12027.360352          \r\n" +
                            "2017 - 03 - 02,12052.799805,12082.589844,12041.900391,12059.570312,79637900,12059.570312";

            var lsYahooRecs = SimpleTextParser.GetListOfType<YahooDataRecord>(testdata);

            Assert.IsTrue(lsYahooRecs.Count > 0, "keine Daten in der Rückgabe");
        }
        [TestMethod]
        public void GetListOfTypeEzbFxRates()
        {
            var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.zip";
            using (var client = new WebClient())
            {
                var zip = client.DownloadData(url);
                var lsEzbRecords = SimpleTextParser.GetListOfType<EzbFxRecord>(zip, true);

                Assert.IsTrue(lsEzbRecords.Count > 0, "keine Daten in der Rückgabe");
            }
        }
    }
}
