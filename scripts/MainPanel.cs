using Godot;

public partial class MainPanel : Panel
{
    [Export] public HeroBook HeroBook;
    
    [Export] public TextureButton ExitButton;
    [Export] public TextureButton DownloadButton;
    [Export] public TextureButton SettingsButton;
    
    [Export] public Button NecromancerPageButton;
    [Export] public Button EnchantressPageButton;
    [Export] public Button ClericPageButton;
    
    public override void _Ready()
    {
        base._Ready();
        DownloadButton.Pressed += () =>
        {
            Main.Instance.GoToDownloadPanel();
            SoundManager.Instance.PlaySound("button_pressed");
        };
        SettingsButton.Pressed += () =>
        {
            Main.Instance.GoToSettingsPanel();
            SoundManager.Instance.PlaySound("button_pressed");
        };
        NecromancerPageButton.Pressed += () =>
        {
            HeroBook.GoToPage("Necromancer");
            GoToHeroBookPanel();
        };
        EnchantressPageButton.Pressed += () =>
        {
            HeroBook.GoToPage("Enchantress");
            GoToHeroBookPanel();
        };
        ClericPageButton.Pressed += () =>
        {
            HeroBook.GoToPage("Cleric");
            GoToHeroBookPanel();
        };
    }

    private void GoToHeroBookPanel()
    {
        Main.Instance.GoToHeroBookPanel();
        SoundManager.Instance.PlaySound("poster_pressed");
    }
}
