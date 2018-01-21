using HelperLibrary.Collections;
using HelperLibrary.Database;
using HelperLibrary.Database.Models;
using HelperLibrary.Interfaces;
using HelperLibrary.Parsing;
using HelperLibrary.Trading;
using HelperLibrary.Trading.PortfolioManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace HelperLibraryTests
{
    [TestClass]
    public class PortfolioManagerTests
    {

        private Dictionary<int, IPriceHistoryCollection> _dicDbData = new Dictionary<int, IPriceHistoryCollection>();

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
        public void GetCandidatesForAGivenDateTest()
        {
            var securities = SQLCmd.Select("TRADING", "securities").Fields("SECURITY_ID").InList("INDEX_MEMBER_OF", new object[] { 1, 5, 368 }).QueryList<object>();
            var testsecIds = SQLCmd.Select("Trading", "validations").Fields("SECURITY_ID").InList("SECURITY_ID", securities).Equal("IS_VALID", 1).QueryList<int>();
            //var cmd = SQLCmd.Select("Trading", "yahoo_data").Fields("ASOF", "SECURITY_ID", "CLOSE_PRICE", "ADJUSTED_CLOSE_PRICE").InList("SECURITY_ID", testsecIds);

            var startTestDateTime = new DateTime(2015, 01, 01);

            //erstellen des Portfoilio Managers
            var portfolioManager = new PortfolioManager();


            foreach (var id in testsecIds)
            {
                var priceHistory = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(id, startTestDateTime));

                if (priceHistory.Count <= 250)
                    continue;

                Trace.TraceInformation("Price history für SecId: " + id + " geladen. Count: " + priceHistory.Count);

                if (!_dicDbData.ContainsKey(id))
                    _dicDbData.Add(id, priceHistory);
            }

            //einen BacktestHandler erstellen
            var backtestHandler = new BacktestHandler(new ScoringProvider(_dicDbData));

            //die Candidatenliste zrückgeben lassen
            var candidates = backtestHandler.DoBacktest(startTestDateTime);

            Assert.IsTrue(candidates.Count() > 0);

        }

        [TestMethod]
        public void GetCurrentPortfolioTest()
        {
            var portfolio = SimpleTextParser.GetListOfTypeFromFilePath<TransactionItem>
                (@"C:\Users\Chris\Source\Repos\HelperLibrary2\HelperLibraryTests\Resources\GetCurrentPortfolioTest.csv");

            var pm = new PortfolioManager();
            pm.PassInTestTransactions(portfolio);

            var currentItems = pm.CurrentPortfolio;

            Assert.IsTrue(pm.CurrentPortfolio.Count() == 10, $"Fehler: der aktuelle count des Portfolios beträgt {pm.CurrentPortfolio.Count()}, anstelle von 10");
        }

        [TestMethod]
        public void ApplyPortfolioRulesTest()
        {
            var portfolio = SimpleTextParser.GetListOfTypeFromFilePath<TransactionItem>
             (@"C:\Users\Chris\Source\Repos\HelperLibrary2\HelperLibraryTests\Resources\GetCurrentPortfolioTest.csv");           
        
            var pm = new PortfolioManager();

            pm.PassInTestTransactions(portfolio);

            var currentItems = pm.CurrentPortfolio;

            var candidates = GetCandidates(new DateTime(2015, 01, 01));

            pm.RegisterScoringProvider(_testScoringProvider);

            pm.PassInCandidates(candidates);

            pm.ApplyPortfolioRules();

        }

        private IScoringProvider _testScoringProvider;

        private IEnumerable<TradingCandidate> GetCandidates(DateTime start)
        {
            SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection());
            var securities = SQLCmd.Select("TRADING", "securities").Fields("SECURITY_ID").InList("INDEX_MEMBER_OF", new object[] { 1, 5, 368 }).QueryList<object>();
            var testsecIds = SQLCmd.Select("Trading", "validations").Fields("SECURITY_ID").InList("SECURITY_ID", securities).Equal("IS_VALID", 1).QueryList<int>();
            //var cmd = SQLCmd.Select("Trading", "yahoo_data").Fields("ASOF", "SECURITY_ID", "CLOSE_PRICE", "ADJUSTED_CLOSE_PRICE").InList("SECURITY_ID", testsecIds);


            foreach (var id in testsecIds)
            {
                var priceHistory = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(id, start));

                if (priceHistory.Count <= 250)
                    continue;

                Trace.TraceInformation("Price history für SecId: " + id + " geladen. Count: " + priceHistory.Count);

                if (!_dicDbData.ContainsKey(id))
                    _dicDbData.Add(id, priceHistory);
            }

            _testScoringProvider = new ScoringProvider(_dicDbData);

            //einen BacktestHandler erstellen
            var backtestHandler = new BacktestHandler(_testScoringProvider);

            //die Candidatenliste zrückgeben lassen
            return backtestHandler.DoBacktest(start);
        }
    }
}
