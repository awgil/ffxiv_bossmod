#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

public enum OID : uint
{
    Boss = 0x48E5,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Weaponskill_Roar = 43950, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_ChainbladeBlow = 43888, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ChainbladeBlow1 = 45077, // Helper->self, 6.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow2 = 45078, // Helper->self, 6.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance = 43892, // Helper->self, 7.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow3 = 43893, // Boss->self, no cast, single-target
    _Weaponskill_ChainbladeBlow4 = 43895, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow5 = 43896, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance1 = 43897, // Helper->self, 2.2s cast, range 80 width 28 rect
    _Weaponskill_ = 45175, // Boss->location, no cast, single-target
    _Weaponskill_GuardianSiegeflight = 43899, // Boss->location, 5.0s cast, range 40 width 4 rect
    _Weaponskill_GuardianSiegeflight1 = 43900, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_1 = 45125, // Helper->self, 7.2s cast, range 40 width 8 rect
    _Weaponskill_WhiteFlash = 43906, // Helper->players, 8.0s cast, range 6 circle
    _Weaponskill_GuardianResonance = 43901, // Helper->self, 10.0s cast, range 40 width 16 rect
    _Weaponskill_WyvernsRattle = 43939, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsRadiance2 = 43940, // Helper->self, 2.5s cast, range 8 width 40 rect
    _Weaponskill_WyvernsRadiance3 = 43942, // Helper->location, 5.0s cast, range 6 circle
    _Weaponskill_WyvernsRadiance4 = 43941, // Helper->self, 1.0s cast, range 8 width 40 rect
    _Weaponskill_WyvernsSiegeflight = 43902, // Boss->location, 5.0s cast, range 40 width 4 rect
    _Weaponskill_WyvernsSiegeflight1 = 43903, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_2 = 45111, // Helper->self, 7.2s cast, range 40 width 8 rect
    _Weaponskill_WyvernsRadiance5 = 43904, // Helper->self, 10.0s cast, range 40 width 18 rect
    _Weaponskill_WyvernsRadiance6 = 43905, // Helper->self, 10.0s cast, range 40 width 18 rect
    _Weaponskill_Dragonspark = 43907, // Helper->players, 8.0s cast, range 6 circle
    _Weaponskill_ChainbladeBlow6 = 43887, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ChainbladeBlow7 = 43889, // Helper->self, 6.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow8 = 43890, // Helper->self, 6.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance7 = 43891, // Helper->self, 7.2s cast, range 80 width 28 rect
    _Weaponskill_ChainbladeBlow9 = 43894, // Boss->self, no cast, single-target
    _Weaponskill_ChainbladeBlow10 = 45079, // Helper->self, 1.2s cast, range 40 width 4 rect
    _Weaponskill_ChainbladeBlow11 = 45080, // Helper->self, 1.6s cast, range 40 width 4 rect
    _Weaponskill_WyvernsRadiance8 = 43898, // Helper->self, 2.2s cast, range 80 width 28 rect
    _AutoAttack_ = 43342, // Boss->player, no cast, single-target
    _Weaponskill_3 = 43908, // Helper->location, 2.5s cast, width 12 rect charge
    _Weaponskill_4 = 45110, // Helper->self, 3.5s cast, range 8 circle
    _Weaponskill_Rush = 43909, // Boss->location, 6.0s cast, width 12 rect charge
    _Weaponskill_WyvernsRadiance9 = 43911, // Helper->self, 7.5s cast, range 8 circle
    _Weaponskill_Rush1 = 43910, // Boss->location, no cast, width 12 rect charge
    _Weaponskill_WyvernsRadiance10 = 43912, // Helper->self, 9.5s cast, range 8-14 donut
    _Weaponskill_WyvernsRadiance11 = 43913, // Helper->self, 11.5s cast, range 14-20 donut
    _Weaponskill_WyvernsRadiance12 = 43914, // Helper->self, 13.5s cast, range 20-26 donut
    _Weaponskill_5 = 43827, // Boss->location, no cast, single-target
    _Weaponskill_WyvernsOuroblade = 43917, // Boss->self, 6.0+1.5s cast, single-target
    _Weaponskill_WyvernsOuroblade1 = 43918, // Helper->self, 7.0s cast, range 40 180-degree cone
    _Weaponskill_WildEnergy = 43932, // Helper->player, 8.0s cast, range 6 circle
    _Weaponskill_SteeltailThrust = 43949, // Boss->self, 4.0s cast, range 60 width 6 rect
    _Weaponskill_SteeltailThrust1 = 44805, // Helper->self, 4.6s cast, range 60 width 6 rect
    _Weaponskill_ChainbladeCharge = 43947, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ChainbladeCharge1 = 43948, // Boss->player, no cast, single-target
    _Weaponskill_ChainbladeCharge2 = 44812, // Helper->location, no cast, range 6 circle
    _Weaponskill_GuardianResonance1 = 43923, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_AethericResonance = 43919, // Boss->self, 9.7+1.3s cast, single-target
    _Weaponskill_GuardianResonance2 = 43920, // Helper->location, 11.0s cast, range 2 circle
    _Weaponskill_GuardianResonance3 = 43921, // Helper->location, 11.0s cast, range 4 circle
    _Weaponskill_GreaterResonance = 43922, // Helper->location, no cast, range 60 circle
    _Weaponskill_6 = 43859, // Boss->self, no cast, single-target
    _Weaponskill_WyvernsVengeance = 43926, // Helper->self, 5.0s cast, range 6 circle
    _Weaponskill_WyvernsVengeance1 = 43927, // Helper->location, no cast, range 6 circle
    _Weaponskill_WyvernsRadiance13 = 43924, // 48E6->self, 0.5s cast, range 6 circle
    _Weaponskill_WyvernsRadiance14 = 43925, // 48E7->self, 0.5s cast, range 12 circle
    _Weaponskill_WyvernsRadiance15 = 44809, // Helper->self, 1.0s cast, range 6 circle
    _Weaponskill_WyvernsRadiance16 = 44810, // Helper->self, 1.0s cast, range 12 circle
    _Weaponskill_ForgedFury = 43934, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ForgedFury1 = 43935, // Helper->self, 7.0s cast, range 60 circle
    _Weaponskill_ForgedFury2 = 44792, // Helper->self, 7.8s cast, range 60 circle
    _Weaponskill_ForgedFury3 = 44793, // Helper->self, 10.2s cast, range 60 circle
    _Weaponskill_Roar1 = 45202, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_ClamorousChase = 43955, // Boss->self, 8.0s cast, single-target
    _Weaponskill_ClamorousChase1 = 43956, // Boss->location, no cast, range 6 circle
    _Weaponskill_ClamorousChase2 = 43957, // Helper->self, 1.0s cast, range 60 180-degree cone
}

class ChainbladeBlow(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ChainbladeBlow1, AID._Weaponskill_ChainbladeBlow2, AID._Weaponskill_ChainbladeBlow4, AID._Weaponskill_ChainbladeBlow5, AID._Weaponskill_ChainbladeBlow7, AID._Weaponskill_ChainbladeBlow8, AID._Weaponskill_ChainbladeBlow10, AID._Weaponskill_ChainbladeBlow11], new AOEShapeRect(40, 2));

class ChainbladeRadiance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance, AID._Weaponskill_WyvernsRadiance1, AID._Weaponskill_WyvernsRadiance7, AID._Weaponskill_WyvernsRadiance8], new AOEShapeRect(80, 14));

class WhiteFlash(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_WhiteFlash, 6, maxStackSize: 4);
class Dragonspark(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_Dragonspark, 6, maxStackSize: 4);

class GuardianSiegeflight(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GuardianSiegeflight1, new AOEShapeRect(40, 4));
class GuardianResonance(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GuardianResonance, new AOEShapeRect(40, 8));

// wyvern exaflare AID._Weaponskill_WyvernsRadiance2 => AID._Weaponskill_WyvernsRadiance4
// 2.5s delay, (0, 8) advance

class WyvernsRadiancePuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WyvernsRadiance3, 6);
class WyvernsRadianceExaflare(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance2)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.5f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsRadiance2 or AID._Weaponskill_WyvernsRadiance4 && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], caster.Position);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);
    }
}

class WyvernsSiegeflightBoss(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WyvernsSiegeflight, new AOEShapeRect(40, 2));
class WyvernsSiegeflightHelper(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WyvernsSiegeflight1, new AOEShapeRect(40, 4));

class WyvernsRadianceSides(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance5, AID._Weaponskill_WyvernsRadiance6], new AOEShapeRect(40, 9));

class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 14), new AOEShapeDonut(14, 20), new AOEShapeDonut(20, 26)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance9)
            AddSequence(caster.Position, Module.CastFinishAt(spell), default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_WyvernsRadiance9 => 0,
            AID._Weaponskill_WyvernsRadiance10 => 1,
            AID._Weaponskill_WyvernsRadiance11 => 2,
            AID._Weaponskill_WyvernsRadiance12 => 3,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.95f));
    }
}

class Ex6GuardianArkveldStates : StateMachineBuilder
{
    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<ChainbladeRadiance>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>()
            .ActivateOnEnter<GuardianSiegeflight>()
            .ActivateOnEnter<GuardianResonance>()
            .ActivateOnEnter<WyvernsRadiancePuddle>()
            .ActivateOnEnter<WyvernsRadianceExaflare>()
            .ActivateOnEnter<WyvernsSiegeflightBoss>()
            .ActivateOnEnter<WyvernsSiegeflightHelper>()
            .ActivateOnEnter<WyvernsRadianceSides>()
            .ActivateOnEnter<WyvernsRadianceQuake>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1044, NameID = 14237, DevOnly = true)]
public class Ex6GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
