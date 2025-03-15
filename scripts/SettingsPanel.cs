using Godot;
using System.IO;
using Godot.Collections;

public partial class SettingsPanel : Panel
{
    [Export] public TextureButton ExitButton;
    
    [Export] public CheckButton SkipSplash;
    [Export] public CheckButton DebugMode;
    [Export] public CheckButton TakeOverAiButton;
    [Export] public CheckButton FullscreenButton;

    public override void _Ready()
    {
        base._Ready();
        ExitButton.Pressed += OnExitButtonPressed;
        
        SkipSplash.Toggled += OnSkipSplashToggled;
        DebugMode.Toggled += OnDebugModeToggled;
        TakeOverAiButton.Toggled += OnAIModeToggled;
        FullscreenButton.Toggled += OnFullscreenToggled;

        Main.Instance.DownloadPanel.ModInstalled += InitSettings;
        Main.Instance.DownloadPanel.ModUninstalled += InitSettings;
        
        InitSettings();
        
        bool isFullscreen = GetSettings()["fullscreen"];
        if (isFullscreen) DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }

    private void InitSettings()
    {
        Dictionary<string, bool> boolProperties = GetSettings();
        InitBoolSettings(boolProperties);
    }

    private void InitBoolSettings(Dictionary<string, bool> boolProperties)
    {
        if (boolProperties.ContainsKey("skip_splash_screen"))
        {
            SkipSplash.SetPressedNoSignal(boolProperties["skip_splash_screen"]);
        }
        if (boolProperties.ContainsKey("debug_mode"))
        {
            DebugMode.SetPressedNoSignal(boolProperties["debug_mode"]);
        }
        if (boolProperties.ContainsKey("take_over_ai"))
        {
            TakeOverAiButton.SetPressedNoSignal(boolProperties["take_over_ai"]);
        }
        if (boolProperties.ContainsKey("fullscreen"))
        {
            FullscreenButton.SetPressedNoSignal(boolProperties["fullscreen"]);
        }
    }

    private Dictionary<string, bool> GetSettings()
    {
        Dictionary<string, bool> properties = new Dictionary<string, bool>();
        string propertiesPath = Path.Combine(DownloadPanel.GetModFolderPath(), "ModManager", "properties.txt");
        
        if (File.Exists(propertiesPath))
        {
            SkipSplash.Show();
            DebugMode.Show();
            TakeOverAiButton.Show();
            
            AddProperties(properties, propertiesPath);
        }
        else
        {
            SkipSplash.Hide();
            DebugMode.Hide();
            TakeOverAiButton.Hide();
        }
        
        propertiesPath = Path.Combine(ProjectSettings.GlobalizePath("user://settings/settings.txt"));
        if (File.Exists(propertiesPath))
        {
            AddProperties(properties, propertiesPath);
        }
        else
        {
            Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://settings"));
            File.WriteAllText(propertiesPath, "fullscreen=false");
            AddProperties(properties, propertiesPath);
        }
        
        return properties;
    }

    private void AddProperties(Dictionary<string, bool> properties, string propertiesPath)
    {
        string[] lines = File.ReadAllLines(propertiesPath);
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('=');
            string propertyName = parts[0].Trim();
                
            if (bool.TryParse(parts[1].Trim(), out var value))
            {
                properties.Add(propertyName, value);
            }
        }
    }

    private void SetProperty(string propertyName, bool value)
    {
        string propertiesPath = Path.Combine(DownloadPanel.GetModFolderPath(), "ModManager", "properties.txt");
        
        if (File.Exists(propertiesPath))
        {
            string[] lines = File.ReadAllLines(propertiesPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(propertyName))
                {
                    lines[i] = propertyName + "=" + value;
                    break;
                }
            }
            File.WriteAllLines(propertiesPath, lines);
        }
        
        propertiesPath = Path.Combine(ProjectSettings.GlobalizePath("user://settings/settings.txt"));
        
        if (File.Exists(propertiesPath))
        {
            string[] lines = File.ReadAllLines(propertiesPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(propertyName))
                {
                    lines[i] = propertyName + "=" + value;
                    break;
                }
            }
            File.WriteAllLines(propertiesPath, lines);
        }
    }

    private void OnSkipSplashToggled(bool state)
    {
        SetProperty("skip_splash_screen", state);
    }

    private void OnDebugModeToggled(bool state)
    {
        SetProperty("debug_mode", state);
    }
    
    private void OnAIModeToggled(bool state)
    {
        SetProperty("take_over_ai", state);
    }

    private void OnFullscreenToggled(bool state)
    {
        SetProperty("fullscreen", state);
        if (state) DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }
    
    private void OnExitButtonPressed()
    {
        Main.Instance.GoToMainPanel();
        SoundManager.Instance.PlaySound("back_pressed");
    }
}
