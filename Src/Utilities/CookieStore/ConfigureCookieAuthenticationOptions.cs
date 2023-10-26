using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Ccf.Ck.Utilities.CookieTicketStore
{
    public class ConfigureCookieAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly ITicketStore _TicketStore;

        public ConfigureCookieAuthenticationOptions(ITicketStore ticketStore)
        {
            _TicketStore = ticketStore;
        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            options.SessionStore = _TicketStore;
        }
    }
}
