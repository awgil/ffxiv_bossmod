using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    // simple window that is automatically opened on creation, and destroys itself when closed
    public abstract class SimpleWindow : Window, IDisposable
    {
        public static bool IsRegistered(string name) => Service.WindowSystem?.Windows.Any(w => w.WindowName == name) ?? false;

        public bool UnregisterOnClose;

        public SimpleWindow(string name, Vector2? sizeHint = null, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool unregisterOnClose = true) : base(name, flags)
        {
            UnregisterOnClose = unregisterOnClose;
            Size = sizeHint;
            SizeCondition = ImGuiCond.FirstUseEver;
            IsOpen = true;
        }

        // register with window system if possible, otherwise (if another window with same name already exists) destroy self
        public bool Register()
        {
            var existingWindow = Service.WindowSystem?.Windows.FirstOrDefault(w => w.WindowName == WindowName);
            if (Service.WindowSystem != null && existingWindow == null)
            {
                Service.WindowSystem.AddWindow(this);
                return true;
            }
            else
            {
                existingWindow?.BringToFront();
                Dispose();
                return false;
            }
        }

        public void Unregister()
        {
            Dispose();
            Service.WindowSystem?.RemoveWindow(this);
        }

        public virtual void Dispose() { }

        public override void OnClose()
        {
            if (UnregisterOnClose)
                Unregister();
        }
    }

    // simple window that executes action on draw; useful when having a separate class is inconvenient
    public class SimpleActionWindow : SimpleWindow
    {
        private Action _draw;

        public SimpleActionWindow(string name, Action draw, Vector2? sizeHint = null, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool unregisterOnClose = true) : base(name, sizeHint, flags, unregisterOnClose)
        {
            _draw = draw;
        }

        public override void Draw() => _draw();
    }
}
