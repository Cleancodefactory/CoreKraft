using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Models.Settings
{
    public class CorsAllowedOrigins
    {
        private const string _DefaultAllowHeaders = "Origin, X-Requested-With, Content-Type, Accept";
        private const string _DefaultAllowMethods = "GET, POST, PUT, DELETE, OPTIONS";
        private const string _DefaultAllowedOrigin = "*";
        public CorsAllowedOrigins()
        {
            WithOrigins = new List<string>();
            AllowMethods = new List<AllowMethod>();
        }
        public bool Enabled { get; set; }
        public List<string> WithOrigins { get; set; }
        public List<AllowMethod> AllowMethods { get; set; }
        public AllowMethod GetAllowMethod(string name)
        {
            AllowMethod returnMethod = AllowMethods.Find(m => m.Method.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (returnMethod == null && name.Equals("OPTIONS", System.StringComparison.OrdinalIgnoreCase))
            {
                returnMethod = new AllowMethod { Method = "OPTIONS", AllowHeaders = _DefaultAllowHeaders, AllowCredentials = true };
            }
            return returnMethod;
        }

        public string GetAllowedOrigins(HttpRequest httpRequest)
        {
            //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Origin
            if (WithOrigins.Count == 0)
            {
                return _DefaultAllowedOrigin;
            }
            else if (WithOrigins.Count == 1) {
                return WithOrigins.FirstOrDefault();
            }
            else
            {
                //<origin> - see above link
                //Specifies an origin. Only a single origin can be specified.
                //If the server supports clients from multiple origins,
                //it must return the origin for the specific client making the request.
                return httpRequest.Headers["Origin"];
            }
        }

        public string GetAllowMethods()
        {
            if (AllowMethods.Count == 0)
            {
                return _DefaultAllowHeaders;
            }
            return string.Join(",", AllowMethods.Select(n => n.Method));
        }
    }

    public class AllowMethod
    {
        public string Method { get; set; }
        public string AllowHeaders { get; set; }
        public bool AllowCredentials { get; set; }
    }
}
