using HelperLibrary.Collections;
using HelperLibrary.Database;
using HelperLibrary.Database.Models;
using HelperLibrary.Trading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;

namespace HelperLibraryTests.Trading.ScoringTests
{
    [TestClass]
    public class ScoringTests
    {
        private static PriceHistoryCollection _testSecurity;
        private static DateTime _testDateTime = new DateTime(2015, 01, 01);


        [ClassInitialize]
        public static void InitSQLConnection(TestContext ctx)
        {
            SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection());

            //Test Security
            var records = SQLCmd.Call().Procedure("getSinglePriceHistoryWithDateTime", 7, _testDateTime).QueryObjects<YahooDataRecord>();
            _testSecurity = new PriceHistoryCollection(records);
        }

        [ClassCleanup]
        public static void Dispose()
        {
            SQLCmd.Connection?.Dispose();
        }



        [TestMethod]
        public void TestScoringIsValid()
        {
            var handler = new ScoringProvider(null);
            var score = handler.GetScore(_testSecurity.SecurityId, _testDateTime);

            Assert.IsTrue(score != null);
            Assert.IsTrue(score.IsValid == false);
        }


        [TestMethod]
        public void TestScoringData()
        {
            var handler = new ScoringProvider(null);
            var score = handler.GetScore(_testSecurity.SecurityId, _testDateTime);

            if (!score.IsValid)
            {
                _testDateTime = _testDateTime.AddDays(+1);
                TestScoringData();
                return;
            }

            Assert.IsTrue(score.IsValid);
            return;
        }
    }
}
