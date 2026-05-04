namespace Bombi.ServerInstance.Game;

public sealed class InputSnapshot
{
    public int ServerTick { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Action { get; set; }
}