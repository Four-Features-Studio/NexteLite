using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Utils
{
    public class Hasher
    {
        public static string ComputeSHA1(byte[] file)
        {
            var sha1 = SHA1Compute(file);
            var hash = ByteArrayToHexViaLookup32(sha1);

            return hash;
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        static byte[] SHA1Compute(byte[] bytes)
        {
            SHA1 hasher = SHA1.Create();
            byte[] hash = hasher.ComputeHash(bytes);
            return hash;
        }
    }
}
