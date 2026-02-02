# ?? Spin a Rayan v4.0.0

Ein modernes C# .NET 8 Windows Forms Idle/Incremental Game mit **Dark Mode**, **500+ einzigartigen Rayans** und **Online-Multiplayer**!

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Windows Forms](https://img.shields.io/badge/Windows%20Forms-WinForms-0078D4?logo=windows)](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## ?? Version 4.0.0 - Major Update

### **Was ist neu?**
- ? **Neues Roll-System** - 6x Rebirth-Kosten (einfacher!)
- ?? **Bronze/Silber/Gold System** - Rayans in Farben nach Seltenheit
- ?? **Dynamische Rarity Quest** - Unbegrenzt skalierend
- ??? **Moderations-System** - Ban & Kick Flags
- ?? **Rebirth Counter** - Zeigt aktuelles Rebirth an
- ? **Version-Check** - Alte Saves werden automatisch gelöscht
- ?? **Performance** - Optimiertes Quest- und DB-System

> ?? **Wichtig**: Version 4.0.0 ist **nicht kompatibel** mit alten Saves (< 4.0.0). Alte Savefiles werden beim ersten Start automatisch gelöscht.

---

## ?? Web-Version

**Jetzt auch als Web-Version verfügbar!**  
?? Spiele direkt im Browser: **[https://n-wachs.github.io/Spin-a-Rayani/](https://n-wachs.github.io/Spin-a-Rayani/)**

Die Web-Version läuft vollständig im Browser ohne Installation und synchronisiert mit der Desktop-Version!

> ?? Für Entwickler: Siehe [GITHUB_PAGES.md](Documentation/GITHUB_PAGES.md)

---

## ?? Inhaltsverzeichnis

- [Features](#-features)
- [Screenshots](#-screenshots)
- [Installation](#-installation)
- [Gameplay](#-gameplay)
- [Systeme](#-systeme)
- [Entwicklung](#-entwicklung)
- [Technologien](#-technologien)

---

## ? Features

### ?? Core Gameplay

#### **500+ Einzigartige Rayans**
- **186 manuelle Rayans**: Tiere, Mythologie (Zeus, Odin, Shiva), Elemente, Edelsteine
- **314 generierte Kombinationen**: "Ancient Warrior", "Eternal Mage", "Void Destroyer"
- **17 Seltenheits-Tiers**: Von Common (1:1) bis Absolute (1:10^36+)

| Tier | Rarity Range | BaseValue | Beispiel |
|------|--------------|-----------|----------|
| 1-5 | 1 - 1,000 | 1 - 1K | Common bis Rare |
| 6-10 | 10K - 25M | 10K - 25M | Epic bis Divine |
| 11-13 | 100M - 25B | 25M - 25B | Transcendent bis Cosmic |
| 14-16 | 100B - 100T | 25B - 25T | Universal bis Eternal |
| 17 | 10Q+ | 25T - 100T | **Absolute** |

#### **30 Suffix-Typen mit Multiplikatoren**
```
Common:        Selbstbewusst (1.5x), GC (2x), Blessed (3x)
Uncommon:      SSL (5x), Radiant (10x), Shadow (15x)
Rare:          Cosmic (25x), Void (50x), Divine (75x)
Epic:          Primordial (100x), Ancient (150x)
Legendary:     Legendary (200x), Eternal (250x), Omega (300x)
Ultra-Legend:  Unstoppable (250x), Infinite (500x), Absolute (1000x) ?
```

---

### ?? Würfel-System

**35+ Dice-Typen** mit 1x - 100.000x Luck Multiplier!

| Tier | Dice | Luck | Kosten | Quantity |
|------|------|------|---------|----------|
| 0 | Basic | 1x | Free | ? |
| 1-3 | Silver - Gold | 1.5x - 3x | 100 - 5K Gems | 10-50 |
| 4-6 | Platinum - Cosmic | 5x - 50x | 10K - 500K Gems | 25-100 |
| 7 | Mythic | 75x - 300x | 1M - 50M Gems | 50-200 |
| 8 | Transcendent | 500x - 2,500x | 100M - 10B Gems | 100-500 |
| 9 | Ultimate | 5,000x - 100,000x | 50B+ Gems | 250-1000 |

**Features:**
- ? Auto-Switch zu Basic Dice wenn leer
- ? MAX-Kauf: Kaufe automatisch Maximum
- ? Quantity-System: Jeder Roll verbraucht 1 Dice

---

### ?? Quest-System

#### **Rolling Quests** (Wiederholbar)
- 100 Rolls ? 100 Gems (+50 pro Wiederholung)
- 1,000 Rolls ? 1,200 Gems (+500 pro Wiederholung)
- 10,000 Rolls ? 5,000 Gems (+5,000 pro Wiederholung)

#### **?? Dynamische Rarity Quest** (Unbegrenzt)
Sammle seltene Rayans für exponentiell steigende Rewards!

| Level | Rarity | Gems |
|-------|--------|------|
| 0 | 1,000 | 50 |
| 1 | 1,000,000 | 200 |
| 2 | 1,000,000,000 | 800 |
| 3 | 1,000,000,000,000 | 3,200 |
| 4 | 1,000,000,000,000,000 | 12,800 |
| 5+ | 1,000^(level+1) | 50 × 4^level |

**Formel:**
- Rarity: `1000 × (1000^level)`
- Gems: `50 × (4^level)`

---

### ?? Rebirth-System

**Progression durch Rebirths:**
- ?? **Money Multiplier**: +400% pro Rebirth
- ?? **Luck Bonus**: +50% pro Rebirth
- ?? **Plot Slots**: +1 pro Rebirth (Max: 10)

**?? Einfachere Kosten** (Version 4.0.0):
```
Formel: 100,000 × (6^Rebirths)

Rebirth 1:  600K
Rebirth 2:  3.6M
Rebirth 3:  21.6M
Rebirth 4:  129.6M
Rebirth 5:  777.6M
```

**Was bleibt erhalten:**
- ? Total Rebirths
- ? Total Rolls
- ? Total Playtime
- ? Best Rayan Ever
- ? Quests & Gems
- ? Dice Inventory

**Was wird zurückgesetzt:**
- ? Money ? 0
- ? Inventory (alle Rayans)
- ? Plot Slots ? 1 (+ Rebirths)

---

### ?? UI-Features

#### **?? Bronze/Silber/Gold System**
Gerollte Rayans werden in Farben angezeigt basierend auf gefühlter Seltenheit:

| Farbe | Adjusted Rarity | Chance | Beispiel (Luck 1.0) |
|-------|-----------------|--------|---------------------|
| ?? **Gold** | ? 125 | ? 0.8% | Episch+ |
| ?? **Silber** | 25 - 124 | 2-4% | Rare+ |
| ?? **Bronze** | 10 - 24 | 4-10% | Uncommon+ |
| ? **Weiß** | < 10 | > 10% | Common |

#### **Modern Dark Theme**
- ?? Dunkles Design mit farbigen Akzenten
- ?? Übersichtliche Statistiken
- ?? Große, klickbare Buttons
- ?? Responsive Layout (1400×850px)

---

### ?? Multiplayer & Cloud

#### **Online-Synchronisation**
- ?? **Login-System**: Username + Passwort
- ?? **Cloud-Saves**: Bis zu 10 Savefiles pro Account
- ?? **Auto-Sync**: Speichert alle 30 Sekunden
- ?? **Leaderboard**: 6 Kategorien (Money, Rarest Rayan, Rolls, etc.)

#### **?? Moderations-Tools** (Admin)
- ?? **Kick Flag**: Temporärer Kick (wird beim Login zurückgesetzt)
- ? **Ban Flag**: Permanenter Ban
- ?? **Admin-Tracking**: Admin-Saves werden vom Leaderboard ausgeschlossen

---

### ?? Event-System

**Suffix-Events:**
- ? Alle 5 Minuten für 2,5 Minuten
- ?? **20x Boost** auf ausgewähltes Suffix
- ?? **50% Uptime**
- ?? **Faire Verteilung**: Alle 30 Suffixes haben gleiche Chance (~3.33%)

---

### ?? Upgrade-System

#### **Luck Booster**
- Level 1: +25% Luck ? 100 Gems
- Level 2: +50% Luck ? 250 Gems
- Max Level: 10 (+250% Luck)

#### **Roll Cooldown**
- Level 1: 1.8s ? 150 Gems
- Level 2: 1.6s ? 350 Gems
- Max Level: 10 (0.2s)

#### **Auto Roll**
- Unlock: 500 Gems
- Toggle On/Off

#### **?? Skip Next Rebirth**
- Kosten: 10,000 Gems
- Effekt: +2 Rebirths auf einmal!

---

## ?? Gameplay

### **Ziel des Spiels**
Sammle immer seltenere Rayans, verdiene Geld, kaufe Upgrades und rebirthe dich für permanente Boni!

### **Loop:**
1. ?? **Roll** für neue Rayans
2. ?? **Equip** die besten auf Plots (passives Einkommen)
3. ?? **Kaufe** Upgrades mit Gems
4. ?? **Rebirth** für permanente Multiplikatoren
5. ?? **Wiederhole** mit mehr Power!

---

## ??? Systeme

### **Merge-System**
```csharp
5 gleiche Rayans ? 1 Merged Rayan (5x Value)
```
- Einzelnes Merge oder "MERGE ALL"
- Automatisches Handle-Management

### **Plot-System**
```csharp
Passives Einkommen = ?(Equipped Rayans) × MoneyMultiplier
```
- Bis zu 10 Plots (durch Rebirths)
- Auto-Equip für beste Rayans

### **Luck-System**
```csharp
Total Luck = BaseLuck × LuckBooster × DiceLuck × RebirthBonus
```
- LuckBooster: +25% pro Level
- RebirthBonus: +50% pro Rebirth
- DiceLuck: 1x - 100,000x

---

## ?? Installation

### **Voraussetzungen**
- Windows 10/11
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### **Download**
1. Neueste Release von [GitHub Releases](https://github.com/N-Wachs/Spin-a-Rayani/releases) herunterladen
2. ZIP entpacken
3. `SpinARayan.exe` starten

### **Aus Source bauen**
```bash
git clone https://github.com/N-Wachs/Spin-a-Rayani.git
cd Spin-a-Rayani
dotnet build -c Release
dotnet run --project SpinARayan.csproj
```

---

## ?? Steuerung

### **Maus**
- **Linksklick**: Roll, Buttons, Inventar-Interaktion
- **Rechtsklick**: Rayan-Kontext-Menü (Equip/Delete/Merge)

### **Tastatur** (Desktop)
- `Space`: Roll
- `E`: Auto-Equip
- `I`: Inventar öffnen

### **?? Debug-Commands** (DEBUG-Build)
```sh
ad                - Toggle Admin Mode
cash -add XXX     - Add Money
gems -add XXX     - Add Gems
roll XXX          - Roll X times (Statistiken)
roll -speed XXX   - Set Roll Cooldown (0.1-60s)
```

---

## ?? Datenbank-Schema

### **Supabase PostgreSQL**

#### **User Table**
```sql
CREATE TABLE "User" (
  id SERIAL PRIMARY KEY,
  username TEXT UNIQUE NOT NULL,
  password TEXT NOT NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  kick_flag BOOLEAN DEFAULT false,
  banned_flag BOOLEAN DEFAULT false,
  feedback_send TEXT
);
```

#### **Savefiles Table**
```sql
CREATE TABLE "Savefiles" (
  id SERIAL PRIMARY KEY,
  user_id INTEGER REFERENCES "User"(id),
  created_in_version TEXT NOT NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  last_played TIMESTAMPTZ,
  admin_used BOOLEAN DEFAULT false,
  
  -- Stats
  money TEXT,
  total_money_earned TEXT,
  gems INTEGER,
  rebirths INTEGER,
  rarity_quest_level INTEGER DEFAULT 0,
  
  -- Game Data (JSONB)
  inventory JSONB,
  equipped_rayan_indices JSONB,
  owned_dice JSONB,
  saved_quests JSONB,
  
  -- Tracking
  best_rayan_ever_name TEXT,
  best_rayan_rarity TEXT,
  best_rayan_value TEXT
);
```

---

## ????? Entwicklung

### **Projekt-Struktur**
```
SpinARayan/
??? Models/              # Data Models (Rayan, PlayerStats, Quest, etc.)
??? Services/            # Business Logic (GameManager, RollService, etc.)
??? Forms/
?   ??? Main/           # MainForm (Haupt-UI)
?   ??? Dialogs/        # Pop-up Dialogs (Inventory, Shop, etc.)
??? Config/             # Configuration & Theme
??? Assets/             # Images (Dice, Icons)
??? docs/               # Web-Version (GitHub Pages)
```

### **Architektur**
```
MainForm
  ?
GameManager (Controller)
  ?
??? RollService (Roll-Logik)
??? QuestService (Quest-Tracking)
??? DatabaseService (Cloud-Sync)
??? PlayerStats (Game State)
```

---

## ?? Technologien

### **Frontend**
- **Windows Forms** (.NET 8)
- **Custom Dark Theme** (ModernTheme.cs)
- **Manual Layout** (Kein Designer)

### **Backend**
- **Supabase** (PostgreSQL + REST API)
- **HTTP Client** (Cloud-Sync)
- **JSON Serialization** (System.Text.Json)

### **Datenstrukturen**
- **BigInteger** (für große Zahlen)
- **JSONB** (für komplexe Arrays)
- **XOR-Encryption** (für Passwörter)

### **Performance**
- **Cached Lists** (Prefixes/Suffixes sortiert einmal)
- **Dirty Flags** (UI nur bei Änderungen updaten)
- **Image Caching** (Dice-Bilder im RAM)

---

## ?? Statistiken

- **500+ Rayans** (186 manuell + 314 generiert)
- **30 Suffixes** (Common bis Ultra-Legendary)
- **35+ Dice** (1x bis 100,000x Luck)
- **17 Tiers** (1:1 bis 1:10^36+)
- **Cloud-Saves** (bis zu 10 pro Account)

---

## ?? Changelog

### **Version 4.0.0** (2025-01-XX)
- ? Neues Roll-System mit fairen Wahrscheinlichkeiten
- ?? Bronze/Silber/Gold System für Rayans
- ?? Dynamische Rarity Quest (unbegrenzt)
- ??? Moderations-System (Ban/Kick)
- ? 6x Rebirth-Kosten (einfacher!)
- ?? Rebirth Counter unter Button
- ?? Version-Check & Auto-Cleanup
- ?? Zahlreiche Bugfixes

### **Version 3.1.0** (2024-XX-XX)
- ?? Event-System
- ?? Quest-System
- ?? Cloud-Saves

---

## ?? Beitragen

Contributions sind willkommen! Bitte erstelle einen Pull Request oder öffne ein Issue.

---

## ?? Lizenz

MIT License - siehe [LICENSE](LICENSE)

---

## ?? Credits

- **Entwicklung**: N-Wachs
- **Konzept**: Spin a Rayan Community
- **Testing**: Beta-Tester

---

## ?? Support

- ?? **Bugs**: [GitHub Issues](https://github.com/N-Wachs/Spin-a-Rayani/issues)
- ?? **Feedback**: In-Game Feedback-Button
- ?? **Kontakt**: Via GitHub

---

**Viel Spaß beim Spielen!** ???
