using HelperLibrary.Calculations;
using HelperLibrary.Collections;
using HelperLibrary.Database;
using HelperLibrary.Database.Interfaces;
using HelperLibrary.Database.Models;
using HelperLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibraryTests
{
    // -----------------------Doku und Anmerkungen --------------------------------------//
    //Falls SQL Connection vorhanden kann man auch mit der untenstehenden Query direkt Daten aus der TRADING Datenbank testen
    //var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));



    /// <summary>
    /// Test Klasse
    /// </summary>
    public class TestQuote : IDataRecord
    {
        public int SecurityId { get; set; }
        public decimal Price { get; set; }
        public DateTime Asof { get; set; }
        public decimal AdjustedPrice { get; set; }
    }

    [TestClass]
    public class PriceHistroyTests
    {
        /// <summary>
        /// TestCollection
        /// </summary>
        public static List<IDataRecord> TestCollection { get; } = new List<IDataRecord>(InitTestHistory());


        /// <summary>
        /// Methode um Testdaten zu generieren
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IDataRecord> InitTestHistory(int min = 89, int max = 111, int testSecurityId = 10)
        {
            var testId = testSecurityId;
            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                var testDate = DateTime.Today.AddDays(-i);
                var testPrice = rand.Next(min, max);
                var testAdjusted = testPrice * (decimal)0.98;

                yield return new TestQuote
                {
                    SecurityId = testId,
                    Price = testPrice,
                    AdjustedPrice = testAdjusted,
                    Asof = testDate
                };
            }
        }

        [TestMethod]
        public void PriceHistoryCountTest()
        {
            var priceHistroy = new PriceHistoryCollection(TestCollection);
            Assert.IsTrue(priceHistroy.Count > 50, "Achtung es konnten keine Daten in die PriceHistory gelanden werden");

        }

        [TestMethod]
        public void PriceHistoryFirstItemTest()
        {
            var priceHistroy = new PriceHistoryCollection(TestCollection);
            var firstItem = priceHistroy.FirstItem;

            Assert.IsTrue(firstItem != null && firstItem.Asof >= DateTime.Today.AddDays(-15), "Achtung First Item ist null, oder älter als 15 Tage");

        }


        [TestMethod]
        public void PriceHistoryLastItemTest()
        {
            var priceHistroy = new PriceHistoryCollection(TestCollection);
            var lastItem = priceHistroy.LastItem;

            Assert.IsTrue(lastItem != null, "Achtung last Item ist null");

        }

        [TestMethod]
        public void PriceHistoryGetTest()
        {
            var priceHistroy = new PriceHistoryCollection(TestCollection);
            var item = priceHistroy.Get(DateTime.Today.AddDays(-25));

            Trace.TraceInformation("item asof :" + item?.Asof.ToShortDateString());

            Assert.IsTrue(item != null, "Achtung die Methode Get konnte kein item zurückgeben");

        }

        [TestMethod]
        public void PriceHistoryRangeTest()
        {
            var priceHistroy = new PriceHistoryCollection(TestCollection);
            var range = priceHistroy.Range(null, DateTime.Today.AddDays(-25));

            Trace.TraceInformation("first item in Range asof :" + range.FirstOrDefault()?.Asof.ToShortDateString());
            Trace.TraceInformation("last item in Range asof :" + range.LastOrDefault()?.Asof.ToShortDateString());

            Assert.IsTrue(range != null && range.Count() > 5, "Achtung die Methode Range konnte keine order zu wenige items zurückgeben");

        }

        [TestMethod]
        public void PriceHistoryCalculationContextTest()
        {

            var priceHistroy = new PriceHistoryCollection(TestCollection);
            var priceHistroy2 = new PriceHistoryCollection(InitTestHistory(95, 121, 11));
            var priceHistroy3 = new PriceHistoryCollection(InitTestHistory(99, 131, 12));


            Assert.IsTrue(priceHistroy.Calc.EnumDailyReturns() != null && priceHistroy.Calc.EnumDailyReturns().Count() == 99);
            Assert.IsTrue(priceHistroy2.Calc.EnumDailyReturns() != null && priceHistroy2.Calc.EnumDailyReturns().Count() == 99);
            Assert.IsTrue(priceHistroy3.Calc.EnumDailyReturns() != null && priceHistroy2.Calc.EnumDailyReturns().Count() == 99);

        }
    }
}
