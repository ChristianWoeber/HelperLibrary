using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary.Yahoo
{
    public class YahooRequest
    {
        private YahooRequestBuilder _builder;

        public YahooRequest(YahooRequestType type)
        {
            _builder = new YahooRequestBuilder(type);
        }

        public YahooRequestBuilder Tickers(params string[] tickers)
        {
            _builder.Tickers(tickers);
            return _builder;
        }

        public YahooRequestBuilder Tickers(ICollection<string> tickers)
        {
            _builder.Tickers(tickers);
            return _builder;
        }

        public YahooRequestBuilder From(DateTime dt)
        {
            _builder.From(dt);
            return _builder;
        }

        public YahooRequestBuilder To(DateTime? dt = null)
        {
            _builder.To(dt ?? DateTime.Today);
            return _builder;
        }

        public YahooRequestBuilder Interval(YahooRequestInterval? interval = null)
        {
            _builder.Interval(interval ?? YahooRequestInterval.Daily);
            return _builder;
        }

        public YahooRequestBuilder Build()
        {
            return _builder.BuildUrls();
        }
    }

    public class YahooRequestBuilder
    {
        private YahooRequestType _type;
        private StringBuilder _sb;
        public List<string> TickersCollection = new List<string>();
        public List<YahooUrlItem> Urls = new List<YahooUrlItem>();


        public YahooRequestBuilder(YahooRequestType type)
        {
            _type = type;
            _sb = new StringBuilder();

            switch (type)
            {
                case YahooRequestType.Single:
                    _sb.Append(Settings.Default.SingleBaseUrl);
                    break;
                case YahooRequestType.Historical:
                    _sb.Append(Settings.Default.HistoricalBaseUrl);
                    break;
            }
        }

        public YahooRequestBuilder From(DateTime dt)
        {
            if (_type == YahooRequestType.Single)
                throw new ArgumentException("Requests vom typen Single können keine historischen Daten abfragen");

            AppendToBase();
            AppendFrom(dt);

            return this;
        }

        private void AppendToBase()
        {
            // zur Basis Url hinzufügen => =s@&
            _sb.Append(YahooFieldsList.SYMBOL);
            _sb.Append("=");
            _sb.Append(YahooFieldsList.TICKER_PLACEHOLDER);
            _sb.Append(YahooFieldsList.AND);
        }

        private void AppendFrom(DateTime dt)
        {
            //Append month//
            _sb.Append($"{YahooFieldsList.START_MONTH}={dt.Month}");
            _sb.Append(YahooFieldsList.AND);

            //Append day//
            _sb.Append($"{YahooFieldsList.START_DAY}={dt.Day}");
            _sb.Append(YahooFieldsList.AND);

            //Append year//
            _sb.Append($"{YahooFieldsList.START_YEAR}={dt.Year}");
        }

        public YahooRequestBuilder Tickers(string[] tickers)
        {
            if (tickers.Length <= 0)
                return null;

            TickersCollection.AddRange(tickers);
            return this;
        }

        public YahooRequestBuilder Ticker(string ticker)
        {
            if (ticker.Length <= 0)
                return null;

            TickersCollection.Add(ticker);
            return this;
        }


        public YahooRequestBuilder Tickers(ICollection<string> tickers)
        {
            if (tickers.Count <= 0)
                return null;

            TickersCollection.AddRange(tickers);
            return this;
        }

        private void AppendTo(DateTime dt)
        {
            //Append month//
            _sb.Append($"{YahooFieldsList.END_MONTH}={dt.Month}");
            _sb.Append(YahooFieldsList.AND);

            //Append day//
            _sb.Append($"{YahooFieldsList.END_DAY}={dt.Day}");
            _sb.Append(YahooFieldsList.AND);

            //Append year//
            _sb.Append($"{YahooFieldsList.END_YEAR}={dt.Year}");
        }


        public YahooRequestBuilder To(DateTime? dt)
        {
            if (dt <= DateTime.MinValue)
                dt = DateTime.Today;

            AppendTo(dt.Value);
            return this;
        }

        public YahooRequestBuilder BuildUrls()
        {
            foreach (var ticker in TickersCollection)
            {
                var baseUrl = Settings.Default.SingleBaseUrl;
                if (_type == YahooRequestType.Single)
                {
                    baseUrl+="=";
                    baseUrl+=ticker;
                    ////&f= fields, n=name, o=open, p = previous close, s=symbol, d1=asof// 
                    baseUrl+="&f=nopsd1";
                    Urls.Add(new YahooUrlItem(baseUrl, ticker));
                }
                else
                {
                    var replacableUrl = _sb.ToString();
                    var url = replacableUrl.Replace("@", ticker);
                    Urls.Add(new YahooUrlItem(url, ticker));
                }
            }
            return this;
        }


        public YahooRequestBuilder Interval(YahooRequestInterval intervalType)
        {
            _sb.Append($"{YahooFieldsList.AND}{YahooFieldsList.INTERVAL}={intervalType.GetAttribute<YahooChar>().Name}");
            return this;
        }
    }

    public class YahooUrlItem : Tuple<string, string>
    {
        public string Url => Item1;
        public string Ticker => Item2;

        public YahooUrlItem(string url, string ticker) : base(url, ticker)
        {

        }
    }
}
