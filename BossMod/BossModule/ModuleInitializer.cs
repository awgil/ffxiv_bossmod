using Dalamud.Plugin.Services;

namespace BossMod;

public record class ModuleInitializer(WorldState World, Actor Primary, ITextureProvider TextureProvider)
{
    public delegate ModuleInitializer Factory(WorldState World, Actor Primary);
}
