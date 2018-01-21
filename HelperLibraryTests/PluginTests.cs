using HelperLibrary.Interfaces;
using HelperLibrary.Util.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibraryTests
{
    [TestClass]
    public class PluginTests
    {
        [TestMethod]
        public void InvokeEZBDownloadPlugin()
        {
            var plugin = PluginHandler.InvokePlugin("EzbDownloadPlugin") as IDownloadPlugin;
            Assert.IsNotNull(plugin, "EzbDownloadPlugin konnte nicht aufgerufgen werden");
        }
        [TestMethod]
        public void InvokeMaxDrawdownPlugin()
        {
            var plugin = PluginHandler.InvokePlugin("MaxDrawdownPlugin") as IDownloadPlugin;
            Assert.IsNotNull(plugin, "MaxDrawdownPlugin konnte nicht aufgerufgen werden");
        }
        [TestMethod]
        public void InvokeYahooDownloadPlugin()
        {
            var plugin = PluginHandler.InvokePlugin("YahooDownloadPlugin") as IDownloadPlugin;
            Assert.IsNotNull(plugin, "YahooDownloadPlugin konnte nicht aufgerufgen werden");

        }
        [TestMethod]
        public void InvokeOnvistaDownloadPlugin()
        {
            var plugin = PluginHandler.InvokePlugin("OnVistaDownloadPlugin") as IDownloadPlugin;
            Assert.IsNotNull(plugin, "OnVistaDownloadPlugin konnte nicht aufgerufgen werden");
        }
    }
}
