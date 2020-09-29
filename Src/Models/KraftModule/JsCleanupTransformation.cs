using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Models.KraftModule
{
    class JsCleanupTransformation : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            if (context.EnableOptimizations == true)
            {
                if (response.Content != null && response.Content.Length > 0)
                {
                    string pattern = @"[""|']use strict[""|']\s*;*";
                    RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled;
                    string input = response.Content.ToString();
                    MatchCollection matchCollection = Regex.Matches(input, pattern, options);
                    if (matchCollection.Count > 0)
                    {
                        for (int i = matchCollection.Count - 1; i >= 0; i--)
                        {
                            response.Content.Replace(matchCollection[i].Value, string.Empty, matchCollection[i].Index, matchCollection[i].Value.Length);
                        }
                        response.Content.Insert(0, "'use strict';");
                    }
                }
            }
        }
    }
}
