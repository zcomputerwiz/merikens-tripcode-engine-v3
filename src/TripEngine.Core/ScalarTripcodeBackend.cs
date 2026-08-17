using System;
using System.Security.Cryptography;
using System.Text;

namespace TripEngine.Core
{
    public class ScalarTripcodeBackend : ITripcodeBackend
    {
        private static readonly char[] Base64CharTable = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '/'
        };

        private static readonly byte[] ExplicitCharTableForSeed = new byte[256] {
            // Fill with exact values from Main.cpp charTableForSeed
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70,
            71, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 97, 98, 99, 100, 101,
            102, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46,
            46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46
        };

        public string GenerateTripcode(string key, bool isSha1)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding sjis = Encoding.GetEncoding("shift_jis");

            if (isSha1)
            {
                // SHA-1 Mode (12-character tripcode)
                byte[] keyBytes = sjis.GetBytes(key);
                byte[] finalKey = new byte[12];
                Array.Copy(keyBytes, finalKey, Math.Min(keyBytes.Length, 12));

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] digest = sha1.ComputeHash(finalKey);

                    uint A = ((uint)digest[0] << 24) | ((uint)digest[1] << 16) | ((uint)digest[2] << 8) | digest[3];
                    uint B = ((uint)digest[4] << 24) | ((uint)digest[5] << 16) | ((uint)digest[6] << 8) | digest[7];
                    uint C = ((uint)digest[8] << 24) | ((uint)digest[9] << 16) | ((uint)digest[10] << 8) | digest[11];

                    char[] trip = new char[12];
                    trip[0]  = Base64CharTable[A >> 26];
                    trip[1]  = Base64CharTable[(A >> 20) & 0x3f];
                    trip[2]  = Base64CharTable[(A >> 14) & 0x3f];
                    trip[3]  = Base64CharTable[(A >> 8) & 0x3f];
                    trip[4]  = Base64CharTable[(A >> 2) & 0x3f];
                    trip[5]  = Base64CharTable[((B >> 28) | (A << 4)) & 0x3f];
                    trip[6]  = Base64CharTable[(B >> 22) & 0x3f];
                    trip[7]  = Base64CharTable[(B >> 16) & 0x3f];
                    trip[8]  = Base64CharTable[(B >> 10) & 0x3f];
                    trip[9]  = Base64CharTable[(B >> 4) & 0x3f];
                    trip[10] = Base64CharTable[((B << 2) | (C >> 30)) & 0x3f];
                    trip[11] = Base64CharTable[(C >> 24) & 0x3f];

                    return new string(trip);
                }
            }
            else
            {
                // DES Mode (10-character tripcode)
                byte[] keyBytes = sjis.GetBytes(key);
                byte[] actualKey = new byte[8];
                Array.Copy(keyBytes, actualKey, Math.Min(keyBytes.Length, 8));

                byte saltChar1 = 46; // '.'
                byte saltChar2 = 46; // '.'

                if (actualKey.Length > 1)
                {
                    saltChar1 = ExplicitCharTableForSeed[actualKey[1]];
                }
                if (actualKey.Length > 2)
                {
                    saltChar2 = ExplicitCharTableForSeed[actualKey[2]];
                }

                byte[] saltBytes = new byte[] { saltChar1, saltChar2 };

                string cryptResult = UnixCrypt.Crypt(actualKey, saltBytes);
                return cryptResult.Substring(3, 10);
            }
        }
    }
}
