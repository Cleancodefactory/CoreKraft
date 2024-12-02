using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    internal class DbCommandDumper
    {
        public static string GetCommandText(DbCommand command)
        {
            StringBuilder sbCommandText = new StringBuilder();

            sbCommandText.AppendLine("-- BEGIN COMMAND");

            SqlCommand sqlCommand = command as SqlCommand;
            if (sqlCommand != null)
            {
                // params
                for (int i = 0; i < sqlCommand.Parameters.Count; i++)
                {
                    LogParameterToSqlBatch(sqlCommand.Parameters[i], sbCommandText);
                }
            }
            else {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    LogParameterToSqlBatchDb(command.Parameters[i], sbCommandText);
                }
            }
            sbCommandText.AppendLine("-- END PARAMS");

            // command
            if (command.CommandType == CommandType.StoredProcedure)
            {
                sbCommandText.Append("EXEC ");

                bool hasReturnValue = false;
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    if (command.Parameters[i].Direction == ParameterDirection.ReturnValue)
                    {
                        hasReturnValue = true;
                    }
                }
                if (hasReturnValue)
                {
                    sbCommandText.Append("@returnValue = ");
                }

                sbCommandText.Append(command.CommandText);

                bool hasPrev = false;
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    var cParam = command.Parameters[i];
                    if (cParam.Direction != ParameterDirection.ReturnValue)
                    {
                        if (hasPrev)
                            sbCommandText.Append(",");
                        sbCommandText.Append(" ");

                        sbCommandText.Append(cParam.ParameterName);
                        sbCommandText.Append(" = ");
                        sbCommandText.Append(cParam.ParameterName);

                        if (cParam.Direction.HasFlag(ParameterDirection.Output))
                            sbCommandText.Append(" OUTPUT");

                        hasPrev = true;
                    }
                }
            }
            else
            {
                sbCommandText.AppendLine(command.CommandText);
            }

            sbCommandText.AppendLine("-- RESULTS");
            sbCommandText.Append("SELECT 1 as Executed");
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                var cParam = command.Parameters[i];

                if (cParam.Direction == ParameterDirection.ReturnValue)
                {
                    sbCommandText.Append(", @returnValue as ReturnValue");
                }
                else if (cParam.Direction.HasFlag(ParameterDirection.Output))
                {
                    sbCommandText.Append(", ");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(" as [");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(']');
                }
            }
            sbCommandText.AppendLine(";");

            sbCommandText.AppendLine("-- END COMMAND");
            return sbCommandText.ToString();
        }

        private static void LogParameterToSqlBatch(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.Append("DECLARE ");
            if (param.Direction == ParameterDirection.ReturnValue)
            {
                sbCommandText.AppendLine("@returnValue INT;");
            }
            else
            {
                sbCommandText.Append(param.ParameterName);

                sbCommandText.Append(' ');
                if (param.SqlDbType != SqlDbType.Structured)
                {
                    LogParameterType(param, sbCommandText);
                    sbCommandText.Append(" = ");
                    LogQuotedParameterValue(param.Value, sbCommandText);

                    sbCommandText.AppendLine(";");
                }
                else
                {
                    LogStructuredParameterSql(param, sbCommandText);
                }
            }
        }

        private static void LogParameterToSqlBatchDb(DbParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.Append("DECLARE ");
            if (param.Direction == ParameterDirection.ReturnValue)
            {
                sbCommandText.AppendLine("@returnValue INT;");
            }
            else
            {
                sbCommandText.Append(param.ParameterName);

                sbCommandText.Append(' ');
                sbCommandText.Append(param.Value);
            }
        }

        private static void LogStructuredParameterSql(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.AppendLine(" {List Type};");
            var dataTable = (DataTable)param.Value;

            for (int rowNo = 0; rowNo < dataTable.Rows.Count; rowNo++)
            {
                sbCommandText.Append("INSERT INTO ");
                sbCommandText.Append(param.ParameterName);
                sbCommandText.Append(" VALUES (");

                bool hasPrev = false;
                for (int colNo = 0; colNo < dataTable.Columns.Count; colNo++)
                {
                    if (hasPrev)
                    {
                        sbCommandText.Append(", ");
                    }
                    LogQuotedParameterValue(dataTable.Rows[rowNo].ItemArray[colNo], sbCommandText);
                    hasPrev = true;
                }
                sbCommandText.AppendLine(");");
            }
        }

        const string DATETIME_FORMAT_ROUNDTRIP = "o";
        private static void LogQuotedParameterValue(object value, StringBuilder sbCommandText)
        {
            try
            {
                if (value == null)
                {
                    sbCommandText.Append("NULL");
                }
                else
                {
                    value = UnboxNullable(value);

                    if (value is string
                        || value is char
                        || value is char[]
                        || value is System.Xml.Linq.XElement
                        || value is System.Xml.Linq.XDocument)
                    {
                        sbCommandText.Append("N'");
                        sbCommandText.Append(value.ToString().Replace("'", "''"));
                        sbCommandText.Append('\'');
                    }
                    else if (value is bool)
                    {
                        // True -> 1, False -> 0
                        sbCommandText.Append(Convert.ToInt32(value));
                    }
                    else if (value is sbyte
                        || value is byte
                        || value is short
                        || value is ushort
                        || value is int
                        || value is uint
                        || value is long
                        || value is ulong
                        || value is float
                        || value is double
                        || value is decimal)
                    {
                        sbCommandText.Append(value.ToString());
                    }
                    else if (value is DateTime)
                    {
                        // SQL Server only supports ISO8601 with 3 digit precision on datetime,
                        // datetime2 (>= SQL Server 2008) parses the .net format, and will 
                        // implicitly cast down to datetime.
                        // Alternatively, use the format string "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"
                        // to match SQL server parsing
                        sbCommandText.Append("CAST('");
                        sbCommandText.Append(((DateTime)value).ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append("' as datetime2)");
                    }
                    else if (value is DateTimeOffset)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(((DateTimeOffset)value).ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append('\'');
                    }
                    else if (value is Guid)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(((Guid)value).ToString());
                        sbCommandText.Append('\'');
                    }
                    else if (value is byte[])
                    {
                        var data = (byte[])value;
                        if (data.Length == 0)
                        {
                            sbCommandText.Append("NULL");
                        }
                        else
                        {
                            sbCommandText.Append("0x");
                            for (int i = 0; i < data.Length; i++)
                            {
                                sbCommandText.Append(data[i].ToString("x"));
                            }
                        }
                    }
                    else
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(value.GetType().ToString());
                        sbCommandText.Append(" *" + "/ N'");
                        sbCommandText.Append(value.ToString());
                        sbCommandText.Append('\'');
                    }
                }
            }

            catch (Exception ex)
            {
                sbCommandText.AppendLine("/* Exception occurred while converting parameter: ");
                sbCommandText.AppendLine(ex.ToString());
                sbCommandText.AppendLine("*/");
            }
        }

        private static object UnboxNullable(object value)
        {
            var typeOriginal = value.GetType();
            if (typeOriginal.IsGenericType
                && typeOriginal.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // generic value, unboxing needed
                return typeOriginal.InvokeMember("GetValueOrDefault",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, value, null);
            }
            else
            {
                return value;
            }
        }

        private static void LogParameterType(SqlParameter param, StringBuilder sbCommandText)
        {
            switch (param.SqlDbType)
            {
                // variable length
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append('(');
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(')');
                    }
                    break;
                case SqlDbType.VarBinary:
                case SqlDbType.Image:
                    {
                        sbCommandText.Append("VARBINARY");
                        sbCommandText.Append("(MAX /* Specified as ");
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(" */)");
                    }
                    break;
                case SqlDbType.VarChar:
                case SqlDbType.Text:
                    {
                        sbCommandText.Append("VARCHAR");
                        sbCommandText.Append("(MAX /* Specified as ");
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(" */)");
                    }
                    break;
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                    {
                        sbCommandText.Append("NVARCHAR");
                        sbCommandText.Append("(MAX /* Specified as ");
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(" */)");
                    }
                    break;
                // fixed length
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt:
                case SqlDbType.Int:
                case SqlDbType.BigInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Decimal:
                case SqlDbType.Real:
                case SqlDbType.Float:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.UniqueIdentifier:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
                // Unknown
                case SqlDbType.Timestamp:
                default:
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append(" *" + "/ ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
            }
        }
    }
}
