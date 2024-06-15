using BossMod.Autorotation;

namespace UIDev;

class AutorotTest : TestWindow
{
    private readonly PresetDatabase _db = new(new("E:\\sources\\ff14\\BossMod\\test"));

    public AutorotTest() : base("Autorot", new(1000, 1000), ImGuiNET.ImGuiWindowFlags.None)
    {
    }

    public override void Draw()
    {
        _db.Editor.Draw();
    }
}
