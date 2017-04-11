using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HelperLibrary.Database
{
    public class SQLCmd
    {
        public static DbConnection Connection { get; set; }
        private static string _db;
        private static string _table;
        private static MySQLCommandBuilder _builder;
        private SQLCommandTypes cmdTpye;


        public SQLCmd(DbConnection connection, SQLCommandTypes cmdtype)
        {
            Connection = connection;
            cmdTpye = cmdtype;
            _builder = new MySQLCommandBuilder(cmdTpye, _db, _table);
        }

        public SQLCmd IsNotNull(string field)
        {
            _builder.IsNotNull(field);
            return this;
        }

        public static SQLCmd Select(string database, string table)
        {
            CheckConnection();
            _db = database;
            _table = table;
            var cmd = new SQLCmd(Connection, SQLCommandTypes.Select);
            return cmd;
        }      

        public SQLCmd Equal(params object[] values)
        {
            _builder.Equal(values);
            return this;
        }

        public SQLCmd Fields(params string[] fields)
        {
            _builder.Fields(fields);
            return this;
        }

        public static IDataReader Query(SQLCmd sqlCmd)
        {
            return DbTools.CreateQuery(sqlCmd, Connection, _builder);
        }

        public IEnumerable<T> QueryObjects<T>()
        {
            using (var rd = Query(this))
            {
                while (rd.Read())
                    yield return ObjectMapper<T>.Create(rd);
            }
        }
        public T QuerySingle<T>()
        {
            using (var rd = Query(this))
            {
                while (rd.Read())
                {
                    var item = (T)rd[0];
                    return item;
                }
            }
            return default(T);
        }

        public HashSet<T> QueryHashSet<T>()
        {
            using (var rd = Query(this))
            {
                var tmp = new HashSet<T>();
                while (rd.Read())
                {

                    var item = (T)rd[0];
                    tmp.Add(item);
                }
                return tmp.Count > 0 ? tmp : null;
            }
        }

        public List<T> QueryList<T>()
        {
            using (var rd = Query(this))
            {
                var tmp = new List<T>();
                while (rd.Read())
                {

                    var item = (T)rd[0];
                    tmp.Add(item);
                }
                return tmp.Count > 0 ? tmp : null;
            }
        }

        public Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>()
        {
            using (var rd = Query(this))
            {
                var tmp = new Dictionary<TKey, TValue>();
                while (rd.Read())
                {

                    var key = (TKey)rd[0];
                    var value = (TValue)rd[1];
                    if (!tmp.ContainsKey(key))
                        tmp.Add(key, value);
                }
                return tmp.Count > 0 ? tmp : null;
            }
        }
        public static SQLCmd Update(string database, string table)
        {
            CheckConnection();
            _db = database;
            _table = table;
            var cmd = new SQLCmd(Connection, SQLCommandTypes.Update);
            return cmd;
        }

        public static SQLCmd Insert(string database, string table)
        {
            CheckConnection();
            _db = database;
            _table = table;
            var cmd = new SQLCmd(Connection, SQLCommandTypes.Insert);
            return cmd;
        }

        //public static SQLCmd InList(IEnumerable<object> enumerable)
        //{
        //   //TODO:
        //}

        public SQLCmd Values(params object[] values)
        {
            if (values.Length % 2 != 0)
                throw new ArgumentException("Es wird ein KeyValue-Pair erwartet");
            for (int i = 0; i < values.Length; i += 2)
            {
                if (!(values[i] is string))
                    throw new ArgumentException("Der FieldnameWert muss vom Typ String sein");
            }
            if (cmdTpye == SQLCommandTypes.Update)
            {
                _builder.CreateValueTypesCmd(SQLValueTypes.UpdateValues, values);
            }
            else
            {
                _builder.CreateValueTypesCmd(SQLValueTypes.Values, values);
            }
            return this;
        }

        private static void CheckConnection()
        {
            if (Connection == null)
                throw new Exception("Achtung noch keine Datenbankverbindung hergestellt");
            else if (Connection.State != ConnectionState.Open)
                throw new Exception("Achtung Connection ist nicht offen");
        }

        public static void Execute()
        {
            DbTools.Exec(Connection, _builder);
        }
    }
}