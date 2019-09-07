using System;
using System.Collections.Generic;
using System.Linq;
using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses;
using Ccf.Ck.Utilities.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;
using System.Globalization;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.BuitIn
{
    /// <summary>
    /// This class contains the built-in standard set of resolvers for parameter resolution expressions.
    /// This is the set 1, marked with ending 1_X in the class name. The naming for the standard built in sets of resolvers
    /// is setnumber_version. Where the version does not signify built version, but the contents of the set. In other words if in time
    /// it is decided to include new resolvers in the set the version must be incremented.
    /// </summary>
    public class BuiltInParameterResolverSet1_0: ParameterResolverSet
    {
        public BuiltInParameterResolverSet1_0(ResolverSet conf):base(conf) {  }

        #region Constants (these are for local usage only)
        const string PARENT = "parent";
        const string PARENTS = "parents";
        const string SERVER = "server";
        const string CLIENT = "client";
        const string SECURITY = "security";
        const string CURRENT = "current";
        const string INPUT = "input";
        const string DATA = "data";
        const string FILTER = "filter";
        #endregion

        #region Internal utilities (make them at least protected)
        protected object StdGetParameterValue(string fromwhere, string paramName, IParameterResolverContext ctx) {

            object GetParameterValue(IDictionary<string, object> row)
            {
                if (row != null && row.ContainsKey(paramName)) return row[paramName];
                return null;
            }            

            if (!string.IsNullOrWhiteSpace(fromwhere)) {
                var tokens = fromwhere.Split(new char[] { ',' }, 10, StringSplitOptions.None)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim());
                object val = null;
                var inputModel = ctx.ProcessingContext.InputModel;
                foreach (var token in tokens) {
                    if (val != null) return val;
                    switch (token) {
                        case INPUT:
                        case CURRENT:
                            if (ctx.Action == ACTION_WRITE) {
                                val = GetParameterValue(ctx.Row);
                            } else {
                                throw new InvalidOperationException("'current' (or 'input') source is available only for write (store) operation.");
                            }
                            break;
                        case FILTER:
                            if (ctx.Action != ACTION_READ) {
                                throw new InvalidOperationException("'filter' source is available only for read (select) operation.");
                            }
                            val = GetParameterValue(inputModel.Data);
                            break;
                        case DATA:
                            val = GetParameterValue(inputModel.Data);
                            break;
                        case PARENT:
                            if (ctx.Datastack is ListStack<Dictionary<string,object>> && ctx.Datastack != null && ctx.Datastack.Count > 0) {
                                val = GetParameterValue((ctx.Datastack as ListStack<Dictionary<string, object>>).Top());
                            }
                            break;
                        case PARENTS:
                            if (ctx.Datastack != null && ctx.Datastack.Count > 0) {
                                for (int i = ctx.Datastack.Count - 1; i >= 0; i--) {
                                    val = GetParameterValue(ctx.Datastack[i]);
                                    if (val != null) break;
                                }
                            }
                            break;
                        case CLIENT:
                            val = GetParameterValue(inputModel.Client);
                            break;
                        case SERVER:
                            val = GetParameterValue(inputModel.Server);
                            break;
                    }
                }
                return val;
            } else {
                throw new ArgumentException("fromwhere argument is mandatory.");
            }
        }
        #endregion

        #region Standard resolvers
        /// <summary>
        /// Configuration entry (c# inline)
        ///  {
        ///     new Resolver() {
        ///       Alias = "GetFrom",
        ///       Arguments = 2,
        ///       Name = "SayHello"
        ///     }
        /// }
        /// Arguments:
        /// 1 - source := "current,parent,parents,filter,data,client,server,sequrity,input"
        /// 2 - name := string - the name of the parameter to look for or the literal name to use the name under which the parameter is specified in the SQL.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="inargs"></param>
        /// <returns></returns>
        public ParameterResolverValue GetFrom(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            ResolverArguments<ParameterResolverValue> argsCasted = args as ResolverArguments<ParameterResolverValue>;
            string paramName = argsCasted[1].Value as string;
            string source = argsCasted[0].Value as string;
            return new ParameterResolverValue(StdGetParameterValue(source, paramName, ctx));
        }

        public ParameterResolverValue IntegerContent(IParameterResolverContext ctx, ParameterResolverValue input) {
            long v = 0;
            if (input.Value != null) {
                if (long.TryParse(input.ToString(), out v)) {
                    return new ParameterResolverValue(v.ToString(), EResolverValueType.ContentType, (uint)EValueDataType.Text);
                } else {
                    throw new InvalidCastException("Cannot cast the input to integer!");
                }
            } else {
                // This result will cause excepton if not handled in some special manner.
                return new ParameterResolverValue(null, EResolverValueType.ContentType, (uint)EValueDataType.Text);
            }
        }
        public ParameterResolverValue Skip(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            return new ParameterResolverValue(null, EResolverValueType.Skip);
        }

        public ParameterResolverValue GetUserId(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            InputModel inputModel = ctx.ProcessingContext.InputModel;
            return new ParameterResolverValue(inputModel.SecurityModel.UserName);
        }

        public ParameterResolverValue GetUserDetails(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            ResolverArguments<ParameterResolverValue> argsCasted = args as ResolverArguments<ParameterResolverValue>;
            string paramName = argsCasted[0].Value as string;
            var inputModel = ctx.ProcessingContext.InputModel;
            switch (paramName)
            {
                case "firstname":
                    {
                        return new ParameterResolverValue(inputModel.SecurityModel.FirstName);
                    }
                case "lastname":
                    {
                        return new ParameterResolverValue(inputModel.SecurityModel.LastName);
                    }
                default:
                    break;
            }
            throw new Exception($"The requested parameter with {paramName} is not supported.");
        }

        public ParameterResolverValue GetUserRoles(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            InputModel inputModel = ctx.ProcessingContext.InputModel;
            return new ParameterResolverValue(inputModel.SecurityModel.Roles, EValueDataType.Text);
        }

        public ParameterResolverValue HasRoleName(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string roleName = args[0].Value as string;
            if (string.IsNullOrEmpty(roleName))
            {
                throw new Exception("HasRoleName expects a non-empty string parameter for rolename");
            }
            ISecurityModel securityModel = ctx.ProcessingContext.InputModel.SecurityModel;
            return new ParameterResolverValue(securityModel.IsInRole(args[0].Value as string), EValueDataType.Boolean);
        }

        public ParameterResolverValue Or(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            object resultLeft = args[0].Value;
            object resultRight = args[1].Value;
            return TrueLike(resultLeft) || TrueLike(resultRight) ? new ParameterResolverValue(1) : new ParameterResolverValue(0);
        }
        public ParameterResolverValue Concat(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string resultLeft = (args[0].Value != null)?args[0].Value.ToString():"";
            string resultRight = (args[1].Value != null) ? args[1].Value.ToString() : "";
            return new ParameterResolverValue(resultLeft + resultRight);
        }
        public ParameterResolverValue Replace(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            string baseString = args[0].Value as string;
            string replaceKey = args[1].Value as string;
            string replaceWith = (args[2].Value != null) ? args[2].Value.ToString() : "";

            if (string.IsNullOrEmpty(baseString) || string.IsNullOrEmpty(baseString)) {
                return new ParameterResolverValue(baseString);
            }
            var result = baseString.Replace(replaceKey, replaceWith);
            return new ParameterResolverValue(result);
        }

        private bool TrueLike(object v)
        {
            if (v == null)
            {
                return false;
            }
            return Convert.ToBoolean(v);
        }
        #endregion Standard resolvers

        #region Test resolvers
        /// <summary>
        /// Digs for a parameter named the same as the name of the expression in a "standart" order
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="inargs"></param>
        /// <returns></returns>
        public ParameterResolverValue StandardParameter(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs) {
            var args = inargs as ResolverArguments<ParameterResolverValue>;
            var inputModel = ctx.ProcessingContext.InputModel;
            object paramValue = null;
            string paramName = args?.Name.Value as string;

            void GetParameterValue(IDictionary <string, object> row)
            {
                if (paramValue == null && row != null && row.ContainsKey(paramName))
                    paramValue = row[paramName];
            }

            if (paramValue == null) GetParameterValue(ctx.Row);
            if (paramValue == null) GetParameterValue(inputModel.Server);
            if (paramValue == null) GetParameterValue(inputModel.Client);
            if (paramValue == null) GetParameterValue(inputModel.Data);

            return new ParameterResolverValue(paramValue, EValueDataType.any);
        }
        public ParameterResolverValue SayHello()
        {
            return new ParameterResolverValue("hello world!");
        }
        public ParameterResolverValue NumParams(IParameterResolverContext ctx) 
        {
            return new ParameterResolverValue(0);
        }
        #endregion

        #region idlist
        /// <summary>
        /// Converts a list/array to SQL content applicable in syntax like IN ( @here goes the list )
        /// Supports numeric list and string list
        /// Arguments:
        ///     - thelist - 
        ///     - null or regexp 
        ///         null - convert to numeric list
        ///         regex - convert to string list, test each item against the regexp.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="inargs"></param>
        /// <returns></returns>
        public ParameterResolverValue idlist(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs)
        {
            ParameterResolverValue input = inargs[0];
            ParameterResolverValue type_and_check = inargs[1];
            StringBuilder sbresult = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(type_and_check.Value as string)) { // RegExp
                var re = type_and_check.Value as string;
                Regex rex = new Regex(re, RegexOptions.CultureInvariant | RegexOptions.Singleline);
                IEnumerable indata = null;
                if (input.Value is IDictionary) {
                    indata = (input.Value as IDictionary).Values;
                } else {
                    indata = input.Value as IEnumerable;
                }
                foreach (var v in indata) {
                    if (v != null)
                    {
                        if (rex.IsMatch(v.ToString())) {
                            if (sbresult.Length > 0) sbresult.Append(',');
                            sbresult.AppendFormat("'{0}'", v.ToString());
                        } else {
                            //don't stop execution when an item doesn't match?
                            //throw new Exception("an item does not match YOUR regular expression");
                        }
                    } else {
                        throw new Exception("null item in a collection while converting to replacable idlist");
                    }
                }
                return new ParameterResolverValue(sbresult.ToString(), EResolverValueType.ContentType);
            } else if (type_and_check.Value == null && input.Value is IEnumerable) { // Numbers
                IEnumerable indata = null;
                if (input.Value is IDictionary)
                {
                    indata = (input.Value as IDictionary).Values;
                } else {
                    indata = input.Value as IEnumerable;
                }
                foreach (var v in indata)
                {
                    if (sbresult.Length > 0) sbresult.Append(',');
                    if (v is int || v is Int16 || v is Int32 || v is Int64) {
                        sbresult.Append(Convert.ToInt64(v).ToString(CultureInfo.InvariantCulture));
                    } else if (v is uint || v is UInt16 || v is UInt32 || v is UInt64) {
                        sbresult.Append(Convert.ToUInt64(v).ToString(CultureInfo.InvariantCulture));
                    } else if (v is float || v is double) {
                        sbresult.Append(Convert.ToDouble(v).ToString(CultureInfo.InvariantCulture));
                    } else {
                        throw new Exception("Non-numeric and non-null item found in the input");
                    }
                }
                return new ParameterResolverValue(sbresult.ToString(), EResolverValueType.ContentType);
            } else {
                throw new Exception("Unacceptable type parameter or the value is not enumerable");
            }
        }
        #endregion
    }
}
