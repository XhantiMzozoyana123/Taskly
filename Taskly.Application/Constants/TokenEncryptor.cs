using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Constants
{

    public static class TokenEncryptor
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY")!);

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);

            var iv = Convert.ToBase64String(aes.IV);
            var cipher = Convert.ToBase64String(ms.ToArray());
            return $"{iv}:{cipher}";
        }

        public static string Decrypt(string encrypted)
        {
            var parts = encrypted.Split(':');
            var iv = Convert.FromBase64String(parts[0]);
            var cipherText = Convert.FromBase64String(parts[1]);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }

}
