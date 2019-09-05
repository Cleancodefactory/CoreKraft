using System;

namespace Ccf.Ck.SysPlugins.Data.UserProfileManager.Http
{
    internal class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }
}
