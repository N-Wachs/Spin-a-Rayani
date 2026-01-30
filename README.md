# 🎲 Spin a Rayan

Ein C# .NET 8 Windows Forms Idle/Incremental Game mit Dark Mode und 500+ einzigartigen Rayans!

## 🌐 Web-Version

**Jetzt auch als Web-Version verfügbar!** 
Spiele direkt im Browser: [https://n-wachs.github.io/Spin-a-Rayani/](https://n-wachs.github.io/Spin-a-Rayani/)

Die Web-Version läuft vollständig im Browser ohne Installation.

## ✨ Features

### 🎯 Core Gameplay
- **500+ Einzigartige Rayans**: Von "Rat" (1:1) bis zu astronomisch seltenen kombinierten Rayans (1:10^36+)
  - **186 manuelle Rayans**: Tiere, Mythologie (Zeus, Odin, Shiva), Elemente, Edelsteine
  - **314 generierte Kombinationen**: "Ancient Warrior", "Eternal Mage", "Void Destroyer"
  - **Tier 1-17**: Exponentiell steigende Seltenheit und Werte
    - Tier 1-10: Common bis Divine (1 - 25M BaseValue)
    - Tier 11-13: Transcendent bis Cosmic (25M - 25B BaseValue)
    - Tier 14-16: Universal bis Eternal (25B - 25T BaseValue)
    - Tier 17: Absolute (25T - 100T BaseValue)
- **30 Suffix-Typen**: Zusätzliche Multiplikatoren (1.5x - 1000x)
  - Common: Selbstbewusst, GC, Blessed, Shiny, Cursed
  - Uncommon: SSL, Radiant, Shadow, Golden, Mystic
  - Rare: Cosmic, Void, Divine, Infernal
  - Epic: Primordial, Ancient, Transcendent
  - Legendary: Legendary, Eternal, Omega
  - **🆕 Ultra-Legendary**: Unstoppable (250x), Infinite (500x), Absolute (1000x)
- **Merge-System**: 5 gleiche Rayans → 1 Merged (5x stärker)
  - Einzelnes Merge oder "MERGE ALL" für alle Gruppen
  - Automatisches Handle-Management (kein Speicher-Leak mehr!)

### 🎲 Dice System
- **35+ Würfel-Typen**: Basic bis Supreme Dice (1x - 100.000x Luck!)
- **Tier 1-6**: Silver bis Cosmic (1.5x - 50x)
- **Tier 7**: Mythic (75x - 300x) - Millionen bis Milliarden
- **Tier 8**: Transcendent (500x - 2.500x) - Milliarden bis Billionen
- **Tier 9**: Ultimate (5.000x - 100.000x) - Billionen+
- **Quantity-System**: Jeder Roll verbraucht einen Dice (außer Basic)
- **Auto-Switch**: Automatischer Wechsel zu Basic Dice wenn leer
- **MAX-Kauf**: Kaufe automatisch so viele wie möglich

### 🔥 Event-System
- **Suffix-Events**: Alle 5 Minuten für 2,5 Minuten
- **20x Boost**: Ausgewähltes Suffix erscheint 20x häufiger!
- **50% Uptime**: Events sind die Hälfte der Zeit aktiv
- **🆕 Faire Events**: ALLE 30 Suffixes haben gleiche Event-Chance (je ~3.33%)
  - Common bis Ultra-Legendary: Jedes Suffix kann Event werden
  - Event-Farben für Unstoppable (Bright Red-Orange), Infinite (Bright Cyan), Absolute (Pure White)
- **Live-Timer**: Countdown-Anzeige ganz oben
- **Admin-Control**: Force Event mit 'E'-Taste

### 📊 Plot & Income System
- **Max 10 Plots**: Erweiterbar nur durch Rebirths
- **Auto-Equip**: Automatisch beste Rayans ausrüsten
- **Total Income**: Zeigt tatsächliches Income (mit Multiplier)
- **Farbcodierung**: Rarity-basierte dunkle Farben

### 🔄 Rebirth System
- **🆕 Neue Kosten-Struktur**:
  - 1. Rebirth: **100K** (statt 10K)
  - 2. Rebirth: **800K** (8x teurer)
  - 3. Rebirth: **6.4M** (8x teurer)
  - Jeder Rebirth: 8^n × 100K
- **+50% Income** pro Rebirth (4x Multiplier)
- **+50% Luck** pro Rebirth
- **+1 Plot Slot** pro Rebirth (max 10)
- **Gratis im Admin Mode**
- **Button zeigt Kosten**: Immer sichtbar wie viel das nächste Rebirth kostet

### 💎 Upgrades (Gems)
- **Auto Roll**: Automatisches Rollen freischalten (100 Gems)
- **🆕 Roll Cooldown**: Reduziere Cooldown (Start: 2.0s, Min: 0.5s)
  - Jedes Upgrade: Cooldown - 0.2s
  - Kosten: Start 200 Gems, dann × 1.5 pro Level
  - Max. 8 Upgrades (2.0s → 0.5s)

### 💰 Upgrades (Money)
- **Luck Booster**: +25% Luck pro Level
- **Plot Slots**: Nur durch Rebirths!

### 📋 Quests (7 Quests)
- **🆕 Persistente Speicherung**: Quest-Fortschritt bleibt nach Neustart erhalten!
- **Rolling**: 100, 1.000, 10.000 Rolls (wiederholbar mit progressivem Ziel)
- **Zeit**: 30 Min, 2 Stunden (einmalig)
- **Rebirth**: 5, 25 Rebirths (einmalig)
- **🆕 Auto-Refresh**: Quest-Liste aktualisiert sich automatisch nach Claim
- **Gesamt-Belohnungen**: Bis zu 14.800 Gems + unbegrenzt durch wiederholbare Quests

### 🎨 UI/UX
- **Dark Mode**: Komplettes dunkles Design
- **Live-Stats**:
  - 💰 Money
  - 💎 Gems
  - 🍀 Luck: +X%
  - 🔄 Rebirth: +X%
- **🆕 Performance-Optimiert**:
  - Selektive UI-Updates (nur geänderte Labels)
  - Dirty-Flags für Plot-Display
  - Gecachte Roll-Tabellen (80% schneller!)
  - Handle-Management im Inventar (kein Crash mehr!)
- **Event-Display**: Farbcodierte Event-Anzeige mit Timer
- **Responsive**: Keine störenden Scrollbars
- **🆕 Roll-Timer Fix**: 2.0s Cooldown funktioniert jetzt korrekt

### 🔧 Admin Mode
- **Cheat-Code**: `,` `-` `.` (Komma, Minus, Punkt)
- **Features**:
  - Gratis Dices kaufen
  - Gratis Rebirths
  - Force Events ('E'-Taste)
  - Sichtbarer [ADMIN] Prefix

### 💾 System
- **Auto-Save**: Alle 60 Sekunden
- **🆕 Quest-Speicherung**: Alle Quests werden im Savefile gespeichert
- **🆕 Fehlerbehandlung**: 
  - Automatisches Backup bei korrupten Savefiles
  - Benutzerfreundliche Fehlermeldungen
  - Savefile-Version-Check
- **XML-Speicher**: Lokale savegame.xml
- **Version-Check**: Automatisch beim Start
- **Basic Dice Init**: Automatisch bei leerem Savefile

### 🌐 Multiplayer (NEU!)
- **🆕 Auto-Setup**: Dialog beim ersten Start für einfache Konfiguration!
- **🆕 Settings-Integration**: Username und Einstellungen jederzeit änderbar!
- **Event-Synchronisation**: Admin startet Events für alle Spieler!
- **Cloud-Based**: OneDrive/Dropbox Sync (1-5s Verzögerung)
- **Admin-Kontrolle**: Nur Admin kann Events starten
- **Event-Auswahl**: Dialog mit allen 30 Suffixes
- **Custom Username**: Zeige deinen Namen statt Windows-Username
- **Bis zu 10 Spieler**: Gleichzeitig unterstützt
- **Setup**: Automatischer Dialog oder manuelle `multiplayer.txt`
- **Sicher**: Nur Events werden geteilt, keine Savegames!
- **Ordner-Erstellung**: Automatisch oder manuell
- **Single-Player**: Jederzeit überspringen möglich

## 📊 Progression Overview

### Early Game (0-5 Rebirths)
- Sammle Basic → Silver → Golden Dices
- Nutze Events für seltene Suffixes
- Erste Merges für stärkere Rayans
- 100-1.000 Rolls für Gem-Quests

### Mid Game (5-15 Rebirths)
- Diamond/Ruby/Sapphire Dices
- 50-100% Rebirth-Bonus
- 5-7 Plot Slots
- Mythic/Divine Rayans sammeln

### Late Game (15+ Rebirths)
- Platinum/Emerald/Crystal Dices
- 750%+ Rebirth-Bonus
- 10 Plot Slots (Maximum)
- Legendary Rayans + Omega Suffixes
- 10.000+ Rolls Achievement

### End Game
- Celestial/Divine/Cosmic Dices (20x-50x)
- 1000%+ Rebirth-Bonus
- Vollständig gemergte Rayans
- Alle Quests abgeschlossen

### Ultra End Game (1000+ Rebirths)
- Mythic Dices (75x-300x)
- Transcendent Dices (500x-2.500x)
- Ultimate Dices (5.000x-100.000x)
- Trillionen+ Income pro Sekunde
- Supreme Dice für maximales Glück

## 🎮 Gameplay-Tipps

1. **Nutze Events**: Während Events aktiv rollen für seltene Suffixes (20x Chance!)
2. **Merge klug**: Warte bis du genug (20+) hast bevor du mergst
3. **Auto-Equip**: Nutze den Button nach jedem Roll-Sprint
4. **Dice-Management**: Kaufe Dices im Bulk mit MAX-Button
5. **Rebirth-Timing**: 
   - Erster Rebirth bei 100K+ Income
   - Rebirthe wenn du Plot-Slots brauchst oder stuck bist
   - 50% Rebirth-Bonus = massive Income-Steigerung!
6. **Quest-Focus**: Priorisiere Time-Quests (laufen passiv)
7. **🆕 Roll-Cooldown**: Investiere in Cooldown-Reduktion für schnelleres Rollen
8. **🆕 Merge-All**: Bei 50+ mergbaren Gruppen nutze MERGE ALL
9. **🆕 Performance**: Bei 500+ unique Rayans werden nur Top 500 angezeigt
10. **🌐 Multiplayer**: Spielt mit Freunden! Admin drückt 'E' oder 'M' → Alle bekommen Event!
11. **⚙️ Username**: Setze deinen Namen in Options → Multiplayer für Event-Anzeige!

## 🌐 Multiplayer Setup

**Für 2-10 Spieler gleichzeitig!**

### 🆕 Einfacher Auto-Setup (empfohlen):

**Beim ersten Start:**
1. Dialog öffnet sich automatisch
2. Wähle "Admin" oder "Client"
3. Gib OneDrive-Pfad ein (wird automatisch erkannt!)
4. Klicke "Multiplayer aktivieren"
5. Fertig! 🎉

**Admin:**
- Drücke 'E' oder 'M' → Wähle Event → Alle bekommen es!

**Client:**
- Warte auf Events vom Admin!

**→ Siehe [MULTIPLAYER_SETUP.md](MULTIPLAYER_SETUP.md) für Details!**

### Manuelles Setup (optional):

**Admin:**
1. Erstelle: `C:\Users\[Name]\OneDrive\Anwendungen\Spin a Rayan\`
2. Teile Ordner (OneDrive → Rechtsklick → Freigeben → **"Kann anzeigen"**)
3. Erstelle `multiplayer.txt` neben .exe:
```
FOLDER=C:\Users\[Name]\OneDrive\Anwendungen\Spin a Rayan
ADMIN=true
```

**Client:**
1. Öffne OneDrive-Link → "Zu meinem OneDrive hinzufügen"
2. Erstelle `multiplayer.txt`:
```
FOLDER=C:\Users\[Name]\OneDrive\Anwendungen\Spin a Rayan
ADMIN=false
```

## 🐛 Bekannte Probleme & Fixes

### ✅ Behoben in 1.2.0:
- ~~Handle-Leak im Inventar~~ → Fixed mit proper Control.Dispose()
- ~~Quest-Fortschritt geht verloren~~ → Quest-Speicherung implementiert
- ~~Roll-Timer funktioniert nicht~~ → Button-Enabled-State korrigiert
- ~~Rebirth-Button zeigt falsche Kosten~~ → Formel angepasst (8^n × 100K)
- ~~Events nur für häufige Suffixes~~ → Alle Suffixes gleiche Chance

### Performance-Tipps:
- Bei 1000+ Rayans: Nutze Merge-All regelmäßig
- Schließe andere Programme bei AutoRoll mit 0.1s Cooldown
- Inventar-Display limitiert auf 500 Gruppen (automatisch)

## 🛠️ Anforderungen
- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 (empfohlen)

## 📥 Installation
1. Repository klonen
2. Projekt in Visual Studio 2022 öffnen
3. Build & Run (F5)
4. Savegame: `savegame.xml` im Projektordner

## 📁 Projektstruktur

```
Spin-a-Rayani/
├── docs/              # GitHub Pages Website (Web-Version)
│   ├── assets/
│   │   ├── css/      # Stylesheets
│   │   ├── js/       # JavaScript Game Logic
│   │   └── images/   # Bilder und Icons
│   └── index.html
├── Forms/            # Windows Forms UI
│   ├── Main/        # Hauptformular
│   └── Dialogs/     # Dialog-Formulare
├── Models/          # Game Models (Dice, Rayan, Quest, etc.)
├── Services/        # Game Services (GameManager, SaveService, etc.)
├── Assets/          # Embedded Resources (Dice Icons)
├── Config/          # Konfigurationsdateien
├── Documentation/   # Zusätzliche Dokumentation
└── Program.cs       # Entry Point
```

## 🎯 Geplante Features
- [ ] Achievements-System
- [ ] Statistiken-Übersicht
- [ ] Sound-Effects
- [ ] Mehr Dice-Typen
- [ ] Prestige-System über Rebirth hinaus
- [ ] Cloud-Save-Option
- [ ] Virtual Mode für sehr große Inventare (10.000+ Rayans)
- [ ] Background Worker für Autosave
- [ ] Export/Import von Savefiles

## 📝 Version
**Aktuelle Version**: 1.2.0

### 🆕 Was ist neu in 1.2.0:
- **🎯 56 neue legendäre Rayans**: Tier 11-17 hinzugefügt (Ananke bis Scylla)
  - Griechische Titanologie: Ananke, Hemera, Hypnos, Morpheus, Eros, etc.
  - Nahtlose Progression: 25M bis 100T BaseValue
  - Rarity: 1.45M bis 513.6B
- **⚡ Ultra-Legendary Suffix-Tier**: 3 neue extrem seltene Suffixes
  - Unstoppable (1:200K, 250x Multiplier)
  - Infinite (1:500K, 500x Multiplier)
  - Absolute (1:1M, 1000x Multiplier!)
- **🔥 Event-System überarbeitet**: 
  - Alle 30 Suffixes haben gleiche Event-Chance
  - Neue Event-Farben für Ultra-Legendary Suffixes
- **💰 Rebirth-System verbessert**:
  - Erster Rebirth: 100K (statt 10K)
  - Button zeigt immer nächste Kosten an
  - Fix: Enabled-State aktualisiert sich mit Money
- **📋 Quest-System erweitert**:
  - Persistente Speicherung (bleibt nach Neustart)
  - Auto-Refresh nach Claim (kein manuelles Aktualisieren mehr)
  - Keine Bestätigungs-MessageBox beim Einsammeln
- **🚀 Massive Performance-Optimierungen**:
  - Roll-Service: Cached sortierte Listen (80% schneller!)
  - UI: Selektive Updates nur bei Änderungen
  - Plot-Display: Dirty-Flag-System
  - Inventar: Handle-Leak Fix (kein Crash mehr bei 100+ Rayans!)
  - Display-Limit: Maximal 500 Gruppen angezeigt
- **🛠️ Roll-Timer Fix**: 2.0s Cooldown funktioniert jetzt korrekt
- **💾 Savefile-Verbesserungen**:
  - Quest-Fortschritt wird gespeichert
  - Automatisches Backup bei korrupten Dateien
  - Benutzerfreundliche Fehlermeldungen
  - Version-Check für Kompatibilität
- **🎮 Qualität-of-Life**:
  - Keine MessageBox mehr nach Merge (weniger Klicks)
  - Auto-Save jede Minute (statt 10 Minuten)
  - Bessere Label-Updates (nur wenn nötig)

### Frühere Versionen:
**Version 1.1.0**:
- Event-Boost: 5x → 20x
- 35+ Dice-Typen (bis 100.000x Luck)
- All-Time Statistics
- Options & Reset-Funktion

**Version 1.0.0**:
- Initial Release
- 500 einzigartige Rayans
- 20 Suffix-Typen
- 7 Quests
- Dice-System
- Rebirth-System

## 🙏 Credits
Inspiriert von "Spin a Badie"
Entwickelt mit C# & .NET 8 Windows Forms

---
**Have fun spinning! 🎲✨**
