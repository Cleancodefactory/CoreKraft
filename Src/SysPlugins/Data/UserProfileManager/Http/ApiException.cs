using System;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager.Http
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }
}
