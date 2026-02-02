/**
 * Authentication UI - Login/Register Modal
 */

class AuthUI {
    constructor(databaseService) {
        this.dbService = databaseService;
        this.rememberMe = false;
        this.createModal();
    }

    createModal() {
        // Create modal overlay
        const overlay = document.createElement('div');
        overlay.id = 'authModal';
        overlay.className = 'modal-overlay';
        overlay.innerHTML = `
            <div class="modal-content auth-modal">
                <h2 class="auth-title">?? Spin a Rayan</h2>
                <p class="auth-subtitle">Login oder Registrieren</p>
                
                <div class="auth-form">
                    <div class="input-group">
                        <label>Username</label>
                        <input type="text" id="authUsername" placeholder="Username eingeben" autocomplete="username">
                    </div>
                    
                    <div class="input-group">
                        <label>Passwort</label>
                        <input type="password" id="authPassword" placeholder="Passwort eingeben" autocomplete="current-password">
                    </div>
                    
                    <div class="checkbox-group">
                        <input type="checkbox" id="authRemember">
                        <label for="authRemember">Angemeldet bleiben</label>
                    </div>
                    
                    <div class="auth-error hidden" id="authError"></div>
                    
                    <div class="auth-buttons">
                        <button id="btnLogin" class="btn btn-primary">Login</button>
                        <button id="btnRegister" class="btn btn-secondary">Registrieren</button>
                    </div>
                </div>
                
                <div class="auth-loading hidden" id="authLoading">
                    <div class="spinner"></div>
                    <p>Verbinde mit Server...</p>
                </div>
            </div>
        `;
        
        document.body.appendChild(overlay);
        this.bindEvents();
    }

    bindEvents() {
        document.getElementById('btnLogin').addEventListener('click', () => this.handleLogin());
        document.getElementById('btnRegister').addEventListener('click', () => this.handleRegister());
        document.getElementById('authPassword').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.handleLogin();
        });
    }

    async handleLogin() {
        const username = document.getElementById('authUsername').value.trim();
        const password = document.getElementById('authPassword').value;
        this.rememberMe = document.getElementById('authRemember').checked;

        if (!username || !password) {
            this.showError('Bitte Username und Passwort eingeben!');
            return;
        }

        this.showLoading(true);
        const result = await this.dbService.authenticate(username, password);
        this.showLoading(false);

        if (result.success) {
            // Save to localStorage if remember me
            if (this.rememberMe) {
                localStorage.setItem('sar_remember', JSON.stringify({ username, password }));
            }
            this.onSuccess(result.userId, username);
        } else {
            this.showError(result.error || 'Login fehlgeschlagen!');
        }
    }

    async handleRegister() {
        const username = document.getElementById('authUsername').value.trim();
        const password = document.getElementById('authPassword').value;

        if (!username || !password) {
            this.showError('Bitte Username und Passwort eingeben!');
            return;
        }

        if (password.length < 4) {
            this.showError('Passwort muss mindestens 4 Zeichen haben!');
            return;
        }

        this.showLoading(true);
        const result = await this.dbService.register(username, password);
        this.showLoading(false);

        if (result.success) {
            this.showSuccess('Account erstellt! Einloggen...');
            setTimeout(() => this.onSuccess(result.userId, username), 1000);
        } else {
            this.showError(result.error || 'Registrierung fehlgeschlagen!');
        }
    }

    async tryAutoLogin() {
        try {
            const saved = localStorage.getItem('sar_remember');
            if (saved) {
                const { username, password } = JSON.parse(saved);
                this.showLoading(true);
                const result = await this.dbService.authenticate(username, password);
                this.showLoading(false);

                if (result.success) {
                    this.onSuccess(result.userId, username);
                    return true;
                } else {
                    localStorage.removeItem('sar_remember');
                }
            }
        } catch (e) {
            console.error('[Auth] Auto-login failed:', e);
        }
        return false;
    }

    showError(message) {
        const errorEl = document.getElementById('authError');
        errorEl.textContent = message;
        errorEl.className = 'auth-error';
    }

    showSuccess(message) {
        const errorEl = document.getElementById('authError');
        errorEl.textContent = message;
        errorEl.className = 'auth-error auth-success';
    }

    showLoading(show) {
        document.getElementById('authLoading').classList.toggle('hidden', !show);
        document.querySelector('.auth-form').classList.toggle('hidden', show);
    }

    show() {
        document.getElementById('authModal').classList.add('active');
    }

    hide() {
        document.getElementById('authModal').classList.remove('active');
    }

    onSuccess(userId, username) {
        // Override this in app.js
        console.log('[Auth] Success:', userId, username);
    }
}

/**
 * Savefile Selection UI
 */
class SavefileUI {
    constructor(databaseService) {
        this.dbService = databaseService;
        this.savefiles = [];
        this.createModal();
    }

    createModal() {
        const overlay = document.createElement('div');
        overlay.id = 'savefileModal';
        overlay.className = 'modal-overlay';
        overlay.innerHTML = `
            <div class="modal-content savefile-modal">
                <h2 class="modal-title">?? Savefile wählen</h2>
                <p class="modal-subtitle">Wähle einen Savefile oder erstelle einen neuen</p>
                
                <div class="savefile-list" id="savefileList">
                    <!-- Savefiles werden hier eingefügt -->
                </div>
                
                <div class="savefile-actions">
                    <button id="btnCreateSavefile" class="btn btn-primary">
                        ? Neuer Savefile
                    </button>
                    <button id="btnLogout" class="btn btn-danger">
                        ?? Logout
                    </button>
                </div>
                
                <div class="savefile-loading hidden" id="savefileLoading">
                    <div class="spinner"></div>
                    <p>Lade Savefiles...</p>
                </div>
            </div>
        `;
        
        document.body.appendChild(overlay);
        this.bindEvents();
    }

    bindEvents() {
        document.getElementById('btnCreateSavefile').addEventListener('click', () => this.handleCreate());
        document.getElementById('btnLogout').addEventListener('click', () => this.handleLogout());
    }

    async load() {
        this.showLoading(true);
        this.savefiles = await this.dbService.getAllSavefiles();
        this.showLoading(false);
        this.renderSavefiles();
    }

    renderSavefiles() {
        const container = document.getElementById('savefileList');
        
        if (this.savefiles.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <p>Keine Savefiles gefunden</p>
                    <p class="text-muted">Erstelle deinen ersten Savefile!</p>
                </div>
            `;
            return;
        }

        container.innerHTML = this.savefiles.map(s => `
            <div class="savefile-item ${s.adminUsed ? 'admin-used' : ''}" data-id="${s.id}">
                <div class="savefile-info">
                    <div class="savefile-header">
                        <span class="savefile-id">Savefile #${s.id}</span>
                        ${s.adminUsed ? '<span class="admin-badge">ADMIN</span>' : ''}
                    </div>
                    <div class="savefile-stats">
                        <span>?? ${this.formatMoney(s.money)}</span>
                        <span>?? ${s.gems}</span>
                        <span>?? ${s.rebirths} Rebirths</span>
                    </div>
                    <div class="savefile-date">
                        Zuletzt gespielt: ${this.formatDate(s.lastPlayed)}
                    </div>
                </div>
                <div class="savefile-actions">
                    <button class="btn btn-sm btn-primary" onclick="savefileUI.handleSelect('${s.id}')">
                        ?? Laden
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="savefileUI.handleDelete('${s.id}')">
                        ???
                    </button>
                </div>
            </div>
        `).join('');
    }

    async handleSelect(savefileId) {
        this.showLoading(true);
        try {
            const stats = await this.dbService.loadSavefile(savefileId);
            this.onSelect(stats, savefileId);
        } catch (error) {
            alert('Fehler beim Laden: ' + error.message);
            this.showLoading(false);
        }
    }

    async handleCreate() {
        this.showLoading(true);
        try {
            const stats = await this.dbService.createNewSavefile();
            this.onSelect(stats, this.dbService.currentSavefileId);
        } catch (error) {
            alert('Fehler beim Erstellen: ' + error.message);
            this.showLoading(false);
        }
    }

    async handleDelete(savefileId) {
        if (!confirm('Savefile wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden!')) {
            return;
        }

        const success = await this.dbService.deleteSavefile(savefileId);
        if (success) {
            await this.load();
        } else {
            alert('Fehler beim Löschen!');
        }
    }

    handleLogout() {
        if (confirm('Wirklich ausloggen?')) {
            localStorage.removeItem('sar_remember');
            location.reload();
        }
    }

    showLoading(show) {
        document.getElementById('savefileLoading').classList.toggle('hidden', !show);
        document.getElementById('savefileList').classList.toggle('hidden', show);
    }

    show() {
        document.getElementById('savefileModal').classList.add('active');
    }

    hide() {
        document.getElementById('savefileModal').classList.remove('active');
    }

    formatMoney(value) {
        const num = BigInt(value);
        if (num < 1000n) return num.toString();
        if (num < 1000000n) return (Number(num) / 1000).toFixed(1) + 'K';
        if (num < 1000000000n) return (Number(num) / 1000000).toFixed(1) + 'M';
        return (Number(num) / 1000000000).toFixed(1) + 'B';
    }

    formatDate(dateStr) {
        if (!dateStr) return 'Nie';
        const date = new Date(dateStr);
        return date.toLocaleString('de-DE', { 
            day: '2-digit', 
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    onSelect(stats, savefileId) {
        // Override in app.js
        console.log('[Savefile] Selected:', savefileId);
    }
}

/**
 * Leaderboard UI
 */
class LeaderboardUI {
    constructor(databaseService) {
        this.dbService = databaseService;
        this.currentCategory = 0;
        this.createModal();
    }

    createModal() {
        const overlay = document.createElement('div');
        overlay.id = 'leaderboardModal';
        overlay.className = 'modal-overlay';
        overlay.innerHTML = `
            <div class="modal-content leaderboard-modal">
                <h2 class="modal-title">?? Leaderboard</h2>
                
                <div class="leaderboard-category">
                    <label>Kategorie:</label>
                    <select id="leaderboardCategory" class="category-select">
                        <option value="0">?? Meistes Geld</option>
                        <option value="1">? Seltenster Rayan</option>
                        <option value="2">?? Meiste Rolls</option>
                        <option value="3">?? Meiste Gems</option>
                        <option value="4">?? Meiste Spielzeit</option>
                        <option value="5">?? Meiste Rayans</option>
                    </select>
                </div>
                
                <div class="leaderboard-list" id="leaderboardList">
                    <!-- Leaderboard entries here -->
                </div>
                
                <div class="leaderboard-info">
                    ?? Nur legitime Spieler (ohne Admin-Modus) werden angezeigt
                </div>
                
                <button id="btnCloseLeaderboard" class="btn btn-secondary">Schließen</button>
                
                <div class="leaderboard-loading hidden" id="leaderboardLoading">
                    <div class="spinner"></div>
                    <p>Lade Leaderboard...</p>
                </div>
            </div>
        `;
        
        document.body.appendChild(overlay);
        this.bindEvents();
    }

    bindEvents() {
        document.getElementById('leaderboardCategory').addEventListener('change', (e) => {
            this.currentCategory = parseInt(e.target.value);
            this.load();
        });
        
        document.getElementById('btnCloseLeaderboard').addEventListener('click', () => this.hide());
        
        // Close on overlay click
        document.getElementById('leaderboardModal').addEventListener('click', (e) => {
            if (e.target.id === 'leaderboardModal') this.hide();
        });
    }

    async load() {
        this.showLoading(true);
        const entries = await this.dbService.getLeaderboard(this.currentCategory);
        this.showLoading(false);
        this.renderLeaderboard(entries);
    }

    renderLeaderboard(entries) {
        const container = document.getElementById('leaderboardList');
        
        if (entries.length === 0) {
            container.innerHTML = '<div class="empty-state">Keine Einträge gefunden</div>';
            return;
        }

        container.innerHTML = entries.map((entry, index) => {
            const rank = index + 1;
            const medal = rank === 1 ? '??' : rank === 2 ? '??' : rank === 3 ? '??' : '';
            const rankClass = rank <= 3 ? `rank-${rank}` : '';
            
            return `
                <div class="leaderboard-entry ${rankClass}">
                    <div class="entry-rank">${medal}${rank}</div>
                    <div class="entry-info">
                        <div class="entry-username">${entry.username}</div>
                        <div class="entry-value">${entry.value}</div>
                    </div>
                </div>
            `;
        }).join('');
    }

    showLoading(show) {
        document.getElementById('leaderboardLoading').classList.toggle('hidden', !show);
        document.getElementById('leaderboardList').classList.toggle('hidden', show);
    }

    show() {
        document.getElementById('leaderboardModal').classList.add('active');
        this.load();
    }

    hide() {
        document.getElementById('leaderboardModal').classList.remove('active');
    }
}

// Make globally available
window.AuthUI = AuthUI;
window.SavefileUI = SavefileUI;
window.LeaderboardUI = LeaderboardUI;
