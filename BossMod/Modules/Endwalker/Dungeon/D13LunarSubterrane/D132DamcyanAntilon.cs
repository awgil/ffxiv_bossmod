namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D132DamcyanAntilon;

public enum OID : uint
{
    Boss = 0x4022, // R=7.5
    StonePillar = 0x4023, // R=3.0
    StonePillar2 = 0x3FD1, // R=1.5
    QuicksandVoidzone = 0x1EB90E,
    Helper = 0x233C,
};

public enum AID : uint
{
    AutoAttack = 872, // Boss, no cast, single-target
    Sandblast = 34813, // Boss->self, 5,0s cast, range 60 circle
    Landslip = 34818, // Boss->self, 7,0s cast, single-target
    Landslip2 = 34819, // Helper->self, 7,7s cast, range 40 width 10 rect, knockback dir 20 forward
    Teleport = 34824, // Boss->location, no cast, single-target
    AntilonMarchTelegraph = 35871, // Helper->location, 1,5s cast, width 8 rect charge
    AntlionMarch = 34816, // Boss->self, 5,5s cast, single-target
    AntlionMarch2 = 34817, // Boss->location, no cast, width 8 rect charge
    Towerfall = 34820, // StonePillar->self, 2,0s cast, range 40 width 10 rect
    EarthenGeyser = 34821, // Boss->self, 4,0s cast, single-target
    EarthenGeyser2 = 34822, // Helper->players, 5,0s cast, range 10 circle
    PoundSand = 34443, // Boss->location, 6,0s cast, range 12 circle
};

class Sandblast : Components.RaidwideCast
{
    public Sandblast() : base(ActionID.MakeSpell(AID.Sandblast)) { }
}

class Voidzone : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
            module.Arena.Bounds = new ArenaBoundsRect(new(0, 60), 19.5f, 20);
    }
}

class Landslip : Components.Knockback
{
    private List<Actor> _casters = new();
    private DateTime _activation;
    private static readonly AOEShapeRect rect = new(40, 5);

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, 20, _activation, rect, c.Rotation, Kind.DirForward);
    }
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
        {
            _activation = spell.NPCFinishAt;
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
            _casters.Remove(caster);
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
    {
        if (module.FindComponent<Towerfall>() != null && module.FindComponent<Towerfall>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (!module.Bounds.Contains(pos))
            return true;
        else
            return false;
    }
}

class EarthenGeyser : Components.StackWithCastTargets
{
    public EarthenGeyser() : base(ActionID.MakeSpell(AID.EarthenGeyser2), 10) { }
}

class QuicksandVoidzone : Components.PersistentVoidzone
{
    public QuicksandVoidzone() : base(10, m => m.Enemies(OID.QuicksandVoidzone).Where(z => z.EventState != 7)) { }
}

class PoundSand : Components.LocationTargetedAOEs
{
    public PoundSand() : base(ActionID.MakeSpell(AID.PoundSand), 12) { }
}

class AntlionMarch : Components.GenericAOEs
{
    private List<(WPos source, AOEShape shape, Angle direction)> _casters = new();
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _activation, ArenaColor.Danger);
        for (int i = 1; i < _casters.Count; ++i)
            yield return new(_casters[i].shape, _casters[i].source, _casters[i].direction, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AntilonMarchTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _casters.Add((caster.Position, new AOEShapeRect(dir.Length(), 4), Angle.FromDirection(dir)));
        }
        if ((AID)spell.Action.ID == AID.AntlionMarch)
            _activation = spell.NPCFinishAt.AddSeconds(0.2f); //since these are charges of different length with 0s cast time, the activation times are different for each and there are different patterns, so we just pretend that they all start after the telegraphs end
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.AntlionMarch2)
            _casters.RemoveAt(0);
    }
}

class Towerfall : Components.GenericAOEs
{
    private List<(WPos source, AOEShape shape, Angle direction, DateTime activation)> _casters = new();
    private static readonly AOEShapeRect rect = new(40, 5);
    private static readonly Angle _rot1 = 89.999f.Degrees();
    private static readonly Angle _rot2 = -90.004f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _casters[0].activation);
        if (_casters.Count > 1)
            yield return new(_casters[1].shape, _casters[1].source, _casters[1].direction, _casters[1].activation);
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x01)
                _casters.Add((new WPos(-20, 45), rect, _rot1, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));  //timings can vary 1-3 seconds depending on Antilonmarch charges duration, so i took the lowest i could find
            if (index == 0x02)
                _casters.Add((new WPos(-20, 55), rect, _rot1, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x03)
                _casters.Add((new WPos(-20, 65), rect, _rot1, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x04)
                _casters.Add((new WPos(-20, 75), rect, _rot1, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x05)
                _casters.Add((new WPos(20, 45), rect, _rot2, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x06)
                _casters.Add((new WPos(20, 55), rect, _rot2, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x07)
                _casters.Add((new WPos(20, 65), rect, _rot2, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
            if (index == 0x08)
                _casters.Add((new WPos(20, 75), rect, _rot2, module.WorldState.CurrentTime.AddSeconds(13 - _casters.Count)));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
public class D132DamcyanAntilon : BossModule
{
    public D132DamcyanAntilon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 60), 19.5f, 25)) { }
}
