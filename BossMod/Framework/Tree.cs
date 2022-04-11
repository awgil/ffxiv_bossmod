using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    public class Tree
    {
        private uint _selectedID;

        // contains 0 elements (if node is closed) or single null (if node is opened)
        // expected usage is 'foreach (_ in Node(...)) { draw subnodes... }'
        public IEnumerable<object?> Node(string text, bool leaf = false)
        {
            if (RawNode(text, leaf))
            {
                yield return null;
                ImGui.TreePop();
            }
            ImGui.PopID();
        }

        // draw a node for each element in collection
        public IEnumerable<T> Nodes<T>(IEnumerable<T> collection, Func<T, (string Text, bool Leaf)> map)
        {
            foreach (var t in collection)
            {
                (string text, bool leaf) = map(t);
                if (RawNode(text, leaf))
                {
                    yield return t;
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }
        }

        public void LeafNode(string text)
        {
            if (RawNode(text, true))
                ImGui.TreePop();
            ImGui.PopID();
        }

        // draw leaf nodes for each element in collection
        public void LeafNodes<T>(IEnumerable<T> collection, Func<T, string> map)
        {
            foreach (var t in collection)
            {
                if (RawNode(map(t), true))
                    ImGui.TreePop();
                ImGui.PopID();
            }
        }

        // handle selection & id scopes
        private bool RawNode(string text, bool leaf)
        {
            var id = ImGui.GetID(text);
            var flags = ImGuiTreeNodeFlags.None;
            if (id == _selectedID)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (leaf)
                flags |= ImGuiTreeNodeFlags.Leaf;

            ImGui.PushID((int)id);
            bool open = ImGui.TreeNodeEx(text, flags);
            if (ImGui.IsItemClicked())
                _selectedID = id;
            return open;
        }
    }
}
