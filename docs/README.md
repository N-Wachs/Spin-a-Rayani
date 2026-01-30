# 🌐 Spin a Rayan - Web Version

Dies ist die Web-Version von **Spin a Rayan**, die über GitHub Pages läuft.

## 🎮 Spielen

Die Website ist live unter: https://n-wachs.github.io/Spin-a-Rayani/

## 📁 Struktur

- `index.html` - Hauptseite der Webanwendung
- `assets/css/` - Stylesheet-Dateien
- `assets/js/` - JavaScript-Dateien (Game Logic)
- `assets/images/` - Bilder und Icons
- `.nojekyll` - Verhindert Jekyll-Processing für GitHub Pages

## 🚀 Deployment

Die Website wird automatisch über GitHub Actions deployt wenn Änderungen in den `main` Branch gepusht werden.

Der Workflow ist definiert in: `.github/workflows/pages.yml`

## 🛠️ Lokales Testen

Um die Website lokal zu testen:

1. Öffne einfach die `index.html` im Browser
2. Oder nutze einen lokalen Webserver:
   ```bash
   python -m http.server 8000
   # Oder
   npx serve .
   ```
3. Öffne http://localhost:8000 im Browser
