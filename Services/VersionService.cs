using System.Reflection;

namespace SpinARayan.Services
{
    public class VersionService
    {
        private const string GITHUB_VERSION_URL = "https://raw.githubusercontent.com/N-Wachs/Spin-a-Rayan/main/version.txt";
        private readonly HttpClient _httpClient;

        public string CurrentVersion { get; private set; }
        public string? LatestVersion { get; private set; }
        public bool IsNewVersionAvailable { get; private set; }

        public VersionService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            CurrentVersion = GetCurrentVersion();
        }

        private string GetCurrentVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_VERSION_URL);
                LatestVersion = response.Trim();

                IsNewVersionAvailable = CompareVersions(CurrentVersion, LatestVersion) < 0;
                return IsNewVersionAvailable;
            }
            catch (Exception ex)
            {
                // Fehler beim Abrufen der Version (z.B. keine Internetverbindung)
                Console.WriteLine($"Version check failed: {ex.Message}");
                IsNewVersionAvailable = false;
                return false;
            }
        }

        private int CompareVersions(string current, string latest)
        {
            try
            {
                var currentParts = current.Split('.').Select(int.Parse).ToArray();
                var latestParts = latest.Split('.').Select(int.Parse).ToArray();

                for (int i = 0; i < Math.Max(currentParts.Length, latestParts.Length); i++)
                {
                    int currentPart = i < currentParts.Length ? currentParts[i] : 0;
                    int latestPart = i < latestParts.Length ? latestParts[i] : 0;

                    if (currentPart < latestPart) return -1;
                    if (currentPart > latestPart) return 1;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public string GetUpdateMessage()
        {
            if (IsNewVersionAvailable)
            {
                return $"?? Neue Version verfügbar!\n\n" +
                       $"Deine Version: {CurrentVersion}\n" +
                       $"Neue Version: {LatestVersion}\n\n" +
                       $"Besuche: https://github.com/N-Wachs/Spin-a-Rayan/releases\n\n" +
                       $"Du kannst jetzt weiterspielen, aber wir empfehlen ein Update!";
            }

            return string.Empty;
        }
    }
}
