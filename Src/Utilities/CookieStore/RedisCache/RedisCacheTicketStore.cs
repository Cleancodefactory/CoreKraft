//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Ccf.Ck.Utilities.CookieTicketStore.RedisCache
//{
//    public class RedisCacheTicketStore : ITicketStore
//    {
//        private readonly ILogger _Logger;
//        private string KeyPrefix = "AuthSessionStore-";
//        private IDistributedCache _Cache;

//        public RedisCacheTicketStore(RedisCacheOptions options, ILogger logger, IConfiguration config)
//        {
//            KeyPrefix = config["Redis:ApplicationName"] + "-";
//            _Cache = new RedisCache(options);
//        }

//        public async Task<string> StoreAsync(AuthenticationTicket ticket)
//        {
//            var guid = Guid.NewGuid();
//            var key = KeyPrefix + guid.ToString();
//            await RenewAsync(key, ticket);
//            return key;
//        }

//        public Task RenewAsync(string key, AuthenticationTicket ticket)
//        {
//            var options = new DistributedCacheEntryOptions();
//            var expiresUtc = ticket.Properties.ExpiresUtc;
//            if (expiresUtc.HasValue)
//            {
//                options.SetAbsoluteExpiration(expiresUtc.Value);
//            }

//            options.SetSlidingExpiration(TimeSpan.FromMinutes(60));

//            byte[] val = SerializeToBytes(ticket, _Logger);
//            _Cache.Set(key, val, options);
//            return Task.FromResult(0);
//        }

//        public Task<AuthenticationTicket> RetrieveAsync(string key)
//        {
//            AuthenticationTicket ticket;
//            byte[] bytes = null;
//            bytes = _Cache.Get(key);
//            ticket = DeserializeFromBytes(bytes, _Logger);
//            return Task.FromResult(ticket);
//        }

//        public Task RemoveAsync(string key)
//        {
//            _Cache.Remove(key);
//            return Task.FromResult(0);
//        }

//        private static byte[] SerializeToBytes(AuthenticationTicket source, ILogger logger)
//        {
//            var ticket = TicketSerializer.Default.Serialize(source);
//            return ticket;
//        }

//        private static AuthenticationTicket DeserializeFromBytes(byte[] source, ILogger logger)
//        {
//            var hold = source == null ? null : TicketSerializer.Default.Deserialize(source);
//            return hold;
//        }

//    }
//}
