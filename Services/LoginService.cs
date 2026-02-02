using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SpinARayan.Services
{
    /// <summary>
    /// Service for managing local login credentials.
    /// Stores username/password encrypted on disk for auto-login.
    /// </summary>
    public class LoginService
    {
        private static readonly string LoginFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpinARayan",
            "login.dat"
        );
        
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("SpinARayanKey123"); // Simple key for local encryption

        /// <summary>
        /// Check if user has saved login credentials
        /// </summary>
        public static bool HasSavedLogin()
        {
            return File.Exists(LoginFilePath);
        }

        /// <summary>
        /// Save login credentials to disk (encrypted)
        /// </summary>
        public static void SaveLogin(string username, string password)
        {
            try
            {
                var loginData = new LoginData
                {
                    Username = username,
                    Password = password,
                    SavedAt = DateTime.Now
                };

                var json = JsonSerializer.Serialize(loginData);
                var encrypted = EncryptString(json);

                Directory.CreateDirectory(Path.GetDirectoryName(LoginFilePath)!);
                File.WriteAllText(LoginFilePath, encrypted);

                Console.WriteLine($"[LoginService] Login saved for user: {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginService] Error saving login: {ex.Message}");
            }
        }

        /// <summary>
        /// Load saved login credentials from disk
        /// </summary>
        public static LoginData? LoadSavedLogin()
        {
            try
            {
                if (!File.Exists(LoginFilePath))
                {
                    return null;
                }

                var encrypted = File.ReadAllText(LoginFilePath);
                var json = DecryptString(encrypted);
                var loginData = JsonSerializer.Deserialize<LoginData>(json);

                Console.WriteLine($"[LoginService] Loaded saved login for user: {loginData?.Username}");
                return loginData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginService] Error loading login: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete saved login credentials (logout)
        /// </summary>
        public static void ClearSavedLogin()
        {
            try
            {
                if (File.Exists(LoginFilePath))
                {
                    File.Delete(LoginFilePath);
                    Console.WriteLine($"[LoginService] Saved login cleared");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginService] Error clearing login: {ex.Message}");
            }
        }

        private static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Login data model
    /// </summary>
    public class LoginData
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime SavedAt { get; set; }
    }
}
