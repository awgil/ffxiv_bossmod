namespace BossMod;

public enum ComponentType
{
    // This shouldn't be used. If existing types do not cover a component, add a new type and color in ArenaColor.
    Unspecified,
    Background,
    Border,
    AOE,
    SafeFromAOE,
    Danger,
    Safe,
    Hint,

    ActorYou,
    ActorEnemy,
    ActorObject,
    PlayerInteresting,
    PlayerGeneric,
    ActorVulnerable,

    WaymarkA1,
    WaymarkB2,
    WaymarkC3,
    WaymarkD4,

    TetherMoveToward,
    TetherMoveAway,
    TetherDontMove,

    ActorA,
    ActorB,
    ActorC,
    ActorD,
    ActorE,
}
