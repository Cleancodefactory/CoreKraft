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
        /// Generates MySql syntax that can be used to check permission levels.
        /// Arguments:
        /// 1 - input params := "permission level, username of the logged user."
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue GetHrmUserPermission(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
            string result = GetHrmUserPermission(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }

        public ParameterResolverValue GetHrmEmployeesInProject(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string inputParams = args[0].Value.ToString();
            string result = GetHrmEmployeesInProject(inputParams);
            return new ParameterResolverValue(result, EResolverValueType.ContentType);
        }

        private string GetHrmUserPermission(string inputParams)
        {
            StringBuilder resultBuilder = new StringBuilder();
            var tokens = inputParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string permission = string.Empty;
            permission = GetParamValue(tokens, 0, nameof(permission));
            if (string.IsNullOrEmpty(permission))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - permission.");
            }

            string username = string.Empty;
            username = GetParamValue(tokens, 1, nameof(username));
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - username of the logged user.");
            }

            resultBuilder.Append($"exists (select * from users where usermail = {username} and is_deleted = 0 and rights >= {permission})");

            return resultBuilder.ToString();
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

            string username = string.Empty;
            username = GetParamValue(inputs, 1, nameof(username));
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("GetPermissions missing required parameter - username of the logged user.");
            }

            resultBuilder.AppendLine($"exists (select * from (select ep.employee_id from users as u left join employees as e on (e.employee_id = u.user_id) left join employees_projects as ep on (ep.employee_id = e.employee_id) where u.usermail = {username} and u.is_deleted = 0 and e.is_deleted = 0 and (u.rights >= {permission} or ep.rights >= {permission})) as logged_user_projects where logged_user_projects.employee_id = employee_id)");

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