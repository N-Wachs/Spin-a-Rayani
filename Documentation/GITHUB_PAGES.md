# 🌐 GitHub Pages Setup Guide

Dieses Dokument beschreibt, wie die Website über GitHub Pages konfiguriert und deployed wird.

## 📋 Setup-Schritte

### 1. GitHub Pages aktivieren

1. Gehe zu den Repository Settings auf GitHub: `https://github.com/N-Wachs/Spin-a-Rayani/settings`
2. Navigiere zu **Pages** im linken Menü
3. Unter **Source** wähle:
   - **Source:** Deploy from a branch
   - **Branch:** `main`
   - **Folder:** `/docs`
4. Klicke auf **Save**

### 2. GitHub Actions aktivieren (empfohlen)

Alternativ kann die Website auch über GitHub Actions deployed werden:

1. Gehe zu Repository Settings → Pages
2. Unter **Source** wähle: **GitHub Actions**
3. Der Workflow in `.github/workflows/pages.yml` wird automatisch ausgeführt

**Vorteil:** Automatisches Deployment bei jedem Push zum `main` Branch.

## 🔧 Workflow-Konfiguration

Die Datei `.github/workflows/pages.yml` enthält die GitHub Actions Konfiguration:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches:
      - main
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write
```

### Workflow-Features

- **Automatisches Deployment:** Bei jedem Push zum `main` Branch
- **Manuelles Deployment:** Über "Run workflow" Button in GitHub Actions
- **Permissions:** Minimale erforderliche Berechtigungen

## 📁 Dateistruktur

```
docs/
├── index.html          # Hauptseite
├── .nojekyll          # Verhindert Jekyll-Processing
├── README.md          # Dokumentation
└── assets/
    ├── css/
    │   └── styles.css  # Styles
    ├── js/             # JavaScript-Dateien
    │   ├── app.js
    │   ├── data.js
    │   ├── models.js
    │   ├── services.js
    │   └── ui.js
    └── images/         # Bilder (für zukünftige Assets)
```

## 🚀 Deployment-Prozess

1. **Code-Änderungen** im `docs/` Ordner
2. **Commit & Push** zum Repository
3. **GitHub Actions** läuft automatisch (oder Branch-Deployment)
4. **Website** wird aktualisiert unter: `https://n-wachs.github.io/Spin-a-Rayani/`

## 🔍 Troubleshooting

### Website zeigt 404-Fehler

- Überprüfe, ob GitHub Pages in den Settings aktiviert ist
- Stelle sicher, dass der `docs` Ordner im `main` Branch existiert
- Warte 1-2 Minuten nach dem ersten Setup

### CSS/JS laden nicht

- Überprüfe, ob die Pfade in `index.html` korrekt sind:
  ```html
  <link rel="stylesheet" href="assets/css/styles.css">
  <script src="assets/js/app.js"></script>
  ```
- Stelle sicher, dass `.nojekyll` Datei existiert

### GitHub Actions Workflow schlägt fehl

- Überprüfe die Permissions in den Repository Settings
- Gehe zu Settings → Actions → General → Workflow permissions
- Wähle "Read and write permissions"

## 🌐 URL

Nach erfolgreicher Konfiguration ist die Website verfügbar unter:

**https://n-wachs.github.io/Spin-a-Rayani/**

## 📝 Hinweise

- Die `.nojekyll` Datei ist wichtig, damit GitHub Pages keine Jekyll-Verarbeitung durchführt
- Änderungen am `main` Branch werden automatisch deployed
- Die Website läuft komplett clientseitig (kein Backend erforderlich)
- Alle Assets sind relativ zum `docs/` Ordner verlinkt

## 🔐 Sicherheit

- Keine sensiblen Daten im `docs/` Ordner speichern
- Alle Dateien sind öffentlich zugänglich
- JavaScript läuft im Browser des Benutzers
