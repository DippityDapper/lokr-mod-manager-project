using Godot;
using System;

public partial class Cursor : Sprite2D
{
    [Export] private Texture2D cursorTexture;
    [Export] private Texture2D cursorDownTexture;
    [Export] private Marker2D ParticleSpawnLocation;
    [Export] private AudioStreamPlayer[] sounds;

    private Vector2 previousMousePosition;
    public static Vector2 Velocity { get; private set; } = Vector2.Zero;
    public static bool withinPanel = false;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        Texture = cursorTexture;
        previousMousePosition = GetGlobalMousePosition();
    }

    public override void _Process(double delta)
    {
        Vector2 currentMousePosition = GetGlobalMousePosition();
        Velocity = (currentMousePosition - previousMousePosition) / (float)delta;
        previousMousePosition = currentMousePosition;

        GlobalPosition = currentMousePosition + new Vector2(24, 24);
        
        if (Input.IsActionJustPressed("click"))
        {
            Texture = cursorDownTexture;
            if (withinPanel)
            {
                GpuParticles2D particles = ResourceLoader.Load<PackedScene>("res://scenes/click_particles.tscn").Instantiate<GpuParticles2D>();
                GetTree().Root.AddChild(particles);
                particles.Emitting = true;
                particles.GlobalPosition = ParticleSpawnLocation.GlobalPosition;
                particles.Finished += () => particles.QueueFree();
                sounds[new Random().Next() % sounds.Length].Play();
            }
        }
        else if (Input.IsActionJustReleased("click"))
        {
            Texture = cursorTexture;
        }
    }
}