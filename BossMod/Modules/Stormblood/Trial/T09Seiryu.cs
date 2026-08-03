namespace BossMod.Stormblood.Trial.WreathOfSnakes.T09Seiryu;

public enum OID : uint
{
    Boss = 0x25F4, // actual boss target
    AoNoShiki = 0x233C, // R0.500, x?, Helper type
    AkaNoShiki = 0x2786, // R2.600, x?
    AoNoShiki1 = 0x2787, // R3.000, x?
    IwaNoShiki = 0x2788, // R4.000, x?
    BlueOrochi = 0x2672, // R1.000, x?
    TenNoShiki = 0x25F8, // R2.700, x?
    NumaNoShiki = 0x25F6, // R2.400, x?
    DoroNoShiki = 0x25F7, // R1.440, x?
    BlueOrochi1 = 0x25F5, // R1.000, x?
    BlueOrochi2 = 0x2658, // R1.000, x?
    BlueOrochi3 = 0x2659, // R1.000, x?
}

public enum AID : uint
{
    DragonsWake = 14336, // Boss->self, transitions the arena to phase 2 (island surrounded by water)
    CoursingRiver = 14350, // BlueOrochi->self, directional knockback ("river current") covering the whole platform - can push players off the island into the water
}

// phase 2 (after Dragon's Wake) the arena expands from the phase-1 island to a larger circle, with only the central island being safe;
// the surrounding water is not instantly lethal but increases damage taken and prevents casting, so it must be avoided
class Phase2Water(BossModule module) : BossComponent(module)
{
    public bool Active { get; private set; }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DragonsWake)
        {
            Active = true;
            Arena.Bounds = new ArenaBoundsCircle(T09Seiryu.ArenaRadius);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.ZoneDonut(Arena.Center, T09Seiryu.IslandRadius, T09Seiryu.ArenaRadius, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !actor.Position.InCircle(Arena.Center, T09Seiryu.IslandRadius))
            hints.Add("Return to the island!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
            hints.AddForbiddenZone(ShapeContains.Donut(Arena.Center, T09Seiryu.IslandRadius, T09Seiryu.ArenaRadius));
    }
}

// Coursing River is a directional knockback along each Blue Orochi's facing; the rect covers the whole platform, so it is effectively unavoidable and the danger is being pushed off the island into the water
// TODO: confirm knockback distance in-game (assumed 15)
class CoursingRiver(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CoursingRiver, 15, shape: new AOEShapeRect(90, 45), kind: Components.Knockback.Kind.DirForward)
{
    // safe area in phase 2 is the island, not the full (water-filled) arena bounds
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(Arena.Center, T09Seiryu.IslandRadius);
}

class T09SeiryuStates : StateMachineBuilder
{
    public T09SeiryuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Phase2Water>()
            .ActivateOnEnter<CoursingRiver>();
    }
}

[ModuleInfo(Contributors = "skmagiik", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637, NameID = 7922)]
public class T09Seiryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(IslandRadius))
{
    public const float IslandRadius = 20; // phase-1 arena and phase-2 safe island
    public const float ArenaRadius = 38; // phase-2 full arena (island + surrounding water)
}
