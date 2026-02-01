# Spin a Rayani - Modernized Edition

Diese Version von **Spin a Rayani** ist eine modernisierte Neuauflage des ursprünglichen Projekts. Sie behält die grundlegende Logik und Struktur bei, setzt jedoch auf moderne Technologien und Architekturmuster.

## 🚀 Neuerungen & Verbesserungen

- **Architektur**: Umstellung auf **MVVM (Model-View-ViewModel)** mit dem CommunityToolkit.Mvvm.
- **UI/UX**: Modernes Design basierend auf **WPF-UI** (Fluent Design), das sich nahtlos in Windows 10/11 einfügt.
- **Wartbarkeit**: Klare Trennung von Logik (Core) und Darstellung (UI) durch Dependency Injection.
- **Performance**: Asynchrone Programmierung (async/await) für eine flüssige Benutzeroberfläche.
- **Code-Stil**: Verwendung moderner C# 12 Features und sauberer Namenskonventionen.

## 📂 Projektstruktur

```text
Spin-a-Rayani-Modern/
├── assets/                 # Bilder, Icons und andere Medien
├── docs/                   # Dokumentation (übernommen vom Original)
├── src/
│   ├── SpinARayani.Core/   # Geschäftslogik, Models, Interfaces
│   └── SpinARayani.UI.WPF/ # WPF-Anwendung, Views, ViewModels
└── SpinARayani.sln         # Visual Studio Solution
```

## 🛠️ Build & Start

1. Öffne die `SpinARayani.sln` in Visual Studio 2022.
2. Stelle sicher, dass das .NET 8 SDK installiert ist.
3. Stelle `SpinARayani.UI.WPF` als Startprojekt ein.
4. Drücke `F5` zum Starten.

## 🔄 Migration für Entwickler

Wenn du vom ursprünglichen Repository kommst, findest du hier die Entsprechungen:

| Original (WinForms) | Modern (WPF/MVVM) | Beschreibung |
|---------------------|-------------------|--------------|
| `MainForm.cs` | `MainWindow.xaml` / `MainViewModel.cs` | Hauptfenster und Logik |
| `Models/*.cs` | `SpinARayani.Core/Models/*.cs` | Datenmodelle (jetzt mit ObservableProperty) |
| `Services/GameManager.cs` | `SpinARayani.Core/Services/GameService.cs` | Zentrale Spielsteuerung |
| `Assets/` | `assets/images/` | Medien-Assets (als Ressourcen eingebunden) |

## 📝 Hinweise

Einige Funktionen wie das Multiplayer-Sync via Supabase müssen im neuen `GameService` noch final implementiert werden. Die Struktur dafür ist bereits vorbereitet.

---
*Basierend auf dem Originalprojekt von N-Wachs.*
