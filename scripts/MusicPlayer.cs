using Godot;

public partial class MusicPlayer : AudioStreamPlayer
{
    public override void _Ready()
    {
        Finished += OnLoopFinish;
    }

    private void OnLoopFinish()
    {
        Play();
        GetChild<AudioStreamPlayer>(0).Play();
    }
}
