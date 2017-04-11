using System;
using System.Collections.Generic;
using System.Linq;
using HelperLibrary.Database.Models;
using MySql.Data.MySqlClient;

namespace HelperLibrary.Database
{
    public class DataBaseQueryHelper
    {
        public static void InsertOrUpdateSecurities(IDictionary<string, Security> securities)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                var dicSecs = SQLCmd.Select("trading", "securities").
                    Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF").
                    IsNotNull("ISIN").
                    QueryObjects<Security>().
                    ToDictionary(x => x.ISIN);

                foreach (var sec in securities.Values)
                {               
                    if (dicSecs.ContainsKey(sec.ISIN))
                    {
                        SQLCmd.Update("trading", "securities").Values(
                            "NAME", sec.Name,
                            "TICKER", sec.Ticker,
                            "DESCRIPTION", sec.Description,
                            "ACTIVE", sec.Active,
                            "SECURITY_TYPE", sec.SecurityType,
                            "CURRENCY", sec.Ccy,
                            "ISIN", sec.ISIN,
                            "SECTOR", sec.Sector,
                            "INDEX_MEMBER_OF", sec.IndexMemberOf,
                            "COUNTRY", sec.Country).Equal("ISIN", sec.ISIN);
                        SQLCmd.Execute();
                    }
                    else
                    {
                        SQLCmd.Insert("trading", "securities").Values(
                            "NAME", sec.Name,
                            "TICKER", sec.Ticker,
                            "DESCRIPTION", sec.Description,
                            "ACTIVE", sec.Active,
                            "SECURITY_TYPE", sec.SecurityType,
                            "CURRENCY", sec.Ccy,
                            "ISIN", sec.ISIN,
                            "SECTOR", sec.Sector,
                            "INDEX_MEMBER_OF", sec.IndexMemberOf,
                            "COUNTRY", sec.Country);
                        SQLCmd.Execute();
                    }
                }
            }
        }

        public static int SelectIndexId(string keyword, string title)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                var dbInt = SQLCmd.Select("Trading", "Securities").Fields("SECURITY_ID").Equal("NAME", keyword).QuerySingle<int>();
                return dbInt == 0 ? SQLCmd.Select("Trading", "Securities").Fields("SECURITY_ID").Equal("NAME", title).QuerySingle<int>() : dbInt;
            }
        }
    }

}