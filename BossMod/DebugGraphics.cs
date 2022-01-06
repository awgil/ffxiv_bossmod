using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BossMod
{
    class DebugGraphics
    {
        private class WatchedRenderObject
        {
            public List<uint> Data = new();
            public List<(int, int)> Modifications = new();
            public bool Live;
        }

        private bool _showGraphicsLeafCharactersOnly = true;
        private Dictionary<IntPtr, WatchedRenderObject> _watchedRenderObjects = new();

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
                bool showNode = !_showGraphicsLeafCharactersOnly || o->ChildObject != null || o->GetObjectType() == FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.CharacterBase;
                if (showNode && ImGui.TreeNodeEx(nodeText, nodeFlags))
                {
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        int size = 0x80;
                        switch (o->GetObjectType())
                        {
                            case FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.CharacterBase:
                                size = 0x8F0;
                                break;
                            case FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.VfxObject:
                                size = 0x1C8;
                                break;
                        }
                        WatchObject(o, size);
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
                v.Value.Live = false;

            var root = FindSceneRoot();
            if (root != null)
                UpdateWatchedMods(root);

            List<IntPtr> del = new();
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
                ImGui.TableNextColumn(); ImGui.Text($"0x{v.Key:X}");
                ImGui.TableNextColumn(); DrawMods(v.Value);
            }
            ImGui.EndTable();
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
                WatchedRenderObject? watch;
                if (_watchedRenderObjects.TryGetValue((IntPtr)o, out watch))
                    UpdateWatchedMod(o, watch!);

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

            List<(int, int)> res = new();
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
            var cn = ImGui.ColorConvertU32ToFloat4(0xff808080);
            var cm = ImGui.ColorConvertU32ToFloat4(0xff0000ff);
            int start = 0;
            var sb = new StringBuilder();
            foreach ((var end, var nextStart) in w.Modifications)
            {
                DrawHexString(w, ref start, end, cn, sb);
                DrawHexString(w, ref start, nextStart, cm, sb);
            }
            sb.Clear();
            DrawHexString(w, ref start, w.Data.Count, cn, sb);
        }

        private void DrawHexString(WatchedRenderObject w, ref int start, int end, Vector4 color, StringBuilder sb)
        {
            sb.Clear();
            while (start < end)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.AppendFormat("{0:X8}", w.Data[start++]);

                if ((start % 16) == 0)
                {
                    ImGui.TextColored(color, sb.ToString());
                    sb.Clear();
                }
            }
            ImGui.TextColored(color, sb.ToString());
            ImGui.SameLine();
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
            PluginLog.Log(res.ToString());
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
                case FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.VfxObject:
                    s += $", ac={Utils.ReadField<int>(o, 0x128):X}, at={Utils.ReadField<int>(o, 0x130):X}, sc={Utils.ReadField<int>(o, 0x1B8):X}, st={Utils.ReadField<int>(o, 0x1C0)}:X";
                    break;
            }
            return s;
        }
    }
}
