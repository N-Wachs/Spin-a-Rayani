# ?? Multiplayer Implementation - FERTIG!

## ? Was implementiert wurde:

### 1. **Services\EventSyncService.cs**
- FileSystemWatcher für OneDrive/Dropbox
- Admin kann Events publishen
- Clients empfangen automatisch
- Fehlerbehandlung für File-Locks
- Duplicate-Detection

### 2. **Models\SharedEventData.cs**
- JSON-Datenmodell für Events
- Enthält: SuffixName, StartTime, Duration, AdminName, Timestamp

### 3. **Services\GameManager.cs**
- Multiplayer-Constructor mit `sharedFolderPath` + `isMultiplayerAdmin`
- `ForceSpecificEvent(suffixName)` - Admin-only
- `ApplyRemoteEvent(suffixName, adminName)` - Event anwenden
- EventSync-Service Integration

### 4. **MainForm.cs**
- Auto-Load von `multiplayer.txt` Config
- Event-Selection Dialog (30 Suffixes)
- Tastenbelegung: 'E' oder 'M' für Event-Start
- Dark-Theme Dialog

### 5. **Config-Dateien**
- `multiplayer_ADMIN.txt` - Für Niklas
- `multiplayer_CLIENT.txt` - Für Freund
- Beide mit ausführlichen Kommentaren

### 6. **MULTIPLAYER_SETUP.md**
- Vollständige Anleitung für Admin + Client
- Troubleshooting-Section
- FAQ
- Status-Check

### 7. **README.md Update**
- Multiplayer-Section hinzugefügt
- Gameplay-Tipps erweitert
- Link zu Setup-Guide

---

## ?? Wie es funktioniert:

```
???????????????????????????????????????????????????????????????
?                                                               ?
?  ADMIN (Niklas)                    CLIENT (Freund)           ?
?  ================                  ===============           ?
?                                                               ?
?  1. Drückt 'E' oder 'M'            1. Spielt normal          ?
?     ?                                  ?                      ?
?  2. Dialog: Wählt "Omega"          2. Wartet...              ?
?     ?                                  ?                      ?
?  3. Event ? events.json            3. FileWatcher: onChange!  ?
?     ?                                  ?                      ?
?  4. OneDrive sync (1-5s)           4. Liest events.json      ?
?     ?                                  ?                      ?
?  5. Event startet lokal            5. Event startet!         ?
?                                                               ?
?  ?? Omega Event! (2:30)            ?? Omega Event!           ?
?                                    (von nikla) (2:30)        ?
?                                                               ?
???????????????????????????????????????????????????????????????
```

---

## ?? Setup für euch:

### **ADMIN (Niklas):**
```powershell
# 1. OneDrive-Ordner erstellen
New-Item -Path "C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan" -ItemType Directory

# 2. Config kopieren
Copy-Item "multiplayer_ADMIN.txt" "multiplayer.txt"

# 3. Ordner teilen (manuell in OneDrive)
# Rechtsklick ? Freigeben ? "Kann anzeigen"

# 4. Spiel starten
.\Spin a Rayani.exe
```

### **CLIENT (Freund):**
```powershell
# 1. OneDrive-Link öffnen (vom Admin)
# ? "Zu meinem OneDrive hinzufügen"

# 2. Config anpassen
Copy-Item "multiplayer_CLIENT.txt" "multiplayer.txt"
notepad multiplayer.txt
# ? FOLDER= mit eigenem Pfad ersetzen

# 3. Spiel starten
.\Spin a Rayani.exe
```

---

## ?? Testing:

### **Test 1: Lokaler Test (auf deinem PC)**
```
1. Erstelle 2 Ordner:
   - C:\Test\Admin\
   - C:\Test\Client\

2. Kopiere Spiel in beide Ordner

3. Admin-Config:
   FOLDER=C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
   ADMIN=true

4. Client-Config (in Client-Ordner):
   FOLDER=C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
   ADMIN=false

5. Starte BEIDE Instanzen

6. Admin: Drücke 'E' ? Wähle Event
   Client sollte Event nach 1-2 Sekunden empfangen!
```

### **Test 2: Mit Freund**
```
1. Du: Starte Admin-Version
2. Freund: Startet Client-Version
3. Du: Drücke 'E' ? "GC Event"
4. Freund sollte nach 3-5 Sekunden Event sehen
5. Beide: Rolled während Event ? 20x Boost!
```

---

## ?? Debug-Checks:

### **Console-Output prüfen:**
```
# Erfolgreicher Admin-Start:
[MainForm] Multiplayer enabled from config:
  Folder: C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
  Admin: True
[EventSync] Initialized - Mode: ADMIN
[GameManager] Multiplayer enabled - Role: ADMIN

# Erfolgreicher Client-Start:
[MainForm] Multiplayer enabled from config:
  Folder: C:\Users\Max\OneDrive\...\Spin a Rayan
  Admin: False
[EventSync] Initialized - Mode: CLIENT
[EventSync] FileWatcher active - waiting for admin events...
[GameManager] Multiplayer enabled - Role: CLIENT

# Event gesendet (Admin):
[GameManager] Published multiplayer event: Omega
[EventSync] ? Published Event: Omega at 14:30:25

# Event empfangen (Client):
[EventSync] ? Applied Event: Omega from nikla
[GameManager] ? Event applied: Omega (Duration: 2.5 min)
```

---

## ?? Nutzung:

**Als Admin:**
- Drücke **'E'**: Admin-Mode Cheat + Event-Dialog
- Drücke **'M'**: Multiplayer-Event (ohne Cheat)
- Dialog: Wähle eines der 30 Suffixes
- Event startet für ALLE!

**Als Client:**
- Einfach spielen!
- Events erscheinen automatisch oben
- Event-Anzeige: "?? Omega Event! (von nikla) - ..."

---

## ?? Features:

? **Event-Auswahl**: Alle 30 Suffixes (nicht random!)
? **Cloud-Sync**: OneDrive/Dropbox kompatibel
? **Admin-Control**: Nur Admin kann Events starten
? **Automatisch**: Clients empfangen ohne Tastendruck
? **Dark-Theme**: Passt zum Spiel
? **Error-Handling**: Fehlermeldungen wenn OneDrive nicht läuft
? **File-Lock-Retry**: Automatischer Retry bei OneDrive-Locks
? **Event-Info**: Zeigt Admin-Name an bei Clients

---

## ?? Sicherheit:

**Was geteilt wird:**
```json
{
  "SuffixName": "Omega",
  "StartTime": "2024-01-15T14:30:00Z",
  "DurationMinutes": 2.5,
  "AdminName": "nikla",
  "Timestamp": 638403966000000000
}
```

**Was NICHT geteilt wird:**
- ? Savegame
- ? Rayans
- ? Geld/Gems
- ? Quest-Progress

---

## ?? Next Steps:

1. ? Pushe zu GitHub
2. ? Erstelle OneDrive-Ordner
3. ? Benenne `multiplayer_ADMIN.txt` ? `multiplayer.txt`
4. ? Teste lokal (2 Instanzen)
5. ? Teile OneDrive-Link mit Freund
6. ? Freund richtet Client ein
7. ? Teste zusammen!

---

**FERTIG! Viel Spaß beim gemeinsamen Spielen! ???**
