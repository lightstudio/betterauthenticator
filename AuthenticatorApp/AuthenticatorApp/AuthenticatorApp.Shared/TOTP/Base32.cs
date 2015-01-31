using System;
using System.IO;
using System.Text;

namespace AuthenticatorApp.Totp
{
    public class Base32
    {
        public static string Base32ToHex(string base32)
        {
            const string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var stringBuilder = new StringBuilder();
            base32 = base32.Trim();
            base32 = base32.TrimEnd(new[]
	        {
		        '='
	        });
            foreach (var t in base32)
            {
                var num = text.IndexOf(char.ToUpper(t));
                if (num >= 0)
                {
                    stringBuilder.Append(Convert.ToString(num, 2).PadLeft(5, '0'));
                }
                else
                {
                    if (!char.IsWhiteSpace(t))
                    {
                        throw new InvalidDataException("Key");
                    }
                }
            }

            var text2 = stringBuilder.ToString();
            stringBuilder.Clear();
            if (text2.Length < 8)
            {
                throw new InvalidDataException("Key");
            }
            var num2 = text2.Length % 8;
            for (var j = 0; j < num2; j++)
            {
                if (text2[text2.Length - (j + 1)] != '0')
                {
                    throw new InvalidDataException("Key");
                }
            }
            var num3 = text2.Length - num2;
            var num4 = 0;
            while (num4 + 4 <= num3)
            {
                var text3 = text2.Substring(num4, 4);
                stringBuilder.Append(Convert.ToUInt16(text3, 2).ToString("X"));
                num4 += 4;
            }
            return stringBuilder.ToString();
        }
    }
}
