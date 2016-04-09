using System;
using System.Linq;
using System.Security.Cryptography;

namespace DataRelay
{
    public static class PasswordStorage
    {
        private const int SaltLength = 32;
        private const int HashIterations = 32000;
        private const int HashLength = 32;

        public static string Hash(string password)
        {
            var salt = GenerateSalt();

            var hash = Pbkdf2(password, salt);

            var storedHash = $"{Convert.ToBase64String(hash)}:{Convert.ToBase64String(salt)}";

            return storedHash;
        }

        public static bool PasswordMatch(string password, string storedHash)
        {
            //split stored hash into "hash" and "salt"
            var split = storedHash.Split(':');
            if (split.Length != 2)
                throw new ArgumentException();
            var hash = Convert.FromBase64String(split[0]);
            var salt = Convert.FromBase64String(split[1]);

            //rehash password and test against the one we stored
            var rehash = Pbkdf2(password, salt);
            return hash.SequenceEqual(rehash);
        }

        private static byte[] Pbkdf2(string password, byte[] salt)
        {
            using (var func = new Rfc2898DeriveBytes(password, salt))
            {
                func.IterationCount = HashIterations;
                return func.GetBytes(HashLength);
            }
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
