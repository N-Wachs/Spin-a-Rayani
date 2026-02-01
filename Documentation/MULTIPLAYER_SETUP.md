# ?? Multiplayer Setup Guide - Spin a Rayan

## ?? Überblick

Mit dem Multiplayer-Feature kann **ein Admin** (Niklas) Events für alle verbundenen Spieler starten!

**Features:**
- ? Admin wählt Suffix aus ? Alle bekommen Event
- ? Event-Sync über OneDrive (1-5 Sekunden)
- ? 20x Boost für ausgewähltes Suffix
- ? 2.5 Minuten Dauer
- ? Bis zu 10 Spieler unterstützt

---

## ?? Setup für ADMIN (Niklas)

### **Schritt 1: OneDrive-Ordner erstellen**

1. Öffne Windows Explorer
2. Navigiere zu: `C:\Users\nikla\OneDrive\`
3. Erstelle Ordnerstruktur: `Anwendungen\Spin a Rayan\`
4. Vollständiger Pfad: `C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan\`

### **Schritt 2: Ordner teilen**

1. Rechtsklick auf `Spin a Rayan` Ordner
2. "Freigeben" oder "Share"
3. **WICHTIG:** Berechtigung auf **"Kann anzeigen"** setzen (READ-ONLY!)
4. Link kopieren
5. Link an Freunde senden (Discord/WhatsApp/etc.)

### **Schritt 3: Config-Datei einrichten**

1. Kopiere `multiplayer_ADMIN.txt` 
2. Benenne um zu: `multiplayer.txt`
3. Lege neben die `Spin a Rayani.exe` (im selben Ordner wie savegame.xml)
4. Überprüfe Inhalt:
```
FOLDER=C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
ADMIN=true
```

### **Schritt 4: Spiel starten**

1. Starte `Spin a Rayani.exe`
2. In der Konsole sollte stehen:
   ```
   [MainForm] Multiplayer enabled from config:
     Folder: C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
     Admin: True
   [EventSync] Initialized - Mode: ADMIN
   ```

### **Schritt 5: Event starten**

1. Im Spiel: Drücke **'E'** oder **'M'**
2. Dialog öffnet sich mit allen 30 Suffixes
3. Wähle z.B. "Omega Event"
4. Klicke "Event starten"
5. Event startet für ALLE Spieler!

**Event-Anzeige oben:**
```
?? Omega Event! (2:30 verbleibend)
```

---

## ?? Setup für CLIENT (Freund)

### **Schritt 1: OneDrive-Link öffnen**

1. Öffne Link vom Admin
2. Klicke "Zu meinem OneDrive hinzufügen"
3. OneDrive synchronisiert automatisch (kann 1-2 Minuten dauern)

### **Schritt 2: Ordner-Pfad finden**

1. Öffne OneDrive-Ordner (Windows Explorer ? OneDrive)
2. Navigiere zu: `...\Anwendungen\Spin a Rayan\`
3. Klicke in Adressleiste ? Strg+C (Pfad kopieren)
4. Beispiel: `C:\Users\[DeinName]\OneDrive\Anwendungen\Spin a Rayan`

### **Schritt 3: Config-Datei einrichten**

1. Kopiere `multiplayer_CLIENT.txt`
2. Benenne um zu: `multiplayer.txt`
3. Öffne mit Notepad
4. Trage deinen Pfad ein:
```
FOLDER=C:\Users\Max\OneDrive\Anwendungen\Spin a Rayan
ADMIN=false
```
5. Speichern!
6. Lege neben die `Spin a Rayani.exe`

### **Schritt 4: Spiel starten**

1. Starte `Spin a Rayani.exe`
2. In der Konsole sollte stehen:
   ```
   [MainForm] Multiplayer enabled from config:
     Folder: C:\Users\Max\OneDrive\...\Spin a Rayan
     Admin: False
   [EventSync] FileWatcher active - waiting for admin events...
   ```

### **Schritt 5: Warten auf Events**

Wenn Admin ein Event startet:
1. Event erscheint nach 1-5 Sekunden
2. Oben im Spiel:
   ```
   ?? Omega Event! (von nikla) - Omega 20x häufiger! (2:30 verbleibend)
   ```
3. Du bekommst automatisch 20x Boost!

---

## ?? Troubleshooting

### **Admin: Event wird nicht gesendet**

**Problem:** "Event konnte nicht veröffentlicht werden"

**Lösungen:**
1. ? OneDrive läuft? (Taskleiste ? OneDrive-Icon)
2. ? Ordner existiert?
3. ? Schreibzugriff? (Rechtsklick ? Eigenschaften ? Kein "Schreibgeschützt")
4. ? Config korrekt? (ADMIN=true, richtiger Pfad)

### **Client: Event kommt nicht an**

**Problem:** "Keine Events empfangen"

**Lösungen:**
1. ? OneDrive synchronisiert? (Taskleiste ? OneDrive ? "Aktuell")
2. ? Ordner sichtbar im Explorer?
3. ? Config korrekt? (ADMIN=false, richtiger Pfad mit [DeinName])
4. ? Spiel neu starten
5. ? Admin soll Testversion senden

### **Sync dauert zu lange (>10 Sekunden)**

**Lösungen:**
1. ? OneDrive auf "Pause" ? "Fortsetzen"
2. ? Internet-Verbindung prüfen
3. ? OneDrive Sync-Status prüfen (Rechtsklick auf Ordner)

### **Wie teste ich ob es funktioniert?**

**Admin:**
```
1. Drücke 'E' ? Wähle "GC Event"
2. Prüfe Ordner: C:\Users\nikla\OneDrive\...\Spin a Rayan\events.json
3. Datei sollte existieren mit Inhalt:
   {
     "SuffixName": "GC",
     "StartTime": "2024-01-15T...",
     ...
   }
```

**Client:**
```
1. Warte 5-10 Sekunden
2. Prüfe DEINEN Ordner: C:\Users\[Du]\OneDrive\...\Spin a Rayan\events.json
3. Datei sollte GLEICH sein!
4. Event sollte im Spiel oben erscheinen
```

---

## ?? Nutzung

### **Als Admin:**

**Option 1: Drücke 'E'**
- Aktiviert Admin-Mode Cheat-Code
- Zeigt Event-Auswahl

**Option 2: Drücke 'M'** (Multiplayer-Only)
- Direkt Event-Auswahl
- Kein Cheat-Code nötig

### **Als Client:**

- Spiel einfach normal!
- Events erscheinen automatisch oben
- Kein Tastendruck nötig

---

## ?? Sicherheit

**Was Freunde SEHEN können:**
- ? `events.json` - Nur Suffix-Name, Timestamp, Admin-Name
- ? **NICHT** dein Savegame!
- ? **NICHT** deine Rayans/Geld/Gems!

**Was Freunde TUN können:**
- ? Events empfangen
- ? **KEINE** Events starten (nur du mit ADMIN=true)
- ? **KEINE** Dateien ändern (Read-Only Berechtigung)

---

## ?? Status prüfen

**Im Spiel drücke:** `,` `-` `.` (Cheat-Code)
? Console öffnet sich mit Debug-Infos

**Erfolgreiche Config:**
```
[MainForm] Multiplayer enabled from config:
  Folder: C:\Users\...\Spin a Rayan
  Admin: True/False
[EventSync] Initialized - Mode: ADMIN/CLIENT
```

**Event gesendet:**
```
[EventSync] ? Published Event: Omega at 14:30:25
```

**Event empfangen:**
```
[EventSync] ? Applied Event: Omega from nikla
```

---

## ?? Tipps & Tricks

1. **Teste zuerst lokal:** Beide auf demselben PC mit 2 Instanzen
2. **Nutze Discord Voice:** Für "Event kommt in 3... 2... 1... jetzt!"
3. **Rare Events:** Spare "Absolute" Events für schwierige Phasen
4. **Event-Rotation:** Wechsle Suffixes ab für Abwechslung
5. **Sync-Zeit:** Rechne mit 2-5 Sekunden Verzögerung

---

## ? FAQ

**Q: Kann ich ohne Multiplayer spielen?**
A: Ja! Lösche einfach `multiplayer.txt` ? Single-Player Modus

**Q: Kann ich zwischen Admin/Client wechseln?**
A: Ja, ändere `ADMIN=true/false` in `multiplayer.txt` ? Neustart

**Q: Funktioniert es auch mit Dropbox/Google Drive?**
A: Ja! Nur den FOLDER= Pfad anpassen

**Q: Wie viele Spieler maximal?**
A: Theoretisch unbegrenzt, empfohlen: 2-10

**Q: Kostet das extra?**
A: Nein! OneDrive kostenlos mit Windows (5GB gratis)

---

## ?? Support

**Probleme?**
1. Prüfe Console (Cheat-Code: `,` `-` `.`)
2. Prüfe `events.json` im OneDrive-Ordner
3. OneDrive Sync-Status prüfen

**Noch Fragen?**
? GitHub Issues: https://github.com/N-Wachs/Spin-a-Rayani/issues

---

**Viel Spaß beim gemeinsamen Spielen! ???**
