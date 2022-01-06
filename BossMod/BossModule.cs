using System;

namespace BossMod
{
    public interface IBossModule : IDisposable
    {
        public abstract void Draw();
    }
}
