using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Dalamud.Bindings.ImGui;
using System.Text;

namespace BossMod;

class DebugGraphics
{
    private class WatchedRenderObject
    {
        public List<uint> Data = [];
        public List<(int, int)> Modifications = [];
        public bool Live;
    }

    private bool _showGraphicsLeafCharactersOnly = true;
    private readonly Dictionary<IntPtr, WatchedRenderObject> _watchedRenderObjects = [];
    private bool _overlayCircle;
    private Vector2 _overlayCenter = new(100, 100);
    private Vector2 _overlayStep = new(2, 2);
    private Vector2 _overlayMaxOffset = new(20, 20);

    public unsafe void DrawSceneTree()
    {
        ImGui.Checkbox("Show only leafs of Character type", ref _showGraphicsLeafCharactersOnly);
        var root = FindSceneRoot();
        if (root != null)
        {
            DrawSceneNode(root);
        }
    }

    private unsafe void DrawSceneNode(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var start = o;
        do
        {
            var nodeText = $"{SceneNodeText(o)}###{(IntPtr)o}";
            ImGuiTreeNodeFlags nodeFlags = (o->ChildObject != null ? ImGuiTreeNodeFlags.None : ImGuiTreeNodeFlags.Leaf) | ImGuiTreeNodeFlags.OpenOnArrow;
            bool showNode = !_showGraphicsLeafCharactersOnly || o->ChildObject != null || o->GetObjectType() == ObjectType.CharacterBase;
            if (showNode && ImGui.TreeNodeEx(nodeText, nodeFlags))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    bool watched = _watchedRenderObjects.ContainsKey((IntPtr)o);
                    if (!watched)
                    {
                        int size = 0x80;
                        switch (o->GetObjectType())
                        {
                            case ObjectType.CharacterBase:
                                size = 0x8F0;
                                break;
                            case ObjectType.VfxObject:
                                size = 0x1C8;
                                break;
                        }
                        WatchObject(o, size);
                    }
                    else
                    {
                        _watchedRenderObjects.Remove((IntPtr)o);
                    }
                }

                if (o->ChildObject != null)
                    DrawSceneNode(o->ChildObject);
                ImGui.TreePop();
            }
            o = o->NextSiblingObject;
        }
        while (o != start);
    }

    public unsafe void DrawWatchedMods()
    {
        if (ImGui.Button("Clear watch list"))
            _watchedRenderObjects.Clear();
        ImGui.SameLine();
        if (ImGui.Button("Clear modifications"))
            foreach (var v in _watchedRenderObjects)
                v.Value.Modifications.Clear();

        if (_watchedRenderObjects.Count == 0)
            return;

        foreach (var v in _watchedRenderObjects)
        {
            v.Value.Live = false;
        }

        var root = FindSceneRoot();
        if (root != null)
            UpdateWatchedMods(root);

        List<IntPtr> del = [];
        foreach (var v in _watchedRenderObjects)
            if (!v.Value.Live)
                del.Add(v.Key);
        foreach (var v in del)
            _watchedRenderObjects.Remove(v);

        ImGui.BeginTable("watched_graphics", 2);
        ImGui.TableSetupColumn("Ptr", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Data");
        ImGui.TableHeadersRow();
        foreach (var v in _watchedRenderObjects)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"0x{v.Key:X}");
            ImGui.TableNextColumn();
            DrawMods(v.Value);
        }
        ImGui.EndTable();

        foreach (var v in _watchedRenderObjects)
        {
            var obj = (FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object*)v.Key;
            Camera.Instance?.DrawWorldLine(Service.ClientState.LocalPlayer!.Position, obj->Position, 0xff0000ff);
        }
    }

    public unsafe void WatchObject(void* o, int size)
    {
        if (_watchedRenderObjects.ContainsKey((IntPtr)o))
            return;

        var w = new WatchedRenderObject();
        for (int i = 0; i < size / 4; ++i)
            w.Data.Add(((uint*)o)[i]);
        _watchedRenderObjects.Add((IntPtr)o, w);
    }

    private unsafe void UpdateWatchedMods(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var start = o;
        do
        {
            WatchedRenderObject? watch = _watchedRenderObjects.GetValueOrDefault((IntPtr)o);
            if (watch != null)
                UpdateWatchedMod(o, watch);

            if (o->ChildObject != null)
                UpdateWatchedMods(o->ChildObject);
            o = o->NextSiblingObject;
        }
        while (o != start);
    }

    private unsafe void UpdateWatchedMod(void* o, WatchedRenderObject w)
    {
        w.Live = true;

        int start = 0;
        for (int i = 0; i < w.Modifications.Count; ++i)
        {
            (int end, int nextStart) = w.Modifications[i];
            var mods = CheckUnmodRange((uint*)o, w, start, end);
            if (mods != null)
            {
                w.Modifications.InsertRange(i, mods);
                i += mods.Count;
            }
            start = nextStart;
        }

        var endMods = CheckUnmodRange((uint*)o, w, start, w.Data.Count);
        if (endMods != null)
            w.Modifications.AddRange(endMods);

        for (int i = 0; i < w.Data.Count; ++i)
            w.Data[i] = ((uint*)o)[i];
    }

    private unsafe List<(int, int)>? CheckUnmodRange(uint* o, WatchedRenderObject w, int start, int end)
    {
        while (start < end && o[start] == w.Data[start])
            ++start;
        if (start == end)
            return null; // nothing changed

        List<(int, int)> res = [];
        while (start < end)
        {
            int m = start + 1;
            while (m < end && o[m] != w.Data[m])
                ++m;

            res.Add((start, m));
            start = m;
            while (start < end && o[start] == w.Data[start])
                ++start;
        }
        return res;
    }

    private void DrawMods(WatchedRenderObject w)
    {
        int start = 0;
        var sb = new StringBuilder();
        foreach ((var end, var nextStart) in w.Modifications)
        {
            DrawHexString(w, ref start, end, 0xff808080, sb);
            DrawHexString(w, ref start, nextStart, 0xff0000ff, sb);
        }
        sb.Clear();
        DrawHexString(w, ref start, w.Data.Count, 0xff808080, sb);
    }

    private void DrawHexString(WatchedRenderObject w, ref int start, int end, uint color, StringBuilder sb)
    {
        sb.Clear();
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        while (start < end)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.AppendFormat("{0:X8}", w.Data[start++]);

            if ((start % 16) == 0)
            {
                ImGui.TextUnformatted(sb.ToString());
                sb.Clear();
            }
        }
        ImGui.TextUnformatted(sb.ToString());
        ImGui.SameLine();
        ImGui.PopStyleColor();
    }

    public unsafe void DrawMatrices()
    {
        var camera = CameraManager.Instance()->CurrentCamera;
        if (camera == null)
            return;

        using var table = ImRaii.Table("matrices", 2);
        if (!table)
            return;

        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("VP");
        ImGui.TableNextColumn();
        DrawMatrix(camera->ViewMatrix * camera->RenderCamera->ProjectionMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("P");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ProjectionMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("P2");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ProjectionMatrix2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("V");
        ImGui.TableNextColumn();
        DrawMatrix(camera->ViewMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("V2");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ViewMatrix);

        var altitude = MathF.Asin(camera->ViewMatrix.M23);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Camera Altitude");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(altitude.Radians().ToString());

        var azimuth = MathF.Atan2(camera->ViewMatrix.M13, camera->ViewMatrix.M33);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Camera Azimuth");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(azimuth.Radians().ToString());

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Origin");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Utils.Vec3String(camera->RenderCamera->Origin));

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Near/far/aspect");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{camera->RenderCamera->NearPlane} / {camera->RenderCamera->FarPlane} / {camera->RenderCamera->AspectRatio}");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Projection flags");
        ImGui.TableNextColumn();
        if (ImGui.Button(camera->RenderCamera->IsOrtho ? $"ortho ({camera->RenderCamera->OrthoHeight})" : "perspective"))
            camera->RenderCamera->IsOrtho ^= true;
        ImGui.SameLine();
        if (ImGui.Button(camera->RenderCamera->StandardZ ? "standard-z" : "reverse-z"))
            camera->RenderCamera->StandardZ ^= true;
        ImGui.SameLine();
        if (ImGui.Button(camera->RenderCamera->FiniteFarPlane ? "finite-far" : "infinite-far"))
            camera->RenderCamera->FiniteFarPlane ^= true;

        var view = camera->ViewMatrix;
        var lx = new Vector3(view.M11, view.M21, view.M31);
        var ly = new Vector3(view.M12, view.M22, view.M32);
        var lz = new Vector3(view.M13, view.M23, view.M33);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("View handedness");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{Vector3.Dot(lz, Vector3.Cross(lx, ly))}");

        view.M44 = 1;
        FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4.Invert(view, out var world);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("W");
        ImGui.TableNextColumn();
        DrawMatrix(world);

        var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Viewport size");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{device->Width:f6} {device->Height:f6}");
    }

    private void DrawMatrix(SharpDX.Matrix mtx)
    {
        ImGui.TextUnformatted($"{mtx[0]:f6} {mtx[1]:f6} {mtx[2]:f6} {mtx[3]:f6}");
        ImGui.TextUnformatted($"{mtx[4]:f6} {mtx[5]:f6} {mtx[6]:f6} {mtx[7]:f6}");
        ImGui.TextUnformatted($"{mtx[8]:f6} {mtx[9]:f6} {mtx[10]:f6} {mtx[11]:f6}");
        ImGui.TextUnformatted($"{mtx[12]:f6} {mtx[13]:f6} {mtx[14]:f6} {mtx[15]:f6}");
    }

    private void DrawMatrix(FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4 mtx)
    {
        ImGui.TextUnformatted($"{mtx[0]:f6} {mtx[1]:f6} {mtx[2]:f6} {mtx[3]:f6}");
        ImGui.TextUnformatted($"{mtx[4]:f6} {mtx[5]:f6} {mtx[6]:f6} {mtx[7]:f6}");
        ImGui.TextUnformatted($"{mtx[8]:f6} {mtx[9]:f6} {mtx[10]:f6} {mtx[11]:f6}");
        ImGui.TextUnformatted($"{mtx[12]:f6} {mtx[13]:f6} {mtx[14]:f6} {mtx[15]:f6}");
    }

    public void DrawOverlay()
    {
        if (Camera.Instance == null || Service.ClientState.LocalPlayer == null)
            return;

        ImGui.Checkbox("Circle", ref _overlayCircle);
        ImGui.DragFloat2("Center", ref _overlayCenter);
        ImGui.DragFloat2("Step", ref _overlayStep, 0.25f, 1, 10);
        ImGui.DragFloat2("Max offset", ref _overlayMaxOffset);

        if (_overlayStep.X < 1 || _overlayStep.Y < 1)
            return;

        int mx = (int)(_overlayMaxOffset.X / _overlayStep.X);
        int mz = (int)(_overlayMaxOffset.Y / _overlayStep.Y);
        float y = Service.ClientState.LocalPlayer.Position.Y;
        if (_overlayCircle)
        {
            var center = new Vector3(_overlayCenter.X, y, _overlayCenter.Y);
            for (int ir = 0; ir <= mx; ++ir)
            {
                Camera.Instance.DrawWorldCircle(center, ir * _overlayStep.X, ArenaColor.PC);
            }
            for (int ia = 0; ia < 8; ++ia)
            {
                var offset = ((ia * 22.5f.Degrees()).ToDirection() * _overlayMaxOffset.X).ToVec3();
                Camera.Instance.DrawWorldLine(center - offset, center + offset, ArenaColor.PC);
            }
        }
        else
        {
            for (int ix = -mx; ix <= mx; ++ix)
            {
                var x = _overlayCenter.X + ix * _overlayStep.X;
                Camera.Instance.DrawWorldLine(new(x, y, _overlayCenter.Y - _overlayMaxOffset.Y), new(x, y, _overlayCenter.Y + _overlayMaxOffset.Y), ArenaColor.PC);
            }
            for (int iz = -mz; iz <= mz; ++iz)
            {
                var z = _overlayCenter.Y + iz * _overlayStep.Y;
                Camera.Instance.DrawWorldLine(new(_overlayCenter.X - _overlayMaxOffset.X, y, z), new(_overlayCenter.X + _overlayMaxOffset.X, y, z), ArenaColor.PC);
            }
        }
    }

    public static unsafe FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* FindSceneRoot()
    {
        var player = Utils.GameObjectInternal(Service.ClientState.LocalPlayer);
        if (player == null || player->DrawObject == null)
            return null;

        var obj = &player->DrawObject->Object;
        while (obj->ParentObject != null)
            obj = obj->ParentObject;
        return obj;
    }

    public static unsafe void DumpScene()
    {
        var res = new StringBuilder("--- graphics scene dump ---");
        var root = FindSceneRoot();
        if (root != null)
        {
            DumpSceneNode(res, root, "");
        }
        Service.Log(res.ToString());
    }

    private static unsafe void DumpSceneNode(StringBuilder res, FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o, string prefix)
    {
        var start = o;
        do
        {
            res.Append($"\n{prefix} {SceneNodeText(o)}");
            if (o->ChildObject != null)
                DumpSceneNode(res, o->ChildObject, prefix + "-");
            o = o->NextSiblingObject;
        }
        while (o != start);
    }

    private static unsafe string SceneNodeText(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var t = o->GetObjectType();
        var s = $"0x{(IntPtr)o:X}: t={t}, flags={Utils.SceneObjectFlags(o):X}, pos={Utils.Vec3String(o->Position)}, rot={Utils.QuatString(o->Rotation)}, scale={Utils.Vec3String(o->Scale)}";
        switch (t)
        {
            case ObjectType.VfxObject:
                s += $", ac={Utils.ReadField<int>(o, 0x128):X}, at={Utils.ReadField<int>(o, 0x130):X}, sc={Utils.ReadField<int>(o, 0x1B8):X}, st={Utils.ReadField<int>(o, 0x1C0)}:X";
                break;
        }
        return s;
    }
}
