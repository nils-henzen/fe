# Fe Chat - Windows Desktop Client

Eine leichtgewichtige Windows Desktop Chat-Anwendung mit Avalonia UI.

## Features

- **System Tray Integration**: Läuft im Hintergrund mit Tray-Icon
- **Automatisches Polling**: Prüft periodisch auf neue Nachrichten (alle 30 Sekunden)
- **Windows Benachrichtigungen**: Zeigt Benachrichtigungen bei neuen Nachrichten
- **Minimale UI**: Einfaches Compose-Fenster zum Senden von Nachrichten
- **Nachrichten-Viewer**: Übersicht aller empfangenen Nachrichten
- **Ressourcenschonend**: Minimaler RAM- und CPU-Verbrauch

## Anforderungen

- .NET 8.0 SDK oder höher
- Windows 10/11

## Installation & Build

### Standard Build

```cmd
dotnet restore
dotnet build
```

### Release Build (Optimiert)

```cmd
dotnet build -c Release
```

### Single-File Executable (Empfohlen)

```cmd
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Die fertige `.exe` findet sich in: `bin\Release\net8.0\win-x64\publish\FeChat.exe`

### Self-Contained Build (Keine .NET Installation nötig)

```cmd
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true
```

## Konfiguration

Die Anwendung erstellt automatisch eine Konfigurationsdatei unter:
`%AppData%\FeChat\config.json`

Beispiel-Konfiguration:

```json
{
  "server_ip": "127.0.0.1",
  "server_port": 5000,
  "user_id": "user1",
  "signature": "default_signature"
}
```

### Polling-Intervall ändern

Im Code in `TrayManager.cs` kann das Polling-Intervall angepasst werden:

```csharp
public int PollingIntervalSeconds { get; set; } = 30; // Standard: 30 Sekunden
```

## Verwendung

### Erste Schritte

1. Stelle sicher, dass der Fe Chat Server läuft
2. Starte `FeChat.exe`
3. Die Anwendung erscheint im System Tray (rechts unten in der Taskleiste)

### Funktionen

**Rechtsklick auf Tray-Icon öffnet Kontextmenü:**

- **Check Messages**: Prüft sofort auf neue Nachrichten
- **View Messages**: Öffnet Nachrichten-Übersicht
- **Compose Message**: Öffnet Fenster zum Senden einer Nachricht
- **Settings**: Zeigt aktuelle Einstellungen
- **Exit**: Beendet die Anwendung

### Nachricht senden

1. Rechtsklick auf Tray-Icon → "Compose Message"
2. Empfänger-ID eingeben
3. Nachricht verfassen
4. "Send" klicken

## Ressourcen-Optimierung

### Tipps für minimalen Ressourcenverbrauch:

1. **Polling-Intervall erhöhen**: Längere Intervalle = weniger CPU-Last
   ```csharp
   public int PollingIntervalSeconds { get; set; } = 60; // 60 Sekunden
   ```

2. **Trimmed Build verwenden**: Reduziert Dateigröße und Speicherverbrauch
   ```cmd
   dotnet publish -p:PublishTrimmed=true
   ```

3. **ReadyToRun deaktivieren**: Bei sehr kleinen Anwendungen kann dies helfen
   ```cmd
   dotnet publish -p:PublishReadyToRun=false
   ```

4. **Garbage Collection optimieren**: In `Program.cs` hinzufügen:
   ```csharp
   GCSettings.LatencyMode = GCLatencyMode.Batch;
   ```

### Typische Ressourcennutzung:

- **RAM**: ~25-40 MB im Leerlauf
- **CPU**: <1% im Hintergrund, kurze Spitzen beim Polling
- **Dateigröße**: ~15-20 MB (single-file), ~150 MB (self-contained)

## Architektur

### Dateistruktur

```
FeChat/
├── Program.cs              # Entry Point
├── App.axaml              # Avalonia App Definition
├── App.axaml.cs           # App Lifecycle
├── TrayManager.cs         # System Tray Logic & Polling
├── ApiClient.cs           # REST API Client
├── ConfigManager.cs       # Configuration Management
├── ComposeWindow.axaml    # UI für Nachricht senden
├── ComposeWindow.axaml.cs # Logic für Compose Window
├── MessagesWindow.axaml   # UI für Nachrichten-Übersicht
└── MessagesWindow.axaml.cs # Logic für Messages Window
```

### Design-Prinzipien

- **Minimalistisch**: Nur notwendige Dependencies
- **Leichtgewichtig**: Avalonia UI ist bereits schlank
- **Wartbar**: Klare Trennung von UI und Logic
- **Erweiterbar**: Einfach neue Features hinzufügen

## Erweiterungsmöglichkeiten

### Datei-Upload hinzufügen

In `ApiClient.cs`:

```csharp
public async Task<bool> SendFileAsync(string receiverId, string filePath)
{
    var config = _configManager.GetConfig();
    var fileData = Convert.ToBase64String(File.ReadAllBytes(filePath));
    
    var payload = new
    {
        signature = config.Signature,
        sender_id = config.UserId,
        receiver_id = receiverId,
        file_name = Path.GetFileName(filePath),
        file_type = "application/octet-stream",
        file_content = fileData
    };

    var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/send_file", payload);
    return response.IsSuccessStatusCode;
}
```

### Chat-Historie speichern

In `TrayManager.cs` eine lokale SQLite-Datenbank hinzufügen.

### Desktop-Benachrichtigungen verbessern

Native Windows Toast Notifications verwenden:
- NuGet: `Microsoft.Toolkit.Uwp.Notifications`

### Dark Mode

In `App.axaml`:

```xaml
<FluentTheme Mode="Dark"/>
```

## Troubleshooting

### App startet nicht

- Prüfe ob .NET 8.0 Runtime installiert ist
- Prüfe Logs in `%AppData%\FeChat\`

### Keine Verbindung zum Server

- Prüfe ob Server läuft
- Prüfe `config.json` Einstellungen
- Prüfe Firewall-Einstellungen

### Hoher Ressourcenverbrauch

- Erhöhe Polling-Intervall
- Verwende Trimmed Build
- Schließe ungenutzte Fenster

## Lizenz

Entspricht der Lizenz des Fe Chat Projekts.
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
    <ApplicationIcon>app.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <AvaloniaResource Include="Assets\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.10" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.10" />
    <PackageReference Include="Avalonia.Controls.DataGrid" Version="11.0.10" />
  </ItemGroup>

</Project>

