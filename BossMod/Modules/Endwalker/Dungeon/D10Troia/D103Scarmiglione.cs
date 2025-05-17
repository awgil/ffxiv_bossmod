namespace BossMod.Endwalker.Dungeon.D10Troia.D103Scarmiglione;

public enum OID : uint
{
    Boss = 0x39C5,
    Helper = 0x233C,
}

public enum AID : uint
{
    CursedEcho = 30257, // Boss->self, 4.0s cast, range 40 circle
    RottenRampageAOE = 30232, // Helper->location, 10.0s cast, range 6 circle
    RottenRampageSpread = 30233, // Helper->player, 10.0s cast, range 6 circle
    BlightedBedevilment = 30235, // Boss->self, 4.9s cast, range 9 circle
    VacuumWave = 30236, // Helper->self, 5.4s cast, range 40 circle
    BlightedBladework = 30260, // Helper->self, 11.0s cast, range 25 circle
    BlightedSweep = 30261, // Boss->self, 7.0s cast, range 40 180-degree cone
    Firedamp = 30263, // Helper->player, 5.4s cast, range 5 circle
    Nox = 30241, // Helper->self, 5.0s cast, range 10 circle
    VoidVortex = 30243, // Helper->player, 5.0s cast, range 6 circle
    VoidGravity = 30242, // Helper->players, 5.0s cast, range 6 circle
}

class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, AID.VoidGravity, 6);
class Firedamp(BossModule module) : Components.BaitAwayCast(module, AID.Firedamp, new AOEShapeCircle(5), true);
class Nox(BossModule module) : Components.StandardAOEs(module, AID.Nox, new AOEShapeCircle(10));
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, AID.VoidVortex, 6);
class BlightedBladework(BossModule module) : Components.StandardAOEs(module, AID.BlightedBladework, new AOEShapeCircle(25));
class BlightedSweep(BossModule module) : Components.StandardAOEs(module, AID.BlightedSweep, new AOEShapeCone(40, 90.Degrees()));
class BlightedBedevilment(BossModule module) : Components.StandardAOEs(module, AID.BlightedBedevilment, new AOEShapeCircle(9));
class CursedEcho(BossModule module) : Components.RaidwideCast(module, AID.CursedEcho);
class RottenRampage(BossModule module) : Components.StandardAOEs(module, AID.RottenRampageAOE, 6);
class RottenRampagePlayer(BossModule module) : Components.SpreadFromCastTargets(module, AID.RottenRampageSpread, 6);
class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.VacuumWave, 30)
{
    private readonly List<(ulong ID, Angle Angle)> Walls = [];

    public const float WallHalfWidth = 6; // this is probably wrong
    public static readonly Angle WallHalfAngle = WallHalfWidth.Degrees();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var (_, w) in Walls)
        {
            // TODO this is really slow, what's the deal with that
            Arena.PathArcTo(Arena.Center, 20, (w - WallHalfAngle).Rad, (w + WallHalfAngle).Rad);
            Arena.PathStroke(false, 0x80ffffff, 2);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x39C8)
            Walls.Add((actor.InstanceID, Angle.FromDirection(actor.Position - Arena.Center)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == 0x39C8)
            Walls.RemoveAll(w => w.ID == actor.InstanceID);
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, WallCheck(actor.Position) ? 19 - (actor.Position - Module.Arena.Center).Length() : Distance, Module.CastFinishAt(c.CastInfo));
    }

    private bool WallCheck(WPos pos) => Walls.Any(w => Angle.FromDirection(pos - Arena.Center).AlmostEqual(w.Angle, 3.Degrees().Rad));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(p => !WallCheck(p), Module.CastFinishAt(c.CastInfo));
    }
}

class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampagePlayer>()
            .ActivateOnEnter<BlightedBedevilment>()
            .ActivateOnEnter<CursedEcho>()
            .ActivateOnEnter<BlightedBladework>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<Firedamp>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<VoidVortex>()
            .ActivateOnEnter<VoidGravity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11372)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -298), new ArenaBoundsCircle(25))
{
    protected override void DrawArenaBackground(int pcSlot, Actor pc) => Arena.ZoneDonut(Arena.Center, 21, 25, ArenaColor.AOE);
}
