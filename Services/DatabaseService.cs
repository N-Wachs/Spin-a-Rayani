using System.Text;
using System.Text.Json;
using SpinARayan.Models;
using System.Numerics;

namespace SpinARayan.Services
{
    /// <summary>
    /// Service for synchronizing player data with Supabase PostgreSQL database.
    /// Handles CRUD operations for User and Savefiles tables with authentication.
    /// NEW: Tracks admin_used and last_played, supports multiple savefiles per user.
    /// SECURITY: Passwords are encrypted before storing in database using XOR cipher.
    /// </summary>
    public class DatabaseService
    {
        private readonly HttpClient _httpClient;
        private string _username;
        private string? _currentSavefileId;
        private string? _currentUserId;
        private bool _adminUsedThisSession = false;

        private const string SUPABASE_URL = "https://gflohnjhunyukdayaahn.supabase.co/rest/v1";
        private const string SUPABASE_KEY = "sb_publishable_dZXMv77hZa3_vZbQTYSKeQ_rZ49Ro9w";

        // Simple encryption key for password storage (XOR cipher)
        private const string ENCRYPTION_KEY = "SpinARayanSecretKey2025";

        // Current game version for savefile tracking
        private const string GAME_VERSION = "4.0.0";
        
        // Minimum required version for savefiles
        private const string MIN_REQUIRED_VERSION = "4.0.0";

        public DatabaseService(string username)
        {
            _username = username;

            // Setup HTTP client with Supabase authentication
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", SUPABASE_KEY);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {SUPABASE_KEY}");
            _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

            // Set timeout to 10 seconds for better error handling
            _httpClient.Timeout = TimeSpan.FromSeconds(10);

            Console.WriteLine($"[DatabaseService] Initialized for user: {_username}");
        }

        /// <summary>
        /// Check if database is reachable
        /// </summary>
        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{SUPABASE_URL}/User?limit=1");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypt password using simple XOR cipher before storing in DB
        /// </summary>
        private string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            byte[] encrypted = new byte[passwordBytes.Length];

            for (int i = 0; i < passwordBytes.Length; i++)
            {
                encrypted[i] = (byte)(passwordBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypt password from DB using XOR cipher
        /// </summary>
        private string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return "";

            try
            {
                byte[] encrypted = Convert.FromBase64String(encryptedPassword);
                byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
                byte[] decrypted = new byte[encrypted.Length];

                for (int i = 0; i < encrypted.Length; i++)
                {
                    decrypted[i] = (byte)(encrypted[i] ^ keyBytes[i % keyBytes.Length]);
                }

                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Track when admin mode is used
        /// </summary>
        public void MarkAdminUsed()
        {
            _adminUsedThisSession = true;
            Console.WriteLine($"[DatabaseService] Admin mode usage tracked");
        }

        /// <summary>
        /// Get current savefile ID
        /// </summary>
        public string? GetCurrentSavefileId() => _currentSavefileId;

        /// <summary>
        /// Get current user ID
        /// </summary>
        public string? GetCurrentUserId() => _currentUserId;

        /// <summary>
        /// Set current savefile and user IDs (for manual selection)
        /// </summary>
        public void SetCurrentSavefile(string savefileId, string userId)
        {
            _currentSavefileId = savefileId;
            _currentUserId = userId;
            Console.WriteLine($"[DatabaseService] Current savefile set to: {savefileId}");
        }

        /// <summary>
        /// Delete a savefile from database
        /// </summary>
        public async Task<bool> DeleteSavefileAsync(string savefileId)
        {
            try
            {
                Console.WriteLine($"[DatabaseService] Deleting savefile: {savefileId}");

                var response = await _httpClient.DeleteAsync($"{SUPABASE_URL}/Savefiles?id=eq.{savefileId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Successfully deleted savefile {savefileId}");

                    // Clear current savefile if it was deleted
                    if (_currentSavefileId == savefileId)
                    {
                        _currentSavefileId = null;
                    }

                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DatabaseService] Failed to delete savefile: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error deleting savefile: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all savefiles for current user (public method)
        /// </summary>
        public async Task<List<SavefileInfo>> GetAllSavefilesAsync()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                Console.WriteLine($"[DatabaseService] No user ID set");
                return new List<SavefileInfo>();
            }

            return await GetUserSavefilesAsync(_currentUserId);
        }

        /// <summary>
        /// Authenticate user and return user ID
        /// </summary>
        public async Task<(bool success, string? userId)> AuthenticateAsync(string username, string password)
        {
            _username = username; // Update username

            var response = await _httpClient.GetAsync($"{SUPABASE_URL}/User?username=eq.{username}");

            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<DbUserData>>(jsonContent);

            if (users == null || users.Count == 0)
            {
                // User doesn't exist
                Console.WriteLine($"[DatabaseService] User {username} not found");
                return (false, null);
            }

            var user = users[0];

            // Check banned_flag FIRST - permanent ban
            if (user.banned_flag)
            {
                Console.WriteLine($"[DatabaseService] User {username} is BANNED!");
                throw new Exception("BANNED:Fehler in der Save-Datei! Bitte kontaktiere den Support.");
            }

            // Decrypt stored password and compare
            string storedPassword = DecryptPassword(user.password);

            if (storedPassword == password)
            {
                _currentUserId = user.id.ToString(); // Convert int to string
                Console.WriteLine($"[DatabaseService] User {username} authenticated successfully");
                
                // Check and reset kick_flag if it's set
                if (user.kick_flag)
                {
                    Console.WriteLine($"[DatabaseService] WARNING: Kick flag was set for user {username}! Resetting to false...");
                    
                    try
                    {
                        var resetData = new Dictionary<string, object>
                        {
                            ["kick_flag"] = false
                        };
                        
                        var resetJson = JsonSerializer.Serialize(resetData);
                        var resetContent = new StringContent(resetJson, Encoding.UTF8, "application/json");
                        
                        var resetResponse = await _httpClient.PatchAsync(
                            $"{SUPABASE_URL}/User?id=eq.{user.id}",
                            resetContent
                        );
                        
                        if (resetResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"[DatabaseService] Kick flag reset successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DatabaseService] Failed to reset kick flag: {ex.Message}");
                    }
                }
                
                return (true, user.id.ToString()); // Convert int to string
            }
            else
            {
                Console.WriteLine($"[DatabaseService] Invalid password for user {username}");
                return (false, null);
            }
        }

        /// <summary>
        /// Register new user - checks if username already exists
        /// </summary>
        public async Task<(bool success, string? userId, string? errorMessage)> RegisterUserAsync(string username, string password)
        {
            _username = username; // Update username

            try
            {
                // Step 1: Check if username already exists
                var checkResponse = await _httpClient.GetAsync($"{SUPABASE_URL}/User?username=eq.{username}");

                if (checkResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await checkResponse.Content.ReadAsStringAsync();
                    var existingUsers = JsonSerializer.Deserialize<List<DbUserData>>(jsonContent);

                    if (existingUsers != null && existingUsers.Count > 0)
                    {
                        // Username already exists!
                        Console.WriteLine($"[DatabaseService] Username {username} already exists");
                        return (false, null, "Username bereits vergeben!");
                    }
                }

                // Step 2: Create new user
                Console.WriteLine($"[DatabaseService] Creating new user: {username}");
                var (success, userId) = await CreateNewUserAsync(username, password);

                if (success && !string.IsNullOrEmpty(userId))
                {
                    return (true, userId, null);
                }
                else
                {
                    return (false, null, "Fehler beim Erstellen des Accounts");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] RegisterUser error: {ex.Message}");
                return (false, null, $"Fehler: {ex.Message}");
            }
        }

        /// <summary>
        /// Create new user with password (password is encrypted before storing)
        /// </summary>
        private async Task<(bool success, string? userId)> CreateNewUserAsync(string username, string password)
        {
            try
            {
                // Encrypt password before storing
                string encryptedPassword = EncryptPassword(password);

                var newUser = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = encryptedPassword, // Store encrypted password
                    ["created_at"] = DateTime.UtcNow.ToString("o"),
                    ["savefile_ids"] = new string[] { }
                };

                var json = JsonSerializer.Serialize(newUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{SUPABASE_URL}/User", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var createdUsers = JsonSerializer.Deserialize<List<DbUserData>>(responseJson);

                    if (createdUsers != null && createdUsers.Count > 0)
                    {
                        var userId = createdUsers[0].id.ToString(); // Convert int to string
                        _currentUserId = userId;
                        Console.WriteLine($"[DatabaseService] Created new user: {userId} (password encrypted)");
                        return (true, userId);
                    }
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] CreateNewUser error: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Save player stats to Savefiles table. Updates last_played and admin_used automatically.
        /// SIMPLE UPLOAD: No conflict resolution, just uploads current state to DB.
        /// </summary>
        public async Task<bool> SavePlayerDataAsync(PlayerStats stats)
        {
            if (string.IsNullOrEmpty(_currentSavefileId) || string.IsNullOrEmpty(_currentUserId))
            {
                Console.WriteLine($"[DatabaseService] No active savefile or user. Call LoadOrCreateSavefileAsync first.");
                return false;
            }

            try
            {
                // STEP 1: Check user flags before saving
                var checkResponse = await _httpClient.GetAsync($"{SUPABASE_URL}/User?id=eq.{_currentUserId}&select=kick_flag,banned_flag");
                
                if (checkResponse.IsSuccessStatusCode)
                {
                    var checkJson = await checkResponse.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<DbUserData>>(checkJson);
                    
                    if (users != null && users.Count > 0)
                    {
                        var user = users[0];
                        
                        // Check banned_flag - permanent ban
                        if (user.banned_flag)
                        {
                            Console.WriteLine($"[DatabaseService] BANNED FLAG DETECTED! User is permanently banned. Closing application...");
                            Environment.Exit(0);
                            return false; // Never reached
                        }
                        
                        // Check kick_flag - temporary kick
                        if (user.kick_flag)
                        {
                            Console.WriteLine($"[DatabaseService] KICK FLAG DETECTED! Closing application...");
                            Environment.Exit(0);
                            return false; // Never reached
                        }
                    }
                }
                
                // STEP 2: Continue with normal save
                var saveData = ConvertStatsToDbFormat(stats);
                var json = JsonSerializer.Serialize(saveData);

                Console.WriteLine($"[DatabaseService] Saving to Savefile {_currentSavefileId}...");
                Console.WriteLine($"[DatabaseService]   Admin used: {_adminUsedThisSession}");

                // Update existing savefile
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync(
                    $"{SUPABASE_URL}/Savefiles?id=eq.{_currentSavefileId}",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Successfully saved Savefile {_currentSavefileId}");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DatabaseService] Failed to save: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error saving data: {ex.Message}");
                return false;
            }
        }

        // REMOVED: MergeStatsFromDb()
        // Old conflict resolution system - no longer used
        // Now using simple "Load once, upload only" approach

        /// <summary>
        /// Load or create savefile for user. Returns PlayerStats or null if failed.
        /// If savefileId provided, loads that specific savefile. Otherwise loads most recent or creates new.
        /// </summary>
        public async Task<PlayerStats?> LoadOrCreateSavefileAsync(string? specificSavefileId = null)
        {
            try
            {
                // Check if we already have user ID (from authentication)
                if (string.IsNullOrEmpty(_currentUserId))
                {
                    // Try to get or create user
                    var (success, userId) = await GetOrCreateUserAsync(_username);

                    if (!success || string.IsNullOrEmpty(userId))
                    {
                        Console.WriteLine($"[DatabaseService] Failed to get/create user");
                        return null;
                    }

                    _currentUserId = userId;
                }

                // If specific savefile requested, load it
                if (!string.IsNullOrEmpty(specificSavefileId))
                {
                    _currentSavefileId = specificSavefileId;
                    return await LoadSavefileAsync(specificSavefileId);
                }

                // Get user's savefiles
                var savefiles = await GetUserSavefilesAsync(_currentUserId);

                if (savefiles.Count == 0)
                {
                    // Create new savefile
                    Console.WriteLine($"[DatabaseService] No savefiles found, creating new one...");
                    var newStats = new PlayerStats(); // Default stats
                    var savefileId = await CreateSavefileAsync(newStats, _currentUserId);

                    if (string.IsNullOrEmpty(savefileId))
                    {
                        Console.WriteLine($"[DatabaseService] Failed to create savefile");
                        return null;
                    }

                    _currentSavefileId = savefileId;
                    return newStats;
                }
                else
                {
                    // Load most recent savefile
                    var mostRecent = savefiles[0];
                    Console.WriteLine($"[DatabaseService] Loading most recent savefile: {mostRecent.Id}");
                    _currentSavefileId = mostRecent.Id;

                    return await LoadSavefileAsync(mostRecent.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] LoadOrCreateSavefile error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load player stats from specific Savefile
        /// </summary>
        private async Task<PlayerStats?> LoadSavefileAsync(string savefileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{SUPABASE_URL}/Savefiles?id=eq.{savefileId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Savefile {savefileId} not found");
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var savefiles = JsonSerializer.Deserialize<List<DbSavefileData>>(jsonContent);

                if (savefiles == null || savefiles.Count == 0)
                {
                    Console.WriteLine($"[DatabaseService] No data found for Savefile {savefileId}");
                    return null;
                }

                var saveData = savefiles[0];
                
                // VERSION CHECK: Ensure savefile is 4.0.0 or higher
                string saveVersion = saveData.created_in_version ?? "0.0.0";
                if (!IsVersionCompatible(saveVersion, MIN_REQUIRED_VERSION))
                {
                    Console.WriteLine($"[DatabaseService] Savefile version {saveVersion} is too old! Minimum required: {MIN_REQUIRED_VERSION}");
                    Console.WriteLine($"[DatabaseService] Deleting incompatible savefile {savefileId}...");
                    
                    // Delete the old savefile
                    await DeleteSavefileAsync(savefileId);
                    
                    throw new Exception($"INCOMPATIBLE_VERSION:Dein Savefile wurde mit Version {saveVersion} erstellt.\n\n" +
                                      $"Version {GAME_VERSION} verwendet ein neues Roll-System und ist nicht kompatibel mit alten Saves.\n\n" +
                                      $"Dein alter Save wurde gelöscht.\n" +
                                      $"Bitte starte das Spiel neu.");
                }

                // Track if admin was used in this save (never downgrade from true to false)
                if (saveData.admin_used && !_adminUsedThisSession)
                {
                    _adminUsedThisSession = true;
                    Console.WriteLine($"[DatabaseService] Admin flag loaded from savefile (was already used)");
                }

                var stats = ConvertDbFormatToStats(saveData);

                Console.WriteLine($"[DatabaseService] Successfully loaded Savefile {savefileId}");
                Console.WriteLine($"[DatabaseService]   Version: {saveVersion}");
                Console.WriteLine($"[DatabaseService]   Last played: {saveData.last_played}");
                Console.WriteLine($"[DatabaseService]   Admin used: {saveData.admin_used}");

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error loading savefile: {ex.Message}");
                
                // Re-throw version incompatibility exceptions
                if (ex.Message.StartsWith("INCOMPATIBLE_VERSION:"))
                {
                    throw;
                }
                
                return null;
            }
        }
        
        /// <summary>
        /// Check if a version is compatible (>= minimum required version)
        /// </summary>
        private bool IsVersionCompatible(string version, string minRequired)
        {
            try
            {
                var vParts = version.Split('.').Select(int.Parse).ToArray();
                var minParts = minRequired.Split('.').Select(int.Parse).ToArray();
                
                // Compare major.minor.patch
                for (int i = 0; i < Math.Min(3, Math.Min(vParts.Length, minParts.Length)); i++)
                {
                    if (vParts[i] > minParts[i]) return true;
                    if (vParts[i] < minParts[i]) return false;
                }
                
                return true; // Equal versions
            }
            catch
            {
                return false; // Invalid version format
            }
        }

        /// <summary>
        /// Get all savefiles for a user
        /// </summary>
        private async Task<List<SavefileInfo>> GetUserSavefilesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{SUPABASE_URL}/Savefiles?user_id=eq.{userId}&order=last_played.desc");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Failed to load savefiles for user {userId}");
                    return new List<SavefileInfo>();
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                
                // Try to deserialize - if it fails, try to at least count the savefiles
                try
                {
                    var savefiles = JsonSerializer.Deserialize<List<DbSavefileData>>(jsonContent);

                    if (savefiles == null || savefiles.Count == 0)
                    {
                        return new List<SavefileInfo>();
                    }

                    Console.WriteLine($"[DatabaseService] Found {savefiles.Count} savefile(s) for user");

                    return savefiles.Select(s => new SavefileInfo
                    {
                        Id = s.id.ToString(), // Convert int to string
                        LastPlayed = s.last_played,
                        Rebirths = s.rebirths,
                        Money = s.money ?? "0",
                        Gems = s.gems,
                        AdminUsed = s.admin_used
                    }).ToList();
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"[DatabaseService] JSON Deserialization Error: {jsonEx.Message}");
                    Console.WriteLine($"[DatabaseService] This usually means the data in the database is corrupted or in wrong format.");
                    Console.WriteLine($"[DatabaseService] Attempting to parse basic info from raw JSON...");
                    
                    // Try to at least count how many savefiles exist using basic JSON parsing
                    try
                    {
                        using var doc = JsonDocument.Parse(jsonContent);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            int count = doc.RootElement.GetArrayLength();
                            Console.WriteLine($"[DatabaseService] Found {count} savefile(s) in database (but data is corrupted)");
                            
                            // Return basic info so the system knows savefiles exist
                            var savefileInfos = new List<SavefileInfo>();
                            for (int i = 0; i < count; i++)
                            {
                                var element = doc.RootElement[i];
                                
                                // Try to extract basic info
                                var info = new SavefileInfo
                                {
                                    Id = element.TryGetProperty("id", out var idProp) ? idProp.GetInt32().ToString() : "unknown",
                                    LastPlayed = element.TryGetProperty("last_played", out var lpProp) ? lpProp.GetString() : null,
                                    Rebirths = element.TryGetProperty("rebirths", out var rebProp) ? rebProp.GetInt32() : 0,
                                    Money = element.TryGetProperty("money", out var moneyProp) ? moneyProp.GetString() ?? "0" : "0",
                                    Gems = element.TryGetProperty("gems", out var gemsProp) ? gemsProp.GetInt32() : 0,
                                    AdminUsed = element.TryGetProperty("admin_used", out var adminProp) && adminProp.GetBoolean()
                                };
                                
                                savefileInfos.Add(info);
                            }
                            
                            if (savefileInfos.Count > 0)
                            {
                                Console.WriteLine($"[DatabaseService] WARNING: Savefiles exist but have data corruption!");
                                Console.WriteLine($"[DatabaseService] You may need to delete corrupted savefiles manually from the database.");
                                Console.WriteLine($"[DatabaseService] Problematic fields are likely: inventory, owned_dice, equipped_rayan_indices, saved_quests");
                            }
                            
                            return savefileInfos;
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"[DatabaseService] Failed to parse even basic JSON: {parseEx.Message}");
                    }
                    
                    return new List<SavefileInfo>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error loading savefiles: {ex.Message}");
                return new List<SavefileInfo>();
            }
        }

        /// <summary>
        /// Create new savefile for user
        /// </summary>
        private async Task<string?> CreateSavefileAsync(PlayerStats stats, string userId)
        {
            try
            {
                Console.WriteLine($"[DatabaseService] Creating savefile for user_id: {userId}");

                var saveData = ConvertStatsToDbFormat(stats);
                saveData["user_id"] = userId;
                saveData["created_at"] = DateTime.UtcNow.ToString("o");

                // Debug: Show what we're sending
                Console.WriteLine($"[DatabaseService] Save data keys: {string.Join(", ", saveData.Keys)}");
                foreach (var key in new[] { "inventory", "equipped_rayan_indices", "owned_dice", "saved_quests" })
                {
                    if (saveData.ContainsKey(key))
                    {
                        var value = saveData[key].ToString();
                        Console.WriteLine($"[DatabaseService]   {key}: {(value?.Length > 50 ? value.Substring(0, 50) + "..." : value)}");
                    }
                }

                var json = JsonSerializer.Serialize(saveData);
                Console.WriteLine($"[DatabaseService] JSON payload size: {json.Length} bytes");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"[DatabaseService] Sending POST request to {SUPABASE_URL}/Savefiles");
                var response = await _httpClient.PostAsync($"{SUPABASE_URL}/Savefiles", content);

                Console.WriteLine($"[DatabaseService] Response status: {response.StatusCode} ({(int)response.StatusCode})");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DatabaseService] Response body: {responseJson}");

                    var createdSavefiles = JsonSerializer.Deserialize<List<DbSavefileData>>(responseJson);

                    if (createdSavefiles != null && createdSavefiles.Count > 0)
                    {
                        var savefileId = createdSavefiles[0].id.ToString(); // Convert int to string
                        Console.WriteLine($"[DatabaseService] Created new Savefile: {savefileId}");
                        return savefileId;
                    }
                    else
                    {
                        Console.WriteLine($"[DatabaseService] ERROR: Failed to parse created savefile response");
                        Console.WriteLine($"[DatabaseService] Response was: {responseJson}");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DatabaseService] Failed to create savefile: {response.StatusCode}");
                    Console.WriteLine($"[DatabaseService] Error details: {error}");

                    // Parse error for helpful hints
                    if (error.Contains("malformed array literal"))
                    {
                        Console.WriteLine($"[DatabaseService] ERROR TYPE: PostgreSQL Array format problem!");
                        Console.WriteLine($"[DatabaseService] HINT: Check if DB columns are JSONB (not ARRAY type)");
                        Console.WriteLine($"[DatabaseService] Problem columns: inventory, equipped_rayan_indices, owned_dice, saved_quests");
                        Console.WriteLine($"[DatabaseService] They should be defined as JSONB, not TEXT[] or similar");
                    }
                    else if (error.Contains("column") && error.Contains("does not exist"))
                    {
                        Console.WriteLine($"[DatabaseService] ERROR TYPE: Missing database column!");
                    }
                    else if (error.Contains("violates") && error.Contains("constraint"))
                    {
                        Console.WriteLine($"[DatabaseService] ERROR TYPE: Constraint violation (duplicate or invalid data)");
                    }
                }

                return null;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[DatabaseService] ERROR: Request timeout after {_httpClient.Timeout.TotalSeconds}s");
                Console.WriteLine($"[DatabaseService] Exception: {ex.Message}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DatabaseService] ERROR: Network/HTTP error");
                Console.WriteLine($"[DatabaseService] Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DatabaseService] Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] ERROR: Unexpected error creating savefile");
                Console.WriteLine($"[DatabaseService] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[DatabaseService] Message: {ex.Message}");
                Console.WriteLine($"[DatabaseService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Create new savefile for current user (public method)
        /// MAX 10 SAVEFILES PER USER
        /// </summary>
        public async Task<string?> CreateNewSavefileAsync()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                Console.WriteLine($"[DatabaseService] No user ID set");
                return null;
            }

            // Check savefile limit
            var existingSavefiles = await GetUserSavefilesAsync(_currentUserId);
            if (existingSavefiles.Count >= 10)
            {
                Console.WriteLine($"[DatabaseService] Cannot create savefile: Limit of 10 savefiles reached!");
                return null;
            }

            var newStats = new PlayerStats(); // Default stats
            var savefileId = await CreateSavefileAsync(newStats, _currentUserId);

            if (!string.IsNullOrEmpty(savefileId))
            {
                _currentSavefileId = savefileId;
            }

            return savefileId;
        }

        /// <summary>
        /// Get or create user by username (fallback method, creates with default password)
        /// </summary>
        private async Task<(bool success, string? userId)> GetOrCreateUserAsync(string username)
        {
            try
            {
                // Try to find existing user
                var response = await _httpClient.GetAsync($"{SUPABASE_URL}/User?username=eq.{username}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<DbUserData>>(jsonContent);

                    if (users != null && users.Count > 0)
                    {
                        var userId = users[0].id.ToString(); // Convert int to string
                        Console.WriteLine($"[DatabaseService] Found existing user: {userId}");
                        return (true, userId);
                    }
                }

                // User doesn't exist, create new one with default password
                Console.WriteLine($"[DatabaseService] User {username} not found, creating with default password...");

                // Encrypt default password
                string encryptedPassword = EncryptPassword("default");

                var newUser = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = encryptedPassword, // Store encrypted default password
                    ["created_at"] = DateTime.UtcNow.ToString("o"),
                    ["savefile_ids"] = new string[] { }
                };

                var json = JsonSerializer.Serialize(newUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var createResponse = await _httpClient.PostAsync($"{SUPABASE_URL}/User", content);

                if (createResponse.IsSuccessStatusCode)
                {
                    var responseJson = await createResponse.Content.ReadAsStringAsync();
                    var createdUsers = JsonSerializer.Deserialize<List<DbUserData>>(responseJson);

                    if (createdUsers != null && createdUsers.Count > 0)
                    {
                        var userId = createdUsers[0].id.ToString(); // Convert int to string
                        Console.WriteLine($"[DatabaseService] Created new user: {userId} (password encrypted)");
                        return (true, userId);
                    }
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] GetOrCreateUser error: {ex.Message}");
                return (false, null);
            }
        }

        private Dictionary<string, object> ConvertStatsToDbFormat(PlayerStats stats)
        {
            // Handle empty collections properly
            var inventoryJson = stats.Inventory?.Count > 0
                ? JsonSerializer.Serialize(stats.Inventory)
                : "[]";

            var equippedIndicesJson = stats.EquippedRayanIndices?.Count > 0
                ? JsonSerializer.Serialize(stats.EquippedRayanIndices)
                : "[]";

            var ownedDiceJson = stats.OwnedDices?.Count > 0
                ? JsonSerializer.Serialize(stats.OwnedDices)
                : "[]";

            var savedQuestsJson = stats.SavedQuests?.Count > 0
                ? JsonSerializer.Serialize(stats.SavedQuests)
                : "[]";

            return new Dictionary<string, object>
            {
                ["last_played"] = DateTime.UtcNow.ToString("o"),
                ["admin_used"] = _adminUsedThisSession,
                ["created_in_version"] = GAME_VERSION,
                ["money"] = stats.Money.ToString(),
                ["total_money_earned"] = stats.TotalMoneyEarned.ToString(),
                ["gems"] = stats.Gems,
                ["rebirths"] = stats.Rebirths,
                ["plot_slots"] = stats.PlotSlots,
                ["roll_colldown"] = stats.RollCooldown.ToString(),
                ["roll_coldown_level"] = stats.RollCooldownLevel,
                ["total_rolls_altime"] = stats.TotalRollsAllTime,
                ["autoroll_unlocked"] = stats.AutoRollUnlocked,
                ["autoroll_active"] = stats.AutoRollActive,
                ["playtime_minutes"] = stats.PlayTimeMinutes,
                ["luckbooster_level"] = stats.LuckBoosterLevel,
                ["total_rebirths"] = stats.TotalRebirthsAllTime,
                ["total_playtime_minutes"] = stats.TotalPlayTimeMinutes,
                ["best_rayan_ever_name"] = stats.BestRayanEverName ?? "",
                ["best_rayan_rarity"] = stats.BestRayanEverRarity.ToString(),
                ["best_rayan_value"] = stats.BestRayanEverValue.ToString(),
                ["inventory"] = inventoryJson,
                ["equipped_rayan_indices"] = equippedIndicesJson,
                ["owned_dice"] = ownedDiceJson,
                ["selected_dice_index"] = stats.SelectedDiceIndex,
                ["saved_quests"] = savedQuestsJson,
                ["next_rebirth"] = stats.NextRebirthTarget.ToString(),
                ["rarity_quest_level"] = stats.RarityQuestLevel
            };
        }

        private PlayerStats ConvertDbFormatToStats(DbSavefileData saveData)
        {
            var stats = new PlayerStats
            {
                Money = BigInteger.Parse(saveData.money ?? "0"),
                TotalMoneyEarned = BigInteger.Parse(saveData.total_money_earned ?? "0"),
                Gems = saveData.gems,
                Rebirths = saveData.rebirths,
                PlotSlots = saveData.plot_slots,
                RollCooldown = (double)decimal.Parse(saveData.roll_colldown ?? "2"), // ?? DB-Tippfehler
                RollCooldownLevel = saveData.roll_coldown_level, // ?? DB-Tippfehler
                TotalRollsAllTime = (int)saveData.total_rolls_altime, // ?? DB-Tippfehler
                AutoRollUnlocked = saveData.autoroll_unlocked,
                AutoRollActive = saveData.autoroll_active,
                PlayTimeMinutes = saveData.playtime_minutes,
                LuckBoosterLevel = saveData.luckbooster_level,
                TotalRebirthsAllTime = saveData.total_rebirths,
                TotalPlayTimeMinutes = saveData.total_playtime_minutes,
                BestRayanEverName = saveData.best_rayan_ever_name,
                BestRayanEverRarity = double.Parse(saveData.best_rayan_rarity ?? "0"),
                BestRayanEverValue = BigInteger.Parse(saveData.best_rayan_value ?? "0"),
                SelectedDiceIndex = saveData.selected_dice_index,
                NextRebirthTarget = int.TryParse(saveData.next_rebirth, out int target) ? target : saveData.rebirths + 1,
                RarityQuestLevel = saveData.rarity_quest_level
            };

            // Deserialize JSONB arrays
            if (!string.IsNullOrEmpty(saveData.equipped_rayan_indices))
            {
                stats.EquippedRayanIndices = JsonSerializer.Deserialize<List<int>>(saveData.equipped_rayan_indices) ?? new List<int>();
            }

            if (!string.IsNullOrEmpty(saveData.inventory))
            {
                stats.Inventory = JsonSerializer.Deserialize<List<Rayan>>(saveData.inventory) ?? new List<Rayan>();
            }

            if (!string.IsNullOrEmpty(saveData.owned_dice))
            {
                stats.OwnedDices = JsonSerializer.Deserialize<List<Dice>>(saveData.owned_dice) ?? new List<Dice>();
            }

            if (!string.IsNullOrEmpty(saveData.saved_quests))
            {
                stats.SavedQuests = JsonSerializer.Deserialize<List<Quest>>(saveData.saved_quests) ?? new List<Quest>();
            }

            return stats;
        }
        
        /// <summary>
        /// Get leaderboard data for different categories (only non-admin players)
        /// </summary>
        /// <param name="category">0=Money, 1=Rarest Rayan, 2=Rolls, 3=Gems, 4=PlayTime, 5=Rayans</param>
        public async Task<List<Forms.Dialogs.LeaderboardEntry>> GetLeaderboardAsync(int category)
        {
            try
            {
                Console.WriteLine($"[DatabaseService] Fetching leaderboard for category: {category}");
                
                // Build query based on category
                string orderByField = category switch
                {
                    0 => "total_money_earned",        // Money
                    1 => "best_rayan_rarity",         // Rarest Rayan
                    2 => "total_rolls_altime",        // Rolls (DB has typo: altime instead of alltime)
                    3 => "gems",                      // Gems
                    4 => "total_playtime_minutes",    // PlayTime (DB has no underscore)
                    5 => "inventory",                 // Rayans (will calculate from JSON)
                    _ => "total_money_earned"
                };
                
                // Fetch top 50 savefiles where:
                // - admin_used = false
                // - created_in_version >= 4.0.0 (new roll system)
                string url = $"{SUPABASE_URL}/Savefiles?" +
                             $"select=id,user_id,{orderByField},best_rayan_ever_name,best_rayan_rarity,inventory,created_in_version" +
                             $"&admin_used=eq.false" +
                             $"&order={orderByField}.desc" +
                             $"&limit=50";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Leaderboard fetch failed: {response.StatusCode}");
                    return new List<Forms.Dialogs.LeaderboardEntry>();
                }
                
                var savefilesJson = await response.Content.ReadAsStringAsync();
                var savefiles = JsonSerializer.Deserialize<List<JsonElement>>(savefilesJson);
                
                if (savefiles == null || savefiles.Count == 0)
                {
                    return new List<Forms.Dialogs.LeaderboardEntry>();
                }
                
                // Get usernames for all user IDs
                var userIds = savefiles
                    .Where(s => s.TryGetProperty("user_id", out _))
                    .Select(s => s.GetProperty("user_id").GetInt32().ToString()) // user_id is INTEGER in DB
                    .Distinct()
                    .ToList();
                
                var usernames = await GetUsernamesForUserIdsAsync(userIds);
                
                // Build leaderboard entries
                var entries = new List<Forms.Dialogs.LeaderboardEntry>();
                
                foreach (var savefile in savefiles)
                {
                    // VERSION FILTER: Only include savefiles from version 4.0.0+
                    string saveVersion = "0.0.0";
                    if (savefile.TryGetProperty("created_in_version", out var versionProp))
                    {
                        saveVersion = versionProp.GetString() ?? "0.0.0";
                    }
                    
                    if (!IsVersionCompatible(saveVersion, MIN_REQUIRED_VERSION))
                    {
                        Console.WriteLine($"[DatabaseService] Skipping savefile with old version: {saveVersion}");
                        continue; // Skip old versions
                    }
                    
                    var userId = savefile.GetProperty("user_id").GetInt32().ToString();
                    if (string.IsNullOrEmpty(userId) || !usernames.ContainsKey(userId))
                        continue;
                    
                    var entry = new Forms.Dialogs.LeaderboardEntry
                    {
                        Username = usernames[userId]
                    };
                    
                    // Format value based on category
                    switch (category)
                    {
                        case 0: // Money
                            if (savefile.TryGetProperty("total_money_earned", out var moneyProp))
                            {
                                var moneyStr = moneyProp.GetString() ?? "0";
                                if (BigInteger.TryParse(moneyStr, out var money))
                                {
                                    entry.ValueFormatted = $"?? {FormatBigNumber(money)} Money verdient";
                                    entry.RawValue = (long)Math.Min((double)money, long.MaxValue);
                                }
                            }
                            break;
                            
                        case 1: // Rarest Rayan
                            if (savefile.TryGetProperty("best_rayan_rarity", out var rarityProp) &&
                                savefile.TryGetProperty("best_rayan_ever_name", out var nameProp))
                            {
                                var rarityStr = rarityProp.GetString() ?? "0";
                                if (double.TryParse(rarityStr, out var rarity))
                                {
                                    var name = nameProp.GetString() ?? "Unbekannt";
                                    entry.ValueFormatted = $"? {name} (1 in {rarity:N0})";
                                    entry.RawValue = (long)rarity;
                                }
                            }
                            break;
                            
                        case 2: // Rolls
                            if (savefile.TryGetProperty("total_rolls_altime", out var rollsProp))
                            {
                                var rolls = rollsProp.GetInt64();
                                entry.ValueFormatted = $"?? {rolls:N0} Rolls";
                                entry.RawValue = rolls;
                            }
                            break;
                            
                        case 3: // Gems
                            if (savefile.TryGetProperty("gems", out var gemsProp))
                            {
                                var gems = gemsProp.GetInt32();
                                entry.ValueFormatted = $"?? {gems:N0} Gems";
                                entry.RawValue = gems;
                            }
                            break;
                            
                        case 4: // PlayTime
                            if (savefile.TryGetProperty("total_playtime_minutes", out var timeProp))
                            {
                                var minutes = timeProp.GetDouble();
                                entry.ValueFormatted = $"?? {FormatPlayTime(minutes)}";
                                entry.RawValue = (long)minutes;
                            }
                            break;
                            
                        case 5: // Rayans (inventory count)
                            if (savefile.TryGetProperty("inventory", out var invProp))
                            {
                                try
                                {
                                    var inventoryJson = invProp.GetString() ?? "[]";
                                    var inventory = JsonSerializer.Deserialize<List<JsonElement>>(inventoryJson);
                                    var count = inventory?.Count ?? 0;
                                    entry.ValueFormatted = $"?? {count:N0} Rayans";
                                    entry.RawValue = count;
                                }
                                catch
                                {
                                    entry.ValueFormatted = "?? 0 Rayans";
                                    entry.RawValue = 0;
                                }
                            }
                            break;
                    }
                    
                    entries.Add(entry);
                }
                
                // Sort by raw value descending
                return entries.OrderByDescending(e => e.RawValue).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error fetching leaderboard: {ex.Message}");
                return new List<Forms.Dialogs.LeaderboardEntry>();
            }
        }
        
        /// <summary>
        /// Get usernames for multiple user IDs
        /// </summary>
        private async Task<Dictionary<string, string>> GetUsernamesForUserIdsAsync(List<string> userIds)
        {
            var result = new Dictionary<string, string>();
            
            if (userIds.Count == 0)
                return result;
            
            try
            {
                // Build query with IN clause (user IDs are integers in DB)
                var idsParam = string.Join(",", userIds);
                string url = $"{SUPABASE_URL}/User?select=id,username&id=in.({idsParam})";
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<JsonElement>>(json);
                    
                    if (users != null)
                    {
                        foreach (var user in users)
                        {
                            var id = user.GetProperty("id").GetInt32().ToString();
                            var username = user.GetProperty("username").GetString();
                            
                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(username))
                            {
                                result[id] = username;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error fetching usernames: {ex.Message}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Format BigInteger for display
        /// </summary>
        private string FormatBigNumber(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            if (value < 1000000000000000) return ((double)value / 1000000000000).ToString("F1") + "T";
            return value.ToString("E1");
        }
        
        /// <summary>
        /// Format play time for display
        /// </summary>
        private string FormatPlayTime(double minutes)
        {
            if (minutes < 60)
                return $"{minutes:F0} Minuten";
            
            double hours = minutes / 60;
            if (hours < 24)
                return $"{hours:F1} Stunden";
            
            double days = hours / 24;
            return $"{days:F1} Tage";
        }
        
        /// <summary>
        /// Save user feedback to User table (appends to feedback_send with "?\?" separator)
        /// </summary>
        public async Task<bool> SaveFeedbackAsync(string username, string feedback)
        {
            try
            {
                Console.WriteLine($"[DatabaseService] Saving feedback for user: {username}");
                
                // Get current feedback from user
                var response = await _httpClient.GetAsync($"{SUPABASE_URL}/User?username=eq.{username}&select=feedback_send");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Failed to fetch user feedback");
                    return false;
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<JsonElement>>(json);
                
                if (users == null || users.Count == 0)
                {
                    Console.WriteLine($"[DatabaseService] User not found");
                    return false;
                }
                
                // Get existing feedback
                string existingFeedback = "";
                if (users[0].TryGetProperty("feedback_send", out var feedbackProp))
                {
                    existingFeedback = feedbackProp.GetString() ?? "";
                }
                
                // Append new feedback with timestamp and separator
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string newFeedbackEntry = $"[{timestamp}] {feedback}";
                
                string updatedFeedback;
                if (string.IsNullOrEmpty(existingFeedback))
                {
                    updatedFeedback = newFeedbackEntry;
                }
                else
                {
                    updatedFeedback = existingFeedback + @"?\?" + newFeedbackEntry;
                }
                
                // Update user feedback
                var updateData = new Dictionary<string, object>
                {
                    ["feedback_send"] = updatedFeedback
                };
                
                var updateJson = JsonSerializer.Serialize(updateData);
                var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                
                var updateResponse = await _httpClient.PatchAsync(
                    $"{SUPABASE_URL}/User?username=eq.{username}",
                    content
                );
                
                if (updateResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DatabaseService] Feedback saved successfully");
                    return true;
                }
                else
                {
                    var error = await updateResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DatabaseService] Failed to save feedback: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error saving feedback: {ex.Message}");
                return false;
            }
        }

        // Database models
        private class DbUserData
        {
            public int id { get; set; } // Changed from string? to int
            public string? created_at { get; set; }
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public string[]? savefile_ids { get; set; }
            public bool kick_flag { get; set; } // NEW: Kick flag for admin moderation (reset on load)
            public bool banned_flag { get; set; } // NEW: Permanent ban flag (never reset)
        }

        private class DbSavefileData
        {
            public int id { get; set; }
            public string? created_at { get; set; }
            public string? created_in_version { get; set; }
            public string? money { get; set; }
            public string? total_money_earned { get; set; }
            public int gems { get; set; }
            public int rebirths { get; set; }
            public int plot_slots { get; set; }
            public string? roll_colldown { get; set; }
            public int roll_coldown_level { get; set; }
            public long total_rolls_altime { get; set; }
            public bool autoroll_unlocked { get; set; }
            public bool autoroll_active { get; set; }
            public double playtime_minutes { get; set; }
            public int luckbooster_level { get; set; }
            public int total_rebirths { get; set; }
            public double total_playtime_minutes { get; set; }
            public string? best_rayan_ever_name { get; set; }
            public string? best_rayan_rarity { get; set; }
            public string? best_rayan_value { get; set; }
            public string? inventory { get; set; }
            public string? equipped_rayan_indices { get; set; }
            public string? owned_dice { get; set; }
            public int selected_dice_index { get; set; }
            public string? saved_quests { get; set; }
            public int user_id { get; set; } // ? INTEGER (wie in DB)
            public string? last_played { get; set; }
            public bool admin_used { get; set; }
            public string? next_rebirth { get; set; } // NEW: Next rebirth target (string number)
            public int rarity_quest_level { get; set; } // NEW: Current level of rarity quest (0-based)
        }
    }
    
    /// <summary>
    /// Info about a savefile for UI display
    /// </summary>
    public class SavefileInfo
    {
        public string Id { get; set; } = "";
        public string? LastPlayed { get; set; }
        public int Rebirths { get; set; }
        public string Money { get; set; } = "0";
        public int Gems { get; set; }
        public bool AdminUsed { get; set; }
    }
}
