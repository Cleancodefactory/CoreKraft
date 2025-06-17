using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace Ccf.Ck.Utilities.GlobalAccessor.Sqlite
{
    internal class SqliteDb
    {
        const string CONNECTIONSTRING = "Data Source=SystemOperations.sqlite;";
        static SqliteConnection _Connection;

        static SqliteDb()
        {
            _Connection = new SqliteConnection(CONNECTIONSTRING);
            _Connection.Open();
        }

        private string GetTableName(Type type)
        {
            // Can add table name mapping logic here if needed
            return type.Name;
        }

        private void EnsureTableExists(string tableName)
        {
            string sql = $"CREATE TABLE IF NOT EXISTS [{tableName}] ([Key] TEXT PRIMARY KEY NOT NULL, [Value] TEXT)";
            using (SqliteCommand cmd = new SqliteCommand(sql, _Connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        internal void Set<T>(string key, T value)
        {
            string tableName = GetTableName(typeof(T));
            EnsureTableExists(tableName);

            string json = JsonSerializer.Serialize(value);
            SqliteParameter keyParam = new SqliteParameter("@Key", DbType.String) { Value = key };
            SqliteParameter valParam = new SqliteParameter("@Value", DbType.String) { Value = json };

            string sql = $"INSERT OR REPLACE INTO [{tableName}] ([Key], [Value]) VALUES (@Key, @Value)";
            using (SqliteCommand cmd = new SqliteCommand(sql, _Connection))
            {
                cmd.Parameters.Add(keyParam);
                cmd.Parameters.Add(valParam);
                cmd.ExecuteNonQuery();
            }
        }

        internal T Get<T>(string key)
        {
            string tableName = GetTableName(typeof(T));
            EnsureTableExists(tableName);

            SqliteParameter keyParam = new SqliteParameter("@Key", DbType.String) { Value = key };
            string sql = $"SELECT [Value] FROM [{tableName}] WHERE [Key] = @Key";

            using (SqliteCommand cmd = new SqliteCommand(sql, _Connection))
            {
                cmd.Parameters.Add(keyParam);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string json = reader.GetString(0);
                        return JsonSerializer.Deserialize<T>(json);
                    }
                }
            }

            return default;
        }

        internal IEnumerable<T> GetAll<T>()
        {
            string tableName = GetTableName(typeof(T));
            EnsureTableExists(tableName);

            string sql = $"SELECT [Value] FROM [{tableName}]";
            using (SqliteCommand cmd = new SqliteCommand(sql, _Connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string json = reader.GetString(0);
                    yield return JsonSerializer.Deserialize<T>(json);
                }
            }
        }

        internal void Remove<T>(string key)
        {
            string tableName = GetTableName(typeof(T));
            EnsureTableExists(tableName);

            SqliteParameter keyParam = new SqliteParameter("@Key", DbType.String) { Value = key };
            string sql = $"DELETE FROM [{tableName}] WHERE [Key] = @Key";

            using (SqliteCommand cmd = new SqliteCommand(sql, _Connection))
            {
                cmd.Parameters.Add(keyParam);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
