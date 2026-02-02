/**  * Spin a Rayan - Web Version  * Main Application Entry Point with Database Auth  */  // Global instances let gameManager; let uiManager; let databaseService; let authUI; let savefileUI; let leaderboardUI;  // Initialize the application document.addEventListener('DOMContentLoaded', async () => { console.log('🎲 Spin a Rayan - Web Version (Database-Only Mode)'); console.log('Initializing...');      // DEV MODE: Enable admin command shortcut // Type 'ad' in console to toggle admin mode if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1' || window.location.port !== '') {     console.log('🔧 DEV MODE: Type "ad" in console to toggle Admin Mode');     window.ad = () => {         if (gameManager) {             gameManager.toggleAdminMode();             if (uiManager) uiManager.updateAll();             console.log(`🛡️ Admin Mode: ${gameManager.adminMode ? 'ENABLED' : 'DISABLED'}`);         } else {             console.warn('⚠️ Game not initialized yet');         }     }; }      // Show loading screen showLoadingScreen();          // Step 1: Create auth UI     databaseService = new DatabaseService('temp');     authUI = new AuthUI(databaseService);          // Step 2: Try auto-login     const autoLoginSuccess = await authUI.tryAutoLogin();          if (autoLoginSuccess) {         // Auto-login successful, proceed to savefile selection         await initializeSavefileSelection();     } else {         // Show login modal         hideLoadingScreen();         authUI.show();     }          // Set up auth success callback     authUI.onSuccess = async (userId, username) => {         authUI.hide();         showLoadingScreen();         await initializeSavefileSelection();     };          console.log(`📦 ${PREFIXES.length} Rayans loaded`);     console.log(`🎲 ${DICE_TEMPLATES.length} Dice types available`);     console.log(`📋 ${QUEST_DEFINITIONS.length} Quests available`); });  async function initializeSavefileSelection() {     // Create savefile UI     savefileUI = new SavefileUI(databaseService);          // Load savefiles     await savefileUI.load();          // Show savefile selection     hideLoadingScreen();     savefileUI.show();          // Set up savefile selection callback     savefileUI.onSelect = (stats, savefileId) => {         savefileUI.hide();         showLoadingScreen();         initializeGame(stats, savefileId);     }; }  function initializeGame(stats, savefileId) {     console.log('✅ Starting game with savefile:', savefileId);          // Create game manager (database-only mode)     gameManager = new GameManager(stats, databaseService);          // Create leaderboard UI     leaderboardUI = new LeaderboardUI(databaseService);          // Create UI manager     uiManager = new UIManager(gameManager);          // Set up game callbacks     gameManager.onStatsChanged = () => {         uiManager.updateStats();         uiManager.updatePlots();         uiManager.updateInventoryPreview();         uiManager.updateEvents();     };          gameManager.onRayanRolled = (rayan) => {         // Already handled in UIManager.handleRoll()     };          gameManager.onEventsChanged = (events) => {         uiManager.updateEvents();     };          // Add leaderboard button handler     document.querySelector('[data-panel="leaderboard"]').addEventListener('click', () => {         leaderboardUI.show();     });          // Initial UI update     uiManager.updateAll();          // Start event timer display update     setInterval(() => {         uiManager.updateEvents();     }, 1000);          // Hide loading screen     hideLoadingScreen();          // Show welcome toast     uiManager.showToast('Willkommen zurück! 🎲', 'info');          console.log('✅ Game initialized!'); }  function showLoadingScreen() {     let loading = document.getElementById('loadingScreen');     if (!loading) {         loading = document.createElement('div');         loading.id = 'loadingScreen';         loading.className = 'loading-screen';         loading.innerHTML = `             <div class="loading-content">                 <div class="spinner"></div>                 <h2>🎲 Spin a Rayan</h2>                 <p>Lade...</p>             </div>         `;         document.body.appendChild(loading);     }     loading.style.display = 'flex'; }  function hideLoadingScreen() {     const loading = document.getElementById('loadingScreen');     if (loading) {         loading.style.display = 'none';     } }  // Save before leaving window.addEventListener('beforeunload', () => {     if (gameManager && databaseService) {         // Synchronous save (will wait up to timeout)         gameManager.saveSync();     } });  // Debug helpers (available in console)
window.debug = {
    addMoney: (amount) => {
        if (gameManager) {
            gameManager.stats.money += BigInt(amount);
            uiManager.updateAll();
        }
    },
    addGems: (amount) => {
        if (gameManager) {
            gameManager.stats.gems += amount;
            uiManager.updateAll();
        }
    },
    toggleAdmin: () => {
        if (gameManager) {
            gameManager.toggleAdminMode();
            uiManager.updateAll();
            console.log(`🛡️ Admin Mode: ${gameManager.adminMode ? 'ENABLED' : 'DISABLED'}`);
        }
    },
    forceEvent: (suffix) => {
        if (gameManager) {
            gameManager.forceEvent(suffix);
        }
    },
    save: async () => {
        if (gameManager && databaseService) {
            const success = await databaseService.saveSavefile(gameManager.stats);
            uiManager.showToast(success ? 'Gespeichert!' : 'Fehler!', success ? 'success' : 'error');
        }
    },
    logout: () => {
        localStorage.removeItem('sar_remember');
        location.reload();
    }
};

// DEV MODE: Show available debug commands
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1' || window.location.port !== '') {
    console.log('🔧 Debug commands available:');
    console.log('  • ad - Toggle Admin Mode (shortcut)');
    console.log('  • debug.toggleAdmin() - Toggle Admin Mode');
    console.log('  • debug.addMoney(amount) - Add money');
    console.log('  • debug.addGems(amount) - Add gems');
    console.log('  • debug.forceEvent(suffix) - Force event');
    console.log('  • debug.save() - Force save');
    console.log('  • debug.logout() - Logout');
} 