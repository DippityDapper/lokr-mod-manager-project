using System.IO;
using Godot;

public partial class Main : Node
{
    public static Main Instance;
    
    [Export] public Camera2D camera;
    [Export] public DownloadPanel DownloadPanel;
    [Export] public MainPanel MainPanel;
    [Export] public SettingsPanel SettingsPanel;
    [Export] public HeroBook HeroBookPanel;
    
    [Export] public Node2D ToDownloadPanel;
    [Export] public Node2D ToMainPanel;
    [Export] public Node2D ToSettingsPanel;
    [Export] public Node2D ToHeroBookPanel;
    
    private Tween positionTween;
    private Tween zoomTween;
    private Control currentPanel;

    public static bool modInstalled;

    public override void _EnterTree()
    {
        Instance = this;
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "LOKR");
        modInstalled = Directory.Exists(path);
    }

    public override void _Ready()
    {
        SettingsPanel.Hide();
        DownloadPanel.Hide();
        GoToMainPanel();
        
        MainPanel.ExitButton.Pressed += () =>
        {
            GetTree().Quit();
            SoundManager.Instance.PlaySound("back_pressed");
        };
    }

    private void SwitchToPanel(Node2D targetPosition, Control panelToShow, Vector2 zoomTarget, float duration = 1.0f)
    {
        positionTween?.Stop();
        zoomTween?.Stop();

        positionTween = CreateTween().SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        zoomTween = CreateTween().SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);

        positionTween.TweenProperty(camera, "global_position", targetPosition.GlobalPosition, duration);
        zoomTween.TweenProperty(camera, "zoom", zoomTarget, duration);
        
        positionTween.Finished += () =>
        {
            currentPanel?.Hide();
            currentPanel = panelToShow;
            currentPanel.Show();
        };

        panelToShow.Show();
    }

    public void GoToMainPanel()
    {
        SwitchToPanel(ToMainPanel, MainPanel, new Vector2(0.8f, 0.8f));
    }

    public void GoToDownloadPanel()
    {
        SwitchToPanel(ToDownloadPanel, DownloadPanel, new Vector2(0.95f, 0.95f));
    }

    public void GoToSettingsPanel()
    {
        SwitchToPanel(ToSettingsPanel, SettingsPanel, new Vector2(0.95f, 0.95f));
    }
    
    public void GoToHeroBookPanel()
    {
        SwitchToPanel(ToHeroBookPanel, HeroBookPanel, new Vector2(0.95f, 0.95f));
    }
}
