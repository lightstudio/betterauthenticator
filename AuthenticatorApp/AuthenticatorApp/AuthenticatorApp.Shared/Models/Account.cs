using System;
using System.Collections.Generic;
using System.Text;

namespace AuthenticatorApp.Models
{
    /// <summary>
    /// Account Model.
    /// </summary>
    public class Account
    {
        public string AccountName { get; set; }
        public string AccountIcon { get; set; }
        public string CurrentMfToken { get; set; }
        public byte[] AccountKey { get; set; }

        public Int64 LastSync { get; set; }

        public Account(string accountName, byte[] accountKey)
        {
            AccountName = accountName;
            AccountKey = accountKey;
        }

        // Design Only
        public Account()
        {
            
        }
    }
}
