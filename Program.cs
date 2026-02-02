using SpinARayan.Forms.Dialogs;
using SpinARayan.Services;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpinARayan
{
    internal static class Program
    {
        // Import Windows API to allocate console
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {
            // Only show console in DEBUG mode
#if DEBUG
            AllocConsole();
            Console.WriteLine("[Program] Debug mode - Console enabled");
            Console.WriteLine("[Program] Commands:");
            Console.WriteLine("[Program]   ad                - Toggle Admin Mode");
            Console.WriteLine("[Program]   cash -add XXX     - Add money");
            Console.WriteLine("[Program]   gems -add XXX     - Add gems");
            Console.WriteLine("[Program]   roll XXX          - Roll X times");
            Console.WriteLine("[Program]   roll -speed XXX   - Set roll cooldown");
#endif
            
            ApplicationConfiguration.Initialize();
            
            // Login loop - retry until successful or cancelled
            string? username = null;
            string? password = null;
            DatabaseService? dbService = null;
            string? userId = null;
            
            // Step 1: Try saved login first
            var savedLogin = LoginService.LoadSavedLogin();
            if (savedLogin != null)
            {
                Console.WriteLine($"[Program] Attempting auto-login for user: {savedLogin.Username}");
                username = savedLogin.Username;
                password = savedLogin.Password;
                
                // Try to authenticate with saved credentials
                dbService = new DatabaseService(username);
                var (authSuccess, authUserId) = dbService.AuthenticateAsync(username, password).Result;
                
                if (authSuccess && !string.IsNullOrEmpty(authUserId))
                {
                    userId = authUserId;
                    Console.WriteLine($"[Program] Auto-login successful");
                }
                else
                {
                    Console.WriteLine($"[Program] Auto-login failed, clearing saved credentials");
                    LoginService.ClearSavedLogin();
                    dbService = null;
                    username = null;
                    password = null;
                }
            }
            
            // Step 2: Show login form if no saved login or auto-login failed
            while (string.IsNullOrEmpty(userId))
            {
                using (var loginForm = new LoginForm())
                {
                    var result = loginForm.ShowDialog();
                    
                    if (result == DialogResult.Cancel)
                    {
                        // User cancelled login
                        Console.WriteLine($"[Program] Login cancelled by user");
                        return;
                    }
                    
                    if (result == DialogResult.OK)
                    {
                        username = loginForm.Username;
                        password = loginForm.Password;
                        
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            MessageBox.Show("Username und Passwort duerfen nicht leer sein!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        
                        // Try to authenticate
                        dbService = new DatabaseService(username);
                        var (authSuccess, authUserId) = dbService.AuthenticateAsync(username, password).Result;
                        
                        if (authSuccess && !string.IsNullOrEmpty(authUserId))
                        {
                            userId = authUserId;
                            Console.WriteLine($"[Program] Login successful for user: {username}");
                            
                            // Save login if checkbox checked
                            if (loginForm.RememberMe)
                            {
                                LoginService.SaveLogin(username, password);
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                "Login fehlgeschlagen!\n\nUngültiger Username oder Passwort.\nBitte versuche es erneut.",
                                "Login Fehler",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            dbService = null;
                        }
                    }
                    else if (result == DialogResult.Retry) // Register action
                    {
                        username = loginForm.Username;
                        password = loginForm.Password;
                        
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            MessageBox.Show("Username und Passwort duerfen nicht leer sein!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        
                        // Register new user (checks if username exists)
                        dbService = new DatabaseService(username);
                        var (regSuccess, regUserId, errorMessage) = dbService.RegisterUserAsync(username, password).Result;
                        
                        if (regSuccess && !string.IsNullOrEmpty(regUserId))
                        {
                            userId = regUserId;
                            Console.WriteLine($"[Program] Registration successful for user: {username}");
                            
                            MessageBox.Show(
                                $"Account erfolgreich erstellt!\n\nWillkommen, {username}!",
                                "Registrierung erfolgreich",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                            
                            // Save login if checkbox checked
                            if (loginForm.RememberMe)
                            {
                                LoginService.SaveLogin(username, password);
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                $"Registrierung fehlgeschlagen!\n\n{errorMessage ?? "Bitte versuche es erneut."}",
                                "Registrierung Fehler",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            dbService = null;
                        }
                    }
                }
            }
            
            // At this point, we have valid username, password, userId and dbService
            if (dbService == null || string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("Login fehlgeschlagen!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Step 3: Get savefiles and let user choose
            var savefiles = dbService.GetAllSavefilesAsync().Result;
            string? selectedSavefileId = null;
            
            // IMPORTANT: Ensure userId is set in dbService before creating savefiles
            if (!string.IsNullOrEmpty(userId))
            {
                dbService.SetCurrentSavefile(null!, userId); // Set userId early
            }
            
            if (savefiles.Count == 0)
            {
                // No savefiles, create first one
                Console.WriteLine($"[Program] No savefiles found, creating first savefile...");
                Console.WriteLine($"[Program] User ID: {userId}");
                Console.WriteLine($"[Program] Username: {username}");
                
                try
                {
                    selectedSavefileId = dbService.CreateNewSavefileAsync().Result;
                    
                    if (string.IsNullOrEmpty(selectedSavefileId))
                    {
                        Console.WriteLine($"[Program] ERROR: CreateNewSavefileAsync returned null/empty!");
                        Console.WriteLine($"[Program] This usually means the DB request failed.");
                        Console.WriteLine($"[Program] Check the [DatabaseService] logs above for details.");
                        
                        MessageBox.Show(
                            "Fehler beim Erstellen des Savefiles!\n\n" +
                            "Details:\n" +
                            $"- User ID: {userId}\n" +
                            $"- Username: {username}\n\n" +
                            "Bitte schau in die Console für mehr Informationen.\n" +
                            "Häufige Ursachen:\n" +
                            "- DB-Tabelle 'Savefiles' fehlt eine Spalte\n" +
                            "- Netzwerk-Timeout\n" +
                            "- Falsche Datentypen\n\n" +
                            "Versuche dich erneut anzumelden.",
                            "Fehler",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        
                        // Clear saved login to force fresh login
                        LoginService.ClearSavedLogin();
                        return;
                    }
                    
                    Console.WriteLine($"[Program] Successfully created savefile: {selectedSavefileId}");
                }
                catch (AggregateException ae)
                {
                    Console.WriteLine($"[Program] EXCEPTION during savefile creation:");
                    foreach (var ex in ae.InnerExceptions)
                    {
                        Console.WriteLine($"[Program]   - {ex.GetType().Name}: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"[Program]     Inner: {ex.InnerException.Message}");
                        }
                    }
                    
                    MessageBox.Show(
                        "Fehler beim Erstellen des Savefiles!\n\n" +
                        $"Exception: {ae.InnerExceptions[0].GetType().Name}\n" +
                        $"Message: {ae.InnerExceptions[0].Message}\n\n" +
                        "Siehe Console für Details.",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    
                    LoginService.ClearSavedLogin();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Program] EXCEPTION during savefile creation:");
                    Console.WriteLine($"[Program]   Type: {ex.GetType().Name}");
                    Console.WriteLine($"[Program]   Message: {ex.Message}");
                    Console.WriteLine($"[Program]   Stack: {ex.StackTrace}");
                    
                    MessageBox.Show(
                        "Fehler beim Erstellen des Savefiles!\n\n" +
                        $"Exception: {ex.GetType().Name}\n" +
                        $"Message: {ex.Message}\n\n" +
                        "Siehe Console für Details.",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    
                    LoginService.ClearSavedLogin();
                    return;
                }
            }
            else if (savefiles.Count == 1)
            {
                // Only one savefile, use it automatically
                selectedSavefileId = savefiles[0].Id;
                Console.WriteLine($"[Program] Using only available savefile: {selectedSavefileId}");
            }
            else
            {
                // Multiple savefiles, show selection dialog
                using (var savefileForm = new SavefileSelectionForm(savefiles))
                {
                    if (savefileForm.ShowDialog() == DialogResult.OK)
                    {
                        if (savefileForm.CreateNewSavefile)
                        {
                            // Create new savefile
                            Console.WriteLine($"[Program] User wants to create new savefile");
                            Console.WriteLine($"[Program] User ID: {userId}");
                            
                            try
                            {
                                selectedSavefileId = dbService.CreateNewSavefileAsync().Result;
                                
                                if (string.IsNullOrEmpty(selectedSavefileId))
                                {
                                    Console.WriteLine($"[Program] ERROR: CreateNewSavefileAsync returned null/empty!");
                                    
                                    MessageBox.Show(
                                        "Fehler beim Erstellen des Savefiles!\n\n" +
                                        $"User ID: {userId}\n" +
                                        "Siehe Console für Details.",
                                        "Fehler",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                    return;
                                }
                                
                                Console.WriteLine($"[Program] Successfully created new savefile: {selectedSavefileId}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Program] EXCEPTION during new savefile creation:");
                                Console.WriteLine($"[Program]   Type: {ex.GetType().Name}");
                                Console.WriteLine($"[Program]   Message: {ex.Message}");
                                
                                MessageBox.Show(
                                    "Fehler beim Erstellen des Savefiles!\n\n" +
                                    $"Exception: {ex.GetType().Name}\n" +
                                    $"Message: {ex.Message}",
                                    "Fehler",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                return;
                            }
                        }
                        else
                        {
                            selectedSavefileId = savefileForm.SelectedSavefileId;
                        }
                    }
                    else
                    {
                        // User cancelled savefile selection
                        Console.WriteLine($"[Program] Savefile selection cancelled");
                        return;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(selectedSavefileId))
            {
                MessageBox.Show("No savefile selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Step 4: Set the selected savefile in database service
            dbService.SetCurrentSavefile(selectedSavefileId, userId);
            
            
            // Step 5: Start game with selected savefile
            Console.WriteLine($"[Program] Starting game with savefile: {selectedSavefileId}");
            
            try
            {
                var mainForm = new MainForm(username, dbService, selectedSavefileId);
                
#if DEBUG
                // Start console input listener for admin mode (DEBUG only)
                StartConsoleListener(mainForm);
#endif
                
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                // Check for version incompatibility
                if (ex.Message.StartsWith("INCOMPATIBLE_VERSION:"))
                {
                    string errorMessage = ex.Message.Substring(21); // Remove prefix
                    MessageBox.Show(
                        errorMessage,
                        "Inkompatible Savefile-Version",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    
                    // Restart the application
                    Console.WriteLine($"[Program] Restarting application after version incompatibility...");
                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                    return;
                }
                else
                {
                    // Other errors
                    MessageBox.Show(
                        $"Fehler beim Starten des Spiels:\n\n{ex.Message}",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    throw;
                }
            }
        }
        
#if DEBUG
        /// <summary>
        /// Background thread that listens for console input (DEBUG only)
        /// Commands:
        /// - "ad" = Toggle Admin Mode
        /// - "cash -add XXX" = Add money (triggers admin flag)
        /// - "gems -add XXX" = Add gems (triggers admin flag)
        /// </summary>
        private static void StartConsoleListener(MainForm mainForm)
        {
            var consoleThread = new System.Threading.Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var input = Console.ReadLine();
                        
                        if (string.IsNullOrWhiteSpace(input))
                            continue;
                        
                        var trimmedInput = input.Trim().ToLower();
                        
                        // Command: "ad" - Toggle Admin Mode
                        if (trimmedInput == "ad")
                        {
                            try
                            {
                                mainForm.Invoke(() =>
                                {
                                    mainForm.ToggleAdminModeFromConsole();
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Console] Failed to toggle admin mode: {ex.Message}");
                            }
                        }
                        // Command: "cash -add XXX" - Add money
                        else if (trimmedInput.StartsWith("cash -add "))
                        {
                            var amountStr = trimmedInput.Replace("cash -add ", "").Trim();
                            if (System.Numerics.BigInteger.TryParse(amountStr, out var amount))
                            {
                                try
                                {
                                    mainForm.Invoke(() =>
                                    {
                                        mainForm.AddMoneyFromConsole(amount);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Console] Failed to add money: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[Console] Invalid amount: {amountStr}");
                                Console.WriteLine($"[Console] Usage: cash -add <number>");
                            }
                        }
                        // Command: "gems -add XXX" - Add gems
                        else if (trimmedInput.StartsWith("gems -add "))
                        {
                            var amountStr = trimmedInput.Replace("gems -add ", "").Trim();
                            if (int.TryParse(amountStr, out var amount))
                            {
                                try
                                {
                                    mainForm.Invoke(() =>
                                    {
                                        mainForm.AddGemsFromConsole(amount);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Console] Failed to add gems: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[Console] Invalid amount: {amountStr}");
                                Console.WriteLine($"[Console] Usage: gems -add <number>");
                            }
                        }
                        // Command: "roll -speed XXX" - Set roll cooldown speed
                        else if (trimmedInput.StartsWith("roll -speed "))
                        {
                            var speedStr = trimmedInput.Replace("roll -speed ", "").Trim();
                            if (double.TryParse(speedStr, out var speed) && speed >= 0.1 && speed <= 60.0)
                            {
                                try
                                {
                                    mainForm.Invoke(() =>
                                    {
                                        mainForm.SetRollSpeedFromConsole(speed);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Console] Failed to set roll speed: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[Console] Invalid speed: {speedStr}");
                                Console.WriteLine($"[Console] Usage: roll -speed <seconds> (0.1-60.0)");
                            }
                        }
                        // Command: "roll XXX" - Roll X times without timer (for testing)
                        else if (trimmedInput.StartsWith("roll "))
                        {
                            var amountStr = trimmedInput.Replace("roll ", "").Trim();
                            if (int.TryParse(amountStr, out var amount) && amount > 0 && amount <= 10000)
                            {
                                try
                                {
                                    mainForm.Invoke(() =>
                                    {
                                        mainForm.RollMultipleFromConsole(amount);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Console] Failed to roll: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[Console] Invalid amount: {amountStr}");
                                Console.WriteLine($"[Console] Usage: roll <number> (1-10000)");
                            }
                        }
                        else if (!string.IsNullOrEmpty(trimmedInput))
                        {
                            Console.WriteLine($"[Console] Unknown command: {trimmedInput}");
                            Console.WriteLine($"[Console] Available commands:");
                            Console.WriteLine($"[Console]   ad              - Toggle Admin Mode");
                            Console.WriteLine($"[Console]   cash -add XXX   - Add money (triggers admin flag)");
                            Console.WriteLine($"[Console]   gems -add XXX   - Add gems (triggers admin flag)");
                            Console.WriteLine($"[Console]   roll XXX        - Roll X times without timer (triggers admin flag)");
                            Console.WriteLine($"[Console]   roll -speed XXX - Set roll cooldown to X.X seconds (triggers admin flag)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Console] Listener error: {ex.Message}");
                }
            })
            {
                IsBackground = true // Background thread dies when app closes
            };
            
            consoleThread.Start();
            Console.WriteLine("[Console] Admin command listener started");
            Console.WriteLine("[Console] Available commands:");
            Console.WriteLine("[Console]   ad                - Toggle Admin Mode");
            Console.WriteLine("[Console]   cash -add XXX     - Add money (triggers admin flag)");
            Console.WriteLine("[Console]   gems -add XXX     - Add gems (triggers admin flag)");
            Console.WriteLine("[Console]   roll XXX          - Roll X times without timer (triggers admin flag)");
            Console.WriteLine("[Console]   roll -speed XXX   - Set roll cooldown to X.X seconds (triggers admin flag)");
        }
#endif
        
    }
}
