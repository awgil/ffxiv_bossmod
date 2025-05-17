namespace BossMod.Stormblood.Dungeon.D15GhimlytDark.D153JuliaQuoSoranus;

public enum OID : uint
{
    Boss = 0x25AF, // R0.600, x1 // Depends on which is attacked 1st? Managed to be 3/3 on starting the fight but... FF RNG at it's finest
    AnniaQuoSoranus = 0x25B0, // R0.600, x1
    CeruleumTank = 0x25B2, // R1.000, x0 (spawn during fight)
    SoranusDuo1 = 0x25B1, // R0.000, x1 (spawn during fight)
    SoranusDuo2 = 0x25B5, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x35, Helper type
}

public enum AID : uint
{
    Teleport1 = 14095, // AnniaQuoSoranus->location, no cast, single-target
    Teleport2 = 14094, // Boss->location, no cast, single-target
    AutoAttackAnnia = 872, // AnniaQuoSoranus->player, no cast, single-target
    AutoAttackJulia = 870, // Boss->player, no cast, single-target

    AglaeaBite = 14103, // AnniaQuoSoranus->CeruleumTank, no cast, single-target
    AngrySalamander = 14124, // AnniaQuoSoranus->self, 3.0s cast, range 45+R width 6 rect
    ArtificialBoostAnnia = 14128, // AnniaQuoSoranus->self, 3.0s cast, single-target
    ArtificialBoostJulia = 14127, // Boss->self, 3.0s cast, single-target 
    ArtificialPlasmaAnnia = 14120, // AnniaQuoSoranus->self, 3.0s cast, range 40+R circle, Raidwide
    ArtificialPlasmaJulia = 14119, // Boss->self, 3.0s cast, range 40+R circle, Raidwide
    Bombardment = 14097, // Helper->location, 7.0s cast, range 10 circle
    Burst = 14106, // CeruleumTank->self, 2.0s cast, range 10+R circle
    CommenceAirStrike = 14102, // Boss->self, 3.0s cast, single-target
    CoveringFire = 14108, // Helper->player, 5.0s cast, range 8 circle
    Crossbones = 15488, // SoranusDuo1->player, 5.0s cast, width 4 rect charge, knockback 15, away from source

    CrosshatchVisualCast = 14113, // SoranusDuo1->self, no cast, single-target
    CrosshatchHelper = 14114, // Helper->self, no cast, range 40+R width 4 rect
    CrossHatch1 = 14115, // Helper->self, 3.5s cast, range 20+R width 4 rect
    CrossHatch2 = 14411, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrossHatch3 = 14412, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrossHatch4 = 14116, // Helper->self, 3.5s cast, range 40+R width 4 rect
    CrossHatch5 = 14698, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrossHatch6 = 14697, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrossHatch7 = 14413, // Helper->self, 3.5s cast, range 29+R width 4 rect
    CrossHatch8 = 14699, // Helper->self, 3.5s cast, range 40+R width 4 rect

    DeltaTrance = 14122, // AnniaQuoSoranus->player, 4.0s cast, single-target, tankbuster
    Heirsbane = 14105, // Boss->CeruleumTank, 3.5s cast, single-target
    ImperialAuthorityAnnia = 14130, // AnniaQuoSoranus->self, 40.0s cast, range 80 circle
    ImperialAuthorityJulia = 14129, // Boss->self, 40.0s cast, range 80 circle
    Innocence = 14121, // Boss->player, 4.0s cast, single-target
    MissileImpact = 14126, // Helper->location, 3.0s cast, range 6 circle
    OrderToBombard = 14096, // Boss->self, 5.0s cast, single-target
    OrderToFire = 14125, // AnniaQuoSoranus->self, 3.0s cast, single-target
    OrderToSupport = 14107, // AnniaQuoSoranus->self, 3.0s cast, single-target
    Quaternity1 = 14131, // SoranusDuo1->self, 3.0s cast, range 40+R width 4 rect
    Quaternity2 = 14729, // SoranusDuo2->self, 3.0s cast, range 25+R width 4 rect
    Roundhouse = 14104, // AnniaQuoSoranus->self, no cast, range 6+R circle
    StunningSweep = 14098, // AnniaQuoSoranus->self, 4.0s cast, range 6+R circle
    TheOrderCast = 14778, // Boss->self, 3.0s cast, single-target
    TheOrderInstaCast = 14099, // Boss->self, no cast, single-target
}

class AngrySalamander(BossModule module) : Components.StandardAOEs(module, AID.AngrySalamander, new AOEShapeRect(45.6f, 3));
class ArtificialPlasmaAnnia(BossModule module) : Components.RaidwideCast(module, AID.ArtificialPlasmaAnnia);
class ArtificialPlasmaJulia(BossModule module) : Components.RaidwideCast(module, AID.ArtificialPlasmaJulia);
class Bombardment(BossModule module) : Components.StandardAOEs(module, AID.Bombardment, 10);
class CoveringFire(BossModule module) : Components.SpreadFromCastTargets(module, AID.CoveringFire, 8);
class DeltaTrance(BossModule module) : Components.SingleTargetCast(module, AID.DeltaTrance);
class Heirsbane(BossModule module) : Components.SingleTargetCast(module, AID.Heirsbane, "Single target, moderate damage"); // wiki says this is targeted, but it's aiming at the tank? Need to source this out more
class ImperialAuthorityAnnia(BossModule module) : Components.RaidwideCast(module, AID.ImperialAuthorityAnnia, "Enrage!");
class ImperialAuthorityJulia(BossModule module) : Components.RaidwideCast(module, AID.ImperialAuthorityJulia, "Enrage!");
class Innocence(BossModule module) : Components.SingleTargetCast(module, AID.Innocence);
class MissileImpact(BossModule module) : Components.StandardAOEs(module, AID.MissileImpact, 6);
class Quaternity1(BossModule module) : Components.StandardAOEs(module, AID.Quaternity1, new AOEShapeRect(41, 2));
class Quaternity2(BossModule module) : Components.StandardAOEs(module, AID.Quaternity2, new AOEShapeRect(26, 2));
class StunningSweep(BossModule module) : Components.StandardAOEs(module, AID.StunningSweep, new AOEShapeCircle(6.6f));

class CeruleumTanks(BossModule module) : Components.GenericAOEs(module) //following things were graciously provided by Malediktus
{
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly AOEShapeCircle circleRoundhouse = new(6.6f);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos[] origins = [new(370.992f, -277.028f), new(362.514f, -273.49f), new(379.485f, -273.49f), new(358.999f, -265.034f),
    new(382.986f, -265.034f), new(379.485f, -256.52f), new(362.514f, -256.52f), new(370.992f, -253.01f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count == 9 ? 4 : _aoes.Count == 8 ? 3 : 2;
        return _aoes.Skip(count).Take(4).Concat(_aoes.Take(count).Select(a => a with { Color = ArenaColor.Danger }));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.CeruleumTank && _aoes.Count == 0)
        {
            for (var i = 0; i < origins.Length; i++)
            {
                float activation;
                if (_aoes.Count == 0)
                    activation = 8.8f;
                else
                {
                    var set = (_aoes.Count + 1) / 2;
                    activation = 8.8f + 2.4f * set;
                }
                _aoes.Add(new(circle, origins[i], default, WorldState.FutureTime(activation)));
            }
            _aoes.Add(new(circleRoundhouse, new(370.992f, -265.034f), default, WorldState.FutureTime(3.1f)));
            _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.Roundhouse or AID.Burst)
            _aoes.RemoveAt(0);
    }
}

class Crosshatch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40.5f, 2);
    private static readonly HashSet<AID> telegraphs = [AID.CrossHatch1, AID.CrossHatch2, AID.CrossHatch3, AID.CrossHatch4,
    AID.CrossHatch5, AID.CrossHatch6, AID.CrossHatch7, AID.CrossHatch8];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
        for (var i = 1; i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (telegraphs.Contains((AID)spell.Action.ID))
            _aoes.Add(new(rect, caster.Position, spell.Rotation, _aoes.Count == 0 ? Module.CastFinishAt(spell, 2.1f) : _aoes[0].Activation.AddSeconds(_aoes.Count * 0.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.CrosshatchHelper)
            _aoes.RemoveAt(0);
    }
}

class Crossbones(BossModule module) : Components.BaitAwayChargeCast(module, AID.Crossbones, 2);
class CrossbonesKB(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private DateTime _activation;
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster != null)
            yield return new(_caster.Position, 15, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Crossbones)
        {
            _activation = Module.CastFinishAt(spell);
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Crossbones)
            _caster = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<Bombardment>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class D153JuliaQuoSoranusStates : StateMachineBuilder
{
    public D153JuliaQuoSoranusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AngrySalamander>()
            .ActivateOnEnter<ArtificialPlasmaAnnia>()
            .ActivateOnEnter<ArtificialPlasmaJulia>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<CoveringFire>()
            .ActivateOnEnter<DeltaTrance>()
            .ActivateOnEnter<Heirsbane>()
            .ActivateOnEnter<ImperialAuthorityAnnia>()
            .ActivateOnEnter<ImperialAuthorityJulia>()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<MissileImpact>()
            .ActivateOnEnter<Quaternity1>()
            .ActivateOnEnter<Quaternity2>()
            .ActivateOnEnter<StunningSweep>()
            .ActivateOnEnter<CeruleumTanks>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<Crossbones>()
            .ActivateOnEnter<CrossbonesKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7857)]
public class D153JuliaQuoSoranus(WorldState ws, Actor primary) : BossModule(ws, primary, new(371, -265.03f), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.AnniaQuoSoranus), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 2,
                OID.AnniaQuoSoranus => 1,
                _ => 0
            };
        }
    }
    protected override bool CheckPull() => Enemies(OID.AnniaQuoSoranus).Concat([PrimaryActor]).Any(x => x.InCombat);
}
