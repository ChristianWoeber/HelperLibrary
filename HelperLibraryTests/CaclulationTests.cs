using HelperLibrary.Calculations;
using HelperLibrary.Collections;
using HelperLibrary.Database;
using HelperLibrary.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace HelperLibraryTests
{

    [TestClass]
    public class CaclulationTests
    {
        private Dictionary<int, PriceHistoryCollection> _dicDbData = new Dictionary<int, PriceHistoryCollection>();

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
        public void ReturnCalculationTest()
        {
            //Get Stock Adidas
            var priceHistory = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var calculationContext = new CalculationContext(priceHistory);


            var returnPerAnnum = calculationContext.GetAverageReturn(new DateTime(2010, 01, 01));
            var returnAbsolute = calculationContext.GetAbsoluteReturn(new DateTime(2010, 01, 01));

            var returnPerAnnumNonAdj = calculationContext.GetAverageReturn(new DateTime(2010, 01, 01), null, CaclulationOption.NonAdjusted);
            var returnAbsoluteNonAdj = calculationContext.GetAbsoluteReturn(new DateTime(2010, 01, 01), null, CaclulationOption.NonAdjusted);

            Trace.TraceInformation($"Adjusted prices return p.a. {returnPerAnnum.ToString("P")}, return absolute: {returnAbsolute.ToString("P")}");
            Trace.TraceInformation($"Non adjusted prices return p.a. {returnPerAnnumNonAdj.ToString("P")}, return absolute: {returnAbsoluteNonAdj.ToString("P")}");
        }

        [TestMethod]
        public void MaxDrawDownCalculationTest()
        {
            //Get Stock Adidas
            var priceHistory = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var calculationContext = new CalculationContext(priceHistory);


            var maxDraw = calculationContext.GetMaximumDrawdownItem(new DateTime(2010, 01, 01));
            var maxDrawNonAdj = calculationContext.GetMaximumDrawdownItem(new DateTime(2010, 01, 01), null, CaclulationOption.NonAdjusted);


            Trace.TraceInformation($"MaxDrawDown: {maxDraw.Drawdown.ToString("P")}, Beginn: {maxDraw.Start.Asof.ToShortDateString()}, End {maxDraw.End.Asof.ToShortDateString()}");
            Trace.TraceInformation($"MaxDrawDown Non adjusted: {maxDrawNonAdj.Drawdown.ToString("P")}, Beginn: {maxDrawNonAdj.Start.Asof.ToShortDateString()}, End {maxDrawNonAdj.End.Asof.ToShortDateString()}");

            Assert.IsTrue(maxDraw.Start.Asof.Date == new DateTime(2014, 01, 22), "Start Datum des Maxdradown stimmt nicht");
            Assert.IsTrue(maxDraw.End.Asof.Date == new DateTime(2014, 10, 16), "End Datum des Maxdradown stimmt nicht");
            Assert.IsTrue(Math.Round(maxDraw.Drawdown, 2) == (decimal)-0.41, "Der Wert des Maxdradown stimmt nicht");


        }

        [TestMethod]
        public void VolatilityCalculationTest()
        {
            //Get Stock Adidas
            var priceHistory = new PriceHistoryCollection(DataBaseQueryHelper.GetSinglePriceHistory(7));
            var calculationContext = new CalculationContext(priceHistory);
            var volatilityMonthly = calculationContext.GetVolatility(new DateTime(2010, 01, 01), new DateTime(2017, 09, 23));

            Trace.TraceInformation($"Adjusted prices Vola on monthly basis p.a. {volatilityMonthly.ToString("P")}");
            Assert.IsTrue(volatilityMonthly < (decimal)0.245 && volatilityMonthly >= (decimal)0.23);
        }
    }
   
}