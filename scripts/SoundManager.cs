using Godot;

public partial class SoundManager : Node
{
    public static SoundManager Instance;
    
    [Export] public float globalVolumeOffset = -5.0f;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void PlaySound(string soundName)
    {
        AudioStreamPlayer soundPlayer = ResourceLoader.Load<PackedScene>("res://scenes/sound.tscn").Instantiate<AudioStreamPlayer>();
        soundPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://sounds/" + soundName + ".wav");
        soundPlayer.Finished += soundPlayer.QueueFree;
        soundPlayer.VolumeDb += globalVolumeOffset;
        GetTree().Root.AddChild(soundPlayer);
    }
}
