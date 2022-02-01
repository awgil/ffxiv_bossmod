using Dalamud.Configuration;
using Dalamud.Plugin;

namespace BossMod
{
    class ConfigRoot : ConfigNode, IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        private DalamudPluginInterface? _dalamud;

        public static ConfigRoot ReadConfig(DalamudPluginInterface dalamud)
        {
            var obj = dalamud.GetPluginConfig() as ConfigRoot ?? new ConfigRoot();
            obj._dalamud = dalamud;
            obj.FixAfterDeserialize();
            return obj;
        }

        protected override void Save()
        {
            _dalamud?.SavePluginConfig(this);
        }
    }
}
