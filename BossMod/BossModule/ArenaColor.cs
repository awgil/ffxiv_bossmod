namespace BossMod;
using static ComponentType;
internal static class ArenaColor
{
    // default color constants (ABGR)
    private static uint DefaultForType(ComponentType type)
    {
        return type switch
        {
            Unspecified => 0,
            Background => 0xc00f0f0f,
            Border => 0xffffffff,
            AOE => 0x80008080,
            SafeFromAOE => 0x80008000,
            Danger => 0xff00ffff,
            Safe => 0xff00ff00,
            Hint => 0x40008080,

            ActorYou => 0xff00ff00,
            ActorEnemy => 0xff0000ff,
            ActorObject => 0xff0080ff,
            PlayerInteresting => 0xffc0c0c0,
            PlayerGeneric => 0xff808080,
            ActorVulnerable => 0xffff00ff,

            WaymarkA1 => 0xff964ee5,
            WaymarkB2 => 0xff11a2c6,
            WaymarkC3 => 0xffe29f30,
            WaymarkD4 => 0xffbc567a,

            TetherMoveToward => 0xffffff00,
            TetherMoveAway => 0xff00ffff,
            TetherDontMove => 0xffff00ff,

            // Reddish Actor
            ActorA => 0xff8080ff,
            // Gold Actor
            ActorB => 0xff40c0c0,
            // Blue Actor
            ActorC => 0xffff8040,
            // Purple Actor
            ActorD => 0xffff80ff,
            // Light Green Actor
            ActorE => 0xff80ff80,

            _ => 0xff0000ff,
        };
    }

    internal static uint ForType(ComponentType type)
    {
        return DefaultForType(type);
    }
}
