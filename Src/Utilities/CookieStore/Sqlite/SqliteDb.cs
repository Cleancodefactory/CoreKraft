using Microsoft.Data.Sqlite;
using System.Data;

namespace Ccf.Ck.Utilities.CookieTicketStore.Sqlite
{
    internal class SqliteDb
    {
        const string CONNECTIONSTRING = "Data Source=cookieCache.sqlite;";
        static SqliteConnection _Connection;

        static SqliteDb()
        {
            EnsureDbExistsAndReady();
        }

        private static void EnsureDbExistsAndReady()
        {
            _Connection = new SqliteConnection(CONNECTIONSTRING);
            _Connection.Open();
            string createIfNotExist = "CREATE TABLE IF NOT EXISTS Cookies ([Key] TEXT PRIMARY KEY NOT NULL, Value BLOB);";
            using (SqliteCommand cmd = new SqliteCommand(createIfNotExist, _Connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        internal void Remove(string key)
        {
            SqliteParameter dataParameter = new SqliteParameter("Key", DbType.String) { Value = key };
            using (SqliteCommand cmd = new SqliteCommand("DELETE FROM Cookies WHERE [Key] = @Key", _Connection))
            {
                cmd.Parameters.Add(dataParameter);
                cmd.ExecuteNonQuery();
            }
        }

        internal T Get<T>(string key)
        {
            SqliteParameter keyParameter = new SqliteParameter("Key", DbType.String) { Value = key };
            using (SqliteCommand cmd = new SqliteCommand("SELECT Value FROM Cookies WHERE [Key]=@Key", _Connection))
            {
                cmd.Parameters.Add(keyParameter);
                using (SqliteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        return (T)rdr.GetValue(0);
                    }
                }
            }
            return default;
        }

        internal void Set(string key, byte[] bytes)
        {
            SqliteParameter keyParameter = new SqliteParameter("Key", DbType.String) { Value = key };
            SqliteParameter valueParameter = new SqliteParameter("Value", DbType.Binary) { Value = bytes };
            using (SqliteCommand cmd = new SqliteCommand("INSERT OR REPLACE INTO Cookies ([Key],Value) VALUES (@Key,@Value)", _Connection))
            {
                cmd.Parameters.Add(keyParameter);
                cmd.Parameters.Add(valueParameter);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
