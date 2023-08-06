using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    // simple window that is automatically opened on creation, and destroys itself when closed
    public abstract class SimpleWindow : Window, IDisposable
    {
        public static bool IsRegistered(string name) => Service.WindowSystem?.Windows.Any(w => w.WindowName == name) ?? false;

        public SimpleWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None) : base(name, flags)
        {
            IsOpen = true;
        }

        // register with window system if possible, otherwise (if another window with same name already exists) destroy self
        public bool Register()
        {
            if (Service.WindowSystem != null && !IsRegistered(WindowName))
            {
                Service.WindowSystem.AddWindow(this);
                return true;
            }
            else
            {
                Dispose();
                return false;
            }
        }

        public virtual void Dispose() { }

        public override void OnClose()
        {
            Dispose();
            Service.WindowSystem?.RemoveWindow(this);
        }
    }
}
