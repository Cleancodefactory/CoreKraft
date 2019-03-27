using System;
using System.Collections.Generic;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.BuiltIn
{
    /// <summary>
    /// Resolvers for the board module.
    /// </summary>
    public class BoardResolverSet1_0 : ParameterResolverSet
    {
        public BoardResolverSet1_0(ResolverSet conf) : base(conf) { }

        /// <summary>
        /// Generates sql syntax that can be used to check permission levels.
        /// Arguments:
        /// 1 - input params := "permission level, fromId, toId, IsDeleted = 0, tablename = Edges"
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue GetPermission(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
            string result = GetPermissionSql(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }
        
        private string GetPermissionSql(string inputParams)
        {
            var tokens = inputParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(tokens.Length < 3)
            {
                throw new InvalidOperationException("There shoould be at least three tokens passed to GetPermissions.");
            }

            string permission = string.Empty;
            permission = GetParamValue(tokens, 0, nameof(permission));
            string fromId = string.Empty;
            fromId = GetParamValue(tokens, 1, nameof(fromId));
            string toId = string.Empty;
            toId = GetParamValue(tokens, 2, nameof(toId));
            string isDeleted = "0";
            if (tokens.Length > 3)
            {
                isDeleted = GetParamValue(tokens, 3, nameof(isDeleted));
            }
            string tableName = "Edges";
            if (tokens.Length > 4)
            {
                tableName = GetParamValue(tokens, 4, nameof(tableName));
            }

            string result = $"EXISTS ( SELECT Id FROM {tableName} WHERE ToId = {toId} AND FromId = {fromId} AND IsDeleted = {isDeleted} AND Permission >= {permission})";

            return result;
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



        public ParameterResolverValue GenerateHash(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string hash = Guid.NewGuid().ToString();
            return new ParameterResolverValue(hash);
        }
    }
}
