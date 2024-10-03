using BossMod;
using ImGuiNET;

namespace UIDev;

class BitmapEditorTest : TestWindow
{
    class Editor(Bitmap initial) : UIBitmapEditor(initial)
    {
        protected override void DrawSidebar()
        {
            base.DrawSidebar();

            ImGui.TextUnformatted($"Size: {Bitmap.Width}x{Bitmap.Height}, Brush radius: {BrushRadius}");

            if (ImGui.Button("+1 on left"))
            {
                var newBitmap = new Bitmap(Bitmap.Width + 1, Bitmap.Height, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution);
                Bitmap.FullRegion.CopyTo(newBitmap, 1, 0);
                CheckpointNoClone(newBitmap);
            }

            if (ImGui.Button("upscale"))
            {
                var newBitmap = new Bitmap(Bitmap.Width * 2, Bitmap.Height * 2, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution * 2);
                Bitmap.FullRegion.UpsampleTo(newBitmap, 0, 0);
                CheckpointNoClone(newBitmap);
            }

            if (ImGui.Button("downscale"))
            {
                var newBitmap = new Bitmap(Bitmap.Width / 2, Bitmap.Height / 2, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution / 2);
                Bitmap.FullRegion.DownsampleTo(newBitmap, 0, 0, true);
                CheckpointNoClone(newBitmap);
            }

            if (ImGui.Button("save"))
            {
                Bitmap.Save("D:\\test.bmp");
            }

            if (ImGui.Button("load"))
            {
                CheckpointNoClone(new("D:\\test.bmp"));
            }
        }
    }

    private readonly Editor _editor;

    public BitmapEditorTest() : base("Bitmap editor test", new(1500, 1500), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        var init = new Bitmap(16, 16, new(0), Color.FromComponents(255, 127, 0));
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
