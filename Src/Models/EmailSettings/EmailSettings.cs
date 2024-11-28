using Ccf.Ck.Utilities.Generic;
using System;
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
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string MailAddressFrom { get; set; }
        public string MailAddressesBccTo
        {
            get { return null; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] recipientsA = value.Split(new char[] { ';', ',' });
                    foreach (string item in recipientsA)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            MailAddressesBccToRecipients.Add(item);
                        }
                    }
                }
            }
        }
        public string MailAddressesTo
        {
            get { return null; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] recipientsA = value.Split(new char[] { ';', ',' });
                    foreach (string item in recipientsA)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            MailAddressesToRecipients.Add(item);
                        }
                    }
                }
            }
        }

        public List<string> MailAddressesToRecipients { get; set; }
        public List<string> MailAddressesBccToRecipients { get; set; }

        public void Validate()
        {
            Utilities.Generic.Utilities.CheckNullOrEmpty(this, p => p.UserName, true);
            Utilities.Generic.Utilities.CheckNullOrEmpty(this, p => p.Password, true);
            Utilities.Generic.Utilities.CheckNullOrEmpty(this, p => p.SmtpServer, true);
            if (SmtpPort == 0)
            {
                throw new Exception($"Variable {SmtpPort} is 0 and not set");
            }
            Utilities.Generic.Utilities.CheckNullOrEmpty(this, p => p.MailAddressFrom, true);
        }
    }
}
