using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility.Numerics;
using Dalamud.Bindings.ImGui;

namespace BossMod;

// a 'simple' bitmap editor utility
public class UIBitmapEditor
{
    private readonly List<Bitmap> _bitmaps; // undo-redo stack; this is not terribly efficient, but oh well
    private int _curUndoPos;
    private bool _paintInProgress;

    private readonly List<string> _modeNames = [];

    public readonly int PanModeId;
    public readonly int BrushModeId;
    public readonly int EraseModeId;

    public Vector2 ScreenSize = new(600, 600); // size of the 'viewport' in screen space
    public Vector2 ScreenOffset; // top-level coordinate of the virtual 'viewport' in screen space coordinates (scaled by zoom level compared to bitmap coordinates)
    public int ZoomLevel = 4; // 0 is 1:1 screen to bitmap, positive if bitmap pixel is bigger than screen pixel (upscaled bitmap), negative otherwise
    public int CurrentMode;
    public float BrushRadius = 1;
    public (int x, int y) HoveredPixel;

    public Bitmap Bitmap => _bitmaps[_curUndoPos];

    public UIBitmapEditor(Bitmap initial)
    {
        _bitmaps = [initial.Clone()];
        PanModeId = RegisterMode("Pan");
        BrushModeId = RegisterMode("Brush");
        EraseModeId = RegisterMode("Erase");
        CurrentMode = PanModeId;
    }

    public void Draw()
    {
        using var table = ImRaii.Table("table", 2);
        if (!table)
            return;

        ImGui.TableSetupColumn("Map", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoClip, ScreenSize.X);
        ImGui.TableSetupColumn("Control");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        DrawGrid();
        ImGui.TableNextColumn();
        DrawSidebar();
    }

    public bool CanUndo() => _curUndoPos > 0;
    public bool CanRedo() => _curUndoPos < _bitmaps.Count - 1;
    public void Undo() => _curUndoPos = Math.Max(0, _curUndoPos - 1);
    public void Redo() => _curUndoPos = Math.Min(_bitmaps.Count - 1, _curUndoPos + 1);
    public void Checkpoint() => Checkpoint(_bitmaps[_curUndoPos]);
    public void Checkpoint(Bitmap image) => CheckpointNoClone(image.Clone());

    protected int RegisterMode(string name)
    {
        _modeNames.Add(name);
        return _modeNames.Count;
    }

    // note: assumes newState has no other references
    protected void CheckpointNoClone(Bitmap newState)
    {
        if (_curUndoPos < _bitmaps.Count - 1)
            _bitmaps.RemoveRange(_curUndoPos + 1, _bitmaps.Count - _curUndoPos - 1);
        _bitmaps.Add(newState);
        ++_curUndoPos;
    }

    protected virtual void DrawSidebar()
    {
        DrawModeButtons();
        DrawUndoRedoButtons();
        UIMisc.HelpMarker("Wheel to zoom, shift-wheel to change brush size");
        ImGui.SameLine();
        ImGui.TextUnformatted($"Brush radius: {BrushRadius}");
        if (HoveredPixel.x >= 0 && HoveredPixel.y >= 0)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"Hovered pixel: {HoveredPixel.x}x{HoveredPixel.y}");
        }
    }

    protected virtual IEnumerable<(int x, int y, Color c)> HighlighedCells() => [];

    protected void DrawModeButtons()
    {
        for (int i = 0; i < _modeNames.Count; ++i)
        {
            var active = CurrentMode == i + 1;
            using var color = ImRaii.PushColor(ImGuiCol.Button, 0xff008080, active);
            if (ImGui.Button(_modeNames[i]))
                CurrentMode = active ? 0 : i + 1;
            ImGui.SameLine();
        }
        ImGui.NewLine();
    }

    protected void DrawUndoRedoButtons()
    {
        using (ImRaii.Disabled(!CanUndo()))
            if (ImGui.Button("Undo"))
                Undo();
        ImGui.SameLine();
        using (ImRaii.Disabled(!CanRedo()))
            if (ImGui.Button("Redo"))
                Redo();
    }

    private void DrawGrid()
    {
        var tl = ImGui.GetCursorScreenPos();
        var br = tl + ScreenSize;
        var dl = ImGui.GetWindowDrawList();
        var mouseOffset = ImGui.GetIO().MousePos - tl;

        ImGui.InvisibleButton("###grid", ScreenSize);
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 0))
            HandleDrag(mouseOffset, ImGui.GetIO().MouseDelta);
        else
            _paintInProgress = false;

        if (ImGui.IsItemHovered() && ImGui.GetIO().MouseWheel is var wheel && wheel != 0)
            HandleWheel(wheel, mouseOffset);

        ImGui.PushClipRect(tl, br, false);

        var bitmapToScreenScale = MathF.Pow(2, ZoomLevel);
        var screenToBitmapScale = 1.0f / bitmapToScreenScale;
        var bitmapTL = ScreenOffset * screenToBitmapScale;
        var bitmapBR = (ScreenOffset + ScreenSize) * screenToBitmapScale;
        var x0 = Math.Max(0, (int)bitmapTL.X);
        var y0 = Math.Max(0, (int)bitmapTL.Y);
        var x1 = Math.Min(Bitmap.Width, (int)MathF.Ceiling(bitmapBR.X) + 1);
        var y1 = Math.Min(Bitmap.Height, (int)MathF.Ceiling(bitmapBR.Y) + 1);
        var numBitmapPixelsPerScreenPixel = Math.Max(1, (int)screenToBitmapScale);
        var numScreenPixelsPerBitmapPixel = Math.Max(1, (int)bitmapToScreenScale);
        var screenX0 = x0 * bitmapToScreenScale - ScreenOffset.X;
        var screenY0 = y0 * bitmapToScreenScale - ScreenOffset.Y;
        var pixelWeight = 1.0f / (numBitmapPixelsPerScreenPixel * numBitmapPixelsPerScreenPixel);

        HoveredPixel = (-1, -1);
        if (mouseOffset.X >= 0 && mouseOffset.Y >= 0 && mouseOffset.X < ScreenSize.X && mouseOffset.Y < ScreenSize.Y)
        {
            var c = (mouseOffset + ScreenOffset) / numScreenPixelsPerBitmapPixel;
            var x = (int)MathF.Floor(c.X);
            var y = (int)MathF.Floor(c.Y);
            if (x >= 0 && y >= 0 && x < Bitmap.Width && y < Bitmap.Height)
                HoveredPixel = (x, y);
        }

        var c0 = Bitmap.Color0.ToFloat4();
        var c1 = Bitmap.Color1.ToFloat4();
        for (int y = y0; y < y1; y += numBitmapPixelsPerScreenPixel)
        {
            var corner = tl + new Vector2(screenX0, screenY0);
            for (int x = x0; x < x1; x += numBitmapPixelsPerScreenPixel)
            {
                var cornerEnd = corner + new Vector2(numScreenPixelsPerBitmapPixel);
                var cellTL = Vector2.Max(tl, corner);
                var cellBR = Vector2.Min(br, cornerEnd);
                if (cellTL.X < cellBR.X && cellTL.Y < cellBR.Y)
                {
                    float opacity = 0;
                    var subXMax = Math.Min(x1, x + numBitmapPixelsPerScreenPixel);
                    var subYMax = Math.Min(y1, y + numBitmapPixelsPerScreenPixel);
                    for (int sy = y; sy < subYMax; ++sy)
                        for (int sx = x; sx < subXMax; ++sx)
                            if (Bitmap[sx, sy])
                                opacity += pixelWeight;
                    var color = Vector4.Lerp(c0, c1, opacity);
                    dl.AddRectFilled(cellTL, cellBR, Color.FromFloat4(color).ABGR);
                }
                corner.X += numScreenPixelsPerBitmapPixel;
            }
            screenY0 += numScreenPixelsPerBitmapPixel;
        }

        void drawLine(Vector2 a, Vector2 b, uint color, int thickness)
        {
            if (a.X < tl.X && b.X < tl.X || a.Y < tl.Y && b.Y < tl.Y || a.X > br.X && b.X > br.X || a.Y > br.Y && b.Y > br.Y)
                return;
            a.X = Math.Clamp(a.X, tl.X, br.X);
            a.Y = Math.Clamp(a.Y, tl.Y, br.Y);
            b.X = Math.Clamp(b.X, tl.X, br.X);
            b.Y = Math.Clamp(b.Y, tl.Y, br.Y);
            dl.AddLine(a, b, color, thickness);
        }

        // border
        var borderA = tl - ScreenOffset;
        var borderB = new Vector2(borderA.X + bitmapToScreenScale * Bitmap.Width, borderA.Y);
        var borderC = new Vector2(borderA.X + bitmapToScreenScale * Bitmap.Width, borderA.Y + bitmapToScreenScale * Bitmap.Height);
        var borderD = new Vector2(borderA.X, borderA.Y + bitmapToScreenScale * Bitmap.Height);
        drawLine(borderA, borderB, 0xffffffff, 2);
        drawLine(borderB, borderC, 0xffffffff, 2);
        drawLine(borderC, borderD, 0xffffffff, 2);
        drawLine(borderD, borderA, 0xffffffff, 2);

        // grid
        if (ZoomLevel > 1)
        {
            for (int x = x0 + 1; x < x1; ++x)
            {
                var off = new Vector2(x * numScreenPixelsPerBitmapPixel, 0);
                drawLine(borderA + off, borderD + off, 0xffffffff, 1);
            }
            for (int y = y0 + 1; y < y1; ++y)
            {
                var off = new Vector2(0, y * numScreenPixelsPerBitmapPixel);
                drawLine(borderA + off, borderB + off, 0xffffffff, 1);
            }
        }

        // brush
        if ((CurrentMode == BrushModeId || CurrentMode == EraseModeId) && ImGui.IsItemHovered())
        {
            dl.AddCircle(tl + mouseOffset, BrushRadius * bitmapToScreenScale, 0xffff00ff);
        }

        // highlights
        if (numScreenPixelsPerBitmapPixel >= 3)
        {
            foreach (var (x, y, c) in HighlighedCells())
            {
                if (x >= x0 && x < x1 && y >= y0 && y < y1)
                {
                    dl.AddCircle(tl + new Vector2(x + 0.5f, y + 0.5f) * bitmapToScreenScale - ScreenOffset, (numScreenPixelsPerBitmapPixel >> 1) - 1, c.ABGR);
                }
            }
        }

        ImGui.PopClipRect();
    }

    private void HandleDrag(Vector2 end, Vector2 delta)
    {
        if (CurrentMode == PanModeId)
        {
            ScreenOffset -= delta;
        }
        else if (CurrentMode == BrushModeId || CurrentMode == EraseModeId)
        {
            if (!_paintInProgress)
            {
                Checkpoint();
                _paintInProgress = true;
            }
            var scale = MathF.Pow(0.5f, ZoomLevel);
            var virtualEnd = ScreenOffset + end + new Vector2(0.5f); // assume we're starting at clicked pixel center
            var p1 = (virtualEnd - delta) * scale;
            var p2 = virtualEnd * scale;
            var d = Vector2.Normalize(delta);
            var n = new Vector2(d.Y, -d.X);
            var brushHalfWidth = n * BrushRadius;
            var brushA = p1 + brushHalfWidth;
            var brushB = p2 + brushHalfWidth;
            var brushC = p2 - brushHalfWidth;
            var brushD = p2 - brushHalfWidth;
            // SAT test for rect/rect portion:
            // for X/Y axes, each pixel's projection is (x, x+1) / (y, y+1)
            // for D/N axes, each pixel's projection is (x,y) dot axis + (0,0,1,1) projection
            // brush projection is obviously constant
            var brushProjX = ProjectRectOnAxis(new(1, 0), brushA, brushB, brushC, brushD);
            var brushProjY = ProjectRectOnAxis(new(0, 1), brushA, brushB, brushC, brushD);
            var brushProjD = ProjectRectOnAxis(d, brushA, brushB, brushC, brushD);
            var brushProjN = ProjectRectOnAxis(n, brushA, brushB, brushC, brushD);
            var unitProjD = ProjectRectOnAxis(d, new(0, 0), new(0, 1), new(1, 0), new(1, 1));
            var unitProjN = ProjectRectOnAxis(n, new(0, 0), new(0, 1), new(1, 0), new(1, 1));

            // check whether brush rect intersects pixel (x to x+1), (y to y+1) (capsule excluding caps)
            bool intersectBrushRectPixel(int x, int y)
            {
                if (RangesDisjoint(brushProjX, (x, x + 1)) || RangesDisjoint(brushProjY, (y, y + 1)))
                    return false; // either X or Y is a separating axis
                var offD = Vector2.Dot(d, new(x, y));
                if (RangesDisjoint(brushProjD, (unitProjD.min + offD, unitProjD.max + offD)))
                    return false; // d is a separating axis
                var offN = Vector2.Dot(n, new(x, y));
                if (RangesDisjoint(brushProjN, (unitProjN.min + offN, unitProjN.max + offN)))
                    return false; // n is a separating axis
                return true; // no separating axis found
            }

            var x0 = Math.Max(0, (int)(Math.Min(p1.X, p2.X) - BrushRadius));
            var x1 = Math.Min(Bitmap.Width, (int)(Math.Max(p1.X, p2.X) + BrushRadius) + 1);
            var y0 = Math.Max(0, (int)(Math.Min(p1.Y, p2.Y) - BrushRadius));
            var y1 = Math.Min(Bitmap.Height, (int)(Math.Max(p1.Y, p2.Y) + BrushRadius) + 1);
            var value = CurrentMode == BrushModeId;
            for (int y = y0; y < y1; ++y)
                for (int x = x0; x < x1; ++x)
                    if (intersectBrushRectPixel(x, y) || IntersectCirclePixel(p1, BrushRadius, x, y) || IntersectCirclePixel(p2, BrushRadius, x, y))
                        Bitmap[x, y] = value;
        }
    }

    private void HandleWheel(float delta, Vector2 offset)
    {
        if (ImGui.IsKeyDown(ImGuiKey.ModShift))
        {
            BrushRadius *= MathF.Pow(1.5f, delta);
        }
        else
        {
            // zoom, keeping pixel under cursor in place
            var pivot = (ScreenOffset + offset) * MathF.Pow(0.5f, ZoomLevel);
            ZoomLevel += (int)delta;
            ScreenOffset = pivot * MathF.Pow(2, ZoomLevel) - offset;
            ScreenOffset.X = MathF.Round(ScreenOffset.X);
            ScreenOffset.Y = MathF.Round(ScreenOffset.Y);
        }
    }

    private static bool IntersectCirclePixel(Vector2 center, float radius, int x, int y)
    {
        center.X -= x + 0.5f;
        center.Y -= y + 0.5f;
        center = Vector2.Abs(center);
        // at this point, center is relative to pixel center, plus we've mirrored it to always be in positive quadrant
        center.X -= 0.5f;
        center.Y -= 0.5f;
        // now center is relative to corner
        return center.X <= 0 ? center.Y <= radius // closest point is on +Y border or center is inside
            : center.Y <= 0 ? center.X <= radius // closest point is on +X border
            : center.X * center.X + center.Y * center.Y <= radius * radius; // closest point is corner
    }

    // note: scaled by dir's length, doesn't matter for SAT test
    private static (float min, float max) ProjectRectOnAxis(Vector2 dir, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var pa = Vector2.Dot(dir, a);
        var pb = Vector2.Dot(dir, b);
        var pc = Vector2.Dot(dir, c);
        var pd = Vector2.Dot(dir, d);
        var min = Math.Min(Math.Min(pa, pb), Math.Min(pc, pd));
        var max = Math.Max(Math.Max(pa, pb), Math.Max(pc, pd));
        return (min, max);
    }

    private static bool RangesDisjoint((float min, float max) a, (float min, float max) b) => a.max < b.min || a.min > b.max;
}
