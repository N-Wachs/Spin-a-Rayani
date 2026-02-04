using System.Text;
using System.Text.Json;
using SpinARayan.Models;
using System.Numerics;
using MySqlConnector;

namespace SpinARayan.Services
{
    /// <summary>
    /// Service for synchronizing player data with MySQL database.
    /// Handles CRUD operations for users and savefile tables with authentication.
    /// NEW: Tracks admin_used and last_played, supports multiple savefiles per user.
    /// SECURITY: Passwords are encrypted before storing in database using XOR cipher.
    /// </summary>
    public class DatabaseService
    {
        private string _username;
        private string? _currentSavefileId;
        private string? _currentUserId;
        private bool _adminUsedThisSession = false;
        private const string SERVER_IP = "Server=10.0.2.15;Database=game_db;Uid=root;Pwd=ServerRoot123;"; // Aktuell �ber NAT

        // Simple encryption key for password storage (XOR cipher)
        private const string ENCRYPTION_KEY = "SpinARayanSecretKey2025";

        // Current game version for savefile tracking
        private const string GAME_VERSION = "4.0.0";
        
        // Minimum required version for savefiles
        private const string MIN_REQUIRED_VERSION = "4.0.0";

        public DatabaseService(string username)
        {
            _username = username;
            Console.WriteLine($"[DatabaseService] Initialized for user: {_username}");
        }

        /// <summary>
        /// Get MySQL connection helper
        /// </summary>
        private async Task<MySqlConnection> GetConnectionAsync()
        {
            var conn = new MySqlConnection(SERVER_IP);
            await conn.OpenAsync();
            return conn;
        }

        /// <summary>
        /// Check if database is reachable
        /// </summary>
        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                await using var conn = await GetConnectionAsync();
                return conn.State == System.Data.ConnectionState.Open;
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
                await using var conn = await GetConnectionAsync();
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM savefile WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", long.Parse(savefileId));

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[DatabaseService] Deleted savefile {savefileId}");
                    
                    if (_currentSavefileId == savefileId)
                        _currentSavefileId = null;
                    
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Delete error: {ex.Message}");
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
            _username = username;

            try
            {
                await using var conn = await GetConnectionAsync();
                
                // SELECT user with flags
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, password, kick_flag, banned_flag 
                    FROM users 
                    WHERE username = @username
                ";
                cmd.Parameters.AddWithValue("@username", username);

                await using var reader = await cmd.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    Console.WriteLine($"[DatabaseService] User {username} not found");
                    return (false, null);
                }

                // Check banned_flag FIRST
                bool isBanned = reader.GetBoolean(3);
                if (isBanned)
                {
                    Console.WriteLine($"[DatabaseService] User {username} is BANNED!");
                    throw new Exception("BANNED:Fehler in der Save-Datei! Bitte kontaktiere den Support.");
                }

                // Check password
                string storedPassword = reader.GetString(1);
                string decryptedPassword = DecryptPassword(storedPassword);
                
                if (decryptedPassword != password)
                {
                    Console.WriteLine($"[DatabaseService] Invalid password for user {username}");
                    return (false, null);
                }

                long userId = reader.GetInt64(0);
                bool kickFlag = reader.GetBoolean(2);
                
                _currentUserId = userId.ToString();
                Console.WriteLine($"[DatabaseService] User {username} authenticated successfully");
                
                // Close reader before UPDATE
                await reader.CloseAsync();
                
                // Reset kick_flag if set
                if (kickFlag)
                {
                    Console.WriteLine($"[DatabaseService] Resetting kick flag...");
                    var updateCmd = conn.CreateCommand();
                    updateCmd.CommandText = "UPDATE users SET kick_flag = 0 WHERE id = @id";
                    updateCmd.Parameters.AddWithValue("@id", userId);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                return (true, userId.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Auth error: {ex.Message}");
                
                // Re-throw BANNED exceptions
                if (ex.Message.StartsWith("BANNED:"))
                {
                    throw;
                }
                
                return (false, null);
            }
        }

        /// <summary>
        /// Register new user - checks if username already exists
        /// </summary>
        public async Task<(bool success, string? userId, string? errorMessage)> RegisterUserAsync(string username, string password)
        {
            _username = username;

            try
            {
                await using var conn = await GetConnectionAsync();
                
                // Check if username exists
                var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
                checkCmd.Parameters.AddWithValue("@username", username);
                
                long count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0L);
                
                if (count > 0)
                {
                    Console.WriteLine($"[DatabaseService] Username {username} already exists");
                    return (false, null, "Username bereits vergeben!");
                }

                // Create new user
                string encryptedPassword = EncryptPassword(password);
                
                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO users (username, password, created_at) 
                    VALUES (@username, @password, @created_at);
                    SELECT LAST_INSERT_ID();
                ";
                insertCmd.Parameters.AddWithValue("@username", username);
                insertCmd.Parameters.AddWithValue("@password", encryptedPassword);
                insertCmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

                var userIdObj = await insertCmd.ExecuteScalarAsync();
                
                if (userIdObj != null)
                {
                    long userId = Convert.ToInt64(userIdObj);
                    _currentUserId = userId.ToString();
                    Console.WriteLine($"[DatabaseService] Created new user: {userId}");
                    return (true, userId.ToString(), null);
                }

                return (false, null, "Fehler beim Erstellen des Accounts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] RegisterUser error: {ex.Message}");
                return (false, null, $"Fehler: {ex.Message}");
            }
        }

        /// <summary>
        /// Save player stats to savefile table. Updates last_played and admin_used automatically.
        /// SIMPLE UPLOAD: No conflict resolution, just uploads current state to DB.
        /// </summary>
        public async Task<bool> SavePlayerDataAsync(PlayerStats stats)
        {
            if (string.IsNullOrEmpty(_currentSavefileId) || string.IsNullOrEmpty(_currentUserId))
            {
                Console.WriteLine($"[DatabaseService] No active savefile or user");
                return false;
            }

            try
            {
                await using var conn = await GetConnectionAsync();
                
                // STEP 1: Check flags
                var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT kick_flag, banned_flag 
                    FROM users 
                    WHERE id = @userId
                ";
                checkCmd.Parameters.AddWithValue("@userId", long.Parse(_currentUserId));

                await using var reader = await checkCmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    bool kickFlag = reader.GetBoolean(0);
                    bool bannedFlag = reader.GetBoolean(1);
                    
                    if (bannedFlag)
                    {
                        Console.WriteLine($"[DatabaseService] BANNED! Closing...");
                        Environment.Exit(0);
                    }
                    
                    if (kickFlag)
                    {
                        Console.WriteLine($"[DatabaseService] KICKED! Closing...");
                        Environment.Exit(0);
                    }
                }
                
                await reader.CloseAsync();
                
                // STEP 2: Save data
                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE savefile SET
                        last_played = @last_played,
                        money = @money,
                        total_money_earned = @total_money_earned,
                        gems = @gems,
                        total_gems = @total_gems,
                        rebirths = @rebirths,
                        next_rebirth = @next_rebirth,
                        total_rebirths = @total_rebirths,
                        plot_slots = @plot_slots,
                        roll_cooldown = @roll_cooldown,
                        roll_cooldown_level = @roll_cooldown_level,
                        total_rolls = @total_rolls,
                        autoroll_unlocked = @autoroll_unlocked,
                        autoroll_active = @autoroll_active,
                        playtime_minutes = @playtime_minutes,
                        luckbooster_level = @luckbooster_level,
                        best_rayan_ever_name = @best_rayan_ever_name,
                        best_rayan_ever_rarity = @best_rayan_ever_rarity,
                        best_rayan_value = @best_rayan_value,
                        inventory = @inventory,
                        equipped_rayan_indices = @equipped_rayan_indices,
                        owned_dice = @owned_dice,
                        saved_quests = @saved_quests,
                        admin_used = @admin_used,
                        rarity_quest_level = @rarity_quest_level,
                        selected_dice_index = @selected_dice_index,
                        total_playtime_minutes = @total_playtime_minutes
                    WHERE id = @savefileId
                ";

                updateCmd.Parameters.AddWithValue("@last_played", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@money", stats.Money.ToString());
                updateCmd.Parameters.AddWithValue("@total_money_earned", stats.TotalMoneyEarned.ToString());
                updateCmd.Parameters.AddWithValue("@gems", stats.Gems.ToString());
                updateCmd.Parameters.AddWithValue("@total_gems", stats.Gems.ToString());
                updateCmd.Parameters.AddWithValue("@rebirths", stats.Rebirths.ToString());
                updateCmd.Parameters.AddWithValue("@next_rebirth", stats.NextRebirthTarget.ToString());
                updateCmd.Parameters.AddWithValue("@total_rebirths", stats.TotalRebirthsAllTime.ToString());
                updateCmd.Parameters.AddWithValue("@plot_slots", stats.PlotSlots);
                updateCmd.Parameters.AddWithValue("@roll_cooldown", stats.RollCooldown);
                updateCmd.Parameters.AddWithValue("@roll_cooldown_level", stats.RollCooldownLevel);
                updateCmd.Parameters.AddWithValue("@total_rolls", stats.TotalRollsAllTime);
                updateCmd.Parameters.AddWithValue("@autoroll_unlocked", stats.AutoRollUnlocked);
                updateCmd.Parameters.AddWithValue("@autoroll_active", stats.AutoRollActive);
                updateCmd.Parameters.AddWithValue("@playtime_minutes", stats.PlayTimeMinutes);
                updateCmd.Parameters.AddWithValue("@luckbooster_level", stats.LuckBoosterLevel);
                updateCmd.Parameters.AddWithValue("@best_rayan_ever_name", stats.BestRayanEverName ?? "");
                updateCmd.Parameters.AddWithValue("@best_rayan_ever_rarity", stats.BestRayanEverRarity.ToString());
                updateCmd.Parameters.AddWithValue("@best_rayan_value", stats.BestRayanEverValue.ToString());
                
                // JSON Arrays as String
                updateCmd.Parameters.AddWithValue("@inventory", JsonSerializer.Serialize(stats.Inventory));
                updateCmd.Parameters.AddWithValue("@equipped_rayan_indices", JsonSerializer.Serialize(stats.EquippedRayanIndices));
                updateCmd.Parameters.AddWithValue("@owned_dice", JsonSerializer.Serialize(stats.OwnedDices));
                updateCmd.Parameters.AddWithValue("@saved_quests", JsonSerializer.Serialize(stats.SavedQuests));
                
                updateCmd.Parameters.AddWithValue("@admin_used", _adminUsedThisSession);
                updateCmd.Parameters.AddWithValue("@rarity_quest_level", stats.RarityQuestLevel);
                updateCmd.Parameters.AddWithValue("@selected_dice_index", stats.SelectedDiceIndex);
                updateCmd.Parameters.AddWithValue("@total_playtime_minutes", stats.TotalPlayTimeMinutes);
                updateCmd.Parameters.AddWithValue("@savefileId", long.Parse(_currentSavefileId));

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[DatabaseService] Saved savefile {_currentSavefileId}");
                    return true;
                }

                return false;
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
                await using var conn = await GetConnectionAsync();
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        id, created_in_version, money, total_money_earned,
                        gems, rebirths, next_rebirth, total_rebirths,
                        plot_slots, roll_cooldown, roll_cooldown_level,
                        total_rolls, autoroll_unlocked, autoroll_active,
                        playtime_minutes, luckbooster_level,
                        best_rayan_ever_name, best_rayan_ever_rarity,
                        inventory, equipped_rayan_indices, owned_dice, saved_quests,
                        admin_used, rarity_quest_level, last_played, best_rayan_value,
                        selected_dice_index, total_playtime_minutes
                    FROM savefile
                    WHERE id = @savefileId
                ";
                cmd.Parameters.AddWithValue("@savefileId", long.Parse(savefileId));

                await using var reader = await cmd.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    Console.WriteLine($"[DatabaseService] Savefile {savefileId} not found");
                    return null;
                }

                // VERSION CHECK
                string saveVersion = reader.IsDBNull(1) ? "0.0.0" : reader.GetString(1);
                if (!IsVersionCompatible(saveVersion, MIN_REQUIRED_VERSION))
                {
                    Console.WriteLine($"[DatabaseService] Version {saveVersion} too old!");
                    await reader.CloseAsync();
                    await DeleteSavefileAsync(savefileId);
                    
                    throw new Exception($"INCOMPATIBLE_VERSION:Dein Savefile wurde mit Version {saveVersion} erstellt.\n\n" +
                                      $"Version {GAME_VERSION} verwendet ein neues Roll-System und ist nicht kompatibel mit alten Saves.\n\n" +
                                      $"Dein alter Save wurde gelöscht.\n" +
                                      $"Bitte starte das Spiel neu.");
                }

                // Parse DECIMAL to BigInteger
                var stats = new PlayerStats
                {
                    Money = BigInteger.Parse(reader.GetDecimal(2).ToString()),
                    TotalMoneyEarned = BigInteger.Parse(reader.GetDecimal(3).ToString()),
                    Gems = (int)reader.GetDecimal(4),
                    Rebirths = (int)reader.GetDecimal(5),
                    NextRebirthTarget = reader.IsDBNull(6) ? 0 : (int)reader.GetDecimal(6),
                    TotalRebirthsAllTime = (int)reader.GetDecimal(7),
                    PlotSlots = reader.GetInt32(8),
                    RollCooldown = (double)reader.GetDecimal(9),
                    RollCooldownLevel = reader.GetInt32(10),
                    TotalRollsAllTime = (int)reader.GetInt64(11),
                    AutoRollUnlocked = reader.GetBoolean(12),
                    AutoRollActive = reader.GetBoolean(13),
                    PlayTimeMinutes = (double)reader.GetDecimal(14),
                    LuckBoosterLevel = reader.GetInt32(15),
                    BestRayanEverName = reader.IsDBNull(16) ? "" : reader.GetString(16),
                    BestRayanEverRarity = reader.IsDBNull(17) ? 0.0 : double.Parse(reader.GetString(17)),
                    RarityQuestLevel = reader.GetInt32(23),
                    BestRayanEverValue = reader.IsDBNull(25) ? BigInteger.Zero : BigInteger.Parse(reader.GetString(25)),
                    SelectedDiceIndex = reader.GetInt32(26),
                    TotalPlayTimeMinutes = (double)reader.GetDecimal(27)
                };

                // Deserialize JSON arrays
                string inventoryJson = reader.IsDBNull(18) ? "[]" : reader.GetString(18);
                stats.Inventory = JsonSerializer.Deserialize<List<Rayan>>(inventoryJson) ?? new List<Rayan>();

                string equippedJson = reader.IsDBNull(19) ? "[]" : reader.GetString(19);
                stats.EquippedRayanIndices = JsonSerializer.Deserialize<List<int>>(equippedJson) ?? new List<int>();

                string diceJson = reader.IsDBNull(20) ? "[]" : reader.GetString(20);
                stats.OwnedDices = JsonSerializer.Deserialize<List<Dice>>(diceJson) ?? new List<Dice>();

                string questsJson = reader.IsDBNull(21) ? "[]" : reader.GetString(21);
                stats.SavedQuests = JsonSerializer.Deserialize<List<Quest>>(questsJson) ?? new List<Quest>();

                // Admin flag
                bool adminUsed = reader.GetBoolean(22);
                if (adminUsed && !_adminUsedThisSession)
                {
                    _adminUsedThisSession = true;
                    Console.WriteLine($"[DatabaseService] Admin flag loaded");
                }

                Console.WriteLine($"[DatabaseService] Loaded savefile {savefileId}");
                Console.WriteLine($"[DatabaseService]   Version: {saveVersion}");
                
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Load error: {ex.Message}");
                
                if (ex.Message.StartsWith("INCOMPATIBLE_VERSION:"))
                    throw;
                
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
                await using var conn = await GetConnectionAsync();
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, last_played, rebirths, money, gems, admin_used
                    FROM savefile
                    WHERE user_id = @userId
                    ORDER BY last_played DESC
                ";
                cmd.Parameters.AddWithValue("@userId", long.Parse(userId));

                await using var reader = await cmd.ExecuteReaderAsync();
                
                var savefiles = new List<SavefileInfo>();
                
                while (await reader.ReadAsync())
                {
                    savefiles.Add(new SavefileInfo
                    {
                        Id = reader.GetInt64(0).ToString(),
                        LastPlayed = reader.IsDBNull(1) ? null : reader.GetDateTime(1).ToString("o"),
                        Rebirths = (int)reader.GetDecimal(2),
                        Money = reader.GetDecimal(3).ToString(),
                        Gems = (int)reader.GetDecimal(4),
                        AdminUsed = reader.GetBoolean(5)
                    });
                }

                Console.WriteLine($"[DatabaseService] Found {savefiles.Count} savefile(s)");
                return savefiles;
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
                await using var conn = await GetConnectionAsync();
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO savefile (
                        user_id, created_at, created_in_version,
                        money, total_money_earned, gems, total_gems,
                        rebirths, next_rebirth, total_rebirths,
                        plot_slots, roll_cooldown, roll_cooldown_level,
                        total_rolls, autoroll_unlocked, autoroll_active,
                        playtime_minutes, luckbooster_level,
                        best_rayan_ever_name, best_rayan_ever_rarity,
                        inventory, equipped_rayan_indices, owned_dice, saved_quests,
                        admin_used, rarity_quest_level, best_rayan_value,
                        selected_dice_index, total_playtime_minutes
                    ) VALUES (
                        @user_id, @created_at, @version,
                        @money, @total_money, @gems, @total_gems,
                        @rebirths, @next_rebirth, @total_rebirths,
                        @plot_slots, @roll_cooldown, @roll_cooldown_level,
                        @total_rolls, @autoroll_unlocked, @autoroll_active,
                        @playtime, @luckbooster,
                        @best_name, @best_rarity,
                        @inventory, @equipped, @dice, @quests,
                        @admin_used, @rarity_level, @best_value,
                        @selected_dice, @total_playtime
                    );
                    SELECT LAST_INSERT_ID();
                ";

                cmd.Parameters.AddWithValue("@user_id", long.Parse(userId));
                cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@version", GAME_VERSION);
                cmd.Parameters.AddWithValue("@money", stats.Money.ToString());
                cmd.Parameters.AddWithValue("@total_money", stats.TotalMoneyEarned.ToString());
                cmd.Parameters.AddWithValue("@gems", stats.Gems.ToString());
                cmd.Parameters.AddWithValue("@total_gems", stats.Gems.ToString());
                cmd.Parameters.AddWithValue("@rebirths", stats.Rebirths.ToString());
                cmd.Parameters.AddWithValue("@next_rebirth", stats.NextRebirthTarget.ToString());
                cmd.Parameters.AddWithValue("@total_rebirths", stats.TotalRebirthsAllTime.ToString());
                cmd.Parameters.AddWithValue("@plot_slots", stats.PlotSlots);
                cmd.Parameters.AddWithValue("@roll_cooldown", stats.RollCooldown);
                cmd.Parameters.AddWithValue("@roll_cooldown_level", stats.RollCooldownLevel);
                cmd.Parameters.AddWithValue("@total_rolls", stats.TotalRollsAllTime);
                cmd.Parameters.AddWithValue("@autoroll_unlocked", stats.AutoRollUnlocked);
                cmd.Parameters.AddWithValue("@autoroll_active", stats.AutoRollActive);
                cmd.Parameters.AddWithValue("@playtime", stats.PlayTimeMinutes);
                cmd.Parameters.AddWithValue("@luckbooster", stats.LuckBoosterLevel);
                cmd.Parameters.AddWithValue("@best_name", stats.BestRayanEverName ?? "");
                cmd.Parameters.AddWithValue("@best_rarity", stats.BestRayanEverRarity.ToString());
                cmd.Parameters.AddWithValue("@inventory", JsonSerializer.Serialize(stats.Inventory));
                cmd.Parameters.AddWithValue("@equipped", JsonSerializer.Serialize(stats.EquippedRayanIndices));
                cmd.Parameters.AddWithValue("@dice", JsonSerializer.Serialize(stats.OwnedDices));
                cmd.Parameters.AddWithValue("@quests", JsonSerializer.Serialize(stats.SavedQuests));
                cmd.Parameters.AddWithValue("@admin_used", _adminUsedThisSession);
                cmd.Parameters.AddWithValue("@rarity_level", stats.RarityQuestLevel);
                cmd.Parameters.AddWithValue("@best_value", stats.BestRayanEverValue.ToString());
                cmd.Parameters.AddWithValue("@selected_dice", stats.SelectedDiceIndex);
                cmd.Parameters.AddWithValue("@total_playtime", stats.TotalPlayTimeMinutes);

                var savefileIdObj = await cmd.ExecuteScalarAsync();
                
                if (savefileIdObj != null)
                {
                    long savefileId = Convert.ToInt64(savefileIdObj);
                    Console.WriteLine($"[DatabaseService] Created savefile: {savefileId}");
                    return savefileId.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Create error: {ex.Message}");
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
                await using var conn = await GetConnectionAsync();
                
                // Try to find existing user
                var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT id FROM users WHERE username = @username";
                checkCmd.Parameters.AddWithValue("@username", username);

                var result = await checkCmd.ExecuteScalarAsync();
                
                if (result != null)
                {
                    var userId = Convert.ToInt64(result).ToString();
                    Console.WriteLine($"[DatabaseService] Found existing user: {userId}");
                    return (true, userId);
                }

                // User doesn't exist, create new one with default password
                Console.WriteLine($"[DatabaseService] User {username} not found, creating with default password...");

                string encryptedPassword = EncryptPassword("default");
                
                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO users (username, password, created_at) 
                    VALUES (@username, @password, @created_at);
                    SELECT LAST_INSERT_ID();
                ";
                insertCmd.Parameters.AddWithValue("@username", username);
                insertCmd.Parameters.AddWithValue("@password", encryptedPassword);
                insertCmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

                var userIdObj = await insertCmd.ExecuteScalarAsync();
                
                if (userIdObj != null)
                {
                    long userId = Convert.ToInt64(userIdObj);
                    Console.WriteLine($"[DatabaseService] Created new user: {userId}");
                    return (true, userId.ToString());
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] GetOrCreateUser error: {ex.Message}");
                return (false, null);
            }
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
                
                await using var conn = await GetConnectionAsync();
                
                // Build query based on category
                string orderByField = category switch
                {
                    0 => "total_money_earned",
                    1 => "best_rayan_ever_rarity",
                    2 => "total_rolls",
                    3 => "gems",
                    4 => "total_playtime_minutes",
                    5 => "inventory",
                    _ => "total_money_earned"
                };
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT 
                        s.id, s.user_id, s.{orderByField}, 
                        s.best_rayan_ever_name, s.best_rayan_ever_rarity, 
                        s.inventory, s.created_in_version, s.total_money_earned,
                        s.total_rolls, s.gems, s.total_playtime_minutes,
                        u.username
                    FROM savefile s
                    JOIN users u ON s.user_id = u.id
                    WHERE s.admin_used = 0
                    ORDER BY s.{orderByField} DESC
                    LIMIT 50
                ";

                await using var reader = await cmd.ExecuteReaderAsync();
                
                var entries = new List<Forms.Dialogs.LeaderboardEntry>();
                
                while (await reader.ReadAsync())
                {
                    // VERSION FILTER
                    string saveVersion = reader.IsDBNull(6) ? "0.0.0" : reader.GetString(6);
                    if (!IsVersionCompatible(saveVersion, MIN_REQUIRED_VERSION))
                    {
                        continue;
                    }
                    
                    var entry = new Forms.Dialogs.LeaderboardEntry
                    {
                        Username = reader.GetString(11)
                    };
                    
                    // Format value based on category
                    switch (category)
                    {
                        case 0: // Money
                            var money = BigInteger.Parse(reader.GetDecimal(7).ToString());
                            entry.ValueFormatted = $"💰 {FormatBigNumber(money)} Money verdient";
                            entry.RawValue = (long)Math.Min((double)money, long.MaxValue);
                            break;
                            
                        case 1: // Rarest Rayan
                            if (!reader.IsDBNull(4) && !reader.IsDBNull(3))
                            {
                                var rarity = double.Parse(reader.GetString(4));
                                var name = reader.GetString(3);
                                entry.ValueFormatted = $"✨ {name} (1 in {rarity:N0})";
                                entry.RawValue = (long)rarity;
                            }
                            break;
                            
                        case 2: // Rolls
                            var rolls = reader.GetInt64(8);
                            entry.ValueFormatted = $"🎲 {rolls:N0} Rolls";
                            entry.RawValue = rolls;
                            break;
                            
                        case 3: // Gems
                            var gems = (int)reader.GetDecimal(9);
                            entry.ValueFormatted = $"💎 {gems:N0} Gems";
                            entry.RawValue = gems;
                            break;
                            
                        case 4: // PlayTime
                            var minutes = (double)reader.GetDecimal(10);
                            entry.ValueFormatted = $"⏰ {FormatPlayTime(minutes)}";
                            entry.RawValue = (long)minutes;
                            break;
                            
                        case 5: // Rayans
                            try
                            {
                                var inventoryJson = reader.IsDBNull(5) ? "[]" : reader.GetString(5);
                                var inventory = JsonSerializer.Deserialize<List<JsonElement>>(inventoryJson);
                                var count = inventory?.Count ?? 0;
                                entry.ValueFormatted = $"👤 {count:N0} Rayans";
                                entry.RawValue = count;
                            }
                            catch
                            {
                                entry.ValueFormatted = "👤 0 Rayans";
                                entry.RawValue = 0;
                            }
                            break;
                    }
                    
                    entries.Add(entry);
                }
                
                return entries.OrderByDescending(e => e.RawValue).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error fetching leaderboard: {ex.Message}");
                return new List<Forms.Dialogs.LeaderboardEntry>();
            }
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
        /// Save user feedback to users table (appends to feedback_send with "?\?" separator)
        /// </summary>
        public async Task<bool> SaveFeedbackAsync(string username, string feedback)
        {
            try
            {
                await using var conn = await GetConnectionAsync();
                
                // Get existing feedback
                var selectCmd = conn.CreateCommand();
                selectCmd.CommandText = "SELECT feedback_send FROM users WHERE username = @username";
                selectCmd.Parameters.AddWithValue("@username", username);

                string existingFeedback = "";
                var result = await selectCmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    existingFeedback = result.ToString() ?? "";
                }

                // Append new feedback
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string newEntry = $"[{timestamp}] {feedback}";
                
                string updatedFeedback = string.IsNullOrEmpty(existingFeedback)
                    ? newEntry
                    : existingFeedback + @"?\?" + newEntry;

                // Update
                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = "UPDATE users SET feedback_send = @feedback WHERE username = @username";
                updateCmd.Parameters.AddWithValue("@feedback", updatedFeedback);
                updateCmd.Parameters.AddWithValue("@username", username);

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[DatabaseService] Feedback saved");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Feedback error: {ex.Message}");
                return false;
            }
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
