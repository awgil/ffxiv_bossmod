namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D111SpectralThief;

public enum OID : uint
{
    Boss = 0x2DEC, // R0.875, x? 
    Shadow = 0x2DED, // R0.875, x?
    ChickenKnife = 0x2E71, // R1.000, x?
    DashMarker = 0x1EAED9, // Circles?
    Helper = 0x233C,
}

public enum AID : uint
{
    SpectralDream = 20427, // Boss->players, 4.0s cast, single-target
    SpectralWhirlwind = 20428, // Boss->self, 4.0s cast, range 60 circle
    SpectralGust2 = 21455, // Helper->player, 6.0s cast, range 5 circle
    CowardsCunning = 20439, // ChickenKnife->self, 3.0s cast, range 60 width 2 rect
    VacuumBlade1 = 20577, // Helper->self, no cast, range 15 circle
    VacuumBlade2 = 20578, // Helper->self, no cast, range 15 circle
    Papercutter1 = 20433, // Helper->self, no cast, range 80 width 14 rect
    Papercutter2 = 20434, // Helper->self, no cast, range 80 width 14 rect
}
public enum SID : uint
{
    DashStatus = 2193,
}
public enum IconID : uint
{
    SpectralGust = 169,
    TankBuster = 198,
}
public enum TetherID : uint
{
    Dash = 12,
}
class SpectralDream(BossModule module) : Components.SingleTargetCast(module, AID.SpectralDream);
class SpectralWhirlwind(BossModule module) : Components.RaidwideCast(module, AID.SpectralWhirlwind);
class SpectralGust(BossModule module) : Components.SpreadFromCastTargets(module, AID.SpectralGust2, 6);
class CowardsCunning(BossModule module) : Components.StandardAOEs(module, AID.CowardsCunning, new AOEShapeRect(60, 1));

class Markers(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _actors = [];

    private AOEShape? _nextShape;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _nextShape is { } s ? _actors.Select(m => new AOEInstance(_nextShape, m.Position, default, _activation)) : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DashStatus)
        {
            _nextShape = status.Extra switch
            {
                0xB0 or 0xB3 => new AOEShapeCircle(15),
                0xB1 or 0xB4 => new AOEShapeRect(60, 7, 60),
                0xB2 or 0xB5 => new AOEShapeRect(7, 60, 7),
                _ => null
            };
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Dash && tether.Target == Module.PrimaryActor.InstanceID && !_actors.Contains(source))
        {
            _activation = WorldState.FutureTime(8.3f);
            _actors.Add(source);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.VacuumBlade1 or AID.VacuumBlade2 or AID.Papercutter1 or AID.Papercutter2)
        {
            _actors.Clear();
            _nextShape = null;
            _activation = default;
        }
    }
}

class D111SpectralThiefStates : StateMachineBuilder
{
    public D111SpectralThiefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Markers>()
            .ActivateOnEnter<SpectralDream>()
            .ActivateOnEnter<SpectralWhirlwind>()
            .ActivateOnEnter<SpectralGust>()
            .ActivateOnEnter<CowardsCunning>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9505)]
public class D111SpectralThief(WorldState ws, Actor primary) : BossModule(ws, primary, new(-680f, 449.97f), new ArenaBoundsSquare(20));
