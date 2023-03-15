using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class RedirectController : Controller
    {
        private const string NODE_ADDRESS_NAME = "$node_address"; // <module>/<nodeset>[/<node_path1.nodepath2....>]
        private const string ACTION_NAME = "$reason";
        private static readonly string[] OUR_NAMES = new String[] { NODE_ADDRESS_NAME, ACTION_NAME };

        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        public RedirectController(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        [HttpGet]
        public IActionResult Index() {
            return View("Home/Error");
        }

        private static readonly Regex _NodeExpr = new Regex(@"^([a-zA-Z0-9_\-]+)/([a-zA-Z0-9_\-]+)(?:/([a-zA-Z0-9_\-\.]+))?$");
        private static Regex _ReJSONMedia = new Regex("^.+/json.*$");
        private static bool _ParseNodeAddress(string addr, InputModel model) {
            var match = _NodeExpr.Match(addr);
            if (match.Success) {
                model.Module = match.Groups[1].Value;
                model.Nodeset = match.Groups[2].Value;
                if (match.Groups[3].Success) {
                    model.Nodepath = match.Groups[3].Value;
                } else {
                    model.Nodepath = null;
                }
                return true;
            }
            return false;
        }

        [HttpGet,HttpPost]
        public IActionResult Return() {
            // TODO: Consider if we need to accept answers only for authenticated users when required by _KraftGlobalConfigurationSettings
            Ccf.Ck.Models.DirectCall.InputModel inpModel = new InputModel() {
                IsWriteOperation = true,
                QueryCollection = new Dictionary<string, object>()
            };
            var values = HttpContext.Request.Query[NODE_ADDRESS_NAME];
            if (values.Count < 1 || !_ParseNodeAddress(values[0], inpModel)) {
                return View("Home/Error");
            }
            values = HttpContext.Request.Query[ACTION_NAME];
            if (values.Count < 1) {
                inpModel.QueryCollection.Add("reason", "unspecified");
            } else {
                inpModel.QueryCollection.Add("reason", values[0]);
            }
            // ??
            if (this.HttpContext.Request.Method == "GET") {
                foreach (var kv in HttpContext.Request.Query) {
                    if (!OUR_NAMES.Any(n => n == kv.Key)) {
                        if (kv.Value.Count > 1) {
                            inpModel.Data.Add(kv.Key, kv.Value.ToList());
                        } else {
                            inpModel.Data.Add(kv.Key, kv.Value[0]);
                        }
                    }
                }
            } else if (this.HttpContext.Request.Method == "POST") {
                if (HttpContext.Request.HasFormContentType) {
                    foreach (var kv in HttpContext.Request.Form) {
                        if (kv.Value.Count > 1) {
                            inpModel.Data.Add(kv.Key, kv.Value.ToList());
                        } else {
                            inpModel.Data.Add(kv.Key, kv.Value[0]);
                        }
                    }
                } else if (_ReJSONMedia.IsMatch(HttpContext.Request.ContentType)) {
                    // TODO REad json into data
                    throw new NotImplementedException();
                }
            }
            DataStateUtility.Instance.SetUpdated(inpModel.Data);
            
            var retModel = DirectCallService.Instance.Call(inpModel);
            //var retModel = new ReturnModel() { IsSuccessful = true }; // For testing only
            ViewData["returnModel"] = retModel;
            return View(_KraftGlobalConfigurationSettings);
        }
    }
}