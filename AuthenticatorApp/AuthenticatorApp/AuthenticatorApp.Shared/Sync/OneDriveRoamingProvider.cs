using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using AuthenticatorApp.Encryption;
using AuthenticatorApp.LocalStorage;
using AuthenticatorApp.Models;

namespace AuthenticatorApp.Sync
{
    public class OneDriveRoamingProvider : IRoamingProvider
    {
        /// <summary>
        /// OneDrive app data folder.
        /// </summary>
        private StorageFolder _roamingFolder;

        /// <summary>
        /// Account folder.
        /// </summary>
        private StorageFolder _accountFolder;

        private bool _isInitialized;

        /// <summary>
        /// Initialize Roaming storage.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        private IAsyncAction InitializeStorageAsync()
        {
            return Task.Run(async () =>
            {
                _roamingFolder = ApplicationData.Current.RoamingFolder;
#if WINDOWS_APP
                var accountFolderW = await _roamingFolder.TryGetItemAsync("AccountsIndexed");
                if (accountFolderW == null)
                {
                    _accountFolder = await _roamingFolder.CreateFolderAsync("AccountsIndexed");
                    _isInitialized = true;
                    return;
                }

                // Folder exist, then return
                if (accountFolderW.IsOfType(StorageItemTypes.Folder))
                {
                    _accountFolder = (StorageFolder)accountFolderW;
                    _isInitialized = true;
                    return;
                }

                // Or wrong type
                if (accountFolderW.IsOfType(StorageItemTypes.File))
                {
                    await accountFolderW.DeleteAsync();
                    _accountFolder = await _roamingFolder.CreateFolderAsync("AccountsIndexed");
                    _isInitialized = true;
                }
#endif
#if WINDOWS_PHONE_APP
                var folders = await _roamingFolder.GetFoldersAsync();
                var query = from c in folders where c.Name == "AccountsIndexed" select c;
                if (query.Any())
                {
                    _isInitialized = true;
                }
                else
                {
                    var files = await _roamingFolder.GetFoldersAsync();
                    var queryFile = from c in files where c.Name == "AccountsIndexed" select c;
                    var storageFolders = queryFile as IList<StorageFolder> ?? queryFile.ToList();
                    if (storageFolders.Any()) await storageFolders.First().DeleteAsync();
                    // Or create folder
                    _accountFolder = await _roamingFolder.CreateFolderAsync("AccountsIndexed");
                    _isInitialized = true;
                }
#endif
            }).AsAsyncAction();
        }

        public IAsyncOperation<List<Account>> GetAvailableAccountsAsync()
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();
                if (IsRoamingStorageEncrypted && String.IsNullOrEmpty(LocalEncryptionTransferKey))
                    throw new InvalidOperationException("The storage is encrypted but no key is provided.");

                if (!IsRoamingStorageEncrypted) throw new SecurityException("NSA is watching you");

                var accountList = new List<Account>();

                foreach (var file in await _roamingFolder.GetFilesAsync())
                {
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    var streamreader = new StreamReader(stream.AsStream());
                    var encryptedData = streamreader.ReadToEnd();

                    var data = AesEncryptionProvider.Decrypt(LocalEncryptionTransferKey, encryptedData);
                    JsonObject accountJsonObject;

                    if (!JsonObject.TryParse(data, out accountJsonObject)) continue;
                    if (accountJsonObject.ContainsKey("AccountName")
                        && accountJsonObject.ContainsKey("AccountKeyBase32")
                        && accountJsonObject.ContainsKey("AccountIcon"))
                    {
                        accountList.Add(new Account
                        {
                            AccountName = accountJsonObject.GetNamedString("AccountName"),
                            AccountKeyBase32 = accountJsonObject.GetNamedString("AccountKeyBase32"),
                            AccountIcon = accountJsonObject.GetNamedString("AccountIcon")
                        });
                    }
                }

                return accountList;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<Guid> AddAccountToRoamingStorageAsync(Account account, Guid roamingGuid)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();
                if (IsRoamingStorageEncrypted && String.IsNullOrEmpty(LocalEncryptionTransferKey))
                    throw new InvalidOperationException("The storage is encrypted but no key is provided.");
                if (!IsRoamingStorageEncrypted) throw new SecurityException("NSA is watching you");

                var accountFile = await _accountFolder.CreateFileAsync(roamingGuid + ".blob");
                var accountJson = new JsonObject
                {
                    {"AccountName", JsonValue.CreateStringValue(account.AccountName)},
                    {"AccountKeyBase32", JsonValue.CreateStringValue(account.AccountKeyBase32)},
                    {"AccountIcon", JsonValue.CreateStringValue(account.AccountIcon)}
                };

                // Encrypt the data
                var accountJsonString = accountJson.Stringify();
                var accountJsonStringEncrypted = AesEncryptionProvider.Encrypt(LocalEncryptionTransferKey,
                    accountJsonString);
                var transcation = await accountFile.OpenTransactedWriteAsync();
                var streamwriter = new StreamWriter(transcation.Stream.AsStream());
                streamwriter.Write(accountJsonStringEncrypted);
                // Commit data
                streamwriter.Flush();
                await transcation.CommitAsync();

                return roamingGuid;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<Guid> AddAccountToRoamingStorageAsync(Account account)
        {
            return AddAccountToRoamingStorageAsync(account, Guid.NewGuid());
        }

        public IAsyncOperation<List<Guid>> AddAccountRangeToRoamingStorageeAsync(IEnumerable<Account> accounts)
        {
            return Task.Run(async () =>
            {
                var list = new List<Guid>();
                foreach (var account in accounts)
                {
                    list.Add(await AddAccountToRoamingStorageAsync(account));
                }
                return list;
            }).AsAsyncOperation();
        }

        public IAsyncAction ModifyAccountToRoamingStorageAsync(Account oldAccount, Account newAccount)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();

                var dbDriver = new LocalStorageProvider();
                await dbDriver.InitializeDatabaseAsync();
                var guid = await dbDriver.GetRoamingGuidFromAccountNameAsync(oldAccount.AccountName);

                // Delete
                await DeleteAccountInRoamingStorageAsync(guid);

                // Create new
                await AddAccountToRoamingStorageAsync(newAccount, guid);

            }).AsAsyncAction();
        }

        public IAsyncAction DeleteAccountInRoamingStorageAsync(Guid roamingGuid)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();

                // Delete
                var accountFile = await _accountFolder.GetItemAsync(roamingGuid + ".blob");
                if (accountFile.IsOfType(StorageItemTypes.File)) await accountFile.DeleteAsync();
            }).AsAsyncAction();
        }

        public IAsyncAction DeleteAccountInRoamingStorageAsync(Account account)
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();

                var dbDriver = new LocalStorageProvider();
                await dbDriver.InitializeDatabaseAsync();
                await DeleteAccountInRoamingStorageAsync(await dbDriver.GetRoamingGuidFromAccountNameAsync(account.AccountName));
            }).AsAsyncAction();
        }

        public IAsyncAction ResetRoamingStorageAsync()
        {
            return Task.Run(async () =>
            {
                if (!_isInitialized) await InitializeStorageAsync();

                foreach (var file in await _roamingFolder.GetFilesAsync()) await file.DeleteAsync();
            }).AsAsyncAction();
        }

        public bool IsRoamingStorageEncrypted { get; set; }
        public string LocalEncryptionTransferKey { get; set; }
        public DateTime RoamingStorageLastSynced { get; set; }
        public Version RoamingStorageVersion { get; set; }
    }
}
