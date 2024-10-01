using BossMod;
using ImGuiNET;

namespace UIDev;

class BitmapEditorTest : TestWindow
{
    private readonly UIBitmapEditor _editor;

    public BitmapEditorTest() : base("Bitmap editor test", new(1500, 1500), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        var init = new Bitmap(16, 16);
        for (int y = 0; y < init.Height; ++y)
            for (int x = 0; x < init.Width; ++x)
                init[x, y] = (x >> 1) == 1 || (y >> 1) == 1;
        _editor = new(init);
    }

    public override void Draw()
    {
        _editor.Draw();
    }
}
