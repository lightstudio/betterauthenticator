using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace AuthenticatorApp.Encryption
{
    public class AesEncryptionProvider
    {
        public static string InitializeEncryptionKey()
        {
            var randomBuffer = CryptographicBuffer.GenerateRandom(24);
            // IBuffer to String
            return CryptographicBuffer.EncodeToBase64String(randomBuffer);
        }

        public static string Encrypt(string keyData, string data)
        {
            // Decode key
            var keyBuffer = CryptographicBuffer.DecodeFromBase64String(keyData);

            // Decode IV and Key
            var iv = CreateInitializationVector(keyData);
            var key = CreateKey(keyBuffer);

            // Encrypt data
            var encryptedBuffer = CryptographicEngine.Encrypt(
                key, CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8), iv);
            return CryptographicBuffer.EncodeToBase64String(encryptedBuffer); 
        }

        public static string Decrypt(string keyData, string encryptedData)
        {
            // Decode key
            var keyBuffer = CryptographicBuffer.DecodeFromBase64String(keyData);

            // Decode IV and Key
            var iv = CreateInitializationVector(keyData);
            var key = CreateKey(keyBuffer);
            
            // Decrypt data
            var decryptedBuffer = CryptographicEngine.Decrypt(
                key, CryptographicBuffer.DecodeFromBase64String(encryptedData), iv);
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedBuffer);
        }

        /// <summary> 
        /// Create initialization vector IV 
        /// </summary> 
        /// <param name="password">Password is used for random vector generation</param> 
        /// <returns>Vector</returns> 
        private static IBuffer CreateInitializationVector(string password)
        {
            // Create vector 
            return CryptographicBuffer.CreateFromByteArray(
              Encoding.UTF8.GetBytes(password));
        }

        /// <summary> 
        /// Create encryption key 
        /// </summary> 
        /// <param name="buffer">Buffer is used for random key generation</param> 
        /// <returns></returns> 
        private static CryptographicKey CreateKey(IBuffer buffer)
        {
            var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            return provider.CreateSymmetricKey(buffer);
        } 
    }
}
