using Microsoft.Data.Sqlite;
using System.Data;

namespace Ccf.Ck.Utilities.CookieTicketStore.Sqlite
{
    internal class SqliteDb
    {
        const string CONNECTIONSTRING = "Data Source=cookieCache.sqlite;";
        static readonly object _InitLock = new object();
        static bool _IsDbInitialized;

        static SqliteDb()
        {
            EnsureDbExistsAndReady();
        }

        private static void EnsureDbExistsAndReady()
        {
            if (_IsDbInitialized)
            {
                return;
            }

            lock (_InitLock)
            {
                if (_IsDbInitialized)
                {
                    return;
                }

                using (SqliteConnection connection = new SqliteConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    string createIfNotExist = "CREATE TABLE IF NOT EXISTS Cookies ([Key] TEXT PRIMARY KEY NOT NULL, Value BLOB);";
                    using (SqliteCommand cmd = new SqliteCommand(createIfNotExist, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                _IsDbInitialized = true;
            }
        }

        internal void Remove(string key)
        {
            EnsureDbExistsAndReady();
            SqliteParameter dataParameter = new SqliteParameter("Key", DbType.String) { Value = key };
            using (SqliteConnection connection = new SqliteConnection(CONNECTIONSTRING))
            {
                connection.Open();
                using (SqliteCommand cmd = new SqliteCommand("DELETE FROM Cookies WHERE [Key] = @Key", connection))
                {
                    cmd.Parameters.Add(dataParameter);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal T Get<T>(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                EnsureDbExistsAndReady();
                SqliteParameter keyParameter = new SqliteParameter("Key", DbType.String) { Value = key };
                using (SqliteConnection connection = new SqliteConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqliteCommand cmd = new SqliteCommand("SELECT Value FROM Cookies WHERE [Key]=@Key", connection))
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
                }
            }
            return default;
        }

        internal void Set(string key, byte[] bytes)
        {
            EnsureDbExistsAndReady();
            SqliteParameter keyParameter = new SqliteParameter("Key", DbType.String) { Value = key };
            SqliteParameter valueParameter = new SqliteParameter("Value", DbType.Binary) { Value = bytes };
            using (SqliteConnection connection = new SqliteConnection(CONNECTIONSTRING))
            {
                connection.Open();
                using (SqliteCommand cmd = new SqliteCommand("INSERT OR REPLACE INTO Cookies ([Key],Value) VALUES (@Key,@Value)", connection))
                {
                    cmd.Parameters.Add(keyParameter);
                    cmd.Parameters.Add(valueParameter);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
