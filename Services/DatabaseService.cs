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
        private const string GAME_VERSION = "3.0.0";

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
            
            // Decrypt stored password and compare
            string storedPassword = DecryptPassword(user.password);
            
            if (storedPassword == password)
            {
                _currentUserId = user.id.ToString(); // Convert int to string
                Console.WriteLine($"[DatabaseService] User {username} authenticated successfully");
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
                
                // Track if admin was used in this save
                _adminUsedThisSession = saveData.admin_used;
                
                var stats = ConvertDbFormatToStats(saveData);
                
                Console.WriteLine($"[DatabaseService] Successfully loaded Savefile {savefileId}");
                Console.WriteLine($"[DatabaseService]   Last played: {saveData.last_played}");
                Console.WriteLine($"[DatabaseService]   Admin used: {saveData.admin_used}");
                
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error loading savefile: {ex.Message}");
                return null;
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
        /// </summary>
        public async Task<string?> CreateNewSavefileAsync()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                Console.WriteLine($"[DatabaseService] No user ID set");
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
                ["saved_quests"] = savedQuestsJson
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
                SelectedDiceIndex = saveData.selected_dice_index
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

        // Database models
        private class DbUserData
        {
            public int id { get; set; } // Changed from string? to int
            public string? created_at { get; set; }
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public string[]? savefile_ids { get; set; }
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
