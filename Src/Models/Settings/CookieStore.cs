namespace Ccf.Ck.Models.Settings
{
    public class CookieStore
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public bool IsInMemoryTicketStore
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    if (Name.Equals("InMemoryTicketStore", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return Enabled;
                    }
                }
                return false;
            }
        }

        public bool IsSqliteTicketStore
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    if (Name.Equals("SqliteTicketStore", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return Enabled;
                    }
                }
                return false;
            }
        }
    }
}
