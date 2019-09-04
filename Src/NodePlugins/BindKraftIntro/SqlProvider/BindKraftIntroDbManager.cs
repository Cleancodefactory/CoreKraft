using System;
using System.Collections.Generic;
using Ccf.Ck.NodePlugins.BindKraftIntro.Models;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.Data.Sqlite;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.SqlProvider
{
    public class BindKraftIntroDbManager : IIntroContentProvider
    {
        private string _ConnectionString;
        private BindKraftIntroQueryBuilder _QueryBuilder;

        public BindKraftIntroDbManager(string connectionString)
        {
            _ConnectionString = connectionString;
            _QueryBuilder = new BindKraftIntroQueryBuilder();
        }

        public IntroMetaData LoadMenu(INodePluginContext pc)
        {
            var roleList = pc.ProcessingContext.InputModel.SecurityModel.Roles;
            var name = pc.ProcessingContext.InputModel.SecurityModel.UserName;
            string command = string.Empty;
            IntroMetaDataDto metaData = new IntroMetaDataDto();
            using (SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                if (roleList.Contains("administrator") || roleList.Contains("manager"))
                {
                    command = _QueryBuilder.GetMenuQuery(true);
                }
                else
                {
                    command = _QueryBuilder.GetMenuQuery(false);
                }
                SqliteCommand readMetaDataCommand = new SqliteCommand
                {
                    Connection = connection,
                    CommandText = command
                };
                readMetaDataCommand.Parameters.Add(new SqliteParameter("name", name));
                using (SqliteDataReader reader = readMetaDataCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            IntroSection sec = new IntroSection
                            {
                                Caption = reader.GetString(reader.GetOrdinal("scaption")),
                                ImagePath = reader.GetString(reader.GetOrdinal("img")),
                                Id = reader.GetString(reader.GetOrdinal("sid")),
                                OrderIdx = int.Parse(reader.GetString(reader.GetOrdinal("sord")))
                            };
                            IntroItem it = new IntroItem
                            {
                                Caption = reader.GetString(reader.GetOrdinal("itcaption")),
                                Id = reader.GetString(reader.GetOrdinal("itid")),
                                Description = reader.GetString(reader.GetOrdinal("desc")),
                                OrderIdx = int.Parse(reader.GetString(reader.GetOrdinal("itord"))),
                                Sources = null,
                                LaunchSpec = null
                            };

                            metaData.AddSection(sec).IntroItems.Add(it);
                        }
                    }
                }
            }
            IntroMetaData introMetaData = new IntroMetaData(metaData.Sections);
            if (introMetaData.Sections.Count == 0)
            {
                throw new Exception("Configuration failure.");
            }
            return introMetaData;
        }

        public IntroItem LoadIntroItem(INodePluginContext pc)
        {
            IntroItem currentItem = new IntroItem();
            string exampleId = pc.Evaluate("exampleName").Value?.ToString()?.Replace("'", string.Empty) ?? string.Empty;
            using (SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                SqliteCommand readIntroItemCommand = new SqliteCommand
                {
                    Connection = connection,
                    CommandText = _QueryBuilder.GetExampleByIdQuery()
                };
                readIntroItemCommand.Parameters.Add(new SqliteParameter("ExampleId", exampleId));
                int id = -1;
                using (SqliteDataReader reader = readIntroItemCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            id = int.Parse(reader.GetString(reader.GetOrdinal("Id")));          
                            currentItem.Caption = reader.GetString(reader.GetOrdinal("Caption"));
                            currentItem.Description = reader.GetString(reader.GetOrdinal("Description"));
                            currentItem.Id = reader.GetString(reader.GetOrdinal("Name"));
                            currentItem.OrderIdx = int.Parse(reader.GetString(reader.GetOrdinal("OrderIdx")));
                            currentItem.Author = reader.GetString(reader.GetOrdinal("Author"));
                            currentItem.Sources = new Sources();
                            currentItem.LaunchSpec = null;
                        }
                    }
                }

                if (id == -1)
                {
                    throw new Exception("Can not find the required example. Check the database");
                }

                SqliteCommand readIntroItemSourcesCommand = new SqliteCommand
                {
                    Connection = connection,
                    CommandText = _QueryBuilder.GetExampleSourceFilesQuery()
                };
                readIntroItemSourcesCommand.Parameters.Add(new SqliteParameter("Id", id));

                using (SqliteDataReader reader = readIntroItemSourcesCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            SourceEntry se = new SourceEntry
                            {
                                Content = reader.GetString(reader.GetOrdinal("Content")),
                                Type = (ESourceType)int.Parse(reader.GetString(reader.GetOrdinal("Type"))),
                                EntryName = reader.GetString(reader.GetOrdinal("EntryName"))
                            };
                            currentItem.Sources.Entries.Add(se);
                        }
                    }
                }
            }
            if (currentItem.Sources.Entries.Count == 0) throw new Exception("Can not find the source section for this example in the database");
            return currentItem;
        }

        public bool CreateIntroItem(string sectionId, IntroItem introItem)
        {
            using (SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                string insertQuery = "";
                using (SqliteCommand insertIntroItemCommand = connection.CreateCommand())
                {
                    insertIntroItemCommand.Parameters.Clear();
                    if (introItem.LaunchSpec == null)
                    {
                        insertIntroItemCommand.Parameters.Add(new SqliteParameter("LaunchSpec", DBNull.Value));
                    }
                    else
                    {
                        insertIntroItemCommand.Parameters.Add(new SqliteParameter("LaunchSpec", introItem.LaunchSpec));
                    }
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("Caption", introItem.Caption));
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("Description", introItem.Description));
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("sectionId", sectionId));
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("Id", introItem.Id));
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("OrderIdx", introItem.OrderIdx));
                    insertIntroItemCommand.Parameters.Add(new SqliteParameter("Author", introItem.Author));
                    insertQuery = _QueryBuilder.GetCreateItemQuery();
                    insertIntroItemCommand.Transaction = transaction;
                    insertIntroItemCommand.CommandText = insertQuery;

                    insertIntroItemCommand.ExecuteNonQuery();
                }

                foreach (SourceEntry source in introItem.Sources.Entries)
                {
                    using (SqliteCommand insertSourcesCommand = connection.CreateCommand())
                    {
                        insertSourcesCommand.Parameters.Clear();
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Id", introItem.Id));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("EntryName", source.EntryName));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Content", source.Content));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Type", (int)source.Type));
                        insertQuery = _QueryBuilder.GetInsertSourcesQuery();
                        insertSourcesCommand.Transaction = transaction;
                        insertSourcesCommand.CommandText = insertQuery;

                        insertSourcesCommand.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            return true;
        }

        public bool DeleteIntroItem(string sectionId, IntroItem introItem)
        {
            using(SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                string deleteQuery = "";
                using(SqliteCommand deleteIntroItemCommand = connection.CreateCommand())
                {
                    deleteIntroItemCommand.Parameters.Clear();
                    deleteIntroItemCommand.Parameters.Add(new SqliteParameter("Id", introItem.Id));
                    deleteIntroItemCommand.Parameters.Add(new SqliteParameter("sectionId", sectionId));
                    deleteQuery = _QueryBuilder.GetDeleteItemQuery();
                    deleteIntroItemCommand.Transaction = transaction;
                    deleteIntroItemCommand.CommandText = deleteQuery;

                    deleteIntroItemCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            return true;
        }

        //Obsolete??
        public bool UpdateIntroItem(string sectionId, IntroItem introItem)
        {
            using (SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                string updateQuery = "";
                using (SqliteCommand updateIntroItemCommand = connection.CreateCommand())
                {
                    updateIntroItemCommand.Parameters.Clear();
                    if (introItem.LaunchSpec == null)
                    {
                        updateIntroItemCommand.Parameters.Add(new SqliteParameter("LaunchSpec", DBNull.Value));
                    }
                    else
                    {
                        updateIntroItemCommand.Parameters.Add(new SqliteParameter("LaunchSpec", introItem.LaunchSpec));
                    }
                    updateIntroItemCommand.Parameters.Add(new SqliteParameter("Caption", introItem.Caption));
                    updateIntroItemCommand.Parameters.Add(new SqliteParameter("Description", introItem.Description));
                    updateIntroItemCommand.Parameters.Add(new SqliteParameter("sectionId", sectionId));
                    updateIntroItemCommand.Parameters.Add(new SqliteParameter("Id", introItem.Id));
                    updateQuery = "UPDATE IntroItems SET Caption = @Caption, Description = @Description, LaunchSpec = @LaunchSpec, UpdateCounter = UpdateCounter + 1, " +
                        "ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') WHERE IntroItems.Id = (SELECT it.Id FROM IntroItems AS [it] INNER JOIN Edges ON Edges.ToId = it.Id " +
                        "INNER JOIN Sections ON Sections.Id = Edges.FromId WHERE Sections.Name = @sectionId AND Sections.IsDeleted = 0 AND it.IsDeleted = 0 AND it.Name = @Id " +
                        "AND Edges.FromType = 'Section' AND Edges.ToType = 'IntroItem' AND Edges.IsDeleted = 0); UPDATE Sources SET IsDeleted = 1, UpdateCounter = UpdateCounter + 1, " +
                        "ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') WHERE Sources.Id IN (SELECT so.Id FROM Sources AS [so] INNER JOIN Edges ON Edges.ToId = so.Id " +
                        "INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId WHERE IntroItems.Name = @Id AND IntroItems.IsDeleted = 0 AND so.IsDeleted = 0 AND Edges.FromType = 'IntroItem' " +
                        "AND Edges.ToType = 'Source' AND Edges.IsDeleted = 0); UPDATE Edges SET IsDeleted = 1, UpdateCounter = UpdateCounter + 1, ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') " +
                        "WHERE Edges.Id IN (SELECT Id FROM Edges WHERE Edges.FromId = (SELECT Id FROM IntroItems WHERE IntroItems.Name = @Id AND IntroItems.IsDeleted = 0) AND Edges.ToId IN " +
                        "(SELECT so.Id FROM Sources AS [so] INNER JOIN Edges ON Edges.ToId = so.Id INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId WHERE IntroItems.Name = @Id " +
                        "AND IntroItems.IsDeleted = 0 AND so.IsDeleted = 1 AND Edges.FromType = 'IntroItem' AND Edges.ToType = 'Source' AND Edges.IsDeleted = 0) AND Edges.FromType = 'IntroItem' " +
                        "AND Edges.ToType = 'Source' AND Edges.IsDeleted = 0); ";
                    updateIntroItemCommand.Transaction = transaction;
                    updateIntroItemCommand.CommandText = updateQuery;

                    updateIntroItemCommand.ExecuteNonQuery();
                }
                foreach (SourceEntry source in introItem.Sources.Entries)
                {
                    using (SqliteCommand insertSourcesCommand = connection.CreateCommand())
                    {
                        insertSourcesCommand.Parameters.Clear();
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Id", introItem.Id));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("EntryName", source.EntryName));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Content", source.Content));
                        insertSourcesCommand.Parameters.Add(new SqliteParameter("Type", (int)source.Type));
                        updateQuery = "INSERT INTO SID SELECT NULL; INSERT INTO Sources SELECT (SELECT MAX(Id) FROM SID), @EntryName, @Content, @Type, 0, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), " +
                            "strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), 1; INSERT INTO SID SELECT NULL; INSERT INTO Edges SELECT (SELECT MAX(Id) FROM SID), (SELECT Id FROM IntroItems " +
                            "WHERE IntroItems.Name = @Id AND IntroItems.IsDeleted = 0), (SELECT MAX(Id) FROM Sources), NULL, 'IntroItem', 'Source', 0, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), " +
                            "strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), 1, 1024; ";
                        insertSourcesCommand.Transaction = transaction;
                        insertSourcesCommand.CommandText = updateQuery;

                        insertSourcesCommand.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            return true;
        }

        public List<IntroItem> LoadDeletedIntroItem(INodePluginContext pc)
        {
            return null;

            //throw new NotImplementedException();
        }

        public bool HardDeleteAll()
        {
            using (SqliteConnection connection = new SqliteConnection(_ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                string deleteQuery = "";
                using (SqliteCommand updateIntroItemCommand = connection.CreateCommand())
                {
                    deleteQuery = "DELETE FROM Sections WHERE ISDELETED = 1";
                    updateIntroItemCommand.Transaction = transaction;
                    updateIntroItemCommand.CommandText = deleteQuery;

                    updateIntroItemCommand.ExecuteNonQuery();
                }
                using (SqliteCommand updateIntroItemCommand = connection.CreateCommand())
                {
                    deleteQuery = "DELETE FROM IntroItems WHERE ISDELETED = 1";
                    updateIntroItemCommand.Transaction = transaction;
                    updateIntroItemCommand.CommandText = deleteQuery;

                    updateIntroItemCommand.ExecuteNonQuery();
                }
                using (SqliteCommand updateIntroItemCommand = connection.CreateCommand())
                {
                    deleteQuery = "DELETE FROM Sources WHERE ISDELETED = 1";
                    updateIntroItemCommand.Transaction = transaction;
                    updateIntroItemCommand.CommandText = deleteQuery;

                    updateIntroItemCommand.ExecuteNonQuery();
                }
                using (SqliteCommand updateIntroItemCommand = connection.CreateCommand())
                {
                    deleteQuery = "DELETE FROM Edges WHERE ISDELETED = 1";
                    updateIntroItemCommand.Transaction = transaction;
                    updateIntroItemCommand.CommandText = deleteQuery;

                    updateIntroItemCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            return true;
        }

        public bool ApproveExample(string sectionId, IntroItem introItem)
        {
            DeleteIntroItem("ForReview", introItem);

            CreateIntroItem(sectionId, introItem);

            return true;
        }
    }
}