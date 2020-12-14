using System.Collections.Generic;

namespace Ccf.Ck.Models.EmailSettings
{
    public class EmailSettings
    {
        public EmailSettings()
        {
            MailAddressesToRecipients = new List<string>();
            MailAddressesBccToRecipients = new List<string>();
        }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string MailAddressFrom { get; set; }
        public string MailAddressesBccTo
        {
            get { return null; }
            set
            {
                string[] recipientsA = value.Split(new char[] { ';', ',' });
                foreach (string item in recipientsA)
                {
                    MailAddressesBccToRecipients.Add(item);
                }
            }
        }
        public string MailAddressesTo
        {
            get { return null; }
            set
            {
                string[] recipientsA = value.Split(new char[] { ';', ',' });
                foreach (string item in recipientsA)
                {
                    MailAddressesToRecipients.Add(item);
                }
            }
        }
        public List<string> MailAddressesToRecipients { get; set; }
        public List<string> MailAddressesBccToRecipients { get; set; }
    }
}
