namespace BossMod.Stormblood.Dungeon.D15GhimlytDark.D151MarkIIIBMagitekColossus;

public enum OID : uint
{
    Boss = 0x25DA, // R3.500, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    FireslashVoidzone = 0x1EA1A1, // R2.000, x10, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    CeruleumVent = 14195, // Boss->self, 4.0s cast, range 40 circle
    Exhaust = 14192, // Boss->self, 3.0s cast, range 40+R width 10 rect
    JarringBlow = 14190, // Boss->player, 4.0s cast, single-target
    MagitekRay = 14191, // Boss->players, 5.0s cast, range 6 circle

    MagitekSlashFirst = 14196, // Boss->self, 5.0s cast, range 20+R 60-degree cone // happened 1st
    MagitekSlashFollowUps = 14671, // Helper->self, no cast, range 20+R ?-degree cone
    MagitekSlashCCWVisual = 14197, // Boss->self, no cast, range 20+R ?-degree cone
    MagitekSlashCWVisual = 14670, // Boss->self, no cast, range 20+R ?-degree cone

    WildFireBeamBoss = 14193, // Boss->self, no cast, single-target
    WildFireBeamHelper = 14194, // Helper->player, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167, // Boss
    Spreadmarker = 139, // player
    Stackmarker = 62, // player
    Tankbuster = 198 // player
}

class CeruleumVent(BossModule module) : Components.RaidwideCast(module, AID.CeruleumVent);
class Exhaust(BossModule module) : Components.StandardAOEs(module, AID.Exhaust, new AOEShapeRect(43.5f, 5));
class JarringBlow(BossModule module) : Components.SingleTargetDelayableCast(module, AID.JarringBlow);
class MagitekRay(BossModule module) : Components.StackWithCastTargets(module, AID.MagitekRay, 6, 4, 4);
class WildFireBeam(BossModule module) : Components.SpreadFromCastTargets(module, AID.WildFireBeamHelper, 6);
class MagitektSlashRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly Angle a60 = 60.Degrees();
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    public static readonly AOEShapeCone Cone = new(23.5f, 30.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -a60,
            IconID.RotateCCW => a60,
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MagitekSlashFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 2.2f);
        }
        if (_rotation != default)
            InitIfReady();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MagitekSlashFirst or AID.MagitekSlashFollowUps)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady()
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(Cone, D151MarkIIIBMagitekColossus.ArenaCenter, _rotation, _increment, _activation, 1.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class MagitektSlashVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if ((OID)actor.OID != OID.FireslashVoidzone)
            return;
        if (state == 0x001)
            _aoes.Add(new(MagitektSlashRotation.Cone, D151MarkIIIBMagitekColossus.ArenaCenter, actor.Rotation));
        else if (state == 0x004)
            _aoes.RemoveAll(x => x.Rotation == actor.Rotation);
    }
}

class D151MarkIIIBMagitekColossusStates : StateMachineBuilder
{
    public D151MarkIIIBMagitekColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CeruleumVent>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<JarringBlow>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<WildFireBeam>()
            .ActivateOnEnter<MagitektSlashRotation>()
            .ActivateOnEnter<MagitektSlashVoidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7855)]
public class D151MarkIIIBMagitekColossus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-180.591f, 68.498f), new ArenaBoundsCircle(20))
{
    public static readonly WPos ArenaCenter = new(-180.591f, 68.498f);
}
