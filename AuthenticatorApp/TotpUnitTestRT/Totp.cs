using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AuthenticatorApp.Totp;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace TotpUnitTestRT
{
    public class TestTimeData
    {
        public long Time { get; set; }
        public string HMacSha1 { get; set; }
        public string HMacSha256 { get; set; }
        public string HMacSha512 { get; set; }
    }

    /// <summary>
    /// Totp Unit Testing class
    /// </summary>
    [TestClass]
    public class Totp
    {
        // Please refer to wikipedia for the test data:
        // http://en.wikipedia.org/wiki/Hash-based_message_authentication_code#Examples_of_HMAC_.28MD5.2C_SHA1.2C_SHA256.29
        // As well as
        // http://stackoverflow.com/questions/13541380/cant-get-the-same-hmac-sha1-results-for-java-and-c-sharp-winrt

        /// <summary>
        /// Test Key
        /// </summary>
        private const string Key = "key";

        /// <summary>
        /// Test Text
        /// </summary>
        private const string Text = "The quick brown fox jumps over the lazy dog";

        private TestTimeData[] _testTimeData =
        {
            new TestTimeData
            {
                Time = 59L,
                HMacSha1 = "94287082",
                HMacSha256 = "46119246",
                HMacSha512 = "90693936"
            },
            new TestTimeData
            {
                Time = 1111111109L,
                HMacSha1 = "07081804",
                HMacSha256 = "68084774",
                HMacSha512 = "25091201"
            },
            new TestTimeData
            {
                Time = 1111111111L,
                HMacSha1 = "14050471",
                HMacSha256 = "67062674",
                HMacSha512 = "99943326"
            },
            new TestTimeData
            {
                Time = 1234567890L,
                HMacSha1 = "89005924",
                HMacSha256 = "91819424",
                HMacSha512 = "93441116"
            },
            new TestTimeData
            {
                Time = 2000000000L,
                HMacSha1 = "69279037",
                HMacSha256 = "90698825",
                HMacSha512 = "38618901"
            },
            new TestTimeData
            {
                Time = 20000000000L,
                HMacSha1 = "65353130",
                HMacSha256 = "77737706",
                HMacSha512 = "47863826"
            }
        };

        /// <summary>
        /// HMAC-SHA1 pre-calculated result
        /// </summary>
        private const string HMacSha1Expected = "de7c9b85b8b78aa6bc8a7a36f70a90701c9db4d9";

        /// <summary>
        /// HMAC-SHA256 pre-calculated result
        /// </summary>
        private const string HMacSha256Expected = "f7bc83f430538424b13298e6aa6fb143ef4d59a14946175997479dbc2d1a3cd8";

        /// <summary>
        /// Encoding helper class
        /// </summary>
        private readonly UTF8Encoding _encoding = new UTF8Encoding();


        /// <summary>
        /// An exception should be thrown for null input.
        /// </summary>
        [TestMethod]
        public void CalcHmacSha1Scenario1Desktop()
        {
            // Please refer to wikipedia for the test data:
            // http://en.wikipedia.org/wiki/Hash-based_message_authentication_code#Examples_of_HMAC_.28MD5.2C_SHA1.2C_SHA256.29
            const string key = "";
            const string text = "";

            try
            {
                TotpProvider.HmacSha(TotpProvider.MacAlgorithmEnum.HmacSha1, _encoding.GetBytes(key),
                    _encoding.GetBytes(text));
            }
            catch (InvalidOperationException)
            {
                Assert.IsTrue(true);
            }

        }

        /// <summary>
        /// Calculcate HMAC-SHA1
        /// </summary>
        [TestMethod]
        public void CalcHmacSha1Scenario2Desktop()
        {
            var result = TotpProvider.HmacSha(TotpProvider.MacAlgorithmEnum.HmacSha1, _encoding.GetBytes(Key),
                _encoding.GetBytes(Text));
            var hexString = String.Join("", result.Select(a => a.ToString("x2")));
            Assert.AreEqual(HMacSha1Expected,
                hexString);
        }

        /// <summary>
        /// Calculcate HMAC-SHA256
        /// </summary>
        [TestMethod]
        public void CalcHmacSha256Scenario1Desktop()
        {
            var result = TotpProvider.HmacSha(TotpProvider.MacAlgorithmEnum.HmacSha256, _encoding.GetBytes(Key),
                _encoding.GetBytes(Text));
            var hexString = String.Join("", result.Select(a => a.ToString("x2")));
            Assert.AreEqual(HMacSha256Expected,
                hexString);
        }

        /// <summary>
        /// Generate TOTP(HMacSha1) from test time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>TOTP</returns>
        public string GenerateTotpHMacSha1FromTime(long time)
        {
            // Seed for HMAC-SHA1 - 20 bytes
            const string seed = "3132333435363738393031323334353637383930";

            long T0 = 0;
            long X = 30;

            var T = (time - T0)/X;
            var steps = T.ToString("X").ToUpper();
            while (steps.Length < 16) steps = "0" + steps;

            return TotpProvider.GenerateTotp(seed, steps, "8", TotpProvider.MacAlgorithmEnum.HmacSha1);
        }

        /// <summary>
        /// Generate TOTP(HMacSha256) from test time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>TOTP</returns>
        public string GenerateTotpHMacSha256FromTime(long time)
        {
            // Seed for HMAC-SHA256 - 32 bytes
            var seed32 = "3132333435363738393031323334353637383930" +
                         "313233343536373839303132";

            long T0 = 0;
            long X = 30;

            var T = (time - T0)/X;
            var steps = T.ToString("X").ToUpper();
            while (steps.Length < 16) steps = "0" + steps;

            return TotpProvider.GenerateTotp(seed32, steps, "8", TotpProvider.MacAlgorithmEnum.HmacSha256);
        }

        /// <summary>
        /// Generate TOTP(HMacSha512) from test time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>TOTP</returns>
        public string GenerateTotpHMacSha512FromTime(long time)
        {
            // Seed for HMAC-SHA512 - 64 bytes
            var seed64 = "3132333435363738393031323334353637383930" +
                         "3132333435363738393031323334353637383930" +
                         "3132333435363738393031323334353637383930" +
                         "31323334";

            long T0 = 0;
            long X = 30;

            var T = (time - T0)/X;
            var steps = T.ToString("X").ToUpper();
            while (steps.Length < 16) steps = "0" + steps;

            return TotpProvider.GenerateTotp(seed64, steps, "8", TotpProvider.MacAlgorithmEnum.HmacSha512);
        }

        private static readonly int[] DigitsPower
            // 0 1  2   3    4     5      6       7        8
        = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 };

        public static string GenerateTotpMicrosoft(byte[] keyBytes, string time, int returnDigits)
        {
            var array = TotpProvider.StringToByteArray(time);
            var array2 = TotpProvider.HmacSha(TotpProvider.MacAlgorithmEnum.HmacSha1, keyBytes, array);

            var num = (int)(array2[array2.Length - 1] & 15);
            var num2 = (int)(array2[num] & 127) << 24 | (int)(array2[num + 1] & 255) << 16 | (int)(array2[num + 2] & 255) << 8 | (int)(array2[num + 3] & 255);
            var text = (num2 % DigitsPower[returnDigits]).ToString();
            while (text.Length < returnDigits)
            {
                text = "0" + text;
            }
            return text;
        }


        [TestMethod]
        public void GenerateSpecificHMacSha1Totp()
        {
            var seed = "werredsxsgfhtdfsde";
            seed = seed.Trim();

            var currentTime = (uint) AuthenticatorApp.Timestamp.UnixTimeStamp.GetCurrentUnixTimestampSeconds();

            long T0 = 0;
            long X = 30;

            var T = (currentTime - T0) / X;
            var steps = T.ToString("X").ToUpper();
            while (steps.Length < 16) steps = "0" + steps;

            var timeString = steps;

            var data = TotpProvider.GenerateTotp(Base32.Base32ToHex(seed), 
                timeString, 6, 
                TotpProvider.MacAlgorithmEnum.HmacSha1);

            Assert.AreEqual(GenerateTotpMicrosoft(TotpProvider.StringToByteArray(Base32.Base32ToHex(seed)), timeString, 6), data);
        }


        /// <summary>
        /// Test TOTP
        /// </summary>
        [TestMethod]
        public void CalcEmulatedTotpToken()
        {
            foreach (var timegroup in _testTimeData)
            {
                Assert.AreEqual(timegroup.HMacSha1, GenerateTotpHMacSha1FromTime(timegroup.Time));
                Assert.AreEqual(timegroup.HMacSha256, GenerateTotpHMacSha256FromTime(timegroup.Time));
                Assert.AreEqual(timegroup.HMacSha512, GenerateTotpHMacSha512FromTime(timegroup.Time));
            }
        }
    }
}