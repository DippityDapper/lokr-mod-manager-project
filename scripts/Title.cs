using Godot;

public partial class Title : TextureRect
{
    [Export] private float moveAmplitude = 0.02f;
    [Export] private float moveSpeed = 0.9f;
    [Export] private float rotationAmplitude = 0.001f;
    [Export] private float rotationSpeed = 0.7f;
    
    [Export] private TextureRect Shadow;
    
    [Export] private Texture2D BannerOn;
    [Export] private Texture2D BannerOff;

    private Vector2 originalMargins;
    private float time = 0.0f;
    
    public override void _Ready()
    {
        originalMargins = new Vector2(AnchorTop, AnchorBottom);
        Main.Instance.DownloadPanel.ModInstalled += OnModInstalled;
        Main.Instance.DownloadPanel.ModUninstalled += OnModUninstalled;
        
        if (Main.modInstalled)
        {
            OnModInstalled();
        }
        else
        {
            OnModUninstalled();
        }
    }

    private void OnModInstalled()
    {
        Texture = BannerOn;
        Shadow.Texture = BannerOn;
        moveAmplitude = 0.02f;
        moveSpeed = 0.9f;
        rotationAmplitude = 0.001f;
        rotationSpeed = 0.7f;
    }
    
    private void OnModUninstalled()
    {
        Texture = BannerOff;
        Shadow.Texture = BannerOff;
        moveAmplitude = 0.01f;
        moveSpeed = 0.6f;
        rotationAmplitude = 0.0007f;
        rotationSpeed = 0.4f;
    }

    public override void _Process(double delta)
    {
        time += (float)delta;
        
        float verticalOffset = Mathf.Sin(time * moveSpeed) * moveAmplitude;
        
        AnchorTop = originalMargins.X + verticalOffset;
        AnchorBottom = originalMargins.Y + verticalOffset;
        
        float rotationOffset = Mathf.Sin(time * rotationSpeed) * Mathf.RadToDeg(rotationAmplitude);
        Rotation = rotationOffset;
    }
}