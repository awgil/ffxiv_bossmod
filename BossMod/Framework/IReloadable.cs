using System.Reflection;

namespace BossMod;

public interface IReloadable
{
    public void Reload(IEnumerable<Assembly> old, IEnumerable<Assembly> @new);
}
