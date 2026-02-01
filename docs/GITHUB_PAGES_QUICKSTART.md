# 🎯 GitHub Pages Aktivierung - Schnellstart

## ⚡ Schnelle Aktivierung (2 Minuten)

Nach dem Merge dieser PR müssen Sie GitHub Pages in den Repository-Einstellungen aktivieren:

### Schritt 1: Repository Settings öffnen
1. Gehe zu: https://github.com/N-Wachs/Spin-a-Rayani/settings
2. Klicke im linken Menü auf **"Pages"**

### Schritt 2: GitHub Pages konfigurieren

**Option A: Branch Deployment (empfohlen für Start)**
1. Unter **"Source"**: Wähle **"Deploy from a branch"**
2. Unter **"Branch"**: 
   - Branch: `main`
   - Folder: `/docs`
3. Klicke **"Save"**
4. Fertig! Website wird in 1-2 Minuten verfügbar sein

**Option B: GitHub Actions (empfohlen für automatisches Deployment)**
1. Unter **"Source"**: Wähle **"GitHub Actions"**
2. Der Workflow `.github/workflows/pages.yml` ist bereits konfiguriert
3. Jeder Push zum `main` Branch deployed automatisch
4. Fertig!

### Schritt 3: Testen

Nach 1-2 Minuten:
- Besuche: https://n-wachs.github.io/Spin-a-Rayani/
- Die Website sollte live sein! 🎉

## 📋 Checkliste

- [ ] Repository Settings → Pages öffnen
- [ ] Source konfigurieren (Branch oder GitHub Actions)
- [ ] 1-2 Minuten warten
- [ ] Website testen: https://n-wachs.github.io/Spin-a-Rayani/

## ❓ Probleme?

Siehe vollständige Dokumentation: [GITHUB_PAGES.md](./GITHUB_PAGES.md)

## 🎮 Lokales Testen

Vor der Aktivierung lokal testen:
```bash
cd docs
python -m http.server 8000
# Öffne http://localhost:8000
```

---
**Hinweis:** Diese Datei kann nach erfolgreicher Aktivierung gelöscht werden.
