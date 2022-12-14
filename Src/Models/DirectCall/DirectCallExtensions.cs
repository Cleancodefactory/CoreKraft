using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.DirectCall {
    public static class DirectCallExtensions {

        #region Call address parse into DirectCall InputModel
        private static Regex reAddress = new Regex(@"^([a-zA-Z][a-zA-Z0-9\-\._]*)/([a-zA-Z][a-zA-Z0-9\-\._]*)(?:/([a-zA-Z][a-zA-Z0-9\-\._]*))?$", RegexOptions.Compiled);
        /// <summary>
        /// Parses the address into module, nodeset, nodepath
        /// 
        /// </summary>
        /// <param name="address">Syntax is: module/nodeset/nodepath</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ParseAddress(this InputModel input, string address) {
            Match m = reAddress.Match(address);
            if (m.Success) {
                if (m.Groups[0].Success) {
                    input.Module = m.Groups[1].Value;
                    if (m.Groups[2].Success) {
                        input.Nodeset = m.Groups[2].Value;
                        if (m.Groups[3].Success) {
                            input.Nodepath = m.Groups[3].Value;
                        } else {
                            input.Nodepath = null;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public static Dictionary<string,object> ToDictionary(this InputModel input) {
            // TODO: Add more stuff
            var result = new Dictionary<string,object>();
            result.Add("module",input.Module);
            result.Add("nodeset", input.Nodeset);
            result.Add("nodepath", input.Nodepath);
            result.Add("write", input.IsWriteOperation);
            result.Add("client", input.QueryCollection);
            result.Add("data", input.Data);
            return result;
        }
        public static Dictionary<string, object> ToDictionary(this ReturnModel ret) {
            var result = new Dictionary<string, object>();
            result.Add("successful", ret.IsSuccessful);
            result.Add("error", ret.ErrorMessage);
            result.Add("data", ret.Data);
            result.Add("binarydata", ret.BinaryData);
            return result;
        }
        #endregion

    }
}
