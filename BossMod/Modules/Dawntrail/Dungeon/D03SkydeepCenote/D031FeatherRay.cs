namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D031FeatherRay;

public enum OID : uint
{
    Boss = 0x41D3, // R5.000, x1
    AiryBubble = 0x41D4, // R1.100-2.200, x36
    Helper = 0x233C, // R0.500, x5, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Immersion = 36739, // Boss->self, 5.0s cast, range 24 circle, raidwide
    TroublesomeTail = 36727, // Boss->self, 4.0s cast, range 24 circle, raidwide + mirror debuff
    WorrisomeWave = 36728, // Boss->self, 4.0s cast, range 24 30-degree cone
    WorrisomeWaveNuisance = 36729, // Helper->self, no cast, range 24 30-degree cone
    HydroRing = 36733, // Boss->self, 5.0s cast, range 12-24 donut, leaves persistent voidzone
    BlowingBubbles = 36732, // Boss->self, 3.0s cast, single-target, visual (start spawning small bubbles from boss)
    BubbleBomb = 36735, // Boss->self, 3.0s cast, single-target, visual (spawn large bubbles)
    RollingCurrent = 36737, // Boss->self, 5.0s cast, single-target, visual (move bubbles)
    RollingCurrentAOE = 38185, // Helper->self, 5.0s cast, range 68 width 32 rect, knock-forward 8 on bubbles
    Burst = 36738, // AiryBubble->self, 1.5s cast, range 6 circle
    TroubleBubbles = 38787, // Boss->self, 3.0s cast, single-target, visual (start spawning small bubbles from players)
    Pop = 36734, // AiryBubble->player, no cast, single-target, damage if bubble is touched
}

public enum IconID : uint
{
    Nuisance = 514, // player
}

class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));
class TroublesomeTail(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TroublesomeTail));
class WorrisomeWave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WorrisomeWave), new AOEShapeCone(24, 15.Degrees()));

class WorrisomeWaveNuisance(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(24, 15.Degrees()), (uint)IconID.Nuisance, ActionID.MakeSpell(AID.WorrisomeWaveNuisance), 5.4f)
{
    public override Actor? BaitSource(Actor target) => target;
}

class HydroRing(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.HydroRing))
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonut _shape = new(12, 24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 19 && state == 0x00080004)
            _aoe = null;
    }
}

class AiryBubbles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _bubbles = [];

    private static readonly AOEShapeCircle _shape = new(1.1f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _bubbles.Select(b => new AOEInstance(_shape, b.Position));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in _bubbles)
        {
            hints.AddForbiddenZone(_shape.Distance(s.Position, s.Rotation));
            hints.AddForbiddenZone(ShapeDistance.Capsule(s.Position, s.Rotation, 5, _shape.Radius), WorldState.FutureTime(2));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.AiryBubble || actor.HitboxRadius > _shape.Radius + 0.01f)
            return;
        switch (id)
        {
            case 0x1E46:
                _bubbles.Add(actor);
                break;
            case 0x1E3C:
                _bubbles.Remove(actor);
                break;
        }
    }
}

class BubbleBomb(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Burst))
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _directionKnown;

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _directionKnown ? _aoes : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.AiryBubble && actor.HitboxRadius > 2.1f && id == 0x1E46)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(10.6f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RollingCurrentAOE)
        {
            var offset = 8 * spell.Rotation.ToDirection();
            foreach (ref var aoe in _aoes.AsSpan())
                aoe.Origin += offset;
            _directionKnown = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _directionKnown = false;
            var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (count != 1)
                ReportError($"Failed to find aoe @ {caster.Position}");
        }
    }
}

class D031FeatherRayStates : StateMachineBuilder
{
    public D031FeatherRayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<TroublesomeTail>()
            .ActivateOnEnter<WorrisomeWave>()
            .ActivateOnEnter<WorrisomeWaveNuisance>()
            .ActivateOnEnter<HydroRing>()
            .ActivateOnEnter<AiryBubbles>()
            .ActivateOnEnter<BubbleBomb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12755)]
public class D031FeatherRay(WorldState ws, Actor primary) : BossModule(ws, primary, new(-105, -160), new ArenaBoundsSquare(15));
