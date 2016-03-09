using System;

namespace JoshCodes.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        private const byte MOD = 35;
        private const byte OFFSET = 0x30;
        private const byte BREAK = 0x39;
        private const byte ALPHA_OFFSET = 0x8;

        private static byte byteMod(byte b, byte mod)
        {
            return (byte)(b % mod);
        }

        public static string ToHashString(this byte [] bytes, bool numeric = false)
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
                if (numeric)
                {
                    bytes.UpdateEach((b) => (byte)(byteMod(b, 0xA) + OFFSET));
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }

                bytes.UpdateEach((b) => (byte)(byteMod(b, MOD) + OFFSET));
                bytes.UpdateEach((b) => (byte)((b > BREAK) ? b + ALPHA_OFFSET : b));
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        public static string ToHexString(this byte[] bytes, string separator = "")
        {
            throw new NotImplementedException();
        }

        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static void UpdateEach(this byte [] bytes, Func<byte, byte> update)
        {
            for(int i=0; i<bytes.Length; i++)
            {
                bytes[i] = update.Invoke(bytes[i]);
            }
        }
    }
}

