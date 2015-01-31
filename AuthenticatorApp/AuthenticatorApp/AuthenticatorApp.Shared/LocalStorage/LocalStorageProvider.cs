using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using AuthenticatorApp.LocalStorage.SQLite;
using AuthenticatorApp.Models;

namespace AuthenticatorApp.LocalStorage
{
    public class LocalStorageProvider
    {
        private const string DatabaseFileName = "accountsDb";
        private bool _isInitialized;
        private SQLiteConnection _databsaeConnection;

        public IAsyncAction InitializeDatabaseAsync()
        {
            return Task.Run(() =>
            {
                _databsaeConnection = new SQLiteConnection(ApplicationData.Current.LocalFolder.Path+ "\\" + DatabaseFileName);
                _databsaeConnection.CreateTable<AccountDb>();
                _isInitialized = true;
            }).AsAsyncAction();
        }

        public IAsyncAction AddAccountAsync(Account account)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.Insert(new AccountDb
                {
                    AccountIcon = account.AccountIcon,
                    AccountName = account.AccountName,
                    AccountKeyBase32 = account.AccountKeyBase32
                });
            }).AsAsyncAction();
        }

        public IAsyncAction AddAccountAsync(Account account, Guid roamingGuid)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.Insert(new AccountDb
                {
                    AccountIcon = account.AccountIcon,
                    AccountName = account.AccountName,
                    AccountKeyBase32 = account.AccountKeyBase32,
                    RoamingGuid = roamingGuid
                });
            }).AsAsyncAction();
        }

        public IAsyncAction AddAccountsAsync(IEnumerable<Account> accounts)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.BeginTransaction();
                foreach (var account in accounts)
                {
                    _databsaeConnection.Insert(new AccountDb
                    {
                        AccountIcon = account.AccountIcon,
                        AccountName = account.AccountName,
                        AccountKeyBase32 = account.AccountKeyBase32
                    });
                }
                _databsaeConnection.Commit();
            }).AsAsyncAction();
        }

        public IAsyncAction DeleteAccountAsync(Account account)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.Delete(new AccountDb
                {
                    AccountIcon = account.AccountIcon,
                    AccountName = account.AccountName,
                    AccountKeyBase32 = account.AccountKeyBase32
                });
            }).AsAsyncAction();
        }

        public IAsyncAction DeleteAccountAsync(Account account, Guid roamingGuid)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.Delete(new AccountDb
                {
                    AccountIcon = account.AccountIcon,
                    AccountName = account.AccountName,
                    AccountKeyBase32 = account.AccountKeyBase32,
                    RoamingGuid = roamingGuid
                });
            }).AsAsyncAction();
        }

        public IAsyncAction ResetDatabaseAsync()
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                _databsaeConnection.DeleteAll<AccountDb>();
            }).AsAsyncAction();
        }

        public IAsyncOperation<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                var query = from c in _databsaeConnection.Table<AccountDb>() select c;
                return query.Select(a => new Account
                {
                    AccountName = a.AccountName,
                    AccountKeyBase32 = a.AccountKeyBase32,
                    AccountIcon = a.AccountIcon
                }).AsEnumerable();

            }).AsAsyncOperation();
        }

        public IAsyncOperation<Guid> GetRoamingGuidFromAccountNameAsync(string accountName)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                var query = from c in _databsaeConnection.Table<AccountDb>() where c.AccountName == accountName select c;
                return query.First().RoamingGuid;

            }).AsAsyncOperation();
        }

        public IAsyncAction ModifyAccountAsync(string accountName, Account account)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeDatabaseAsync();
                var query = from c in _databsaeConnection.Table<AccountDb>() where c.AccountName == accountName select c;
                if (!query.Any()) return;

                var item = query.FirstOrDefault();
                item.AccountIcon = account.AccountIcon;
                _databsaeConnection.Update(item);

            }).AsAsyncAction();
        }
    }
}
