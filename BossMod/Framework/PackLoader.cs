using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace BossMod;

sealed class PackLoader : IDisposable
{
    class LoadContext() : AssemblyLoadContext(true)
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // TODO: wtf does this even do? will this break unloading?
            if (assemblyName.Name == "BossMod")
                return Assembly.GetExecutingAssembly();

            return base.Load(assemblyName);
        }
    }

    private LoadContext? _context;
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    private string _prevDirectory = "";

    public IEnumerable<Assembly> Loaded => _context?.Assemblies ?? [];

    private readonly EventSubscriptions _subscriptions;

    public PackLoader()
    {
        _subscriptions = new(
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                var curDirectory = _config.ModulePackDirectory;
                if (_prevDirectory != curDirectory)
                {
                    Reload(curDirectory);
                }
                _prevDirectory = curDirectory;
            })
        );
    }

    private void Reload(string packDirectory)
    {
        Service.Log($"triggered reload from {packDirectory}");
        if (packDirectory.Length == 0)
            return;

        _context = new();
        var dir = new DirectoryInfo(packDirectory);
        if (dir.Exists)
        {
            foreach (var file in dir.EnumerateFiles())
            {
                if (file.Extension == ".dll")
                {
                    _context.LoadFromAssemblyPath(file.FullName);
                    Service.Log($"loaded assembly from {file.FullName}");
                }
            }
        }
    }

    public void Dispose()
    {
        _context?.Unload();
        _subscriptions.Dispose();
    }
}
