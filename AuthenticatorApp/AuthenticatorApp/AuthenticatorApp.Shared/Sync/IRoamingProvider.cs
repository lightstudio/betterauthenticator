using System;
using System.Collections.Generic;
using Windows.Foundation;

using AuthenticatorApp.Models;

namespace AuthenticatorApp.Sync
{
    /// <summary>
    /// Basic Roaming Storage Provider Interface, which can be adapted to a range of online services.
    /// </summary>
    interface IRoamingProvider
    {
        /// <summary>
        /// Get all available accounts in roaming storage.
        /// </summary>
        /// <returns>A list, which contains all available accounts.</returns>
        IAsyncOperation<List<Account>> GetAvailableAccountsAsync();

        /// <summary>
        /// Add single account to roaming storage.
        /// </summary>
        /// <param name="account">An account.</param>
        /// <returns>An awaitable task.</returns>
        IAsyncOperation<Guid> AddAccountToRoamingStorageAsync(Account account);

        /// <summary>
        /// Add multiple account to roaming storage.
        /// </summary>
        /// <param name="accounts">Accounts.</param>
        /// <returns>An awaitable task.</returns>
        IAsyncOperation<List<Guid>>  AddAccountRangeToRoamingStorageeAsync(IEnumerable<Account> accounts);

        /// <summary>
        /// Modify an account.
        /// </summary>
        /// <param name="oldAccount">The account to be modified.</param>
        /// <param name="newAccount">The account.</param>
        /// <returns>An awaitable task.</returns>
        IAsyncAction ModifyAccountToRoamingStorageAsync(Account oldAccount, Account newAccount);

        /// <summary>
        /// Delete an account.
        /// </summary>
        /// <param name="oldAccount">The account to be deleted.</param>
        /// <returns>An awaitable task.</returns>
        IAsyncAction DeleteAccountInRoamingStorageAsync(Account oldAccount);

        /// <summary>
        /// Delete all accounts in roaming storage and reset the version log, as well as encryption key.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        IAsyncAction ResetRoamingStorageAsync();

        /// <summary>
        /// A value indicates whether the roaming storage is encrypted.
        /// </summary>
        bool IsRoamingStorageEncrypted { get; set; }

        /// <summary>
        /// Local encryption transfer key, which is protected by a user-input PIN code.
        /// </summary>
        string LocalEncryptionTransferKey{ get; set; }

        /// <summary>
        /// Last time the storage synced.
        /// </summary>
        DateTime RoamingStorageLastSynced { get; set; }

        /// <summary>
        /// The version of roaming storage
        /// </summary>
        Version RoamingStorageVersion { get; set; }
    }
}
