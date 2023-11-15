using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.BaseClasses;
using Ccf.Ck.Utilities.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;
using InputModel = Ccf.Ck.Models.NodeRequest.InputModel;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.BuitIn
{
    /// <summary>
    /// This class contains the built-in standard set of resolvers for parameter resolution expressions.
    /// This is the set 1, marked with ending 1_X in the class name. The naming for the standard built in sets of resolvers
    /// is setnumber_version. Where the version does not signify built version, but the contents of the set. In other words if in time
    /// it is decided to include new resolvers in the set the version must be incremented.
    /// </summary>
    public class BuiltInParameterResolverSet1_0 : ParameterResolverSet {
        public BuiltInParameterResolverSet1_0(ResolverSet conf) : base(conf) { }

        #region Constants for sources of parameters (these are for local usage only)
        const string PARENT = "parent";
        const string PARENTS = "parents";
        const string SERVER = "server";
        const string CLIENT = "client";
        const string SECURITY = "security";
        const string CURRENT = "current";
        const string INPUT = "input";
        const string DATA = "data";
        const string FILTER = "filter";
        const string ROOT = "root";

        #endregion

        #region Constants for type names
        const string T_INT = "int";
        const string T_UINT = "uint";
        const string T_DBL = "double";
        const string T_STR = "string";
        const string T_NULL = "null";
        const string T_BOOLEAN = "boolean";
        const string T_DATETIME = "datetime";
        const string T_DATETIMEUTC = "datetimeutc";
        const string T_DATETIMEASUTC = "datetimeasutc";

        readonly List<string> T_TYPEORDER = new List<string>() { T_UINT, T_INT, T_DBL };

        private enum Number_Formats {
            Unknown = 0,
            Decimal = 10,
            Hexdecimal = 16,
            Octal = 8,
            Binary = 2
        };

        static readonly Type[] g_NUMERIC_TYPES = new Type[] { typeof(int), typeof(double), typeof(Int16), typeof(Int32),
                                                              typeof(Int64), typeof(sbyte), typeof(uint), typeof(UInt16),
                                                              typeof(UInt32), typeof(UInt64), typeof(float), typeof(decimal), typeof(byte), typeof(string)};

        static readonly Type[] g_INT_TYPES = new Type[] { typeof(int), typeof(Int16), typeof(Int32),
                                                              typeof(Int64), typeof(sbyte), typeof(uint), typeof(UInt16),
                                                              typeof(UInt32), typeof(UInt64), typeof(byte)};

        static readonly Type[] g_FLOAT_TYPES = new Type[] { typeof(double), typeof(float), typeof(decimal) };


        #endregion

        #region Internal utilities (make them at least protected)
        protected object StdGetParameterValue(string fromwhere, string paramName, IParameterResolverContext ctx) {

            object GetParameterValue(IDictionary<string, object> row) {
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
                                if (ctx.ParentAccessNotAllowed) {
                                    throw new InvalidOperationException("'current' Cannot be used in this situation. It is resolved while parent and current are not yet determined");
                                }
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
                            if (ctx.ParentAccessNotAllowed) {
                                throw new InvalidOperationException("'parent' Cannot be used in this situation. It is resolved while parent and current are not yet determined");
                            }
                            if (ctx.Datastack is ListStack<Dictionary<string, object>> && ctx.Datastack != null && ctx.Datastack.Count > 0) {
                                val = GetParameterValue((ctx.Datastack as ListStack<Dictionary<string, object>>).Top());
                            }
                            break;
                        case PARENTS:
                            if (ctx.ParentAccessNotAllowed) {
                                throw new InvalidOperationException("'parent' Cannot be used in this situation. It is resolved while parent and current are not yet determined");
                            }
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
                        default:
                            throw new InvalidOperationException($"{token} Unknown source location for parameter.");
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
        /// <summary>
        /// Returns source as (raw (string,object)) dictionary. Good for usage in scripts - they will need to pack this with ToDict()
        /// 
        /// 
        /// Configuration entry (c# inline)
        ///  {
        ///     new Resolver() {
        ///       Alias = "GetAll",
        ///       Arguments = 1,
        ///       Name = "getAll"
        ///     }
        /// }
        /// Arguments:
        /// 1 - source := "current,parent,parents,filter,data,client,server,sequrity,input"
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="inargs"></param>
        /// <returns></returns>
        public ParameterResolverValue CombineSources(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            ResolverArguments<ParameterResolverValue> argsCasted = args as ResolverArguments<ParameterResolverValue>;

            string fromwhere = Convert.ToString(args[0].Value);

            Dictionary<string, object> SuckDict(Dictionary<string, object> target, IDictionary<string, object> source) {
                foreach (KeyValuePair<string, object> kvp in source) {
                    target[kvp.Key] = kvp.Value;
                }
                return target;
            }

            if (!string.IsNullOrWhiteSpace(fromwhere)) {

                var tokens = fromwhere.Split(new char[] { ',' }, 10, StringSplitOptions.None)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s.Trim());
                var inputModel = ctx.ProcessingContext.InputModel;
                var result = new Dictionary<string, object>();
                foreach (var token in tokens) {
                    switch (token) {
                        case INPUT:
                        case CURRENT:
                            if (ctx.Action == ACTION_WRITE) {
                                SuckDict(result, ctx.Row);
                            } else {
                                throw new InvalidOperationException("'current' (or 'input') source is available only for write (store) operation.");
                            }
                            break;
                        case FILTER:
                            if (ctx.Action != ACTION_READ) {
                                throw new InvalidOperationException("'filter' source is available only for read (select) operation.");
                            }
                            SuckDict(result, inputModel.Data);
                            break;
                        case DATA:
                            SuckDict(result, inputModel.Data);
                            break;
                        case PARENT:
                            SuckDict(result, (ctx.Datastack as ListStack<Dictionary<string, object>>).Top());
                            break;
                        case CLIENT:
                            SuckDict(result, inputModel.Client);
                            break;
                        case SERVER:
                            SuckDict(result, inputModel.Server);
                            break;

                    }
                }
                return new ParameterResolverValue(result);
            }
            return new ParameterResolverValue(null);
        }
        /// <summary>
        /// Navigates to a value from the specified point
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ParameterResolverValue NavGetFrom(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count != 2) throw new ArgumentException("NavGetFrom requires 2 arguments");
            var fromwhere = args[0].Value as string;
            var inputModel = ctx.ProcessingContext.InputModel;
            if (fromwhere != null) {
                Dictionary<string, object> start = fromwhere switch {
                    INPUT => ctx.Row,
                    CURRENT => ctx.Row,
                    FILTER => inputModel.Data.ToDictionary(kv => kv.Key, kv => kv.Value),
                    DATA => inputModel.Data.ToDictionary(kv => kv.Key, kv => kv.Value),
                    PARENT =>
                        (ctx.Datastack is ListStack<Dictionary<string, object>> stack && stack != null && stack.Count > 0 && !ctx.ParentAccessNotAllowed) ?
                            stack.Top() as Dictionary<string, object> :
                            null,
                    // In BeginReadOperation we add empty dictionary which is used as an anchor for the further results DURING EXCECUTION
                    // this fake object is not returned or used when the execution is done
                    // READ and WRITE differ!
                    // In BeginWriteOperation no such ancher is added 
                    ROOT => GetStackBottom(ctx),
                    _ => null                    
                };
                if (start != null) {
                    var path = args[1].Value as string;
                    if (path != null) {
                        object current = start;
                        var chain = path.Split('.');
                        for (int i = 0; i < chain.Length; i++) {
                            var idx = chain[i].Trim();
                            if (current is Dictionary<string, object> dict) {
                                current = dict[idx];
                            } else if (current is IEnumerable<Dictionary<string, object>> list) {
                                var n = Convert.ToInt32(idx);
                                current = list.ToArray()[n];
                            } else {
                                return new ParameterResolverValue(null);
                            }
                        }
                        return new ParameterResolverValue(current);
                    }
                }
            }
            return new ParameterResolverValue(null);
        }

        private Dictionary<string, object> GetStackBottom(IParameterResolverContext ctx)
        {
            if (ctx.Action == ACTION_WRITE)
            {
                return (ctx.Datastack is ListStack<Dictionary<string, object>> stack && stack != null && stack.Count > 0) ?
                    stack[0] as Dictionary<string, object> : null;
            }
            else if (ctx.Action == ACTION_READ)
            {
                return (ctx.Datastack is ListStack<Dictionary<string, object>> stack && stack != null && stack.Count > 1) ?
                   stack[1] as Dictionary<string, object> : null;
            }
            return null;
        }

        public ParameterResolverValue CurrentData(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            ResolverArguments<ParameterResolverValue> argsCasted = args as ResolverArguments<ParameterResolverValue>;
            if (ctx.Action == ACTION_WRITE) {
                return new ParameterResolverValue(ctx.Row);
            } else {
                throw new InvalidOperationException("The Currentdata resolver canbe used only for write operations.");
            }
        }


        public ParameterResolverValue IntegerContent(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count != 1) throw new Exception("IntegerCountent declared with wrong number of arguments (must be 1)");
            ParameterResolverValue input = args[0];
            if (input.Value != null) {
                if (long.TryParse(input.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out long v)) {
                    return new ParameterResolverValue(v.ToString(), EResolverValueType.ContentType, (uint)EValueDataType.Text);
                } else {
                    throw new InvalidCastException("Cannot cast the input to integer!");
                }
            } else {
                // This result will cause excepton if not handled in some special manner.
                return new ParameterResolverValue(null, EResolverValueType.ContentType, (uint)EValueDataType.Text);
            }
        }
        public ParameterResolverValue IsEmpty(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count == 1) {
                var input = args[0].Value;
                if (input == null) return new ParameterResolverValue(true);
                if (input is string sinput) {
                    if (string.IsNullOrEmpty(sinput)) return new ParameterResolverValue(true);
                    return new ParameterResolverValue(false);
                }
                if (input is IEnumerable einput) {
                    foreach (var x in einput) {
                        return new ParameterResolverValue(false);
                    }
                    return new ParameterResolverValue(true);
                }
                var type = input.GetType();
                if (g_INT_TYPES.Any(t => t == type)) {
                    var i = Convert.ToInt64(input);
                    if (i == 0) return new ParameterResolverValue(true);
                    return new ParameterResolverValue(false);
                }
                if (g_FLOAT_TYPES.Any(t => t == type)) {
                    var d = Convert.ToDouble(input);
                    if (d == 0) return new ParameterResolverValue(true);
                    return new ParameterResolverValue(false);
                }
                return new ParameterResolverValue(false);
            }
            return new ParameterResolverValue(true);
        }
        public ParameterResolverValue Skip(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            return new ParameterResolverValue(null, EResolverValueType.Skip);
        }

        public ParameterResolverValue GetHostingUrl(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            KraftGlobalConfigurationSettings settings = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            return new ParameterResolverValue(settings.GeneralSettings.HostingUrl);
        }
        public ParameterResolverValue GlobalSetting(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            KraftGlobalConfigurationSettings settings = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            if (args.Count > 0) {
                if (args[0].Value == null) return new ParameterResolverValue(null);
                var key = Convert.ToString(args[0].Value);
                return new ParameterResolverValue(
                    key switch {
                        "CssSegment" => settings?.GeneralSettings?.KraftUrlCssJsSegment,
                        "RootSegment" => settings?.GeneralSettings?.KraftUrlSegment,
                        "ResourceSegment" => settings?.GeneralSettings?.KraftUrlResourceSegment,
                        "ModuleImages" => settings?.GeneralSettings?.KraftUrlModuleImages,
                        "ModulePublic" => settings?.GeneralSettings?.KraftUrlModulePublic,
                        "Theme" => settings?.GeneralSettings?.Theme,
                        "HostKey" => settings?.GeneralSettings?.ServerHostKey,
                        "StartModule" => settings?.GeneralSettings?.DefaultStartModule,
                        "SignalRHub" => settings?.GeneralSettings?.SignalRSettings?.HubRoute,
                        "EnvironmentName" => settings?.EnvironmentSettings?.EnvironmentName,
                        "ContentRootPath" => settings?.EnvironmentSettings?.ContentRootPath,
                        "ApplicationName" => settings?.EnvironmentSettings?.ApplicationName,
                        "ClientId" => settings?.GeneralSettings?.ClientId,
                        "HostingUrl" => settings?.GeneralSettings?.HostingUrl,
                        _ => null
                    }
                );

            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue ModuleName(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Module);
        }
        /// <summary>
        /// UrlBase(options)
        /// options - string: (("action" | "resource") | ("read" | "write" | "new")) [, ("module" | module) [, ("images | "public")]]
        /// action - sets the same as current action
        /// resource - puts resource segment name
        /// read, write, new - adds the action in the path
        /// module - adds the current module name, any other word is treated as explicitly provided module name.
        /// images, public - add the corresponding segments.
        /// 
        /// Examples (assume current module is module1 and action is read and typical settings for segment names):
        /// UrlBase("new , module2") -> /node/new/module2/
        /// UrlBase("new , module") -> /node/new/module1/
        /// UrlBase("write , module") -> /node/write/module1/
        /// UrlBase("resource, module, images") -> /node/raw/module1/images/
        /// 
        /// </summary>
        /// <returns></returns>
        public ParameterResolverValue UrlBase(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count > 0) {
                var options = args[0].Value != null ? Convert.ToString(args[0].Value) : null;
                KraftGlobalConfigurationSettings settings = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
                StringBuilder path = new StringBuilder();
                path.Append("/").Append(settings.GeneralSettings.KraftUrlSegment).Append("/");
                if (options != null) {
                    Regex rex = new Regex(@"(?:(?:(action|resource)|(read|write|new))(?:\s*,\s*(?:(module)(?:\s*,\s*(images|public))?)?)?)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    var match = rex.Match(options);
                    if (match.Success) {
                        for (var i = 1; i < match.Groups.Count; i++) {
                            if (match.Groups[i].Success) {
                                if (match.Groups[i].Value == "action") {
                                    path.Append(ctx.Action.ToLower()).Append("/");
                                } else if (match.Groups[i].Value == "resource") {
                                    path.Append(settings?.GeneralSettings?.KraftUrlResourceSegment).Append("/");
                                }
                                if (i == 2 && match.Groups[i].Success) {
                                    path.Append(match.Groups[i].ValueSpan).Append("/");
                                }
                                if (match.Groups[i].Value == "module") {
                                    path.Append(ctx.ProcessingContext.InputModel.Module).Append("/");
                                }
                                if (match.Groups[i].Value == "images") {
                                    path.Append(settings?.GeneralSettings?.KraftUrlModuleImages).Append("/");
                                } else if (match.Groups[i].Value == "public") {
                                    path.Append(settings?.GeneralSettings?.KraftUrlModulePublic).Append("/");
                                }
                            }

                        }

                    }
                }
                return new ParameterResolverValue(path.ToString());
            }
            return new ParameterResolverValue(null);
        }
        #region Call type and purpose queries into the server variables
        /// <summary>
        /// RequestType()
        /// Returns the type of request see ECallType
        /// 0 - WebRequest
        /// 1 - DirectCall
        /// 2 - Signal
        /// 3 - ServiceCall (indirect/sheduled call)
        /// </summary>
        /// <returns></returns>
        public ParameterResolverValue RequestType(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var inputModel = ctx.ProcessingContext.InputModel;
            int call_type = 0;
            if (inputModel.Server != null && inputModel.Server.TryGetValue(CallTypeConstants.REQUEST_CALL_TYPE, out object val)) {
                call_type = Convert.ToInt32(val);
            }
            return new ParameterResolverValue(call_type);
        }
        public ParameterResolverValue RequestProcessor(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var inputModel = ctx.ProcessingContext.InputModel;
            if (inputModel.Server != null && inputModel.Server.TryGetValue(CallTypeConstants.REQUEST_PROCESSOR, out object val)) {
                return new ParameterResolverValue(val);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue RequestTask(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var inputModel = ctx.ProcessingContext.InputModel;
            if (inputModel.Server != null && inputModel.Server.TryGetValue(CallTypeConstants.TASK_KIND, out object val)) {
                return new ParameterResolverValue(val);
            }
            return new ParameterResolverValue(null);
        }
        #endregion

        public ParameterResolverValue NewGuid(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            return new ParameterResolverValue(Guid.NewGuid().ToString());
        }

        public ParameterResolverValue GetUserId(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            InputModel inputModel = ctx.ProcessingContext.InputModel;
            return new ParameterResolverValue(inputModel.SecurityModel?.UserName);
        }

        public ParameterResolverValue GetAuthBearerToken(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            IHttpContextAccessor httpContextAccessor = ctx.PluginServiceManager.GetService<IHttpContextAccessor>(typeof(HttpContextAccessor));

            Task<string> accessTokenTask = httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (accessTokenTask.IsFaulted)//occurs when no authentication is included
            {
                return new ParameterResolverValue(null);
            }
            return new ParameterResolverValue(accessTokenTask.Result);
        }

        public ParameterResolverValue GetUserEmail(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            InputModel inputModel = ctx.ProcessingContext.InputModel;
            return new ParameterResolverValue(inputModel.SecurityModel?.UserEmail);
        }

        public ParameterResolverValue GetUserDetails(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            ResolverArguments<ParameterResolverValue> argsCasted = args as ResolverArguments<ParameterResolverValue>;
            string paramName = argsCasted[0].Value as string;
            var inputModel = ctx.ProcessingContext.InputModel;
            switch (paramName) {
                case "firstname": {
                        return new ParameterResolverValue(inputModel.SecurityModel.FirstName);
                    }
                case "lastname": {
                        return new ParameterResolverValue(inputModel.SecurityModel.LastName);
                    }
                default:
                    break;
            }
            throw new Exception($"The requested parameter with {paramName} is not supported.");
        }

        public ParameterResolverValue GetAuthorizationPasswordEndPoint(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
        {
            KraftGlobalConfigurationSettings settings = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            string authority = settings.GeneralSettings.Authority;
            authority = authority.TrimEnd('/');
            return new ParameterResolverValue(authority + "/account/forgotpassword", EValueDataType.Text);
        }

        public ParameterResolverValue GetUserRoles(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            InputModel inputModel = ctx.ProcessingContext.InputModel;
            return new ParameterResolverValue(inputModel.SecurityModel.Roles, EValueDataType.Text);
        }

        public ParameterResolverValue HasRoleName(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string roleName = args[0].Value as string;
            if (string.IsNullOrEmpty(roleName)) {
                throw new Exception("HasRoleName expects a non-empty string parameter for rolename");
            }
            ISecurityModel securityModel = ctx.ProcessingContext.InputModel.SecurityModel;
            return new ParameterResolverValue(securityModel.IsInRole(args[0].Value as string), EValueDataType.Boolean);
        }

        public ParameterResolverValue Or(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            object resultLeft = args[0].Value;
            object resultRight = args[1].Value;
            return TrueLike(resultLeft) || TrueLike(resultRight) ? new ParameterResolverValue(1) : new ParameterResolverValue(0);
        }
        public ParameterResolverValue And(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            object resultLeft = args[0].Value;
            object resultRight = args[1].Value;
            return TrueLike(resultLeft) && TrueLike(resultRight) ? new ParameterResolverValue(1) : new ParameterResolverValue(0);
        }
        public ParameterResolverValue Not(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            object resultLeft = args[0].Value;

            return TrueLike(resultLeft) ? new ParameterResolverValue(0) : new ParameterResolverValue(1);
        }
        public ParameterResolverValue Concat(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string resultLeft = (args[0].Value != null) ? args[0].Value.ToString() : "";
            string resultRight = (args[1].Value != null) ? args[1].Value.ToString() : "";
            return new ParameterResolverValue(resultLeft + resultRight);
        }
        public ParameterResolverValue Replace(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string baseString = args[0].Value as string;
            string replaceKey = args[1].Value as string;
            string replaceWith = (args[2].Value != null) ? args[2].Value.ToString() : "";

            if (string.IsNullOrEmpty(baseString) || string.IsNullOrEmpty(replaceKey)) {
                return new ParameterResolverValue(baseString);
            }
            var result = baseString.Replace(replaceKey, replaceWith);
            return new ParameterResolverValue(result);
        }
        /// <summary>
        /// If not null return the first value and the second otherwise
        /// Coalesce(v1,v2)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue Coalesce(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args[0].Value == null) {
                return new ParameterResolverValue(args[1].Value);
            } else {
                return new ParameterResolverValue(args[0].Value);
            }
        }
        /// <summary>
        /// Argument count: 2 (number|null, string)
        /// Return the value as textual representation suitable for insertion in SQL and other queries processed by 
        /// plugins consuming the expression where this resolver is used.
        /// NumAsText(v)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns>String representation of the number - the default generated by C# ToString. 
        /// If the value is not one of a supported numeric type the resolver will use its second argument and if it is string that
        /// string marked for insertion will be returned. In all other cases the string "null" marked for insertion is returned.</returns>
        public ParameterResolverValue NumAsText(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var x = args[0].Value;
            if (x == null) {
                return new ParameterResolverValue("null", EResolverValueType.ContentType);
            } else if (x.GetType() == typeof(int) || x.GetType() == typeof(long) || x.GetType() == typeof(uint) || x.GetType() == typeof(Int16) ||
                x.GetType() == typeof(Int32) || x.GetType() == typeof(Int64) || x.GetType() == typeof(UInt16) || x.GetType() == typeof(UInt32) || x.GetType() == typeof(UInt64) ||
                x.GetType() == typeof(float) || x.GetType() == typeof(double)) {
                return new ParameterResolverValue(x.ToString(), EResolverValueType.ContentType);
            } else {
                if (!(args[1].Value is string y)) y = "null";
                return new ParameterResolverValue(y, EResolverValueType.ContentType);
            }
        }
        /// <summary>
        /// Argument count: 1
        /// Returns the passed value as a string marked as CONTENT_TYPE - for insertion/replacement in the query of the corresponding plugin 
        /// consuming the expression in which the final value is returned through this routine
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">Single argument is expected. The resolver will convert some types to string, 
        /// but will do it in general manner. It is recommended to pass strings.</param>
        /// <returns>Returns the argument as string marked for insertion. If it is null the "null" string marked for insertion is returned.</returns>
        public ParameterResolverValue AsContent(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var x = args[0].Value;
            string resultval;
            if (x == null) {
                return new ParameterResolverValue("null", EResolverValueType.ContentType);
            } else if (x.GetType() != typeof(string)) {
                resultval = x.ToString();
            } else {
                resultval = x as string;
            }
            return new ParameterResolverValue(resultval, EResolverValueType.ContentType);
        }
        /// <summary>
        ///     Returns the first argument only if it  converted to string matches the regular expression defined by the second
        /// Arguments:
        ///     0 - First parameter
        ///     1 - Second parameter
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue CheckedText(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args[0].Value == null) return new ParameterResolverValue(null, EResolverValueType.Invalid);
            var text = args[0].Value.ToString();

            string refield = args[2].Value?.ToString() ?? null;
            if (refield == null) {
                throw new ArgumentException("2-d argument of CheckedText is required and has to specify a regular expression for the first argument's validation.");
            }
            Regex reField = new Regex(refield, RegexOptions.IgnoreCase);
            if (reField.IsMatch(text)) {
                // Returned as content type to help use it directly (not recommended though - use it as argument to OrderBy)
                return new ParameterResolverValue(text, EResolverValueType.ContentType);
            }
            return new ParameterResolverValue(null, EResolverValueType.Invalid);
        }
        /// <summary>
        /// Generates an order by entry FIELDNAME ASC|DESC
        /// Arguments:
        ///     0 - Fieldname
        ///     1 - ASC|DESC|1|-1
        ///     2 - Regexp tester for the Fieldname (applied with ignore case)
        ///     
        ///  If fieldname is null or empty returns null(hm?)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue OrderByEntry(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args[0].Value == null) return new ParameterResolverValue(null, EResolverValueType.Invalid);
            var fieldname = args[0].Value.ToString();
            var _dir = args[1].Value;
            string dir = "ASC";
            if (_dir != null) {
                if (_dir is string sdir) {
                    // string
                    if (__reAscDesc.IsMatch(sdir)) {
                        dir = sdir.ToUpper();
                    } else {
                        if (double.TryParse(sdir, NumberStyles.Any, CultureInfo.InvariantCulture, out var ddir)) {
                            if (ddir < 0) dir = "DESC";
                        }
                    }
                } else {
                    if (double.TryParse(_dir.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var xdir)) {
                        if (xdir < 0) dir = "DESC";
                    }
                }
            }
            string refield = args[2].Value?.ToString() ?? null;
            if (refield == null) {
                throw new ArgumentException("3-d argument of OrderByEntry is required and has to specify a regular expression for the field name validation.");
            }
            Regex reField = new Regex(refield, RegexOptions.IgnoreCase);
            if (reField.IsMatch(fieldname)) {
                // Returned as content type to help use it directly (not recommended though - use it as argument to OrderBy)
                return new ParameterResolverValue(String.Format("{0} {1}", fieldname, dir), EResolverValueType.ContentType);
            }
            return new ParameterResolverValue(null, EResolverValueType.Invalid);
        }
        private readonly Regex __reAscDesc = new Regex(@"asc|desc", RegexOptions.IgnoreCase);
        /// <summary>
        /// This one deals with any number of arguments and how many it accepts depends on the declaration!
        /// Produces an ORDER BY clause containing all the entries. If none of the entries resolves to something - empty string is returned
        /// </summary>
        /// <example>
        ///     OrderBy(OrderByEntry(GetFrom('client','field1'),GetFrom('client','field1dir'),'FirstName|LastName|Title'))
        ///     // A way to have shorter expressions
        ///     parameters: [
        ///         { name: "field1", Expression: "GetFrom('client',name)" },
        ///         { name: "field1dir", Expression: "GetFrom('client',name)" },
        ///         { name: "orderclause", Expression: "OrderBy(OrderByEntry(field1,field1dir,'FirstName|LastName|Title'))" }
        ///     ]
        /// </example>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue OrderBy(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            StringBuilder sb = new StringBuilder("ORDER BY ");
            string coma = "";
            bool bSuccess = false;
            for (var i = 0; i < args.Count; i++) {
                var arg = args[i];
                if (arg.ValueType == EResolverValueType.ValueType || arg.ValueType == EResolverValueType.ContentType) {
                    if (arg.Value is string) {
                        sb.AppendFormat("{0} {1}", coma, arg.Value);
                        coma = ",";
                        bSuccess = true;
                    }
                }
            }
            if (bSuccess) {
                return new ParameterResolverValue(sb.ToString(), EResolverValueType.ContentType);
            } else {
                return new ParameterResolverValue("", EResolverValueType.ContentType);
            }
        }
        public ParameterResolverValue To8601String(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count != 1) throw new ArgumentException("To8601String accepts one argument");
            var dt = args[0].Value;
            if (dt == null) return new ParameterResolverValue(null);
            if (!(dt is DateTime)) {
                try { 
                    dt = Convert.ToDateTime(dt);
                } catch {
                    return new ParameterResolverValue(null);
                }
            }
            if (dt is DateTime vdt) {
                return new ParameterResolverValue(vdt.ToUniversalTime().ToString("u").Replace(" ", "T"));
            }
            return new ParameterResolverValue(null); // This should not happen
        }
        /// <summary>
        /// CastAs(type, value)
        /// Casts the input value to the specified type. The available types are: uint, double, string, null and int.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue CastAs(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string stype = args[0].Value as string;
            object v = args[1].Value;
            if (v == null) {
                if (stype == T_BOOLEAN) {
                    return new ParameterResolverValue(false);
                }
                return new ParameterResolverValue(null);
            }

            if (stype == T_DATETIME) {
                if (v is DateTime) {
                    return new ParameterResolverValue(v);
                } else if (v is string) {
                    if (DateTime.TryParse(v as string, out DateTime dtr)) {
                        return new ParameterResolverValue(dtr);
                    }
                }
                try {
                    v = Convert.ToDateTime(v);
                    return new ParameterResolverValue(v);
                } catch {
                    return new ParameterResolverValue(null);
                }
            }
            if (stype == T_DATETIMEUTC) {
                if (v is DateTime vdt) {
                    return new ParameterResolverValue(vdt.ToUniversalTime());
                } else if (v is string) {
                    if (DateTime.TryParse(v as string, out DateTime dtr)) {
                        dtr = dtr.ToUniversalTime();
                        return new ParameterResolverValue(dtr);
                    }
                }
                try {
                    v = Convert.ToDateTime(v).ToUniversalTime();
                    return new ParameterResolverValue(v);
                } catch {
                    return new ParameterResolverValue(null);
                }
            }
            if (stype == T_DATETIMEASUTC) {
                if (v is DateTime vdt) {
                    return new ParameterResolverValue(vdt.ToUniversalTime());
                } else if (v is string) {
                    if (DateTime.TryParse(v as string, out DateTime dtr)) {
                        dtr = dtr.ToUniversalTime();
                        return new ParameterResolverValue(dtr);
                    }
                }
                try {
                    v = Convert.ToDateTime(v).ToUniversalTime();
                    return new ParameterResolverValue(v);
                } catch {
                    return new ParameterResolverValue(null);
                }
            }
            string sv = v.ToString();
            // If conversion is to string - just do it.
            if (stype == T_STR) return new ParameterResolverValue(sv, EValueDataType.Text);
            if (string.IsNullOrWhiteSpace(sv)) return new ParameterResolverValue(null);
            if (stype == T_BOOLEAN) {
                if (TrueLike(sv)) return new ParameterResolverValue(true);
                return new ParameterResolverValue(false);
            }
            // This has to be here to support legacy behavior
            Number_Formats fmt = DetectNumberFormat(sv, out string clean_string_value);
            if (fmt == Number_Formats.Unknown) {
                throw new Exception("Cannot detect the number format of the second argument");
            }
            if (stype != null) {
                switch (stype) {
                    case T_INT:
                        int i_val;
                        if (fmt == Number_Formats.Decimal) {
                            if (int.TryParse(clean_string_value, NumberStyles.Any, CultureInfo.InvariantCulture, out i_val)) {
                                return new ParameterResolverValue(i_val, EValueDataType.Int);
                            } else {
                                throw new ArgumentException("CastAs cannot convert the value to long integer");
                            }
                        } else {
                            return new ParameterResolverValue(Convert.ToInt32(clean_string_value, (int)fmt), EValueDataType.Int);
                        }
                    case T_UINT:
                        uint u_val;
                        if (fmt == Number_Formats.Decimal) {
                            if (uint.TryParse(clean_string_value, NumberStyles.Any, CultureInfo.InvariantCulture, out u_val)) {
                                return new ParameterResolverValue(u_val, EValueDataType.UInt);
                            } else {
                                throw new ArgumentException("CastAs cannot convert the value to uint");
                            }
                        } else {
                            return new ParameterResolverValue(Convert.ToUInt32(clean_string_value, (int)fmt), EValueDataType.UInt);
                        }
                    case T_DBL:
                        double d_val;
                        if (double.TryParse(clean_string_value, NumberStyles.Any, CultureInfo.InvariantCulture, out d_val)) {
                            return new ParameterResolverValue(d_val, EValueDataType.Real);
                        } else {
                            throw new ArgumentException("CastAs cannot convert the value to double");
                        }
                    default:
                        throw new ArgumentException("CastAs cannot understand the type specified: " + stype);
                }
            } else {
                throw new ArgumentException("The first argument to CastAs must be a string that specify the type to cast the second value to. Supported are: int, uint, double, string");
            }
        }
        /// <summary>
        /// Caches the first execution and returns the cached value if available
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ParameterResolverValue Once(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (args.Count > 1) {
                var name = Convert.ToString(args[0].Value);
                if (!string.IsNullOrEmpty(name)) {
                    if (ctx is IActionHelpers helper) {
                        var cache = helper.NodeCache;
                        if (cache != null) {
                            if (cache.ContainsKey(name)) {
                                return cache[name];
                            }
                            var ret = args[1];
                            cache[name] = ret;
                            return ret;
                        }
                    }
                    return args[1];
                } else {
                    throw new ArgumentException("First argument of Once must be string - the name of the cached value.");
                }
            } else {
                throw new ArgumentException("Once needs 2 arguments, check the registration.");
            }
        }
        /// <summary>
        /// Generates a random integer number between the two parameters
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue Random(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            int min = 1;
            int max = 100;

            if (args.Count > 0) {
                if (args[0].Value is int n) {
                    min = n;
                } else if (args[0].Value is long l) {
                    min = (int)l;
                }
                if (args.Count > 1) {
                    if (args[1].Value is int maxi) {
                        max = maxi;
                    } else if (args[1].Value is long maxl) {
                        max = (int)maxl;
                    }
                    if (min >= max) {
                        throw new ArgumentException("Min value is greater or equals than Max value.");
                    }
                }
            }

            return new ParameterResolverValue(RandomNumberGenerator.GetInt32(min, max));
        }


        /// <summary>
        /// Resolver summing TWO numbers, returns number as Value (not Content)
        /// The value is converted to the most preferable type in this order (low to high):
        /// unit int double
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">2 - numbers/strings</param>
        /// <returns>numeric</returns>
        public ParameterResolverValue Add(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string totype = DetectBestNumericType(args.ToArray());
            if (totype == T_NULL) return new ParameterResolverValue(null);
            ParameterResolverValue[] values = args.ToArray(); //new ParameterResolverValue[args.Count];
            ParameterResolverValue[] _valsCasted = values.Select(v => CastAs(ctx, new List<ParameterResolverValue>() { new ParameterResolverValue(totype), new ParameterResolverValue(v.Value) })).ToArray();
            return totype switch {
                T_INT => new ParameterResolverValue(_valsCasted.Sum(x => (int)x.Value), EValueDataType.Int),
                T_UINT => new ParameterResolverValue(_valsCasted.Sum(x => (uint)x.Value), EValueDataType.UInt),
                T_DBL => new ParameterResolverValue(_valsCasted.Sum(x => (double)x.Value), EValueDataType.Real),
                _ => new ParameterResolverValue(null),
            };
        }

        public ParameterResolverValue Sub(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string totype = DetectBestNumericType(args.ToArray());
            if (totype == T_NULL) return new ParameterResolverValue(null);
            ParameterResolverValue[] values = args.ToArray();  //new ParameterResolverValue[args.Count];
            ParameterResolverValue[] _valsCasted = values.Select(v => CastAs(ctx, new List<ParameterResolverValue>() { new ParameterResolverValue(totype), new ParameterResolverValue(v.Value) })).ToArray();

            return totype switch {
                T_INT => new ParameterResolverValue((int)_valsCasted[0].Value - (int)_valsCasted[1].Value, EValueDataType.Int),
                T_UINT => new ParameterResolverValue((uint)_valsCasted[0].Value - (uint)_valsCasted[1].Value, EValueDataType.UInt),
                T_DBL => new ParameterResolverValue((double)_valsCasted[0].Value - (double)_valsCasted[1].Value, EValueDataType.Real),
                _ => new ParameterResolverValue(null),
            };
        }
        public ParameterResolverValue IfThenElse(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var condition = args[0].Value;
            var positive = args[1].Value;
            var negative = args[2].Value;

            if (condition != null && int.TryParse(condition.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int ncondition)) {
                if (ncondition != 0) {
                    return new ParameterResolverValue(positive);
                }
                return new ParameterResolverValue(negative);
            } else {
                return new ParameterResolverValue(negative);
            }

        }
        public ParameterResolverValue Equal(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {

            int? i = null;
            double? d = null;
            uint? u = null;
            string s = null;

            var useType = DetectBestNumericTypeEx(args.ToArray());
            if (useType == null) useType = T_STR;
            return useType switch {
                T_INT => new ParameterResolverValue(args.All(a => {
                    if (i == null) {
                        i = Convert.ToInt32(a.Value); return true;
                    } else {
                        return Convert.ToInt32(a.Value) == i;
                    }
                })),
                T_UINT => new ParameterResolverValue(args.All(a => {
                    if (u == null) {
                        u = Convert.ToUInt32(a.Value); return true;
                    } else {
                        return Convert.ToUInt32(a.Value) == u;
                    }
                })),
                T_DBL => new ParameterResolverValue(args.All(a => {
                    if (d == null) {
                        d = Convert.ToDouble(a.Value); return true;
                    } else {
                        return Convert.ToDouble(a.Value) == d;
                    }
                })),
                T_STR => new ParameterResolverValue(args.All(a => {
                    if (s == null) {
                        s = Convert.ToString(a.Value); return true;
                    } else {
                        return string.CompareOrdinal(Convert.ToString(a.Value), s) == 0;
                    }
                })),
                _ => new ParameterResolverValue(false)
            };
        }
        public ParameterResolverValue Greater(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var v1 = args[0];
            var v2 = args[1];
            var useType = DetectBestNumericType(new ParameterResolverValue[] { v1, v2 });
            return new ParameterResolverValue(useType switch {
                T_INT => Convert.ToInt32(v1.Value) > Convert.ToInt32(v2.Value),
                T_UINT => Convert.ToUInt32(v1.Value) > Convert.ToUInt32(v2.Value),
                T_DBL => Convert.ToDouble(v1.Value) > Convert.ToDouble(v2.Value),
                T_STR => String.CompareOrdinal(Convert.ToString(v1.Value), Convert.ToString(v2.Value)) > 0,
                _ => false

            });
        }
        public ParameterResolverValue Lower(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            var v1 = args[0];
            var v2 = args[1];
            var useType = DetectBestNumericType(new ParameterResolverValue[] { v1, v2 });
            return new ParameterResolverValue(useType switch {
                T_INT => Convert.ToInt32(v1.Value) < Convert.ToInt32(v2.Value),
                T_UINT => Convert.ToUInt32(v1.Value) < Convert.ToUInt32(v2.Value),
                T_DBL => Convert.ToDouble(v1.Value) < Convert.ToDouble(v2.Value),
                T_STR => String.CompareOrdinal(Convert.ToString(v1.Value), Convert.ToString(v2.Value)) < 0,
                _ => false

            });
        }

        /// <summary>
        /// Resolver to get an auth token, from the authorization server, for the currently supported providers.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args">1- arg - provider name as understood by the authorization server</param>
        /// <returns></returns>
        public ParameterResolverValue ApiTokenFromAuth(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            string provider = args[0].Value as string;
            if (string.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("Provider is null!");

            // 1. Read the custom settings from appsettings_XXX.json -> get the auth server address
            KraftGlobalConfigurationSettings settings = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));

            // 1.1 - construct the endpoint address for the token API method
            string url = settings.GeneralSettings.Authority + "/api/accesstoken?lp=" + provider;

            // 2. Make the call
            // 2.1 Wait and get the token from ret data
            // This should reside elsewhere / or reuse some existing?
            using HttpClient client = new HttpClient(new HttpClientHandler());
            IHttpContextAccessor accessor = ctx.PluginServiceManager.GetService<IHttpContextAccessor>(typeof(HttpContextAccessor));
            string our_token = accessor.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken).Result;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", our_token);

            // Why global?
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            client.Timeout = new TimeSpan(0, 0, 10);
            using HttpResponseMessage response = client.Send(request, HttpCompletionOption.ResponseHeadersRead, new System.Threading.CancellationToken());
            if (response.IsSuccessStatusCode) {
                JsonSerializer js = new JsonSerializer();

                Dictionary<string, object> res = js.Deserialize<Dictionary<string, object>>
                    (new JsonTextReader(new StreamReader(response.Content.ReadAsStreamAsync().Result)));

                return new ParameterResolverValue(res["access_token"]);
            } else {
                throw new Exception("Communication error while obtaining the provider's token, using the login token to call the authorization server.");
            }
        }

        #region Helpers for standart resolvers
        private readonly Regex NUMFMTCheck = new Regex(
            @"^\s*(?:(\-|\+)?(?:(\d+)(?:(\.)(?:(\d*)?(?:e(\-|\+)?(\d+))?)?)?)|(?:0x([0-9a-fA-F]+))|(?:0o([0-7]+))|(?:0b([01]+))\s*)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ECMAScript
        );
        private Number_Formats DetectNumberFormat(string str, out string cleanNum) {
            Match match = NUMFMTCheck.Match(str);
            if (match.Success) {
                if (match.Groups[2].Success) { // decimal
                    cleanNum = string.Format("{0}{1}{2}{3}{4}{5}",
                        match.Groups[1].Success ? match.Groups[1].Value : string.Empty,
                        match.Groups[2].Success ? match.Groups[2].Value : string.Empty,
                        match.Groups[3].Success ? match.Groups[3].Value : string.Empty,
                        match.Groups[4].Success ? match.Groups[4].Value : string.Empty,
                        match.Groups[5].Success ? match.Groups[5].Value : string.Empty,
                        match.Groups[6].Success ? match.Groups[6].Value : string.Empty
                    );
                    return Number_Formats.Decimal;
                } else if (match.Groups[7].Success) { // hex
                    cleanNum = match.Groups[7].Value;
                    return Number_Formats.Hexdecimal;
                } else if (match.Groups[8].Success) { // Octal
                    cleanNum = match.Groups[8].Value;
                    return Number_Formats.Octal;
                } else if (match.Groups[9].Success) { // Binary
                    cleanNum = match.Groups[9].Value;
                    return Number_Formats.Binary;
                }
            }
            cleanNum = null;
            return Number_Formats.Unknown;
        }
        private string DetectBestNumericTypeEx(params ParameterResolverValue[] vals) {
            int maxtype = -1;
            int n;

            for (var i = 0; i < vals.Length; i++) {
                var v = vals[i];
                if (v.Value == null) return T_NULL;
                // TODO: In future we should support the EValueDataType here, but some standard routines for its handling are necessary first.
                string sv = v.Value.ToString();
                if (string.IsNullOrWhiteSpace(sv)) return T_NULL;
                if (string.Compare(sv, "null", true, CultureInfo.InvariantCulture) == 0) return T_NULL;
                Match match = NUMFMTCheck.Match(sv);
                if (match.Success) {
                    if (match.Groups[2].Success) { // decimal
                        if (match.Groups[3].Success) { // double
                            if (double.TryParse(sv, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) {
                                n = T_TYPEORDER.IndexOf(T_DBL);
                                if (n > maxtype) maxtype = n;
                            }
                        } else { // integer
                            if (int.TryParse(sv, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) {
                                n = T_TYPEORDER.IndexOf(T_INT);
                                if (n > maxtype) maxtype = n;
                            }
                        }
                    } else if (match.Groups[7].Success) { // hex
                        if (maxtype < 0) maxtype = 0;
                    } else if (match.Groups[8].Success) { // Octal
                        if (maxtype < 0) maxtype = 0;
                    } else if (match.Groups[9].Success) { // Binary
                        if (maxtype < 0) maxtype = 0;
                    } else {
                        // This should be impossible
                        return null;
                    }
                } else { // not a number
                    return null;
                    
                }
            }
            return T_TYPEORDER[maxtype];
        }
        private string DetectBestNumericType(params ParameterResolverValue[] vals) {
            string t = DetectBestNumericTypeEx(vals);
            if (t == null) {
                throw new Exception("The parameters of an arithmetic resolver have to be numbers or null");
            }
            return t;
        }
        private bool TrueLike(object v) {
            if (v == null) {
                return false;
            }
            return Convert.ToBoolean(v);
        }
        #endregion
        #endregion Standard resolvers

        #region Arithmetic resolvers

        #endregion

        #region MetaInfo resolvers
        private MetaNode _NavMetaNodes(MetaNode current, int level) {
            MetaNode result = current;
            for (int i = 0; i < level; i++) {
                if (result != null) {
                    result = result.GetParent();
                } else {
                    break;
                }
            }
            return current;
        }
        /// <summary>
        /// Fetches a general node meta information field
        /// MetaNodeInfo(param, level)
        /// param - name of the parameter to return from the metanode:
        ///     name - node name
        ///     step - execution step
        ///     executions - number of executions
        /// level - current or parrent to inspect
        ///     0 - current
        ///     1 - parent
        ///     2 - parent of the parent 
        ///     etc.
        /// </summary>
        public ParameterResolverValue MetaNode(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (ctx is IActionHelpers helper) {
                int level = 0;
                if (args.Count > 1) {
                    level = Convert.ToInt32(args[1].Value);
                }
                if (helper.NodeMeta is MetaNode _node) {
                    MetaNode node = _NavMetaNodes(_node, level);
                    if (node != null && args.Count > 1) {
                        var param = args[0].Value as string;
                        if (param != null) {
                            return new ParameterResolverValue(param switch {
                                "name" => node.Name,
                                "step" => node.Step,
                                "executions" => node.Executions,
                                _ => null
                            });
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        /// <summary>
        /// Fetches meta field from the ADO last execution. Used before the data loader 
        /// will return data from an execution in a previously executed node 
        /// (strongly not recommended in such a context, any solution will be too fragile even if it works correctly in the beginning)
        /// 
        /// MetaADOResult(param, level)
        /// param - name of the parameter to return from the metanode:
        ///     rowsaffected - on select will be -1 on write will be the number of actually affected rows in the corresponding database
        ///     rows - rows fetched
        ///     fields - number of fields in each row.
        /// level - current or parrent to inspect
        ///     0 - current
        ///     1 - parent
        ///     2 - parent of the parent 
        ///     etc.
        /// </summary>
        public ParameterResolverValue MetaADOResult(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (ctx is IActionHelpers helper) {
                int level = 0;
                if (args.Count > 1) {
                    level = Convert.ToInt32(args[1].Value);
                }
                if (helper.NodeMeta is MetaNode _node) {
                    MetaNode node = _NavMetaNodes(_node, level);
                    if (node != null && node.GetInfo<ADOInfo>() is ADOInfo ado) {
                        var lastResult = ado.LastResult;
                        if (args.Count > 0) {
                            var param = args[0].Value as string;
                            if (param != null) {
                                return new ParameterResolverValue(param switch {
                                    "rowsaffected" => ado.RowsAffected,
                                    "rows" => lastResult?.Rows,
                                    "fields" => lastResult?.Fields,
                                    _ => null
                                });
                            }
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }
        /// <summary>
        /// Fetches meta field from the meta tre's root. 
        /// 
        /// MetaRoot(param)
        /// param - name of the parameter to return from the metanode:
        ///     steps - on select will be -1 on write will be the number of actually affected rows in the corresponding database
        ///     flags - rows fetched
        ///     basic - has or not Basic flag set
        ///     trace - has or not Trace flag set
        ///     debug - has or not Debug flag set
        ///     log - has or not Log flag set
        /// </summary>
        public ParameterResolverValue MetaRoot(IParameterResolverContext ctx, IList<ParameterResolverValue> args) {
            if (ctx is IActionHelpers helper) {
                if (helper.NodeMeta is MetaNode node && node.Root is MetaRoot root) {
                    if (args.Count > 1) {
                        var param = args[0].Value as string;
                        if (param != null) {
                            return new ParameterResolverValue(param switch {
                                "steps" => root.Steps,
                                "flags" => (int)root.Flags,
                                "basic" => root.Flags.HasFlag(EMetaInfoFlags.Basic) ? true : false,
                                "trace" => root.Flags.HasFlag(EMetaInfoFlags.Trace) ? true : false,
                                "debug" => root.Flags.HasFlag(EMetaInfoFlags.Debug) ? true : false,
                                "output" => root.Flags.HasFlag(EMetaInfoFlags.Output) ? true : false,
                                "profile" => root.Flags.HasFlag(EMetaInfoFlags.Profile) ? true : false,
                                "log" => root.Flags.HasFlag(EMetaInfoFlags.Log) ? true : false,
                                _ => null
                            });
                        }
                    }
                }
            }
            return new ParameterResolverValue(null);
        }


        #endregion

        #region Test resolvers
        /// <summary>
        /// Digs for a parameter named the same as the name of the expression in a "standart" order
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="inargs"></param>
        /// <returns></returns>
        //public ParameterResolverValue StandardParameter(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs) {
        //    var args = inargs as ResolverArguments<ParameterResolverValue>;
        //    var inputModel = ctx.ProcessingContext.InputModel;
        //    object paramValue = null;
        //    string paramName = args?.Name.Value as string;

        //    void GetParameterValue(IDictionary <string, object> row)
        //    {
        //        if (paramValue == null && row != null && row.ContainsKey(paramName))
        //            paramValue = row[paramName];
        //    }

        //    if (paramValue == null) GetParameterValue(ctx.Row);
        //    if (paramValue == null) GetParameterValue(inputModel.Server);
        //    if (paramValue == null) GetParameterValue(inputModel.Client);
        //    if (paramValue == null) GetParameterValue(inputModel.Data);

        //    return new ParameterResolverValue(paramValue, EValueDataType.any);
        //}
        #endregion

        #region idlist and other list oriented
        /// Splits a string by the specified delimiter (no defaults) or passes the first argument through if it is not a string.
        /// Arguments: 
        ///     - argument to split (see above)
        ///     - delimiter
        /// Returns the first argument or enumerable of strings
        /// Suggested usage with IdList e.g. IdList(Split(parameter,','),null) - will treat the values in comma separated values as numbers
        public ParameterResolverValue Split(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs) {
            if (inargs.Count > 1) {
                var input = inargs[0].Value as string;
                if (input == null) return inargs[0];
                var delimiter = Convert.ToString(inargs[1].Value);
                return new ParameterResolverValue(input.Split(delimiter));
            } else {
                throw new ArgumentException("Split needs two arguments - check the registration of the resolver");
            }
        }




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
        public ParameterResolverValue idlist(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs) {
            return _idlist(ctx, inargs, false);
        }
        public ParameterResolverValue idlistPadded(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs) {
            return _idlist(ctx, inargs, true);
        }
        public ParameterResolverValue _idlist(IParameterResolverContext ctx, IList<ParameterResolverValue> inargs, bool pad) {
            ParameterResolverValue input = inargs[0];
            ParameterResolverValue type_and_check = inargs[1];
            int numValues = 0;
            int padsize = 0;
            if (pad) {
                if (inargs.Count > 2) {
                    padsize = Convert.ToInt32(inargs[2].Value);
                }
            }
            // TODO Apply the padding
            string _padResult(string _result, int resultCount, string padVal = "NULL") {
                if (resultCount < padsize) {
                    StringBuilder sb = new StringBuilder(_result);
                    for (int i = resultCount; i < padsize; i++) {
                        if (sb.Length > 0) sb.Append(',');
                        sb.Append(padVal);
                    }
                    return sb.ToString();
                }
                return _result;
            }
            StringBuilder sbresult = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(type_and_check.Value as string)) { // RegExp
                var re = type_and_check.Value as string;
                Regex rex = new Regex(re, RegexOptions.CultureInvariant | RegexOptions.Singleline);
                IEnumerable indata;
                if (input.Value is string str) { // single string
                    if (rex.IsMatch(str)) {
                        return new ParameterResolverValue(_padResult(string.Format("'{0}'", str.Replace("'", "''")), 1), EResolverValueType.ContentType);
                    } else {
                        return new ParameterResolverValue(_padResult("NULL", 1), EResolverValueType.ContentType);
                    }
                } else if (input.Value is IDictionary) {
                    indata = (input.Value as IDictionary).Values; // Only values (e.g. object with Id-s)
                } else {
                    indata = input.Value as IEnumerable; // Array of values (keys)
                }
                if (indata != null) { // Enumerate multiple values
                    numValues = 0;
                    foreach (var v in indata) {
                        if (v != null) {
                            if (rex.IsMatch(v.ToString())) {
                                if (sbresult.Length > 0) sbresult.Append(',');
                                sbresult.AppendFormat("'{0}'", v.ToString().Replace("'", "''"));
                                numValues++;
                            } else {
                                //don't stop execution when an item doesn't match?
                                throw new Exception("an item does not match YOUR regular expression in idlist or idlistPadded");
                            }
                        } else {
                            throw new Exception("null item in a collection while converting to replacable idlist");
                        }
                    }
                }
                if (sbresult.Length == 0) {
                    return new ParameterResolverValue(_padResult("NULL", 1), EResolverValueType.ContentType);
                } else {
                    return new ParameterResolverValue(_padResult(sbresult.ToString(), numValues), EResolverValueType.ContentType);
                }
            } else if (type_and_check.Value == null) { // Numbers
                IEnumerable indata;
                Regex rex = new Regex(@"^(\+-)?\d+(\.(\d+)?)?$", RegexOptions.CultureInvariant | RegexOptions.Singleline);
                if (input.Value == null) return new ParameterResolverValue("NULL", EResolverValueType.ContentType);
                var vtype = input.Value.GetType();
                var _input = input.Value;
                if (g_NUMERIC_TYPES.Any(t => t == vtype)) {
                    indata = new Object[] { _input };
                } else if (_input is IDictionary) {
                    indata = (_input as IDictionary).Values;
                } else {
                    indata = _input as IEnumerable;
                }

                numValues = 0;
                if (indata != null) {

                    foreach (var v in indata) {
                        if (sbresult.Length > 0) sbresult.Append(',');
                        if (v is int || v is Int16 || v is Int32 || v is Int64 || v is sbyte) {
                            sbresult.Append(Convert.ToInt64(v).ToString(CultureInfo.InvariantCulture));
                        } else if (v is uint || v is UInt16 || v is UInt32 || v is UInt64 || v is byte) {
                            sbresult.Append(Convert.ToUInt64(v).ToString(CultureInfo.InvariantCulture));
                        } else if (v is decimal) {
                            sbresult.Append(Convert.ToDecimal(v).ToString(CultureInfo.InvariantCulture));
                        } else if (v is float || v is double) {
                            sbresult.Append(Convert.ToDouble(v).ToString(CultureInfo.InvariantCulture));
                        } else if (v is string s && !string.IsNullOrWhiteSpace(s) && rex.IsMatch(s)) {
                            if (s.Contains('.')) {
                                sbresult.Append(Convert.ToDouble(s).ToString(CultureInfo.InvariantCulture));
                            } else {
                                sbresult.Append(Convert.ToInt64(s).ToString(CultureInfo.InvariantCulture));
                            }
                        } else if (v == null) {
                            // Nothing - we just skip it
                        } else {
                            throw new Exception("Non-numeric (or numbers in string)  item found in the input");
                        }
                        numValues++;
                    }
                }
                if (sbresult.Length == 0) {
                    return new ParameterResolverValue(_padResult("NULL", 1), EResolverValueType.ContentType);
                } else {
                    return new ParameterResolverValue(_padResult(sbresult.ToString(), numValues), EResolverValueType.ContentType);
                }
            } else {
                throw new Exception("Unacceptable type parameter or the value is not enumerable");
            }
        }
        #endregion
    }
}
