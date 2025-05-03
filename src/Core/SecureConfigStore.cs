using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lucent.Core
{
    public class SecureConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }

    public static class SecureConfigStore
    {
        private static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "config", "token.dat");

        public static SecureConfig Load()
        {
            if (!File.Exists(ConfigFilePath))
                throw new FileNotFoundException($"Secure config file not found at: {ConfigFilePath}");

            var encrypted = File.ReadAllBytes(ConfigFilePath);
            var decrypted = Decrypt(encrypted);
            return JsonSerializer.Deserialize<SecureConfig>(decrypted) ?? throw new InvalidOperationException("Failed to parse config");
        }

        public static void Save(SecureConfig config)
        {
            var json = JsonSerializer.Serialize(config);
            var encrypted = Encrypt(json);

            var configDir = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            File.WriteAllBytes(ConfigFilePath, encrypted);
        }

        private static byte[] Encrypt(string plainText)
        {
            return ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser);
        }

        private static string Decrypt(byte[] encryptedData)
        {
            var decryptedBytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
