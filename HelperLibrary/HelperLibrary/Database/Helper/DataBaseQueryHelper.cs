using System;
using System.Collections.Generic;
using System.Linq;
using HelperLibrary.Database.Models;
using MySql.Data.MySqlClient;
using HelperLibrary.Database.Enums;

namespace HelperLibrary.Database
{
    public class DataBaseQueryHelper
    {
        public static Dictionary<int, Security> GetAllSecurities()
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                return SQLCmd.Select("trading", "securities").
                    Fields("SECURITY_ID, NAME, TICKER, DESCRIPTION, ACTIVE, SECURITY_TYPE, CURRENCY, ISIN, SECTOR,INDEX_MEMBER_OF,COUNTRY").
                    QueryObjects<Security>().
                    ToDictionary(x => x.SecurityId);
            }
        }

        private static HashSet<string> _dbKeys;

        public static List<Validation> GetValidations(int securityId)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                return SQLCmd.Select("trading", "validations").
                    Fields("ID_,SECURITY_ID,VALIDATION_TYPE,LAST_VALIDATION,IS_VALID").Equal("SECURITY_ID", securityId).
                    QueryObjects<Validation>().ToList();
            }
        }

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

        public static void StoreOrUpdateValidationResult(int securityId, bool validationResult, ValidationType type)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                var keys = SQLCmd.Select("trading", "validations").Fields("SECURITY_ID,VALIDATION_TYPE").QueryKeySet(typeof(int), typeof(int));

                if (keys != null && keys.Contains($"{securityId}_{type.ToString()}"))
                {
                    SQLCmd.Update("trading", "validations").Values(
                          "SECURITY_ID", securityId,
                          "VALIDATION_TYPE", (int)type,
                          "IS_VALID", validationResult == true ? 1 : 0,
                          "LAST_VALIDATION", DateTime.Now).
                          Equal("SECURITY_ID", securityId, "VALIDATION_TYPE", (int)type);
                }
                else
                {
                    SQLCmd.Insert("trading", "validations").Values(
                          "SECURITY_ID", securityId,
                          "VALIDATION_TYPE", (int)type,
                          "IS_VALID", validationResult == true ? 1 : 0,
                          "LAST_VALIDATION", DateTime.Now);

                }
                SQLCmd.Execute();
            }
        }

        public static void SaveSecurity(Security sec)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
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
                            "COUNTRY", sec.Country).
                            Equal("SECURITY_ID", sec.SecurityId);
                SQLCmd.Execute();
            }
        }

        public static Dictionary<int, string> GetValidationTypeTable()
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                return SQLCmd.Select("Trading", "validation_types").Fields("ID_", "TYPE").QueryDictionary<int, string>();
            }
        }

        public static Dictionary<int, List<Validation>> GetValidationTable()
        {
            var dic = new Dictionary<int, List<Validation>>();

            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                foreach (var item in SQLCmd.Select("Trading", "validations").Fields("ID_", "SECURITY_ID", "VALIDATION_TYPE", "LAST_VALIDATION", "IS_VALID").QueryObjects<Validation>())
                {
                    if (!dic.ContainsKey(item.SecurityId))
                        dic.Add(item.SecurityId, new List<Validation>());

                    dic[item.SecurityId].Add(item);
                }
            }
            return dic;
        }

        public static Dictionary<int, List<YahooDataRecord>> GetDataTable()
        {
            var dic = new Dictionary<int, List<YahooDataRecord>>();
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                var cmd = SQLCmd.Select("Trading", "yahoo_data").Fields("ASOF", "SECURITY_ID", "CLOSE_PRICE", "ADJUSTED_CLOSE_PRICE");

                foreach (var item in cmd.QueryObjects<YahooDataRecord>())
                {
                    if (!dic.ContainsKey(item.SecurityId))
                        dic.Add(item.SecurityId, new List<YahooDataRecord>());

                    dic[item.SecurityId].Add(item);
                }
            }
            return dic;
        }

        public static int SelectIndexId(string keyword, string title)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                var dbInt = SQLCmd.Select("Trading", "Securities").Fields("SECURITY_ID").Equal("NAME", keyword).QuerySingle<int>();
                return dbInt == 0 ? SQLCmd.Select("Trading", "Securities").Fields("SECURITY_ID").Equal("NAME", title).QuerySingle<int>() : dbInt;
            }
        }

        public static List<string> GetTickers(bool onlyValidTickers = false)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                if (onlyValidTickers)
                {
                    var secIds = SQLCmd.Select("Trading", "validations").Fields("SECURITY_ID").Equal("VALIDATION_TYPE", 1).QueryList<object>();
                    return SQLCmd.Select("Trading", "Securities").Fields("TICKER").InList("SECURITY_ID", secIds).QueryList<string>();

                }
                return SQLCmd.Select("Trading", "Securities").Fields("TICKER").QueryList<string>();

            }
        }

        public static int GetSecIdFromTicker(string ticker)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                return SQLCmd.Select("Trading", "Securities").Fields("SECURITY_ID").Equal("TICKER", ticker).QuerySingle<int>();
            }
        }

        public static void InsertOrUpdateSecurityData(YahooDataRecord model)
        {
            using (SQLCmd.Connection = DataBaseFactory.Create(new MySqlConnection()))
            {
                // Lazy Loading //
                if (_dbKeys == null)
                    _dbKeys = SQLCmd.Select("Trading", "yahoo_data").Fields("ASOF", "SECURITY_ID").QueryKeySet();

                var inputKey = $"{model.Asof.Date}_{model.SecurityId}";

                if (_dbKeys.Contains(inputKey))
                {
                    SQLCmd.Update("Trading", "yahoo_data").
                        Values("CLOSE_PRICE", model.ClosePrice,
                               "ADJUSTED_CLOSE_PRICE", model.AdjClosePrice).
                        Equal("ASOF", model.Asof,
                              "SECURITY_ID", model.SecurityId);
                }
                else
                {
                    SQLCmd.Insert("Trading", "yahoo_data").
                           Values(
                           "ASOF", model.Asof,
                           "CLOSE_PRICE", model.ClosePrice,
                           "ADJUSTED_CLOSE_PRICE", model.AdjClosePrice,
                           "SECURITY_ID", model.SecurityId);
                }
                SQLCmd.Execute();
            }
        }
    }
}