# ?? Auto-Setup Feature - Multiplayer

## ? Was ist neu?

Kein manuelles Config-Erstellen mehr! Das Spiel macht alles für dich! ??

## ?? Wie es funktioniert:

### **Beim ERSTEN Start:**

```
1. Spiel öffnet sich
2. Dialog erscheint: "Multiplayer Setup"
3. Du wählst:
   - ? Admin (Ich starte Events)
   - ? Client (Ich empfange Events)
4. Du gibst OneDrive-Pfad ein
   (wird automatisch erkannt!)
5. Klicke "Multiplayer aktivieren"
6. Fertig!
```

**Der Dialog erstellt automatisch:**
- ? `multiplayer.txt` Datei
- ? OneDrive-Ordner (falls nicht existiert)
- ? Korrekte Config (FOLDER= und ADMIN=)

---

## ?? Für Admin:

### **Setup:**
```
1. Starte Spiel
2. Dialog: Wähle "Admin"
3. Pfad: C:\Users\[DeinName]\OneDrive\Anwendungen\Spin a Rayan
   (meist schon korrekt!)
4. Klicke "Multiplayer aktivieren"
```

### **Ordner teilen:**
```
1. Öffne Windows Explorer
2. Gehe zu: C:\Users\[DeinName]\OneDrive\Anwendungen\
3. Rechtsklick auf "Spin a Rayan" Ordner
4. "Freigeben"
5. Berechtigung: "Kann anzeigen" (READ-ONLY!)
6. Link kopieren ? An Freund senden
```

### **Events starten:**
```
Im Spiel:
- Drücke 'E' (Admin-Mode Cheat)
- Oder 'M' (Multiplayer direkt)
- Dialog: Wähle Suffix (z.B. "Omega")
- Event startet für ALLE!
```

---

## ?? Für Client:

### **Setup:**
```
1. Admin sendet dir OneDrive-Link
2. Öffne Link ? "Zu meinem OneDrive hinzufügen"
3. OneDrive synchronisiert (1-2 Minuten warten)
4. Starte Spiel
5. Dialog: Wähle "Client"
6. Pfad: C:\Users\[DeinName]\OneDrive\Anwendungen\Spin a Rayan
   (Prüfe dass es DEIN Pfad ist!)
7. Klicke "Multiplayer aktivieren"
```

### **Events empfangen:**
```
- Einfach spielen!
- Wenn Admin Event startet:
  ? Event erscheint nach 1-5 Sekunden oben
  ? ?? Omega Event! (von Admin-Name)
  ? 20x Boost automatisch!
```

---

## ?? Single-Player Modus:

Willst du ohne Multiplayer spielen?

```
1. Dialog: Klicke "Später / Single-Player"
2. Spiel startet normal (ohne Events-Sync)
3. Du kannst jederzeit Multiplayer aktivieren:
   ? Erstelle manuell "multiplayer.txt" neben .exe
```

---

## ?? Features:

### ? **Auto-Detection:**
- OneDrive-Pfad wird automatisch erkannt
- Dropbox wird auch erkannt
- Fallback auf Standard-Pfad

### ? **Ordner-Erstellung:**
- Dialog fragt: "Ordner erstellen?"
- Automatische Erstellung wenn gewünscht
- Fehler-Handling wenn keine Rechte

### ? **Config-Speicherung:**
- `multiplayer.txt` wird automatisch erstellt
- Mit Kommentaren und Timestamp
- Neben der .exe (wo auch savegame.xml liegt)

### ? **Validation:**
- Prüft ob Pfad leer ist
- Zeigt Fehler-Meldungen
- Erstellt Ordner auf Wunsch

---

## ?? Was wird erstellt:

### **multiplayer.txt Inhalt:**
```
# Multiplayer Config - Auto-Generated
# Created: 2024-01-15 14:30:00

FOLDER=C:\Users\nikla\OneDrive\Anwendungen\Spin a Rayan
ADMIN=true

# Role: ADMIN
# Du kannst Events starten mit 'E' oder 'M'
```

---

## ?? Beispiel-Workflow:

### **Du (Admin) + 1 Freund:**

**Schritt 1: DU (Admin)**
```
1. Starte Spiel ? Dialog
2. Wähle "Admin"
3. Pfad ok? ? "Multiplayer aktivieren"
4. Erfolg! ?
```

**Schritt 2: DU (Admin) - Ordner teilen**
```
1. Windows Explorer
2. C:\Users\[Du]\OneDrive\Anwendungen\Spin a Rayan
3. Rechtsklick ? Freigeben
4. "Kann anzeigen"
5. Link ? Discord/WhatsApp an Freund
```

**Schritt 3: FREUND (Client)**
```
1. Link öffnen ? "Zu OneDrive hinzufügen"
2. Warten (1-2 Min)
3. Spiel starten ? Dialog
4. Wähle "Client"
5. Pfad anpassen (SEIN Username!)
6. "Multiplayer aktivieren"
7. Fertig! ?
```

**Schritt 4: TESTEN**
```
DU: Drücke 'E' ? Wähle "GC Event" ? OK
FREUND: Nach 3-5 Sekunden ? Event erscheint!
        ?? GC Event! (von nikla)
```

---

## ?? Advanced:

### **Username ändern:**
```
1. Im Spiel: Options-Menü (??)
2. Scrolle runter zu "?? Multiplayer Einstellungen"
3. Trage deinen Namen ein (z.B. "Niklas" statt "nikla")
4. Klicke ?? (Speichern)
5. Dein Name wird jetzt in Events angezeigt!
```

### **Multiplayer-Einstellungen ändern:**
```
1. Options-Menü ? "?? Multiplayer konfigurieren"
2. Dialog öffnet sich
3. Ändere OneDrive-Pfad oder Admin/Client
4. Spiel neu starten
5. Fertig!
```

### **Config später ändern:**
```
1. Öffne: [.exe-Ordner]\multiplayer.txt
2. Ändere FOLDER= oder ADMIN=
3. Speichern
4. Spiel neu starten
```

### **Multiplayer deaktivieren:**
```
1. Lösche: multiplayer.txt
2. Oder benenne um zu: multiplayer.txt.backup
3. Spiel startet im Single-Player Modus
```

### **Ordner-Browser nutzen:**
```
Im Dialog: Klicke ??-Button
? Windows Ordner-Auswahl öffnet sich
? Navigiere zu deinem Cloud-Ordner
? OK ? Pfad wird eingetragen
```

---

## ?? Troubleshooting:

### **Dialog erscheint nicht beim Start:**
? `multiplayer.txt` existiert bereits!
? Lösche sie um Dialog erneut zu sehen

### **"Fehler beim Erstellen des Ordners":**
? Keine Schreibrechte
? Erstelle Ordner manuell im Explorer
? Versuche erneut

### **"Fehler beim Speichern der Config":**
? .exe-Ordner ist schreibgeschützt
? Rechtsklick ? Eigenschaften ? Schreibschutz entfernen

### **OneDrive-Pfad falsch:**
? Klicke ??-Button im Dialog
? Navigiere manuell zu OneDrive
? Oder gib komplett eigenen Pfad ein

---

## ? Checklist:

**Admin:**
- [ ] Dialog erscheint beim Start
- [ ] "Admin" gewählt
- [ ] Pfad korrekt (mit eigenem Username!)
- [ ] "Multiplayer aktivieren" geklickt
- [ ] Erfolgs-Meldung erscheint
- [ ] Ordner in OneDrive freigegeben
- [ ] Link an Freund gesendet

**Client:**
- [ ] OneDrive-Link vom Admin erhalten
- [ ] Link geöffnet ? "Zu OneDrive hinzufügen"
- [ ] OneDrive synchronisiert (Ordner sichtbar)
- [ ] Dialog erscheint beim Spiel-Start
- [ ] "Client" gewählt
- [ ] Pfad mit EIGENEM Username angepasst!
- [ ] "Multiplayer aktivieren" geklickt
- [ ] Erfolgs-Meldung erscheint

---

**Das war's! Viel einfacher als vorher! ??**

Keine manuelle Config-Erstellung, kein Umbenennen von Dateien, alles automatisch! ??
