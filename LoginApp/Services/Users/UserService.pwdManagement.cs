
using System.Security.Cryptography;

namespace LoginApp.Services
{
    public partial class UserService
    {
        public static bool VerifyPassword(string inputPwd, string persistedHash, string persistedSalt)
        {
            byte[] hashBytes = Convert.FromBase64String(persistedHash);
            byte[] saltBytes = Convert.FromBase64String(persistedSalt);

            using var pbkdf2 = new Rfc2898DeriveBytes(inputPwd, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] enteredHash = pbkdf2.GetBytes(32);

            return CompareHashes(hashBytes, enteredHash);
        }

        private static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length) return false;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }

            return true;
        }
    }
}
