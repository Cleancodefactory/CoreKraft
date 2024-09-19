using Ccf.Ck.Libs.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.CookieTicketStore.Sqlite
{
    public class SqliteTicketStore : ITicketStore
    {
        SqliteDb _SqliteDb;
        public SqliteTicketStore()
        {
            _SqliteDb = new SqliteDb();
        }
        
        public Task RemoveAsync(string key)
        {
            _SqliteDb.Remove(key);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket ticket = null;
            try
            {
                byte[] ticketAsByteArray = _SqliteDb.Get<byte[]>(key);
                
                if (ticketAsByteArray != null)
                {
                    TicketSerializer serializer = new TicketSerializer();
                    ticket = serializer.Deserialize(ticketAsByteArray);
                }
                return Task.FromResult(ticket);
            }
            catch (Exception ex)
            {
                KraftLogger.LogError($"SqliteTicketStore.RetrieveAsync: {ex.Message}.", ex);
            }
            return Task.FromResult(ticket);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            _SqliteDb.Set(key, serializer.Serialize(ticket));

            return Task.CompletedTask;
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            Claim claim = ticket.Principal.Claims.FirstOrDefault(c => c.Type == "name");
            if (claim != null)
            {
                var key = claim.Value;
                TicketSerializer serializer = new TicketSerializer();
                _SqliteDb.Set(key, serializer.Serialize(ticket));

                return Task.FromResult(key);
            }
            return null;
        }
    }
}
