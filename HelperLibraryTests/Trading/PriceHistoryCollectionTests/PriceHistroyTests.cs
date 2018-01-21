using HelperLibrary.Collections;
using HelperLibrary.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibraryTests
{
    [TestClass]
    public class PriceHistroyTests
    {
        [TestMethod]
        public void PriceHistoryCountTest()
        {
            var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));

            Assert.IsTrue(priceHistroy.Count > 100, "Achtung es konnten keine Daten in die PriceHistory gelanden werden");

        }

        [TestMethod]
        public void PriceHistoryFirstItemTest()
        {
            var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var firstItem = priceHistroy.FirstItem;

            Assert.IsTrue(firstItem != null && firstItem.Asof >= DateTime.Today.AddDays(-15), "Achtung First Item ist null, oder älter als 15 Tage");

        }


        [TestMethod]
        public void PriceHistoryLastItemTest()
        {
            var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var lastItem = priceHistroy.LastItem;

            Assert.IsTrue(lastItem != null, "Achtung last Item ist null");

        }

        [TestMethod]
        public void PriceHistoryGetTest()
        {
            var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var item = priceHistroy.Get(new DateTime(2017, 07, 30));

            Trace.TraceInformation("item asof :" + item?.Asof.ToShortDateString());

            Assert.IsTrue(item != null, "Achtung die Methode Get konnte kein item zurückgeben");

        }

        [TestMethod]
        public void PriceHistoryRangeTest()
        {
            var priceHistroy = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var range = priceHistroy.Range(null,new DateTime(2015,01,01));

            Trace.TraceInformation("first item in Range asof :" + range.FirstOrDefault()?.Asof.ToShortDateString());
            Trace.TraceInformation("last item in Range asof :" + range.LastOrDefault()?.Asof.ToShortDateString());

            Assert.IsTrue(range != null && range.Count() > 5, "Achtung die Methode Range konnte keine order zu wenige items zurückgeben");

        }
    }
}
