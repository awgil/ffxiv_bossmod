namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE18FeelingTheBurn;

public enum OID : uint
{
    Boss = 0x31A0, // R3.640, x1
    Escort1 = 0x31A1, // R2.800, x24
    Escort2 = 0x32FD, // R2.800, spawn during fight
    Helper = 0x233C, // R0.500, x26
}

public enum AID : uint
{
    AutoAttackBoss = 24281, // Boss->player, no cast, single-target
    AutoAttackEscort = 24319, // Escort2->player, no cast, single-target
    ReadOrdersCoordinatedAssault = 23604, // Boss->self, 19.0s cast, single-target, visual
    DiveFormation = 23606, // Escort1->self, 5.0s cast, range 60 width 6 rect aoe
    AntiPersonnelMissile1 = 23609, // Boss->self, 10.0s cast, single-target, visual (3 impact pairs)
    AntiPersonnelMissile2 = 23618, // Boss->self, 12.0s cast, single-target, visual (4 impact pairs)
    BallisticImpact = 23610, // Helper->self, no cast, range 24 width 24 rect aoe
    ReadOrdersShotsFired = 23611, // Boss->self, 3.0s cast, single-target, visual
    ChainCannonEscort = 23612, // Escort2->self, 1.0s cast, range 60 width 5 rect visual
    ChainCannonEscortAOE = 23613, // Helper->self, no cast, range 60 width 5 rect 'voidzone'
    ChainCannonBoss = 24658, // Boss->self, 3.0s cast, range 60 width 6 rect, visual
    ChainCannonBossAOE = 24659, // Helper->self, no cast, range 60 width 6 rect
    SurfaceMissile = 23614, // Boss->self, 3.0s cast, single-target, visual
    SurfaceMissileAOE = 23615, // Helper->location, 3.0s cast, range 6 circle puddle
    SuppressiveMagitekRays = 23616, // Boss->self, 5.0s cast, single-target, visual
    SuppressiveMagitekRaysAOE = 23617, // Helper->self, 5.5s cast, ???, raidwide
    Analysis = 23607, // Boss->self, 3.0s cast, single-target, visual
    PreciseStrike = 23619, // Escort1->self, 5.0s cast, range 60 width 6 rect aoe (should orient correctly to avoid vuln)
}

public enum SID : uint
{
    Tracking = 2056, // none->Escort2, extra=0x87
    FrontUnseen = 2644, // Boss->player, extra=0x120
    BackUnseen = 1709, // Boss->player, extra=0xE8
}

public enum IconID : uint
{
    BallisticImpact = 261, // Helper
    ChainCannon = 164, // player
}

class DiveFormation(BossModule module) : Components.StandardAOEs(module, AID.DiveFormation, new AOEShapeRect(60, 3));

class AntiPersonnelMissile(BossModule module) : Components.GenericAOEs(module, AID.BallisticImpact)
{
    private readonly List<WPos> _positions = [];
    private static readonly AOEShapeRect _shape = new(12, 12, 12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _positions.Take(2).Select(p => new AOEInstance(_shape, p));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // TODO: activation time (icon pairs are ~3s apart, but explosion pairs are ~2.6s apart; first explosion is ~2.1s after visual cast end)
        if ((IconID)iconID == IconID.BallisticImpact)
            _positions.Add(actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _positions.Count > 0)
            _positions.RemoveAt(0);
    }
}

class ChainCannonEscort(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, int numCasts, DateTime activation)> _casters = [];
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casters.Where(c => !IsTrackingPlayer(c, actor)).Select(c => new AOEInstance(_shape, c.caster.Position, c.caster.Rotation, c.activation));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in _casters.Where(c => IsTrackingPlayer(c, pc)))
            _shape.Outline(Arena, c.caster);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Tracking)
            _casters.Add((actor, 0, status.ExpireAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ChainCannonEscortAOE)
        {
            var index = _casters.FindIndex(c => c.caster.Position.AlmostEqual(caster.Position, 1));
            if (index >= 0)
            {
                int numCasts = _casters[index].numCasts + 1;
                if (numCasts >= 6)
                    _casters.RemoveAt(index);
                else
                    _casters[index] = (_casters[index].caster, numCasts, WorldState.FutureTime(1));
            }
        }
    }

    private bool IsTrackingPlayer((Actor caster, int numCasts, DateTime activation) c, Actor actor) => c.numCasts == 0 && c.caster.CastInfo == null && c.caster.TargetID == actor.InstanceID;
}

class ChainCannonBoss(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _instance;
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_instance);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChainCannonBoss)
            _instance = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ChainCannonBossAOE)
        {
            if (++NumCasts >= 4)
            {
                _instance = null;
                NumCasts = 0;
            }
            else
            {
                _instance = new(_shape, caster.Position, caster.Rotation, WorldState.FutureTime(1));
            }
        }
    }
}

class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissileAOE, 6);
class SuppressiveMagitekRays(BossModule module) : Components.RaidwideCast(module, AID.SuppressiveMagitekRays);
class Analysis(BossModule module) : Components.CastHint(module, AID.Analysis, "Face open weakpoint to charging adds");
class PreciseStrike(BossModule module) : Components.CastWeakpoint(module, AID.PreciseStrike, new AOEShapeRect(60, 3), (uint)SID.FrontUnseen, (uint)SID.BackUnseen, 0, 0);

class BlackburnStates : StateMachineBuilder
{
    public BlackburnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiveFormation>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<ChainCannonEscort>()
            .ActivateOnEnter<ChainCannonBoss>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<SuppressiveMagitekRays>()
            .ActivateOnEnter<Analysis>()
            .ActivateOnEnter<PreciseStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 18)] // bnpcname=9945
public class Blackburn : BossModule
{
    public IReadOnlyList<Actor> Escorts;

    public Blackburn(WorldState ws, Actor primary) : base(ws, primary, new(-240, -230), new ArenaBoundsSquare(24))
    {
        Escorts = Enemies(OID.Escort2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Escorts, ArenaColor.Enemy);
    }
}
