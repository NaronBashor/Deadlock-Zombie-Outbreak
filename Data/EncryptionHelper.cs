using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    private static readonly string encryptionKey = "2HahaMineIsGizmo"; // Change this to your secret key (should be 16, 24, or 32 characters long for AES)

    public static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create()) {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] iv = aes.IV;

            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream()) {
                ms.Write(iv, 0, iv.Length); // Prepend IV to the ciphertext for decryption later

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                    using (StreamWriter writer = new StreamWriter(cs)) {
                        writer.Write(plainText);
                    }
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string encryptedText)
    {
        using (Aes aes = Aes.Create()) {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] buffer = Convert.FromBase64String(encryptedText);

            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(buffer, iv, iv.Length);
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length)) {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read)) {
                    using (StreamReader reader = new StreamReader(cs)) {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
