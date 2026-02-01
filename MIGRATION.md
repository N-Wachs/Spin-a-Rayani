# Migrationsleitfaden für Entwickler

Dieses Dokument hilft Entwicklern, die mit dem ursprünglichen „Spin a Rayani“-Repository vertraut sind, sich im neuen modernisierten Projekt zurechtzufinden.

## Architektur-Änderungen

| Konzept | Alt (WinForms) | Neu (WPF/MVVM) | Grund |
|---------|----------------|----------------|-------|
| **UI-Framework** | Windows Forms | WPF mit WPF-UI | Modernes Design, bessere Skalierbarkeit |
| **Pattern** | Code-Behind (MainForm.cs) | MVVM (MainViewModel.cs) | Trennung von Logik und UI |
| **Dependency Injection** | Manuelle Instanziierung | Microsoft.Extensions.DependencyInjection | Bessere Testbarkeit und Wartbarkeit |
| **Datenbindung** | Manuelle UI-Updates | WPF Data Binding | Weniger Boilerplate-Code |
| **Asynchronität** | Teilweise (Task.Run) | Durchgängig async/await | Flüssigere UI |

## Wo finde ich was?

### Logik & Daten
- **Original**: `Models/`, `Services/` im Hauptverzeichnis.
- **Neu**: `src/SpinARayani.Core/Models/` und `src/SpinARayani.Core/Services/`.
- *Hinweis*: Die Logik wurde in eine separate Klassenbibliothek ausgelagert, um sie plattformunabhängig zu machen.

### Benutzeroberfläche
- **Original**: `Forms/Main/MainForm.cs`, `Forms/Dialogs/*.cs`.
- **Neu**: `src/SpinARayani.UI.WPF/Views/*.xaml` (Layout) und `src/SpinARayani.UI.WPF/ViewModels/*.cs` (Logik).

### Assets
- **Original**: `Assets/`.
- **Neu**: `assets/images/` (Physisch) und `src/SpinARayani.UI.WPF/Resources/` (WPF-Ressourcen).

## Code-Beispiel: Zugriff auf PlayerStats

**Alt:**
```csharp
// In MainForm.cs
lblMoney.Text = _gameManager.Stats.Money.ToString();
```

**Neu:**
```xml
<!-- In MainWindow.xaml -->
<TextBlock Text="{Binding Stats.Money}" />
```
Die UI aktualisiert sich automatisch, sobald sich der Wert im ViewModel ändert (dank `ObservableProperty`).

## Nächste Schritte für Contributor
1. Schau dir `App.xaml.cs` an, um zu sehen, wie Services registriert werden.
2. Öffne `MainViewModel.cs`, um die Interaktionslogik zu verstehen.
3. Erweitere `MainWindow.xaml`, um neue UI-Elemente hinzuzufügen.
