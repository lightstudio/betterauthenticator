using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticatorApp.Encryption;
using AuthenticatorApp.Models;
using AuthenticatorApp.Sync;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace TotpUnitTestRT.Tests.Sync.SyncEngine
{
    [TestClass]
    public class SyncEngineTest
    {
        readonly string _key = AesEncryptionProvider.InitializeEncryptionKey();

        private IEnumerable<Account> GenerateBatchTestData()
        {
            var list = new List<Account>();

            for (var i = 1; i <= 100; i++) list.AddRange(new List<Account>
            {
                new Account
                {
                    AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                    AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                    AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png"
                },
                new Account
                {
                    AccountName = "LightStudio:hjc@lightstudio.onmicrosoft.com",
                    AccountKeyBase32 = "swer3243d5432wfe5yklv==/ewed",
                    AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png"
                },
                new Account
                {
                    AccountName = "LightStudio:uve@lightstudio.onmicrosoft.com",
                    AccountKeyBase32 = "swer3243dwe4r34fe5yklv==/ewed",
                    AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png"
                }
            });

            return list;
        }
            
        [TestMethod, TestCategory("OneDrive Sync")]
        public async Task InsertOneEntityToOneDrive()
        {
            var syncengine = new OneDriveRoamingProvider
            {
                IsRoamingStorageEncrypted = true,
                LocalEncryptionTransferKey = _key
            };

            await syncengine.AddAccountToRoamingStorageAsync(new Account
            {
                AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            });
        }

        [TestMethod, TestCategory("OneDrive Sync")]
        public async Task InsertMultipleEntityToOneDrive()
        {
            var syncengine = new OneDriveRoamingProvider
            {
                IsRoamingStorageEncrypted = true,
                LocalEncryptionTransferKey = _key
            };

            await syncengine.AddAccountRangeToRoamingStorageeAsync(GenerateBatchTestData());
        }

        [TestMethod, TestCategory("OneDrive Sync")]
        public async Task DeleteOneEntity()
        {
            var syncengine = new OneDriveRoamingProvider
            {
                IsRoamingStorageEncrypted = true,
                LocalEncryptionTransferKey = _key
            };

            var guid = Guid.NewGuid();

            await syncengine.AddAccountToRoamingStorageAsync(new Account
            {
                AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            },guid);

            await syncengine.DeleteAccountInRoamingStorageAsync(guid);
        }

        [TestMethod, TestCategory("OneDrive Sync")]
        public async Task ResetOneDriveStorage()
        {
            var syncengine = new OneDriveRoamingProvider
            {
                IsRoamingStorageEncrypted = true,
                LocalEncryptionTransferKey = _key
            };

            await InsertMultipleEntityToOneDrive();

            await syncengine.ResetRoamingStorageAsync();
        }
    }
}
