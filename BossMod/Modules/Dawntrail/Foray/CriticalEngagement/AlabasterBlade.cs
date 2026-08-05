namespace BossMod.Dawntrail.Foray.CriticalEngagement.AlabasterBlade;

public enum OID : uint
{
    Boss = 0x4BBE, // R4.000, x1
    Helper = 0x233C, // R0.500, x34, Helper type
    GolemArrows = 0x4EBD, // R1.000, x4
    AlabasterGolem = 0x4BBF, // R1.650, x4
    LightAether = 0x4BC0, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50760, // Boss->player, no cast, single-target
    EmbrittlingBladeCast = 47171, // Boss->self, 5.0s cast, single-target
    EmbrittlingBlade = 47172, // Helper->self, no cast, ???
    Summon = 47154, // Boss->self, 3.0s cast, single-target
    FourfoldAttackOrder = 47155, // Boss->self, 10.0s cast, single-target
    AttackOrder = 47156, // Boss->self, no cast, single-target
    AcclaimSlow = 47157, // 4BBF->self, 12.0s cast, range 40 90-degree cone
    AcclaimFast = 47158, // 4BBF->self, 3.0s cast, range 40 90-degree cone
    OccultAeroIII = 47170, // Helper->self, 5.0s cast, range 50 width 10 rect
    RightLeftCombination = 47166, // Boss->self, 5.0s cast, range 40 180-degree cone
    LeftRightCombination = 47167, // Boss->self, 5.0s cast, range 40 180-degree cone
    ClearoutRight = 47168, // Boss->self, no cast, range 40 ?-degree cone
    ClearoutLeft = 47169, // Boss->self, no cast, range 40 ?-degree cone
    LightPrayer = 47159, // Boss->self, 3.0s cast, single-target
    OccultAero = 47163, // Helper->self, 5.0s cast, range 50 width 10 rect
    OccultTornado = 47165, // Helper->location, 5.0s cast, range 5 circle
    OccultStoneII = 47164, // Helper->self, 5.0s cast, range 40 60-degree cone
    FalseSpellbladeHolyCast = 47757, // Boss->self, 32.0s cast, single-target
    FalseSpellbladeHoly = 47161, // Helper->self, no cast, ???
}

public enum SID : uint
{
    Unk2056 = 2056, // none->4EBD, extra=0x43B/0x43D/0x43C
}

class EmbrittlingBlade(BossModule module) : Components.RaidwideCastDelay(module, AID.EmbrittlingBladeCast, AID.EmbrittlingBlade, 1.4f);

class Acclaim(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor Source, List<Angle> Angles, DateTime Next)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeCone(40, 45.Degrees()), c.Source.Position, c.Angles[0], c.Next));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AcclaimSlow)
            _casters.Add((caster, [spell.Rotation], Module.CastFinishAt(spell)));
        if ((AID)spell.Action.ID == AID.AcclaimFast)
        {
            var ix = _casters.FindIndex(c => c.Source == caster);
            if (ix >= 0)
                _casters.Ref(ix).Next = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AcclaimSlow or AID.AcclaimFast)
        {
            var c = _casters.FindIndex(c => c.Source == caster);
            if (c < 0)
            {
                ReportError($"Unable to find golem {caster} in caster list");
                return;
            }

            ref var c2 = ref _casters.Ref(c);

            c2.Next = WorldState.FutureTime(8.4f);
            if (c2.Angles.Count > 0)
                c2.Angles.RemoveAt(0);
            if (c2.Angles.Count == 0)
                _casters.RemoveAt(c);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Unk2056)
        {
            var ix = _casters.FindIndex(c => c.Source.Position.AlmostEqual(actor.Position, 1));
            if (ix < 0)
            {
                ReportError($"Unable to find golem {actor} in caster list");
                return;
            }

            var starting = _casters[ix].Angles[0];

            switch (status.Extra)
            {
                case 0x43B:
                    _casters[ix].Angles.AddRange([starting - 90.Degrees(), starting - 180.Degrees(), starting - 270.Degrees()]);
                    break;
                case 0x43C:
                    _casters[ix].Angles.AddRange([starting - 90.Degrees(), starting - 180.Degrees(), starting - 180.Degrees()]);
                    break;
                case 0x43D:
                    _casters[ix].Angles.AddRange(Enumerable.Repeat(starting - 90.Degrees(), 3));
                    break;
            }
        }
    }
}

class OccultAeroIII(BossModule module) : Components.StandardAOEs(module, AID.OccultAeroIII, new AOEShapeRect(50, 5));

class Combination(BossModule module) : Components.GenericAOEs(module)
{
    int _seq;

    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_seq == 1 && _predicted.Count > 0)
        {
            var aoe = _predicted[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(aoe.Origin, aoe.Rotation, 2, 2, 40), aoe.Activation.AddSeconds(2.1f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftRightCombination or AID.RightLeftCombination)
        {
            _seq = 1;

            _predicted.Add(new(new AOEShapeCone(40, 90.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _predicted.Add(new(new AOEShapeCone(40, 90.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LeftRightCombination or AID.RightLeftCombination)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);

            _seq = 2;
        }

        if ((AID)spell.Action.ID is AID.ClearoutRight or AID.ClearoutLeft)
        {
            _predicted.Clear();
        }
    }
}

class LightAether(BossModule module) : Components.Adds(module, (uint)OID.LightAether, 1, true);

class OccultAero(BossModule module) : Components.StandardAOEs(module, AID.OccultAero, new AOEShapeRect(50, 5))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).TakeSpan(TimeSpan.FromSeconds(1));
}

class OccultTornado(BossModule module) : Components.StandardAOEs(module, AID.OccultTornado, 5);
class OccultStoneII(BossModule module) : Components.StandardAOEs(module, AID.OccultStoneII, new AOEShapeCone(40, 30.Degrees()), 3);

class FalseSpellbladeHoly(BossModule module) : Components.RaidwideCastDelay(module, AID.FalseSpellbladeHolyCast, AID.FalseSpellbladeHoly, 0.9f);

class AlabasterBladeStates : StateMachineBuilder
{
    public AlabasterBladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmbrittlingBlade>()
            .ActivateOnEnter<Acclaim>()
            .ActivateOnEnter<OccultAeroIII>()
            .ActivateOnEnter<Combination>()
            .ActivateOnEnter<OccultAero>()
            .ActivateOnEnter<LightAether>()
            .ActivateOnEnter<OccultTornado>()
            .ActivateOnEnter<OccultStoneII>()
            .ActivateOnEnter<FalseSpellbladeHoly>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14509)]
public class AlabasterBlade(WorldState ws, Actor primary) : CEModule(ws, primary, new(-519, -641), new ArenaBoundsCircle(24.5f));

