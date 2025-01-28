using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using MauiApp1.Components.Model;

namespace MauiApp1.Services
{
    public class MainService
    {
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static readonly string FolderPath = Path.Combine(DesktopPath, "LocalDB");
        private static readonly string FilePath = Path.Combine(FolderPath, "users.json");

        // Load AppData asynchronously
        public async Task<ApplicationModel> LoadDataAsync()
        {
            if (!File.Exists(FilePath))
                return new ApplicationModel(); // Return empty AppData if no users exist

            try
            {
                var json = await File.ReadAllTextAsync(FilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Deserialize<ApplicationModel>(json, options) ?? new ApplicationModel(); // Deserialize to AppData
            }
            catch (JsonException ex)
            {
                // Log or handle the deserialization error
                Console.WriteLine($"Error deserializing data: {ex.Message}");
                return new ApplicationModel(); // Return empty AppData on failure
            }
        }

        // Save AppData asynchronously
        public async Task SaveDataAsync(ApplicationModel appData)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath); // Create the folder if it does not exist
            }

            // Ensure the file exists
            if (!File.Exists(FilePath))
            {
                // Create the file if it does not exist
                using (File.Create(FilePath)) { } // Using statement ensures the file handle is properly released
            }

            var json = JsonSerializer.Serialize(appData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json); // Write the data asynchronously
        }

        // Hash password asynchronously
        public async Task<string> HashPasswordAsync(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = await Task.Run(() => sha256.ComputeHash(bytes)); // Run the hash computation in a separate thread to avoid blocking
            return Convert.ToBase64String(hash); // Return hashed password
        }

        // Validate password asynchronously
        public async Task<bool> ValidatePasswordAsync(string inputPassword, string storedPassword)
        {
            var hashedInputPassword = await HashPasswordAsync(inputPassword);
            return hashedInputPassword == storedPassword;
        }
    }
}
