using System;
using Lucent.Core;

namespace Lucent.Tools.ConfigInit
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Lucent Secure Config Initialiser\n");

            var config = new SecureConfig();

            Console.Write("Client ID: ");
            config.ClientId = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Client Secret: ");
            config.ClientSecret = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Redirect URI: ");
            config.RedirectUri = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Refresh Token (leave blank if not yet obtained): ");
            config.RefreshToken = Console.ReadLine()?.Trim() ?? string.Empty;

            SecureConfigStore.Save(config);

            Console.WriteLine("\nConfig saved to encrypted token.dat.\n");

            // Test read-back for confirmation
            try
            {
                var reloaded = SecureConfigStore.Load();
                Console.WriteLine("Decryption successful. Reloaded values:");
                Console.WriteLine($"Client ID: {reloaded.ClientId}");
                Console.WriteLine($"Redirect URI: {reloaded.RedirectUri}");
                Console.WriteLine("(Client Secret and Refresh Token omitted for security.)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reload and decrypt config: {ex.Message}");
            }
        }
    }
}
