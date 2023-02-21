using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Ccf.Ck.Web.Middleware.Cookies
{
    public sealed class OptimizedTicketDataFormat : SecureDataFormat<AuthenticationTicket>
    {
        public OptimizedTicketDataFormat(ILoggerFactory loggerFactory, IDataProtector protector)
            : base(new OptimizedTicketSerializer(propertiesSerializer: null, loggerFactory), protector)
        {
        }

        internal const Int32 FormatVersion = 100; // Using 100 because AspNetCore and the OWIN-interopable format is version 5. Though consider using a 4-byte value like "0xDE 0xAD 0xBE 0xEF" or any other cool 8-letter hex words?
        internal static readonly Byte[] FormatVersionArr = new Byte[] { FormatVersion, 0, 0, 0 };

        // This set of JWT Claim names is not updated dynamically (e.g. with T4) because it ABSOLUTELY MUST remain static so we don't break currently authenticated users' cookies.
        // If we ever do change this list of claim names, then change the `FormatVersion` and add code-paths for all versions from 100 onwards.
        internal static readonly IReadOnlyList<String> Format100CommonCodebook = new[]
        {
			// Common ClaimValueTypes:
			ClaimValueTypes.String,
            ClaimValueTypes.Integer,
            ClaimValueTypes.Integer32,
            ClaimValueTypes.Integer64,
            ClaimValueTypes.Boolean,

			// Claims that ASP.NET Core keeps around:
			"idp",
            "sid",

			// Claim types currently used by the application:
			"name",
            "preferred_username",
            "email",
            "email_verified",
            "picture"
			// etc... (in my own code there's about 30 claim names here)
		};
    }
}
