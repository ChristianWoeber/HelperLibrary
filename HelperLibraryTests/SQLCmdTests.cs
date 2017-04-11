using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperLibrary.Database;
using HelperLibrary.Database.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

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
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").
                QueryObjects<Security>().
                ToDictionary(x => x.ISIN + "_" + x.Ticker);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLIsNotNullTest()
        {
            var dbSecs = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").IsNotNull("ISIN").
               QueryObjects<Security>().
               ToDictionary(x => x.ISIN);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLOperatorsTest()
        {
            var dbSecs = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").
               IsNotNull("ISIN").
               Equal("INDEX_MEMBER_OF",1).
               QueryObjects<Security>().
               ToDictionary(x => x.ISIN);

            Assert.IsTrue(dbSecs != null && dbSecs.Count > 0, "Fehler bei der Funktion QueryObject");
        }

        [TestMethod]
        public void SQLInListTest()
        {
            var dbIndices = SQLCmd.Select("trading", "securities").
               Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").             
               Equal("SECURITY_TYPE", "index").
               QueryObjects<Security>().
               ToList();

            //var stocks = SQLCmd.Select("trading", "securities").
            //   Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").
            //   InList(dbIndices.Select(x=>x.SecurityId)).
            //   QueryObjects<Security>().


            Assert.IsTrue(dbIndices != null && dbIndices.Count > 0, "Fehler bei der Funktion QueryObject");
        }
    }
}
