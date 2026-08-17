using System;
using System.Text;

namespace TripEngine.Core
{
    public static class UnixCrypt
    {
        private const ulong KS_MASK = 0xfcfcfcfcffffffffUL;

        private static ulong Permute64(ulong c, ulong[][] p)
        {
            ulong outp = 0;
            for (int i = 0; i < p.Length; i++)
            {
                outp |= p[i][c & 0xf];
                c >>= 4;
            }
            return outp;
        }

        public static string Crypt(byte[] keyBytes, byte[] saltBytes)
        {
            // Parse salt bytes into 12-bit integer
            char salt0 = saltBytes.Length > 0 ? (char)saltBytes[0] : '.';
            char salt1 = saltBytes.Length > 1 ? (char)saltBytes[1] : salt0;

            int s0 = AsciiToBin(salt0);
            int s1 = AsciiToBin(salt1);
            if (s0 < 0 || s0 > 63) s0 = 0;
            if (s1 < 0 || s1 > 63) s1 = 0;
            int saltInt = s0 | (s1 << 6);

            // Expand 12 bit salt -> 32 bit per des_crypt / glibc
            uint salt = (
                ((uint)(saltInt & 0x00003f) << 26) |
                ((uint)(saltInt & 0x000fc0) << 12) |
                ((uint)(saltInt & 0x03f000) >> 2) |
                ((uint)(saltInt & 0xfc0000) >> 16)
            );

            // Secret key bytes to 64-bit integer
            ulong keyVal = 0;
            for (int i = 0; i < 8; i++)
            {
                byte b = i < keyBytes.Length ? keyBytes[i] : (byte)0;
                keyVal = (keyVal << 8) | (byte)((b & 0x7F) << 1);
            }

            // Generate key schedule
            ulong[] ks_list_even = new ulong[8];
            ulong[] ks_list_odd = new ulong[8];
            ulong ks_odd = keyVal;

            for (int r = 0; r < 8; r++)
            {
                ulong ks_even = Permute64(ks_odd, DES_Tables.PCXROT[r][0]);
                ks_odd = Permute64(ks_even, DES_Tables.PCXROT[r][1]);
                ks_list_even[r] = ks_even & KS_MASK;
                ks_list_odd[r] = ks_odd & KS_MASK;
            }

            // Init L & R
            ulong L = 0;
            ulong R = 0;

            // Main DES loop - 25 rounds
            for (int step = 0; step < 25; step++)
            {
                for (int r = 0; r < 8; r++)
                {
                    ulong k_even = ((R >> 32) ^ R) & salt;
                    ulong B_even = (k_even << 32) ^ k_even ^ R ^ ks_list_even[r];

                    L ^= (DES_Tables.SPE[0][(B_even >> 58) & 0x3f] ^
                          DES_Tables.SPE[1][(B_even >> 50) & 0x3f] ^
                          DES_Tables.SPE[2][(B_even >> 42) & 0x3f] ^
                          DES_Tables.SPE[3][(B_even >> 34) & 0x3f] ^
                          DES_Tables.SPE[4][(B_even >> 26) & 0x3f] ^
                          DES_Tables.SPE[5][(B_even >> 18) & 0x3f] ^
                          DES_Tables.SPE[6][(B_even >> 10) & 0x3f] ^
                          DES_Tables.SPE[7][(B_even >> 2) & 0x3f]);

                    ulong k_odd = ((L >> 32) ^ L) & salt;
                    ulong B_odd = (k_odd << 32) ^ k_odd ^ L ^ ks_list_odd[r];

                    R ^= (DES_Tables.SPE[0][(B_odd >> 58) & 0x3f] ^
                          DES_Tables.SPE[1][(B_odd >> 50) & 0x3f] ^
                          DES_Tables.SPE[2][(B_odd >> 42) & 0x3f] ^
                          DES_Tables.SPE[3][(B_odd >> 34) & 0x3f] ^
                          DES_Tables.SPE[4][(B_odd >> 26) & 0x3f] ^
                          DES_Tables.SPE[5][(B_odd >> 18) & 0x3f] ^
                          DES_Tables.SPE[6][(B_odd >> 10) & 0x3f] ^
                          DES_Tables.SPE[7][(B_odd >> 2) & 0x3f]);
                }

                ulong t = L;
                L = R;
                R = t;
            }

            ulong C = (
                (((ulong)L >> 3) & 0x0f0f0f0f00000000UL)
                |
                (((ulong)L << 33) & 0xf0f0f0f000000000UL)
                |
                (((ulong)R >> 35) & 0x000000000f0f0f0fUL)
                |
                (((ulong)R << 1) & 0x00000000f0f0f0f0UL)
            );

            ulong finalBlock = Permute64(C, DES_Tables.CF6464);

            char[] outbuf = new char[13];
            outbuf[0] = salt0;
            outbuf[1] = salt1;

            ulong v = finalBlock << 2;
            for (int i = 0; i < 11; i++)
            {
                outbuf[2 + i] = BinToAscii((int)((v >> (60 - 6 * i)) & 0x3f));
            }

            return new string(outbuf);
        }

        private static int AsciiToBin(char c)
        {
            if (c >= 'a') return c - 59;
            if (c >= 'A') return c - 53;
            return c - '.';
        }

        private static char BinToAscii(int c)
        {
            if (c >= 38) return (char)(c - 38 + 'a');
            if (c >= 12) return (char)(c - 12 + 'A');
            return (char)(c + '.');
        }
    }
}
