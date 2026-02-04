# ?? MySQL Migration Guide - DatabaseService.cs

## ?? Übersicht

**Von:** Supabase PostgreSQL (REST API via HttpClient)  
**Nach:** MySQL (Direct Connection via MySqlConnector)

**Status:** ?? In Planung  
**Priorität:** ? Hoch  
**Geschätzter Aufwand:** 4-6 Stunden

---

## ?? Ziele

1. ? Ersetze REST API Calls durch direkte SQL Queries
2. ? Nutze DECIMAL(65,0) für BigInteger-Werte
3. ? Behalte XOR Password-Encryption bei
4. ? Unterstütze JSON-Felder für Arrays
5. ? Implementiere Connection Pooling
6. ? Behalte alle Features (Auth, Savefiles, Leaderboard, Feedback)

---

## ?? Schema-Mapping

### Alte vs. Neue Struktur

| Alt (PostgreSQL) | Neu (MySQL) | Typ-Änderung |
|------------------|-------------|--------------|
| `User` Tabelle | `users` Tabelle | Tabellenname lowercase |
| `Savefiles` Tabelle | `savefile` Tabelle | Singular + lowercase |
| `id` SERIAL | `id` BIGINT AUTO_INCREMENT | Typ-Änderung |
| JSONB | JSON | Typ-Änderung |
| TEXT für BigInt | DECIMAL(65,0) | **Wichtig!** |
| `total_rolls_altime` | `total_rolls` | Spaltenname |
| `roll_colldown` | `roll_cooldown` | Typo fix |
| `best_rayan_value` | **FEHLT** | Muss hinzugefügt werden! |

### ?? Kritische Unterschiede

```sql
-- PostgreSQL (alt)
money TEXT
gems INTEGER
inventory JSONB

-- MySQL (neu)
money DECIMAL(65,0)
gems DECIMAL(65,0)  -- Auch Gems sind jetzt DECIMAL!
inventory JSON
```

---

## ?? Implementierung

### **1. Connection-Setup ersetzen**

#### ? Alt (HttpClient):
```csharp
private readonly HttpClient _httpClient;

public DatabaseService(string username)
{
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.Add("apikey", SUPABASE_KEY);
    // ...
}
```

#### ? Neu (MySqlConnection):
```csharp
private const string SERVER_IP = "Server=10.0.2.15;Database=game_db;Uid=root;Pwd=ServerRoot123;";

// Helper-Methode für Connection
private async Task<MySqlConnection> GetConnectionAsync()
{
    var conn = new MySqlConnection(SERVER_IP);
    await conn.OpenAsync();
    return conn;
}
```

---

### **2. IsOnlineAsync() anpassen**

#### ? Alt:
```csharp
public async Task<bool> IsOnlineAsync()
{
    try
    {
        var response = await _httpClient.GetAsync($"{SUPABASE_URL}/User?limit=1");
        return response.IsSuccessStatusCode;
    }
    catch { return false; }
}
```

#### ? Neu:
```csharp
public async Task<bool> IsOnlineAsync()
{
    try
    {
        await using var conn = await GetConnectionAsync();
        return conn.State == System.Data.ConnectionState.Open;
    }
    catch { return false; }
}
```

---

### **3. AuthenticateAsync() - MySQL Version**

#### ? Neu:
```csharp
public async Task<(bool success, string? userId)> AuthenticateAsync(string username, string password)
{
    _username = username;

    try
    {
        await using var conn = await GetConnectionAsync();
        
        // SELECT user mit flags
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
        return (false, null);
    }
}
```

---

### **4. RegisterUserAsync() - MySQL Version**

```csharp
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
```

---

### **5. SavePlayerDataAsync() - MySQL Version**

```csharp
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
                inventory = @inventory,
                equipped_rayan_indices = @equipped_rayan_indices,
                owned_dice = @owned_dice,
                saved_quests = @saved_quests,
                admin_used = @admin_used,
                rarity_quest_level = @rarity_quest_level
            WHERE id = @savefileId
        ";

        // WICHTIG: DECIMAL(65,0) für BigInteger!
        updateCmd.Parameters.AddWithValue("@last_played", DateTime.UtcNow);
        updateCmd.Parameters.AddWithValue("@money", stats.Money.ToString()); // String ? DECIMAL
        updateCmd.Parameters.AddWithValue("@total_money_earned", stats.TotalMoneyEarned.ToString());
        updateCmd.Parameters.AddWithValue("@gems", stats.Gems.ToString()); // DECIMAL jetzt!
        updateCmd.Parameters.AddWithValue("@total_gems", stats.Gems.ToString()); // Für Tracking
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
        
        // JSON Arrays als String
        updateCmd.Parameters.AddWithValue("@inventory", JsonSerializer.Serialize(stats.Inventory));
        updateCmd.Parameters.AddWithValue("@equipped_rayan_indices", JsonSerializer.Serialize(stats.EquippedRayanIndices));
        updateCmd.Parameters.AddWithValue("@owned_dice", JsonSerializer.Serialize(stats.OwnedDices));
        updateCmd.Parameters.AddWithValue("@saved_quests", JsonSerializer.Serialize(stats.SavedQuests));
        
        updateCmd.Parameters.AddWithValue("@admin_used", _adminUsedThisSession);
        updateCmd.Parameters.AddWithValue("@rarity_quest_level", stats.RarityQuestLevel);
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
        Console.WriteLine($"[DatabaseService] Save error: {ex.Message}");
        return false;
    }
}
```

---

### **6. LoadSavefileAsync() - MySQL Version**

```csharp
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
                admin_used, rarity_quest_level, last_played
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
            
            throw new Exception($"INCOMPATIBLE_VERSION:Dein Savefile wurde mit Version {saveVersion} erstellt...");
        }

        // Parse DECIMAL to BigInteger
        var stats = new PlayerStats
        {
            Money = BigInteger.Parse(reader.GetDecimal(2).ToString()),
            TotalMoneyEarned = BigInteger.Parse(reader.GetDecimal(3).ToString()),
            Gems = (int)reader.GetDecimal(4), // DECIMAL ? int
            Rebirths = (int)reader.GetDecimal(5),
            NextRebirthTarget = (int)reader.GetDecimal(6),
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
            BestRayanEverRarity = double.Parse(reader.GetString(17)),
            RarityQuestLevel = reader.GetInt32(23)
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
```

---

### **7. GetUserSavefilesAsync() - MySQL Version**

```csharp
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
```

---

### **8. CreateSavefileAsync() - MySQL Version**

```csharp
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
                admin_used, rarity_quest_level
            ) VALUES (
                @user_id, @created_at, @version,
                @money, @total_money, @gems, @total_gems,
                @rebirths, @next_rebirth, @total_rebirths,
                @plot_slots, @roll_cooldown, @roll_cooldown_level,
                @total_rolls, @autoroll_unlocked, @autoroll_active,
                @playtime, @luckbooster,
                @best_name, @best_rarity,
                @inventory, @equipped, @dice, @quests,
                @admin_used, @rarity_level
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
```

---

### **9. DeleteSavefileAsync() - MySQL Version**

```csharp
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
```

---

### **10. SaveFeedbackAsync() - MySQL Version**

```csharp
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
```

---

## ??? Was kann gelöscht werden

```csharp
// Diese können entfernt werden:
private readonly HttpClient _httpClient;
private const string SUPABASE_URL = "...";
private const string SUPABASE_KEY = "...";

// Diese Klassen nicht mehr benötigt (JSON-Deserialisierung direkt):
// Eventuell DbUserData und DbSavefileData vereinfachen
```

---

## ?? Kritische Punkte

### **1. DECIMAL(65,0) Handling**

```csharp
// WICHTIG: BigInteger ? String ? DECIMAL
BigInteger money = 123456789;
cmd.Parameters.AddWithValue("@money", money.ToString());

// Und zurück:
decimal moneyDecimal = reader.GetDecimal(0);
BigInteger money = BigInteger.Parse(moneyDecimal.ToString());
```

### **2. JSON Arrays**

```csharp
// Speichern:
string inventoryJson = JsonSerializer.Serialize(stats.Inventory);
cmd.Parameters.AddWithValue("@inventory", inventoryJson);

// Laden:
string inventoryJson = reader.GetString(0);
var inventory = JsonSerializer.Deserialize<List<Rayan>>(inventoryJson);
```

### **3. Connection Management**

```csharp
// IMMER using/await using verwenden!
await using var conn = await GetConnectionAsync();

// Nicht vergessen: Reader closen vor nächstem Command!
await reader.CloseAsync();
```

### **4. LAST_INSERT_ID()**

```csharp
// MySQL gibt neue ID zurück:
var id = await cmd.ExecuteScalarAsync();
long newId = Convert.ToInt64(id);
```

---

## ?? Fehlende Spalte hinzufügen

```sql
-- best_rayan_value fehlt in savefile Tabelle!
ALTER TABLE savefile 
ADD COLUMN best_rayan_value VARCHAR(100) AFTER best_rayan_ever_rarity;
```

---

## ? Testing-Checkliste

- [ ] 1. Verbindung zur DB herstellen (`IsOnlineAsync()`)
- [ ] 2. Neuen User registrieren (`RegisterUserAsync()`)
- [ ] 3. Login mit User (`AuthenticateAsync()`)
- [ ] 4. Savefile erstellen (`CreateSavefileAsync()`)
- [ ] 5. Savefile laden (`LoadSavefileAsync()`)
- [ ] 6. Savefile speichern (`SavePlayerDataAsync()`)
- [ ] 7. Mehrere Savefiles laden (`GetUserSavefilesAsync()`)
- [ ] 8. Savefile löschen (`DeleteSavefileAsync()`)
- [ ] 9. Feedback speichern (`SaveFeedbackAsync()`)
- [ ] 10. Kick/Ban-Flags testen
- [ ] 11. Version-Check testen (alte Saves löschen)
- [ ] 12. BigInteger-Werte testen (sehr große Zahlen!)
- [ ] 13. JSON-Arrays testen (Inventory, Dice, etc.)
- [ ] 14. Connection-Fehler simulieren

---

## ?? Häufige Fehler

### **1. "Data too long for column"**
```
Ursache: String zu lang für VARCHAR
Lösung: TEXT verwenden oder VARCHAR erhöhen
```

### **2. "Out of range value for column"**
```
Ursache: DECIMAL(65,0) kann nur positive Zahlen
Lösung: Negative Werte vermeiden
```

### **3. "JSON text is not well formed"**
```
Ursache: Invalides JSON
Lösung: JsonSerializer.Serialize() verwenden
```

### **4. "Connection timeout"**
```
Ursache: DB nicht erreichbar
Lösung: SERVER_IP prüfen, Firewall checken
```

---

## ?? Nützliche Links

- [MySqlConnector Docs](https://mysqlconnector.net/)
- [MySQL JSON Functions](https://dev.mysql.com/doc/refman/8.0/en/json.html)
- [MySQL DECIMAL](https://dev.mysql.com/doc/refman/8.0/en/fixed-point-types.html)

---

## ?? Nächste Schritte

1. ? Backup der aktuellen DatabaseService.cs erstellen
2. ? Neue Connection-Methoden implementieren
3. ? Auth-Methoden umschreiben
4. ? Save/Load-Methoden umschreiben
5. ? Testing mit lokaler DB
6. ? Leaderboard implementieren (optional)
7. ? Deployment auf Server

---

**Geschätzte Zeit:** 4-6 Stunden  
**Schwierigkeit:** ??? Mittel

Viel Erfolg! ??
