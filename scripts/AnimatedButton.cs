using Godot;

public partial class AnimatedButton : TextureButton
{
    [Export] private TextureRect border;

    private bool shouldAlphaIncrease = true;
    private float alpha = 0.0f;
    
    [Export] public float maxAlpha = 0.5f;
    [Export] public float fadeSpeed = 1.0f;

    public override void _Process(double delta)
    {
        if (IsPressed())
        {
            SelfModulate = new Color(0.9f, 0.9f, 0.9f);
        }
        else
        {
            SelfModulate = new Color(1.0f, 1.0f, 1.0f);
        }
        
        
        if (IsHovered() && !Disabled)
        {
            // Determine whether to increase or decrease alpha
            if (shouldAlphaIncrease && alpha >= maxAlpha)
            {
                shouldAlphaIncrease = false;
            }
            else if (!shouldAlphaIncrease && alpha <= 0.0f)
            {
                shouldAlphaIncrease = true;
            }

            // Smoothly change alpha value toward target
            alpha = Mathf.MoveToward(alpha, shouldAlphaIncrease ? maxAlpha : 0.0f, (float)delta * fadeSpeed);
        }
        else
        {
            // Fade out the border when not hovered
            alpha = Mathf.MoveToward(alpha, 0.0f, (float)delta * fadeSpeed);
        }

        // Apply the updated alpha value to the border's modulation
        if (border != null) // Ensure border is assigned
        {
            border.SelfModulate = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
        else
        {
            SelfModulate = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }
}