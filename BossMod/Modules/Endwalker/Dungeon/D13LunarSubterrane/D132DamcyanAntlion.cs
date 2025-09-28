namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D132DamcyanAntlion;

public enum OID : uint
{
    Boss = 0x4022, // R=7.5
    StonePillar = 0x4023, // R=3.0
    StonePillar2 = 0x3FD1, // R=1.5
    QuicksandVoidzone = 0x1EB90E,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss, no cast, single-target
    Sandblast = 34813, // Boss->self, 5.0s cast, range 60 circle
    Landslip = 34818, // Boss->self, 7.0s cast, single-target
    Landslip2 = 34819, // Helper->self, 7.7s cast, range 40 width 10 rect, knockback dir 20 forward
    Teleport = 34824, // Boss->location, no cast, single-target
    AntilonMarchTelegraph = 35871, // Helper->location, 1.5s cast, width 8 rect charge
    AntlionMarch = 34816, // Boss->self, 5.5s cast, single-target
    AntlionMarch2 = 34817, // Boss->location, no cast, width 8 rect charge
    Towerfall = 34820, // StonePillar->self, 2.0s cast, range 40 width 10 rect
    EarthenGeyser = 34821, // Boss->self, 4.0s cast, single-target
    EarthenGeyser2 = 34822, // Helper->players, 5.0s cast, range 10 circle
    PoundSand = 34443, // Boss->location, 6.0s cast, range 12 circle
}

class Sandblast(BossModule module) : Components.RaidwideCast(module, AID.Sandblast);

class Voidzone(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
            Module.Arena.Bounds = new ArenaBoundsRect(19.5f, 20);
    }
}

class Landslip(BossModule module) : Components.Knockback(module)
{
    private readonly List<Actor> _casters = [];
    private DateTime _activation;
    private static readonly AOEShapeRect rect = new(40, 5);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, 20, _activation, rect, c.Rotation, Kind.DirForward);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
        {
            _activation = Module.CastFinishAt(spell);
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
            _casters.Remove(caster);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (Module.FindComponent<Towerfall>() is var towerfall && towerfall != null && towerfall.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (!Module.InBounds(pos))
            return true;
        else
            return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in _casters)
            // forbid the half of each lane that would result in the player being pushed into the death wall
            // TODO make this work with towerfall too...not a huge priority because duty support healer can keep
            // player alive in that case
            hints.AddForbiddenZone(new AOEShapeRect(10, 5, 10), c.Position + c.Rotation.ToDirection() * 30);
    }
}

class EarthenGeyser(BossModule module) : Components.StackWithCastTargets(module, AID.EarthenGeyser2, 10);
class QuicksandVoidzone(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.QuicksandVoidzone).Where(z => z.EventState != 7));
class PoundSand(BossModule module) : Components.StandardAOEs(module, AID.PoundSand, 12);

class AntlionMarch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos source, AOEShape shape, Angle direction)> _casters = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _activation, ArenaColor.Danger);
        for (int i = 1; i < _casters.Count; ++i)
            yield return new(_casters[i].shape, _casters[i].source, _casters[i].direction, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AntilonMarchTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _casters.Add((caster.Position, new AOEShapeRect(dir.Length(), 4), Angle.FromDirection(dir)));
        }
        if ((AID)spell.Action.ID == AID.AntlionMarch)
            _activation = Module.CastFinishAt(spell, 0.2f); //since these are charges of different length with 0s cast time, the activation times are different for each and there are different patterns, so we just pretend that they all start after the telegraphs end
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.AntlionMarch2)
            _casters.RemoveAt(0);
    }
}

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos source, AOEShape shape, Angle direction, DateTime activation)> _casters = [];
    private static readonly AOEShapeRect rect = new(40, 5);
    private static readonly Angle _rot1 = 89.999f.Degrees();
    private static readonly Angle _rot2 = -90.004f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _casters[0].activation);
        if (_casters.Count > 1)
            yield return new(_casters[1].shape, _casters[1].source, _casters[1].direction, _casters[1].activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x01)
                _casters.Add((new WPos(-20, 45), rect, _rot1, WorldState.FutureTime(13 - _casters.Count))); // timings can vary 1-3 seconds depending on Antilonmarch charges duration, so i took the lowest i could find
            if (index == 0x02)
                _casters.Add((new WPos(-20, 55), rect, _rot1, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x03)
                _casters.Add((new WPos(-20, 65), rect, _rot1, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x04)
                _casters.Add((new WPos(-20, 75), rect, _rot1, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x05)
                _casters.Add((new WPos(20, 45), rect, _rot2, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x06)
                _casters.Add((new WPos(20, 55), rect, _rot2, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x07)
                _casters.Add((new WPos(20, 65), rect, _rot2, WorldState.FutureTime(13 - _casters.Count)));
            if (index == 0x08)
                _casters.Add((new WPos(20, 75), rect, _rot2, WorldState.FutureTime(13 - _casters.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Towerfall)
            _casters.Clear();
    }
}

class D132DamcyanAntilonStates : StateMachineBuilder
{
    public D132DamcyanAntilonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<Sandblast>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<EarthenGeyser>()
            .ActivateOnEnter<QuicksandVoidzone>()
            .ActivateOnEnter<PoundSand>()
            .ActivateOnEnter<AntlionMarch>()
            .ActivateOnEnter<Towerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12484)]
public class D132DamcyanAntilon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 60), new ArenaBoundsRect(19.5f, 25));
