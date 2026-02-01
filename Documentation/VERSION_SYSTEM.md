# Version Update System

## Überblick
Dieses Projekt verfügt über ein automatisches Versions-Check-System, das beim Start der Anwendung prüft, ob eine neue Version verfügbar ist.

## Wie funktioniert es?

### 1. Version-Datei (version.txt)
Die Datei `version.txt` im Root-Verzeichnis des Repositories enthält die aktuelle Version im Format:
```
1.0.0
```

### 2. Version-Check beim Start
- Beim Start der Anwendung wird automatisch die `version.txt` vom GitHub Repository geladen
- Die lokale Version wird mit der GitHub-Version verglichen
- Wenn eine neuere Version verfügbar ist, wird dem Benutzer eine Benachrichtigung angezeigt
- Der Benutzer kann trotzdem weiterspielen

### 3. Neue Version veröffentlichen

Um eine neue Version zu veröffentlichen:

1. **Aktualisiere die Version in der .csproj-Datei:**
   ```xml
   <Version>1.0.1</Version>
   <AssemblyVersion>1.0.1.0</AssemblyVersion>
   <FileVersion>1.0.1.0</FileVersion>
   ```

2. **Aktualisiere die version.txt im Root:**
   ```
   1.0.1
   ```

3. **Commit und Push zum GitHub Repository:**
   ```bash
   git add version.txt SpinARayan/SpinARayan.csproj
   git commit -m "Release Version 1.0.1"
   git push origin main
   ```

4. **Optional: Erstelle ein GitHub Release**
   - Gehe zu: https://github.com/N-Wachs/Spin-a-Rayan/releases
   - Klicke auf "Create a new release"
   - Tag: `v1.0.1`
   - Title: `Version 1.0.1`
   - Beschreibung: Was ist neu in dieser Version
   - Lade die kompilierte .exe hoch

## Versionsnummern-Format
Das Projekt verwendet Semantic Versioning (SemVer):
- **MAJOR.MINOR.PATCH** (z.B. 1.0.0)
- **MAJOR**: Breaking Changes
- **MINOR**: Neue Features (rückwärtskompatibel)
- **PATCH**: Bug Fixes

## Beispiele

### Patch Update (Bug Fix)
```
1.0.0 ? 1.0.1
```

### Minor Update (Neues Feature)
```
1.0.1 ? 1.1.0
```

### Major Update (Breaking Changes)
```
1.1.0 ? 2.0.0
```

## Technische Details

### VersionService.cs
Der `VersionService` ist verantwortlich für:
- Abrufen der lokalen Version aus der Assembly
- Laden der GitHub Version von: `https://raw.githubusercontent.com/N-Wachs/Spin-a-Rayan/main/version.txt`
- Vergleichen der Versionen
- Erstellen der Update-Benachrichtigung

### Fehlerbehandlung
- Wenn kein Internet verfügbar ist, wird der Check übersprungen
- Wenn die version.txt nicht geladen werden kann, spielt die Anwendung normal weiter
- Timeout nach 5 Sekunden

## Benutzererfahrung
Wenn eine neue Version verfügbar ist, sieht der Benutzer beim Start:

```
?? Neue Version verfügbar!

Deine Version: 1.0.0
Neue Version: 1.0.1

Besuche: https://github.com/N-Wachs/Spin-a-Rayan/releases

Du kannst jetzt weiterspielen, aber wir empfehlen ein Update!
```

Die Nachricht erscheint bei **jedem Start**, bis die Version aktualisiert wurde.
