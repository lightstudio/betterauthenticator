using System;
using AuthenticatorApp.LocalStorage.SQLite;

namespace AuthenticatorApp.Models
{
    public class AccountDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(1024)]
        public string AccountName { get; set; }

        [MaxLength(4096)]
        public string AccountIcon { get; set; }

        [MaxLength(4096)]
        public string AccountKeyBase32 { get; set; }

        public Guid RoamingGuid { get; set; }

        public int LastSync { get; set; }
    }

    /// <summary>
    /// Account Model.
    /// </summary>
    public class Account
    {
        
        public string AccountName { get; set; }

        [MaxLength(4096)]
        public string AccountIcon { get; set; }

        public string CurrentMfToken { get; set; }

        public byte[] AccountKey { get; set; }
        public string AccountKeyBase32 { get; set; }

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
