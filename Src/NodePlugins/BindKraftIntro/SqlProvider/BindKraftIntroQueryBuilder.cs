<<<<<<< HEAD
﻿using System.Text;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.SqlProvider
{
    internal class BindKraftIntroQueryBuilder
    {
        internal BindKraftIntroQueryBuilder()
        {
        }
        internal string GetMenuQuery(bool isAdmin)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT ");
            sb.AppendLine("sec.Caption as [scaption],");
            sb.AppendLine("sec.ImagePath as [img],");
            sb.AppendLine("sec.Name AS [sid],");
            sb.AppendLine("sec.OrderIdx as [sord],");
            sb.AppendLine("it.Caption as [itcaption],");
            sb.AppendLine("it.Name AS [itid],");
            sb.AppendLine("it.Description as [desc],");
            sb.AppendLine("it.OrderIdx AS [itord],");
            sb.AppendLine("it.Caption as [itcaption]");
            sb.AppendLine("FROM Sections AS [sec]");
            sb.AppendLine("INNER JOIN Edges On Edges.FromId = sec.Id INNER JOIN IntroItems AS [it] ON it.Id = Edges.ToId");
            sb.AppendLine("WHERE it.IsDeleted = 0");
            if (!isAdmin)
            {
                sb.AppendLine("AND CASE WHEN sid = 'ForReview' THEN it.Author == @name ELSE 1 END");
            }
            sb.AppendLine("ORDER BY sord ASC, itord ASC;");
            return sb.ToString();
        }
        
        internal string GetExampleByIdQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT ");
            sb.AppendLine("Id,");
            sb.AppendLine("Caption,");
            sb.AppendLine("Description,");
            sb.AppendLine("LaunchSpec,");
            sb.AppendLine("Name,");
            sb.AppendLine("OrderIdx,");
            sb.AppendLine("Author");
            sb.AppendLine("FROM IntroItems");
            sb.AppendLine("WHERE Name = @ExampleId");
            sb.AppendLine("AND IsDeleted = 0;");
            return sb.ToString();
        }

        internal string GetExampleSourceFilesQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT ");
            sb.AppendLine("so.Content, ");
            sb.AppendLine("so.Type,");
            sb.AppendLine("so.EntryName");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.FromId = @Id");
            sb.AppendLine("AND Edges.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 0;");

            return sb.ToString();
        }

        internal string GetCreateItemQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO SID SELECT NULL; ");

            sb.AppendLine("INSERT INTO IntroItems ");
            sb.AppendLine("SELECT ");
            sb.AppendLine("(SELECT MAX(Id) FROM SID), ");
            sb.AppendLine("@Caption, ");
            sb.AppendLine("@Description, ");
            sb.AppendLine("@LaunchSpec, ");
            sb.AppendLine("@Id, ");
            sb.AppendLine("0, ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("1, ");
            sb.AppendLine("1024, ");
            sb.AppendLine("@OrderIdx, ");
            sb.AppendLine("@Author; ");
            sb.AppendLine("INSERT INTO SID SELECT NULL; ");

            sb.AppendLine("INSERT INTO Edges ");
            sb.AppendLine("SELECT");
            sb.AppendLine("(SELECT MAX(Id) FROM SID),");
            sb.AppendLine("(SELECT Id FROM Sections WHERE Sections.Name = @sectionId AND Sections.IsDeleted = 0),");
            sb.AppendLine("(SELECT MAX(Id) FROM IntroItems),");
            sb.AppendLine("NULL,");
            sb.AppendLine("'Section',");
            sb.AppendLine("'IntroItem', ");
            sb.AppendLine("0, ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("1, ");
            sb.AppendLine("1024; ");


            return sb.ToString();
        }

        internal string GetInsertSourcesQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO SID SELECT NULL;");
            sb.AppendLine("INSERT INTO Sources");
            sb.AppendLine("SELECT (SELECT MAX(Id) FROM SID),");
            sb.AppendLine("@EntryName,");
            sb.AppendLine("@Content,");
            sb.AppendLine("@Type,");
            sb.AppendLine("0,");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("1;");

            sb.AppendLine("INSERT INTO SID SELECT NULL;");
            sb.AppendLine("INSERT INTO Edges");
            sb.AppendLine("SELECT (SELECT MAX(Id) FROM SID),");
            sb.AppendLine("(SELECT Id FROM IntroItems");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0),");
            sb.AppendLine("(SELECT MAX(Id) FROM Sources),");
            sb.AppendLine("NULL,");
            sb.AppendLine("'IntroItem',");
            sb.AppendLine("'Source',");
            sb.AppendLine("0,");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("1,");
            sb.AppendLine("1024;");

            return sb.ToString();
        }

        internal string GetDeleteItemQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE Sources");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Sources.Id IN");
            sb.AppendLine("(SELECT so.Id");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 0");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE Edges");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Edges.Id IN");
            sb.AppendLine("(SELECT Id");
            sb.AppendLine("FROM Edges");
            sb.AppendLine("WHERE Edges.FromId =");
            sb.AppendLine("(SELECT Id");
            sb.AppendLine("FROM IntroItems");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0)");
            sb.AppendLine("AND Edges.ToId IN");
            sb.AppendLine("(SELECT so.Id");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 1");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0)");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE Edges");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Edges.Id =");
            sb.AppendLine("(SELECT Edges.Id");
            sb.AppendLine("FROM Edges");
            sb.AppendLine("INNER JOIN Sections ON Sections.id = Edges.FromId");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.ToId");
            sb.AppendLine("WHERE Sections.Name = @sectionId");
            sb.AppendLine("AND Sections.IsDeleted = 0");
            sb.AppendLine("AND IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND Edges.FromType = 'Section'");
            sb.AppendLine("AND Edges.ToType = 'IntroItem'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE IntroItems");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE IntroItems.Name = @Id;");

            return sb.ToString();
        }
    }
}
=======
﻿using System.Text;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.SqlProvider
{
    internal class BindKraftIntroQueryBuilder
    {
        internal BindKraftIntroQueryBuilder()
        {
        }
        internal string GetMenuQuery(bool isAdmin)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT ");
            sb.AppendLine("sec.Caption as [scaption],");
            sb.AppendLine("sec.ImagePath as [img],");
            sb.AppendLine("sec.Name AS [sid],");
            sb.AppendLine("sec.OrderIdx as [sord],");
            sb.AppendLine("it.Caption as [itcaption],");
            sb.AppendLine("it.Name AS [itid],");
            sb.AppendLine("it.Description as [desc],");
            sb.AppendLine("it.OrderIdx AS [itord],");
            sb.AppendLine("it.Caption as [itcaption]");
            sb.AppendLine("FROM Sections AS [sec]");
            sb.AppendLine("INNER JOIN Edges On Edges.FromId = sec.Id INNER JOIN IntroItems AS [it] ON it.Id = Edges.ToId");
            sb.AppendLine("WHERE it.IsDeleted = 0");
            if (!isAdmin)
            {
                sb.AppendLine("AND CASE WHEN sid = 'ForReview' THEN it.Author == @name ELSE 1 END");
            }
            sb.AppendLine("ORDER BY sord ASC, itord ASC;");
            return sb.ToString();
        }
        
        internal string GetExampleByIdQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT ");
            sb.AppendLine("Id,");
            sb.AppendLine("Caption,");
            sb.AppendLine("Description,");
            sb.AppendLine("LaunchSpec,");
            sb.AppendLine("Name,");
            sb.AppendLine("OrderIdx,");
            sb.AppendLine("Author");
            sb.AppendLine("FROM IntroItems");
            sb.AppendLine("WHERE Name = @ExampleId");
            sb.AppendLine("AND IsDeleted = 0;");
            return sb.ToString();
        }

        internal string GetExampleSourceFilesQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT ");
            sb.AppendLine("so.Content, ");
            sb.AppendLine("so.Type,");
            sb.AppendLine("so.EntryName");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.FromId = @Id");
            sb.AppendLine("AND Edges.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 0;");

            return sb.ToString();
        }

        internal string GetCreateItemQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO SID SELECT NULL; ");

            sb.AppendLine("INSERT INTO IntroItems ");
            sb.AppendLine("SELECT ");
            sb.AppendLine("(SELECT MAX(Id) FROM SID), ");
            sb.AppendLine("@Caption, ");
            sb.AppendLine("@Description, ");
            sb.AppendLine("@LaunchSpec, ");
            sb.AppendLine("@Id, ");
            sb.AppendLine("0, ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("1, ");
            sb.AppendLine("1024, ");
            sb.AppendLine("@OrderIdx, ");
            sb.AppendLine("@Author; ");
            sb.AppendLine("INSERT INTO SID SELECT NULL; ");

            sb.AppendLine("INSERT INTO Edges ");
            sb.AppendLine("SELECT");
            sb.AppendLine("(SELECT MAX(Id) FROM SID),");
            sb.AppendLine("(SELECT Id FROM Sections WHERE Sections.Name = @sectionId AND Sections.IsDeleted = 0),");
            sb.AppendLine("(SELECT MAX(Id) FROM IntroItems),");
            sb.AppendLine("NULL,");
            sb.AppendLine("'Section',");
            sb.AppendLine("'IntroItem', ");
            sb.AppendLine("0, ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), ");
            sb.AppendLine("1, ");
            sb.AppendLine("1024; ");


            return sb.ToString();
        }

        internal string GetInsertSourcesQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO SID SELECT NULL;");
            sb.AppendLine("INSERT INTO Sources");
            sb.AppendLine("SELECT (SELECT MAX(Id) FROM SID),");
            sb.AppendLine("@EntryName,");
            sb.AppendLine("@Content,");
            sb.AppendLine("@Type,");
            sb.AppendLine("0,");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("1;");

            sb.AppendLine("INSERT INTO SID SELECT NULL;");
            sb.AppendLine("INSERT INTO Edges");
            sb.AppendLine("SELECT (SELECT MAX(Id) FROM SID),");
            sb.AppendLine("(SELECT Id FROM IntroItems");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0),");
            sb.AppendLine("(SELECT MAX(Id) FROM Sources),");
            sb.AppendLine("NULL,");
            sb.AppendLine("'IntroItem',");
            sb.AppendLine("'Source',");
            sb.AppendLine("0,");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),");
            sb.AppendLine("1,");
            sb.AppendLine("1024;");

            return sb.ToString();
        }

        internal string GetDeleteItemQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE Sources");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Sources.Id IN");
            sb.AppendLine("(SELECT so.Id");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 0");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE Edges");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Edges.Id IN");
            sb.AppendLine("(SELECT Id");
            sb.AppendLine("FROM Edges");
            sb.AppendLine("WHERE Edges.FromId =");
            sb.AppendLine("(SELECT Id");
            sb.AppendLine("FROM IntroItems");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0)");
            sb.AppendLine("AND Edges.ToId IN");
            sb.AppendLine("(SELECT so.Id");
            sb.AppendLine("FROM Sources AS [so]");
            sb.AppendLine("INNER JOIN Edges ON Edges.ToId = so.Id");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.FromId");
            sb.AppendLine("WHERE IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND so.IsDeleted = 1");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0)");
            sb.AppendLine("AND Edges.FromType = 'IntroItem'");
            sb.AppendLine("AND Edges.ToType = 'Source'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE Edges");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE Edges.Id =");
            sb.AppendLine("(SELECT Edges.Id");
            sb.AppendLine("FROM Edges");
            sb.AppendLine("INNER JOIN Sections ON Sections.id = Edges.FromId");
            sb.AppendLine("INNER JOIN IntroItems ON IntroItems.Id = Edges.ToId");
            sb.AppendLine("WHERE Sections.Name = @sectionId");
            sb.AppendLine("AND Sections.IsDeleted = 0");
            sb.AppendLine("AND IntroItems.Name = @Id");
            sb.AppendLine("AND IntroItems.IsDeleted = 0");
            sb.AppendLine("AND Edges.FromType = 'Section'");
            sb.AppendLine("AND Edges.ToType = 'IntroItem'");
            sb.AppendLine("AND Edges.IsDeleted = 0);");

            sb.AppendLine("UPDATE IntroItems");
            sb.AppendLine("SET IsDeleted = 1,");
            sb.AppendLine("UpdateCounter = UpdateCounter + 1,");
            sb.AppendLine("ModifiedAt = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')");
            sb.AppendLine("WHERE IntroItems.Name = @Id;");

            return sb.ToString();
        }
    }
}
>>>>>>> develop
