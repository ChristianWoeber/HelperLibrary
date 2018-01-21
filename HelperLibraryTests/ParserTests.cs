using System;
using System.Net;
using HelperLibrary.Database.Models;
using HelperLibrary.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HelperLibrary.Yahoo;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HelperLibraryTests
{
    [TestClass]
    public class ParserTests
    {

        [TestMethod]
        public void GetDailyDataFeedParsingTest()
        {
            var data = new WebClient().DownloadString("http://download.finance.yahoo.com/d/quotes.csv?s=^GDAXI&f=nopsd1");
            var parsed = SimpleTextParser.GetSingleYahooLineHcMapping(data);

            Assert.IsTrue(parsed != null && parsed.Asof > DateTime.MinValue);

        }

        [TestMethod]
        public void GetListOfTypeYahooRecordTest()
        {

            var testdata = "Date,Open,High,Low,Close,Volume,Adj Close \r\n" +
                            "2017 - 03 - 20,12050.80957,12082.30957,12033.240234,12052.900391,107350200,12052.900391           \r\n" +
                            "2017 - 03 - 17,12039.240234,null,12018.269531,null,null,null                                      \r\n" +
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
        [TestMethod]
        public void NewYahooUrlTest()
        {
            using (var webcrawler = new WebCrawler())
            {
                // test Request der Cookies container und collection befüllt
                var respHtml = webcrawler.DownloadString("https://finance.yahoo.com/quote/%5EGSPC?p=^GSPC");

                //get the crumb keyword CrumbStore //
                var crumb = GetCrumb(respHtml);


                var req = new YahooRequest(YahooRequestType.Historical).
                    From(new DateTime(1970, 01, 01)).
                    Ticker("APPL").
                    To(DateTime.Today).
                    Interval(YahooRequestInterval.Daily).
                    SetCrumb(crumb).
                    BuildUrls();

                foreach (var ulrItem in req.Urls)
                {
                    var data = webcrawler.DownloadString(ulrItem.Url);
                }
            }
        }

        private string GetCrumb(string content)
        {
            var regex = new Regex("CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

            MatchCollection matches = regex.Matches(content);

            if (matches.Count > 0)
            {
                var crumb = matches[0].Groups["crumb"].Value;
                return crumb;
            }
            else
            {
                Debug.Print("Regex no match");
            }

            //prevent regex memory leak
            matches = null;
            return null;

        }
    }

    public class WebCrawler : WebClient
    {
        public WebCrawler()
        {
            CookieContainer = new CookieContainer();
        }

        public CookieContainer CookieContainer { get; private set; }
        public CookieCollection ResponseCookies { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = CookieContainer;
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request) as HttpWebResponse;
            if (response != null)
                if (ResponseCookies == null)
                {
                    ResponseCookies = response.Cookies;
                    CookieContainer.Add(request.RequestUri, ResponseCookies[0]);
                }
            return response;
        }
    }
}
