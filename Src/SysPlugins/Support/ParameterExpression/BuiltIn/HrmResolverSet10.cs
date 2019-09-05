using System;
using System.Collections.Generic;
using System.Text;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.BuiltIn
{
    public class HrmResolverSet1_0 : ParameterResolverSet
    {
        public HrmResolverSet1_0(ResolverSet conf) : base(conf) { }

        /// <summary>
        /// Generates MySql syntax that can be used to check permission levels in Edge table.
        /// Arguments:
        /// 1 - input params := "permission level, fromId, toId, IsDeleted = 0"
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue GetHrmEdgePermission(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
            string result = GetHrmEdgePermission(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }

        public ParameterResolverValue GetHrmUserPermission(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
            string result = GetHrmUserPermission(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }

        public ParameterResolverValue GetHrmEmployeesInProject(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
          // string usermail = ctx.ProcessingContext.InputModel.SecurityModel.UserName;
            string result = GetHrmEmployeesInProject(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }

        private string GetHrmUserPermission(string inputParams)
        {
            StringBuilder resultBuilder = new StringBuilder();
            var tokens = inputParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                throw new InvalidOperationException("There should be at least three tokens passed to GetPermissions.");
            }


            string permission = string.Empty;
            permission = GetParamValue(tokens, 0, nameof(permission));
            if (string.IsNullOrEmpty(permission))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - permission.");
            }

            string tableName = string.Empty;
            tableName = GetParamValue(tokens, 1, nameof(tableName));
            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - tableName.");
            }

            string idcolumnName = string.Empty;
            idcolumnName = GetParamValue(tokens, 2, nameof(idcolumnName));
            if (string.IsNullOrEmpty(idcolumnName))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - idcolumnName.");
            }


            string isDeleted = "0";
            isDeleted = GetParamValue(tokens, 3, nameof(isDeleted));


            string mail = string.Empty;
            mail = GetParamValue(tokens, 4, nameof(mail));

            resultBuilder.Append($"EXISTS ( SELECT `{idcolumnName}` FROM `{tableName}` WHERE `usermail` = {mail} and `IsDeleted` = {isDeleted} and `Rights` >= {permission})");

            return resultBuilder.ToString();
        }

        private string GetHrmEdgePermission(string inputParams)
        {
            var tokens = inputParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                throw new InvalidOperationException("There should be at least three tokens passed to GetPermissions.");
            }

            string permission = GetParamValue(tokens, 0, nameof(permission));
            if (string.IsNullOrEmpty(permission))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - permission.");
            }

            string toType = GetParamValue(tokens, 1, nameof(toType));
            if (string.IsNullOrEmpty(toType))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - toType.");
            }

            string userEmail = GetParamValue(tokens, 2, nameof(userEmail));
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - userEmail.");
            }

            // Get Optional Parameters
            string isDeleted = "0";
            if (tokens.Length > 3)
            {
                isDeleted = GetParamValue(tokens, 3, nameof(isDeleted));
            }

            return new StringBuilder()
                .Append("EXISTS (")
                .Append(' ', 2).Append("SELECT DISTINCT `e`.`ToId`").AppendLine()
                .Append(' ', 2).Append("FROM `edges` AS `e` ").AppendLine()
                .Append(' ', 2).Append("WHERE `e`.`FromType` = 'Employee'").AppendLine()
                .Append(' ', 2).AppendFormat(@"AND `e`.`ToType` ='{0}'", toType).AppendLine()
                .Append(' ', 2).AppendFormat(@"AND `e`.`IsDeleted` = {0}", isDeleted).AppendLine()
                .Append(' ', 2).Append("AND EXISTS (").AppendLine()
                .Append(' ', 4).Append("SELECT *").AppendLine()
                .Append(' ', 4).Append("FROM `users` AS `u`").AppendLine()
                .Append(' ', 4).Append("INNER JOIN `edges` AS `e1` ON").AppendLine()
                .Append(' ', 6).Append("(`e1`.`FromType` = 'Employee' AND `e1`.`ToType` = 'User' AND `e1`.`ToId`=`u`.`UserId`)").AppendLine()
                .Append(' ', 4).AppendFormat("WHERE `u`.`UserMail` = {0}", userEmail).AppendLine()
                .Append(' ', 4).Append("AND ((`u`.`Rights` >= 1024").AppendLine()
                .Append(' ', 10).Append("AND NOT EXISTS (").AppendLine()
                .Append(' ', 12).Append("SELECT *").AppendLine()
                .Append(' ', 12).Append("FROM `users` AS `u`").AppendLine()
                .Append(' ', 12).Append("INNER JOIN `edges` AS `e1` ON").AppendLine()
                .Append(' ', 14).Append("(`e1`.`FromType` = 'Employee' AND `e1`.`ToType` = 'User' AND `e1`.`ToId`=`u`.`UserId`)").AppendLine()
                .Append(' ', 12).AppendFormat("WHERE `u`.`UserMail` = {0}", userEmail).AppendLine()
                .Append(' ', 12).AppendFormat("AND (`e`.`FromId` = `e1`.`FromId` AND `e`.`Permission` < {0})", permission).AppendLine()
                .Append(' ', 10).Append("))").AppendLine()
                .Append(' ', 12).Append("OR").AppendLine()
                .Append(' ', 12).Append("(`u`.`Rights` < 1024").AppendLine()
                .Append(' ', 12).Append("AND EXISTS (").AppendLine()
                .Append(' ', 14).Append("SELECT *").AppendLine()
                .Append(' ', 14).Append("FROM `users` AS `u`").AppendLine()
                .Append(' ', 14).Append("INNER JOIN `edges` AS `e1` ON").AppendLine()
                .Append(' ', 16).Append("(`e1`.`FromType` = 'Employee' AND `e1`.`ToType` = 'User' AND `e1`.`ToId`=`u`.`UserId`)").AppendLine()
                .Append(' ', 14).AppendFormat("WHERE `u`.`UserMail` = {0}", userEmail).AppendLine()
                .Append(' ', 14).AppendFormat("AND (`e`.`FromId` = `e1`.`FromId` AND `e`.`Permission` >= {0})", permission).AppendLine()
                .Append(' ', 12).Append("))").AppendLine()
                .Append(' ', 8).Append(")").AppendLine()
                .AppendFormat(") or ((select (select `u`.`Rights` from users as `u` where `u`.`UserMail` = {0}) >= 1024)))", userEmail).ToString()
                ;
        }

        private string GetHrmEmployeesInProject(string inputParams)
        {
            StringBuilder resultBuilder = new StringBuilder();
            var inputs = inputParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string permission = "";
            permission = GetParamValue(inputs, 0, nameof(permission));

            if (string.IsNullOrEmpty(permission))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - permission.");
            }

            string mail = "";
            mail= GetParamValue(inputs, 1, nameof(mail));

            resultBuilder.Append($"exists (select distinct edge.FromId from edges as edge where edge.FromType = 'Employee' and  edge.ToType = 'Project' and edge.IsDeleted = 0 and exists( select * from edges as ed where ed.ToId = edge.ToId  and  ed.IsDeleted = 0 and ed.Permission > {permission} and  ed.ToType = 'Project'  and ed.FromId = ( select e.FromId  from edges as e  inner join users as u on  (u.UserId = e.ToId and e.FromType = 'Employee' and e.ToType = 'User')  where u.usermail = {mail}) and edge.FromId = employeeid order by ed.EdgeId limit 1))");

            return resultBuilder.ToString();
        }

        private string GetParamValue(string[] tokens, int index, string paramName)
        {
            var result = tokens[index];
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new NullReferenceException($"{paramName} cannot be null.");
            }

            return result;
        }
    }
}