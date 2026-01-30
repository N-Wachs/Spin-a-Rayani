/**
 * Spin a Rayan - Web Version
 * Main Application Entry Point
 */

// Global game manager and UI manager
let gameManager;
let uiManager;

// Initialize the application
document.addEventListener('DOMContentLoaded', () => {
    console.log('🎲 Spin a Rayan - Web Version');
    console.log('Initializing...');
    
    // Create game manager
    gameManager = new GameManager();
    
    // Create UI manager
    uiManager = new UIManager(gameManager);
    
    // Set up game callbacks
    gameManager.onStatsChanged = () => {
        uiManager.updateStats();
        uiManager.updatePlots();
        uiManager.updateInventoryPreview();
        uiManager.updateEvents();
    };
    
    gameManager.onRayanRolled = (rayan) => {
        // Already handled in UIManager.handleRoll()
    };
    
    gameManager.onEventsChanged = (events) => {
        uiManager.updateEvents();
    };
    
    // Initial UI update
    uiManager.updateAll();
    
    // Start event timer display update
    setInterval(() => {
        uiManager.updateEvents();
    }, 1000);
    
    console.log('✅ Game initialized!');
    console.log(`📦 ${PREFIXES.length} Rayans loaded`);
    console.log(`🎲 ${DICE_TEMPLATES.length} Dice types available`);
    console.log(`📋 ${QUEST_DEFINITIONS.length} Quests available`);
    
    // Show welcome toast
    uiManager.showToast('Willkommen bei Spin a Rayan! 🎲', 'info');
});

// Save before leaving
window.addEventListener('beforeunload', () => {
    if (gameManager) {
        gameManager.save();
    }
});

// Debug helpers (available in console)
window.debug = {
    addMoney: (amount) => {
        gameManager.stats.money += BigInt(amount);
        uiManager.updateAll();
    },
    addGems: (amount) => {
        gameManager.stats.gems += amount;
        uiManager.updateAll();
    },
    forceEvent: (suffix) => {
        gameManager.forceEvent(suffix);
    },
    resetGame: () => {
        if (confirm('Wirklich alles zurücksetzen?')) {
            gameManager.resetGame();
            uiManager.updateAll();
        }
    },
    save: () => {
        gameManager.save();
        uiManager.showToast('Spiel gespeichert!', 'success');
    }
};
