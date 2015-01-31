using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticatorApp.LocalStorage;
using AuthenticatorApp.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace TotpUnitTestRT.Tests.Database
{
    [TestClass]
    public class DatabaseTest
    {
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

        [TestMethod,TestCategory("Database")]
        public async Task InitializeDatabase()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();
        }

        [TestMethod, TestCategory("Database")]
        public async Task InsertSingleEntityIntoDatabase()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();
            await database.AddAccountAsync(new Account
            {
                AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            });
        }

        [TestMethod, TestCategory("Database")]
        public async Task InsertMultipleEntityIntoDatabase()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();

            await database.AddAccountsAsync(GenerateBatchTestData());
        }

        [TestMethod, TestCategory("Database")]
        public async Task ModifySingleEntity()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();
            await database.AddAccountAsync(new Account
            {
                AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            });
            await database.ModifyAccountAsync("LightStudio:test@lightstudio.onmicrosoft.com",
                new Account
                {
                    AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                    AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                    AccountIcon = "ms-appx:///Assets/Services/AppleID.png",
                });
        }

        [TestMethod, TestCategory("Database")]
        public async Task QueryDatabase()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();

            await database.AddAccountsAsync(GenerateBatchTestData());
            await database.GetAllAccountsAsync();

            // No assert needed.
        }

        [TestMethod, TestCategory("Database")]
        public async Task GetRoamingGuid()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();
            var account = new Account
            {
                AccountName = "LightStudio:test2@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/eweddiff",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            };
            var guid = Guid.NewGuid();
            await database.AddAccountAsync(account,guid);

            var resultguid = await database.GetRoamingGuidFromAccountNameAsync(account.AccountName);

            Assert.AreEqual(guid, resultguid);
        }

        [TestMethod, TestCategory("Database")]
        public async Task DeleteSingleEntity()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();
            var account = new Account
            {
                AccountName = "LightStudio:test@lightstudio.onmicrosoft.com",
                AccountKeyBase32 = "sertcew43d5432wfe5yklv==/ewed",
                AccountIcon = "ms-appx:///Assets/Services/MicrosoftAccount.png",
            };
            var guid = Guid.NewGuid();
            await database.AddAccountAsync(account, guid);
            await database.DeleteAccountAsync(account, guid);
        }

        [TestMethod, TestCategory("Database")]
        public async Task ResetDatabase()
        {
            var database = new LocalStorageProvider();
            await database.InitializeDatabaseAsync();

            await InsertMultipleEntityIntoDatabase();

            await database.ResetDatabaseAsync();
        }
    }
}
