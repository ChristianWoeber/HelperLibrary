using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperLibrary.Database;
using HelperLibrary.Database.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using HelperLibrary.Database.Interfaces;

namespace HelperLibraryTests
{
    [TestClass]
    public class SQLCmdTests
    {
        [ClassInitialize]
        public static void InitSQLConnection(TestContext ctx)
        {
            SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection());
        }

        [ClassCleanup]
        public static void Dispose()
        {
            SQLCmd.Connection?.Dispose();
        }

        [TestMethod]
        public void SQLUpdateTest()
        {
            var now = DateTime.Now;
            SQLCmd.Update(" trading_test", "indices").Values("NAME", "Test2", "DESCRIPTION", "Das ist ein Desciption TEst", "CHANGED", now, "ASSETCLASS_ID", 101).Equal("ID_", 1);
            SQLCmd.Execute();

            var dbNow = SQLCmd.Select(" trading_test", "indices").Fields("CHANGED").QuerySingle<DateTime>();
            Assert.IsTrue(now.Date == dbNow.Date, "Fehler beim Datum");
        }

   
        public void SQLDeleteTest()
        {          
            SQLCmd.Delete(" trading_test", "indices").Equal("ID_", 3);
            SQLCmd.Execute();         
            //Assert.IsTrue(now.Date == dbNow.Date, "Fehler beim Datum");
        }

        [TestMethod]
        public void SQLInsertDateTest()
        {
            var todays = DateTime.Today.Date;
            SQLCmd.Update(" trading_test", "indices").Values("NAME", "Test2", "DESCRIPTION", "Das ist ein Desciption TEst", "CHANGED", todays, "ASSETCLASS_ID", 101).Equal("ID_", 1);
            SQLCmd.Execute();

            var today = SQLCmd.Select(" trading_test", "indices").Fields("CHANGED").QuerySingle<DateTime>();
            Assert.IsTrue(todays == today, "Fehler beim Datum");
        }
        [TestMethod]
        public void SQLQueryObjectsTest()
        {
            var dbSecs = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF,COUNTRY").
                QueryObjects<Security>().
                ToDictionary(x => x.ISIN + "_" + x.Ticker);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLQueryObjectsYahooDataTest()
        {
            //ASOF, SECURITY_ID, CLOSE_PRICE, ADJUSTED_CLOSE_PRICE
            var sw = new Stopwatch();
            sw.Start();
            var dbSecs = SQLCmd.Select("trading", "yahoo_data").
               Fields("ASOF, SECURITY_ID, CLOSE_PRICE, ADJUSTED_CLOSE_PRICE").
                QueryObjects<YahooDataRecord>().ToList()
              ;
            sw.Stop();
            Trace.TraceInformation($"Total Seconds: {sw.Elapsed.TotalSeconds} | Total Minutes {sw.Elapsed.TotalMinutes}");

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLIsNotNullTest()
        {
            var dbSecs = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF,COUNTRY").IsNotNull("ISIN").
               QueryObjects<Security>().
               ToDictionary(x => x.ISIN);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]

        public void SQLGetIndicesCatalog()
        {
            var data = DataBaseQueryHelper.GetIndicesCatalog();
            Assert.IsNotNull(data);

        }

        [TestMethod]
        public void SQLOperatorsTest()
        {
            var dbSecs = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").
               IsNotNull("ISIN").
               Equal("INDEX_MEMBER_OF", 1).
               QueryObjects<Security>().
               ToDictionary(x => x.ISIN);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLInListTest()
        {
            var secIds = SQLCmd.Select("Trading", "validations").Fields("SECURITY_ID").Equal("VALIDATION_TYPE", 1).QueryList<object>();
            var tickers = SQLCmd.Select("Trading", "Securities").Fields("TICKER").InList("SECURITY_ID", secIds).QueryList<string>();
            Assert.IsTrue(tickers != null && tickers.Count > 0, "Fehler bei der Funktion QueryObject");
        }


        [TestMethod]
        public void SQLQueryKeySet()
        {
            var keys = SQLCmd.Select("Trading", "yahoo_data").Fields("ASOF", "SECURITY_ID").QueryKeySet(); 
            Assert.IsTrue(keys != null && keys.Count > 0, "Fehler bei der Funktion SQLQueryKeySet");
        }


        [TestMethod]
        public void SQLGetValidTickers()
        {
            var tickers = DataBaseQueryHelper.GetTickers(true);
            Assert.IsTrue(tickers != null && tickers.Count > 0, "Fehler bei der Funktion SQLQueryKeySet");
        }

        [TestMethod]
        public void SQLCallProceduresTest()
        {
            var secid = 1;
            var PriceHistory = new List<IDataRecord>(DataBaseQueryHelper.GetSinglePriceHistory(secid));

           Assert.IsTrue(PriceHistory != null && PriceHistory.Count > 0, "Fehler in der Call Procedute Test Methode");

        }

        [TestMethod]
        public void SQLGetValidFirstAndLastItems()
        {
            var deleteIds = SQLCmd.Select("Trading", "yahoo_data").Fields("SECURITY_ID").Less("ASOF", new DateTime(1960,01,01)).QueryList<int>();


            foreach (var id in deleteIds)
            {
                SQLCmd.Delete("Trading", "yahoo_data").Equal("SECURITY_ID", id);
                SQLCmd.Execute();
            }

            var secIds = SQLCmd.Select("Trading", "validations").Fields("SECURITY_ID").Equal("VALIDATION_TYPE", 1, "IS_VALID", 1).QueryList<object>();
            var dic = DataBaseQueryHelper.SQLGetAllFirstAndLastItems(secIds);

            Assert.IsTrue(dic != null && dic.Count > 0, "Fehler in der SQLGetValidFirstAndLastItems Test Methode");

        }
    }
}
