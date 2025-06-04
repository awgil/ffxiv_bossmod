#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Foray.FATE.GildedHeadstone;

public enum OID : uint
{
    Boss = 0x471A,
    Helper = 0x471B,
}

public enum AID : uint
{
    AutoAttack = 41788, // Boss->player, no cast, single-target
    ErosiveEyeCast = 41791, // Boss->self, 4.0s cast, single-target, visual only
    ErosiveEye = 41792, // Helper->location, 5.0s cast, range 50 circle, regular gaze?
    ErosiveEyeSlow = 41793, // Helper->location, 8.0s cast, range 50 circle, regular gaze
    ErosiveEyeInverse = 41794, // Helper->location, 5.0s cast, range 50 circle, inverted gaze?
    ErosiveEyeInverseSlow = 41795, // Helper->location, 8.0s cast, range 50 circle, inverted gaze
    FlamingEpigraphCast = 41802, // Boss->self, 5.6+0.4s cast, single-target
    FlamingEpigraph = 41803, // Helper->location, 6.0s cast, range 60 60-degree cone
    Unk1 = 41789, // Boss->location, no cast, single-target
    Epigraph = 41790, // Boss->self, 4.0s cast, range 45 width 5 rect
    EpigraphicFireIICast = 41804, // Boss->self, 2.6+0.4s cast, single-target
    EpigraphicFireII = 41805, // Helper->location, 3.0s cast, range 5 circle
    WideningTwinflame = 41796, // Boss->self, 4.6+0.4s cast, single-target
    TongueOfFlameFast = 41797, // Helper->location, 5.0s cast, range 10 circle
    TongueOfFlameSlow = 41798, // Helper->location, 7.0s cast, range 10 circle
    NarrowingTwinflame = 41799, // Boss->self, 4.6+0.4s cast, single-target
    LickOfFlameFast = 41800, // Helper->location, 5.0s cast, range 10?-40 donut
    LickOfFlameSlow = 41801, // Helper->location, 7.0s cast, range 10?-40 donut
    FlaringEpigraphCast = 41808, // Boss->self, 4.6+0.4s cast, single-target
    FlaringEpigraph = 41809, // Helper->location, 5.0s cast, range 60 circle
}

class Epigraph(BossModule module) : Components.StandardAOEs(module, AID.Epigraph, new AOEShapeRect(45, 2.5f));
class FlaringEpigraph(BossModule module) : Components.RaidwideCast(module, AID.FlaringEpigraph);
class FlamingEpigraph(BossModule module) : Components.StandardAOEs(module, AID.FlamingEpigraph, new AOEShapeCone(60, 30.Degrees()), maxCasts: 4, highlightImminent: true);
class EpigraphicFireII(BossModule module) : Components.StandardAOEs(module, AID.EpigraphicFireII, 5);

class Twinflame(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TongueOfFlameSlow:
            case AID.TongueOfFlameFast:
                _predicted.Add(new(new AOEShapeCircle(10), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
            case AID.LickOfFlameFast:
            case AID.LickOfFlameSlow:
                _predicted.Add(new(new AOEShapeDonut(10, 40), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TongueOfFlameSlow or AID.TongueOfFlameFast or AID.LickOfFlameFast or AID.LickOfFlameSlow)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class ErosiveEye(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<(WPos Source, DateTime Activation, bool Inverted)> _casts = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_casts.Count > 0)
            yield return new(_casts[0].Source, _casts[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ErosiveEyeInverse:
            case AID.ErosiveEyeInverseSlow:
                _casts.Add((spell.LocXZ, Module.CastFinishAt(spell), true));
                _casts.SortBy(c => c.Activation);
                Inverted = _casts[0].Inverted;
                break;
            case AID.ErosiveEye:
            case AID.ErosiveEyeSlow:
                _casts.Add((spell.LocXZ, Module.CastFinishAt(spell), false));
                _casts.SortBy(c => c.Activation);
                Inverted = _casts[0].Inverted;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ErosiveEyeInverse or AID.ErosiveEye or AID.ErosiveEyeSlow or AID.ErosiveEyeInverseSlow)
        {
            NumCasts++;
            if (_casts.Count > 0)
                _casts.RemoveAt(0);
            if (_casts.Count > 0)
                Inverted = _casts[0].Inverted;
        }
    }
}

class GildedHeadstoneStates : StateMachineBuilder
{
    public GildedHeadstoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Epigraph>()
            .ActivateOnEnter<Twinflame>()
            .ActivateOnEnter<ErosiveEye>()
            .ActivateOnEnter<FlaringEpigraph>()
            .ActivateOnEnter<FlamingEpigraph>()
            .ActivateOnEnter<EpigraphicFireII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13702)]
public class GildedHeadstone(WorldState ws, Actor primary) : BossModule(ws, primary, new(373.2f, 486), new ArenaBoundsCircle(40));
