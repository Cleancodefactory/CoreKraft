using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class Error
    {
        private string _Message = null;
        private int _Code = 0;
        public Error(string message = null)
        {
            _Message = message;
        }
        public Error(int code = 0,string message = null)
        {
            _Message = message;
            _Code = code;
        }
        #region Helpers for internal usage
        public static ParameterResolverValue Create(int code = 0, string message = null) {
            return new ParameterResolverValue(new Error(code, message));
        }
        public static ParameterResolverValue Create(string message = null) {
            return new ParameterResolverValue(new Error(message));
        }

        #endregion

        #region Exported functions
        public static ParameterResolverValue IsError<HostInterface>(HostInterface ctx, ParameterResolverValue[] args) where HostInterface: class
        {
            if (args.Length != 1) throw new ArgumentException("IsError requires an argument");
            if (args[0].Value is Error) return new ParameterResolverValue(true);
            return new ParameterResolverValue(false);
        }
        public static ParameterResolverValue GenError<HostInterface>(HostInterface ctx, ParameterResolverValue[] args) where HostInterface : class
        {
            if (args.Length < 1) throw new ArgumentException("Error requires an argument or two");
            int code = 0;
            string message = null;
            if (DefaultLibraryBase<HostInterface>.IsNumeric(args[0].Value))
            {
                code = Convert.ToInt32(args[0].Value);
                if (args.Length > 1)
                {
                    message = Convert.ToString(args[1].Value);
                }
            } 
            else if (args[0].Value is string str)
            {
                message = str;
            }
            return new ParameterResolverValue(new Error(code, message));
        }
        public static ParameterResolverValue ErrorText<HostInterface>(HostInterface ctx, ParameterResolverValue[] args) where HostInterface : class
        {
            if (args.Length != 1) throw new ArgumentException("ErrorText requires one agument");
            if (args[0].Value is Error err)
            {
                if (string.IsNullOrWhiteSpace(err._Message))
                {
                    return new ParameterResolverValue("unspecified error");
                } else {
                    return new ParameterResolverValue(err._Message);
                }
            } else {
                throw new ArgumentException("ErrorText requires argument containing an error. Use IsError to determine this before calling ErrorText.");
            }
        }
        public static ParameterResolverValue ErrorCode<HostInterface>(HostInterface ctx, ParameterResolverValue[] args) where HostInterface : class {
            if (args.Length != 1) throw new ArgumentException("ErrorCode requires one agument");
            if (args[0].Value is Error err) {
                return new ParameterResolverValue(err._Code);
            } else {
                throw new ArgumentException("ErrorCode requires argument containing an error. Use IsError to determine this before calling ErrorCode.");
            }
        }
        #endregion
    }
}
