using Dalamud.Plugin.Services;

namespace BossMod;

public record class ModuleArgs(WorldState World, Actor Primary, ITextureProvider TextureProvider)
{
    public delegate ModuleArgs Factory(WorldState World, Actor Primary);
}
