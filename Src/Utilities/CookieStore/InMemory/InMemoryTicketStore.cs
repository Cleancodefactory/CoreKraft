using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.CookieTicketStore.InMemory
{
    public class InMemoryTicketStore : ITicketStore
    {
        private readonly IMemoryCache _Cache;

        public InMemoryTicketStore(IMemoryCache cache)
        {
            _Cache = cache;
        }

        public Task RemoveAsync(string key)
        {
            _Cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket ticket = _Cache.Get<AuthenticationTicket>(key);
            return Task.FromResult(ticket);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            _Cache.Set(key, ticket);
            return Task.CompletedTask;
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            Claim claim = ticket.Principal.Claims.FirstOrDefault(c => c.Type == "name");
            if (claim != null)
            {
                string key = claim.Value;
                _Cache.Set(key, ticket);
                return Task.FromResult(key);
            }
            return null;
        }
    }
}
