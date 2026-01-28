# 🎲 Spin a Rayan

Ein C# .NET 8 Windows Forms Idle/Incremental Game mit Dark Mode und 500+ einzigartigen Rayans!

## ✨ Features

### 🎯 Core Gameplay
- **500+ Einzigartige Rayans**: Von "Rat" (1:1) bis "Cosmic Titan" (1:Billionen+)
  - Tiere, Mythologie, Elemente, Edelsteine
  - Dynamisch generierte Kombinationen
  - Exponentiell steigende Seltenheit
- **20 Suffix-Typen**: Zusätzliche Multiplikatoren (1.5x - 200x)
  - Common: Selbstbewusst, GC, Blessed, Shiny, Cursed
  - Uncommon: SSL, Radiant, Shadow, Golden, Mystic
  - Rare: Cosmic, Void, Divine, Infernal
  - Epic: Primordial, Ancient, Transcendent
  - Legendary: Legendary, Eternal, Omega
- **Merge-System**: 5 gleiche Rayans → 1 Merged (5x stärker)

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
- **Gewichtete Events**: Seltenere Suffixes = seltenere Events
- **Live-Timer**: Countdown-Anzeige ganz oben
- **Admin-Control**: Force Event mit 'E'-Taste

### 📊 Plot & Income System
- **Max 10 Plots**: Erweiterbar nur durch Rebirths
- **Auto-Equip**: Automatisch beste Rayans ausrüsten
- **Total Income**: Zeigt tatsächliches Income (mit Multiplier)
- **Farbcodierung**: Rarity-basierte dunkle Farben

### 🔄 Rebirth System
- **+50% Income** pro Rebirth
- **+50% Luck** pro Rebirth
- **+1 Plot Slot** pro Rebirth (max 10)
- **Gratis im Admin Mode**
- **Progressive Kosten**: Exponentiell steigend

### 💎 Upgrades (Gems)
- **Auto Roll**: Automatisches Rollen freischalten (100 Gems)
- **Roll Cooldown**: Reduziere Cooldown (Start: 2.0s, Min: 0.5s)

### 💰 Upgrades (Money)
- **Luck Booster**: +25% Luck pro Level
- **Plot Slots**: Nur durch Rebirths!

### 📋 Quests (7 Quests)
- **Rolling**: 100, 1.000, 10.000 Rolls
- **Zeit**: 30 Min, 2 Stunden
- **Rebirth**: 5, 25 Rebirths
- **Gesamt-Belohnungen**: Bis zu 14.800 Gems

### 🎨 UI/UX
- **Dark Mode**: Komplettes dunkles Design
- **Live-Stats**:
  - 💰 Money
  - 💎 Gems
  - 🍀 Luck: +X%
  - 🔄 Rebirth: +X%
- **Smooth Updates**: Double-buffering für flüssige Anzeige
- **Event-Display**: Farbcodierte Event-Anzeige mit Timer
- **Responsive**: Keine störenden Scrollbars

### 🔧 Admin Mode
- **Cheat-Code**: `,` `-` `.` (Komma, Minus, Punkt)
- **Features**:
  - Gratis Dices kaufen
  - Gratis Rebirths
  - Force Events ('E'-Taste)
  - Sichtbarer [ADMIN] Prefix

### 💾 System
- **Auto-Save**: Alle 10 Minuten
- **XML-Speicher**: Lokale savegame.xml
- **Version-Check**: Automatisch beim Start
- **Basic Dice Init**: Automatisch bei leerem Savefile

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

1. **Nutze Events**: Während Events aktiv rollen für seltene Suffixes!
2. **Merge klug**: Warte bis du genug (20+) hast bevor du mergst
3. **Auto-Equip**: Nutze den Button nach jedem Roll-Sprint
4. **Dice-Management**: Kaufe Dices im Bulk mit MAX-Button
5. **Rebirth-Timing**: Rebirthe wenn du Plot-Slots brauchst oder stuck bist
6. **Quest-Focus**: Priorisiere Time-Quests (laufen passiv)

## 🛠️ Anforderungen
- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 (empfohlen)

## 📥 Installation
1. Repository klonen
2. Projekt in Visual Studio 2022 öffnen
3. Build & Run (F5)
4. Savegame: `savegame.xml` im Projektordner

## 🎯 Geplante Features
- [ ] Achievements-System
- [ ] Statistiken-Übersicht
- [ ] Sound-Effects
- [ ] Mehr Dice-Typen
- [ ] Prestige-System über Rebirth hinaus
- [ ] Cloud-Save-Option

## 📝 Version
**Aktuelle Version**: 1.4.0
- Event-Boost: 5x → **20x**! 
- 35+ Dice-Typen (bis 100.000x Luck)
- Billionen-Bereich Progression
- All-Time Statistics
- Options & Reset-Funktion
- 500 einzigartige Rayans
- 20 Suffix-Typen
- 7 Quests

## 🙏 Credits
Inspiriert von "Spin a Badie"
Entwickelt mit C# & .NET 8 Windows Forms

---
**Have fun spinning! 🎲✨**
