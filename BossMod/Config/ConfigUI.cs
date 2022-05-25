using System;
using System.Collections.Generic;

namespace BossMod
{
    public class ConfigUI
    {
        private class UINode
        {
            public ConfigNode Node;
            public string Name = "";
            public int Order;
            public UINode? Parent;
            public List<UINode> Children = new();

            public UINode(ConfigNode node)
            {
                Node = node;
            }
        }

        private List<UINode> _roots = new();
        private Tree _tree = new();

        public ConfigUI(ConfigRoot config)
        {
            Dictionary<Type, UINode> nodes = new();
            foreach (var n in config.Nodes)
            {
                nodes[n.GetType()] = new(n);
            }

            foreach (var (t, n) in nodes)
            {
                var props = Attribute.GetCustomAttribute(t, typeof(ConfigDisplayAttribute)) as ConfigDisplayAttribute;
                n.Name = props?.Name ?? GenerateNodeName(t);
                n.Order = props?.Order ?? 0;
                if (props?.Parent != null)
                    n.Parent = nodes.GetValueOrDefault(props.Parent);

                if (n.Parent != null)
                    n.Parent.Children.Add(n);
                else
                    _roots.Add(n);
            }

            SortByOrder(_roots);
        }

        public void Draw()
        {
            DrawNodes(_roots);
        }

        private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config") ? t.Name.Remove(t.Name.Length - "Config".Length) : t.Name;

        private void SortByOrder(List<UINode> nodes)
        {
            nodes.Sort((l, r) => l.Order.CompareTo(r.Order));
            foreach (var n in nodes)
                SortByOrder(n.Children);
        }

        private void DrawNodes(List<UINode> nodes)
        {
            foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
            {
                n.Node.DrawContents(_tree);
                DrawNodes(n.Children);
            }
        }
    }
}
