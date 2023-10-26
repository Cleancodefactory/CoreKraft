using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Settings
{
    public class CookieStoreSection
    {
        private bool _EnableCookieSizeReduction;

        public CookieStoreSection()
        {
            CookieStore = new CookieStore();
        }
        public CookieStore CookieStore { get; set; }
        public bool EnableCookieSizeReduction
        {
            get
            {
                if (CookieStore.IsSqliteTicketStore || CookieStore.IsInMemoryTicketStore)
                {
                    return false;
                }
                return _EnableCookieSizeReduction;
            }
            set
            {
                _EnableCookieSizeReduction = value;
            }
        }
    }
}
