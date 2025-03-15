using Godot;

public partial class Panel : Control
{
    public bool isMouseOver = false;

    public override void _Ready()
    {
        MouseEntered += OnBodyMouseEnter;
        MouseExited += OnBodyMouseExit;
    }

    private void OnBodyMouseEnter()
    {
        Cursor.withinPanel = true;
    }

    private void OnBodyMouseExit()
    {
        Cursor.withinPanel = false;
    }
}
