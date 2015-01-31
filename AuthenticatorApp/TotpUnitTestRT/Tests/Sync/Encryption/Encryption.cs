using AuthenticatorApp.Encryption;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace TotpUnitTestRT.Tests.Sync.Encryption
{
    [TestClass]
    public class RandomAndDataProtection
    {
        const string Data = "Test";

        [TestMethod, TestCategory("Data Protection")]
        public void TestRandomKey()
        {
            AesEncryptionProvider.InitializeEncryptionKey();
        }

        [TestMethod, TestCategory("Data Protection")]
        public void TestEncryption()
        {
            var key = AesEncryptionProvider.InitializeEncryptionKey();
            AesEncryptionProvider.Encrypt(key, Data);
        }

        [TestMethod, TestCategory("Data Protection")]
        public void TestEncryptionAndDecryption()
        {
            var key = AesEncryptionProvider.InitializeEncryptionKey();
            var encrypted = AesEncryptionProvider.Encrypt(key, Data);
            var decrypted = AesEncryptionProvider.Decrypt(key, encrypted);
            Assert.AreEqual(Data,decrypted);
        }
    }
}
