using Godot;

public partial class Poster : TextureRect
{
    [Export] private float swayStrength = 1f;
    [Export] private float maxRotation = 0.35f;
    [Export] private float damping = 2.0f;
    [Export] private float springForce = 20.0f;

    private float rotationVelocity = 0.0f;
    private float targetRotation = 0.0f;
    private bool isMouseEntered = false;
    
    private float originalRotation;

    public override void _Ready()
    {
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        
        originalRotation = Rotation;
        targetRotation = Rotation;
    }

    public override void _Process(double delta)
    {
        if (isMouseEntered)
        {
            Vector2 cursorVelocity = -Cursor.Velocity;
        
            if (cursorVelocity.Length() > 400.0f)
            {
                targetRotation = Mathf.Clamp(cursorVelocity.X * swayStrength, -maxRotation, maxRotation);
                targetRotation += originalRotation;
            }
            else
            {
                targetRotation = originalRotation;
            }
        }
        
        float rotationDifference = targetRotation - Rotation;
        float springAcceleration = rotationDifference * springForce;
        float dampingForce = -rotationVelocity * damping;
        float acceleration = springAcceleration + dampingForce;
        
        rotationVelocity += acceleration * (float)delta;
        Rotation += rotationVelocity * (float)delta;
        
        if (Mathf.Abs(rotationVelocity) < 0.001f && Mathf.Abs(rotationDifference) < 0.001f)
        {
            rotationVelocity = 0.0f;
            Rotation = targetRotation;
        }
    }

    private void OnMouseEnter()
    {
        isMouseEntered = true;
    }

    private void OnMouseExit()
    {
        isMouseEntered = false;
        targetRotation = originalRotation;
    }
}
