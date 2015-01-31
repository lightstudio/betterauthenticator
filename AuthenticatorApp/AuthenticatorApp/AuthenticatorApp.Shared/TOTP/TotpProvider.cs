using System;
using System.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace AuthenticatorApp.Totp
{
    /// <summary>
    /// A TOTP Provide reference implementation
    /// Referred from http://tools.ietf.org/html/rfc6238
    /// </summary>
    public class TotpProvider
    {
        /// <summary>
        /// The crypto algorithm (HmacSHA1, HmacSHA256, HmacSHA512)
        /// </summary>
        public enum MacAlgorithmEnum 
        {
            HmacSha1 = 0,
            HmacSha256 = 1,
            HmacSha512 = 2
        }

        /// <summary>
        /// This method uses the Windows Runtime APIs to provide the crypto algorithm.
        /// HMAC computes a Hashed Message Authentication Code with the
        /// crypto hash algorithm as a parameter.
        /// </summary>
        /// <param name="crypto">The crypto algorithm.</param>
        /// <param name="keyBytes">The bytes to use for the HMAC key.</param>
        /// <param name="text">The message or text to be authenticated.</param>
        /// <returns></returns>
        public static byte[] HmacSha(MacAlgorithmEnum crypto, byte[] keyBytes,
            byte[] text)
        {
            // Select crypto algorithm
            MacAlgorithmProvider macAlgorithmProvider = null;
            switch (crypto)
            {
                case MacAlgorithmEnum.HmacSha1:
                    macAlgorithmProvider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
                    break;
                case MacAlgorithmEnum.HmacSha256:
                    macAlgorithmProvider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
                    break;
                case MacAlgorithmEnum.HmacSha512:
                    macAlgorithmProvider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha512);
                    break;
            }
            if (macAlgorithmProvider == null) throw new InvalidOperationException("MacAlgorithmProvider failed to initialize.");
            // Get buffer
            var keyBuffer = CryptographicBuffer.CreateFromByteArray(keyBytes);
            if (keyBuffer == null) throw new InvalidOperationException("Invaild Key buffer.");
            var key = macAlgorithmProvider.CreateKey(keyBuffer);
            if (key == null) throw new InvalidOperationException("Invaild Key.");

            // Get hash
            var dataBuffer = CryptographicBuffer.CreateFromByteArray(text);
            if (dataBuffer == null) throw new InvalidOperationException("Invaild data buffer.");
                
            // Sign hash
            var dataBufferSigned = CryptographicEngine.Sign(key, dataBuffer);
            if (dataBufferSigned == null) throw new InvalidOperationException("Invaild signed data.");
                
            byte[] hashBytes;
            CryptographicBuffer.CopyToByteArray(dataBufferSigned, out hashBytes);
            return hashBytes;
        }

        /// http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        /// <summary>
        /// Convert hex string to byte array,.
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Byte array.</returns>
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static readonly int[] DigitsPower
        // 0 1  2   3    4     5      6       7        8
        = {1,10,100,1000,10000,100000,1000000,10000000,100000000 };

        /// <summary>
        /// This method generates a TOTP value for the given
        /// set of parameters.
        /// </summary>
        /// <param name="key">The shared secret, HEX encoded</param>
        /// <param name="time">A value that reflects a time</param>
        /// <param name="returnDigits">Number of digits to return</param>
        /// <returns>A numeric string in base 10 that includes 
        /// {@link truncationDigits} digits</returns>
        public static string GenerateTotp(string key,
             string time,
             string returnDigits)
        {
            return GenerateTotp(key, time, returnDigits, MacAlgorithmEnum.HmacSha1);
        }

        /// <summary>
        /// This method generates a TOTP value for the given
        /// set of parameters.
        /// </summary>
        /// <param name="key">The shared secret, HEX encoded</param>
        /// <param name="time">A value that reflects a time</param>
        /// <param name="returnDigits">Number of digits to return</param>
        /// <returns>A numeric string in base 10 that includes 
        /// {@link truncationDigits} digits</returns>
        public static string GenerateTotp256(string key,
             string time,
             string returnDigits)
        {
            return GenerateTotp(key, time, returnDigits, MacAlgorithmEnum.HmacSha256);
        }

        /// <summary>
        /// This method generates a TOTP value for the given
        /// set of parameters.
        /// </summary>
        /// <param name="key">The shared secret, HEX encoded</param>
        /// <param name="time">A value that reflects a time</param>
        /// <param name="returnDigits">Number of digits to return</param>
        /// <returns>A numeric string in base 10 that includes 
        /// {@link truncationDigits} digits</returns>
        public static string GenerateTotp512(string key,
             string time,
             string returnDigits)
        {
            return GenerateTotp(key, time, returnDigits, MacAlgorithmEnum.HmacSha512);
        }
       
        /// <summary>
        /// This method generates a TOTP value for the given
        /// set of parameters.
        /// </summary>
        /// <param name="key">The shared secret, HEX encoded</param>
        /// <param name="time">A value that reflects a time</param>
        /// <param name="returnDigits">Number of digits to return</param>
        /// <param name="crypto">The crypto function to use</param>
        /// <returns>A numeric string in base 10 that includes
        /// {@link truncationDigits} digits
        /// </returns>
        public static string GenerateTotp(string key,
            string time,
            string returnDigits,
            MacAlgorithmEnum crypto)
        {
            var codeDigits = int.Parse(returnDigits);

            // Using the counter
            // First 8 bytes are for the movingFactor
            // Compliant with base RFC 4226 (HOTP)
            while (time.Length < 16)
                time = "0" + time;

            // Get the HEX in a Byte[]
            var msg = StringToByteArray(time);
            var k = StringToByteArray(key);
            var hash = HmacSha(crypto, k, msg);

            // put selected bytes into result int
            var offset = hash[hash.Length - 1] & 0xf;

            var binary =
                ((hash[offset] & 0x7f) << 24) |
                ((hash[offset + 1] & 0xff) << 16) |
                ((hash[offset + 2] & 0xff) << 8) |
                (hash[offset + 3] & 0xff);

            var otp = binary % DigitsPower[codeDigits];

            var result = otp.ToString();
            while (result.Length < codeDigits)
            {
                result = "0" + result;
            }
            return result;

        }

        /// <summary>
        /// This method generates a TOTP value for the given
        /// set of parameters.
        /// </summary>
        /// <param name="key">The shared secret, HEX encoded</param>
        /// <param name="time">A value that reflects a time</param>
        /// <param name="returnDigits">Number of digits to return</param>
        /// <param name="crypto">The crypto function to use</param>
        /// <returns>A numeric string in base 10 that includes
        /// {@link truncationDigits} digits
        /// </returns>
        public static string GenerateTotp(string key,
            string time,
            int returnDigits,
            MacAlgorithmEnum crypto)
        {
            var codeDigits = returnDigits;

            // Using the counter
            // First 8 bytes are for the movingFactor
            // Compliant with base RFC 4226 (HOTP)
            while (time.Length < 16)
                time = "0" + time;

            // Get the HEX in a Byte[]
            var msg = StringToByteArray(time);
            var k = StringToByteArray(key);
            var hash = HmacSha(crypto, k, msg);

            // put selected bytes into result int
            var offset = hash[hash.Length - 1] & 0xf;

            var binary =
                ((hash[offset] & 0x7f) << 24) |
                ((hash[offset + 1] & 0xff) << 16) |
                ((hash[offset + 2] & 0xff) << 8) |
                (hash[offset + 3] & 0xff);

            var otp = binary % DigitsPower[codeDigits];

            var result = otp.ToString();
            while (result.Length < codeDigits)
            {
                result = "0" + result;
            }
            return result;

        }
    }
}
