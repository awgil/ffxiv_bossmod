namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D064AscianPrime;

public enum OID : uint
{
    Boss = 0x3DA7, // R3.8
    LahabreasShade = 0x3DAB, // R3.5
    IgeyorhmsShade = 0x3DAA, // R3.5
    FrozenStar = 0x3DA8, // R1.5
    BurningStar = 0x3DA9, // R1.5
    ArcaneSphere = 0x3DAC, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    AncientCircle = 31901, // Helper->self, no cast, range 10-20 donut, player targeted donut AOE, kind of a stack
    AncientDarkness = 31903, // Helper->self, no cast, range 6 circle

    AncientEruptionVisual = 31908, // Boss->self, 5.0s cast, single-target
    AncientEruption = 31909, // Helper->location, 5.0s cast, range 5 circle

    AncientFrost = 31904, // Helper->players, no cast, range 6 circle

    Annihilation = 31927, // Boss->location, no cast, single-target
    AnnihilationAOE = 33024, // Helper->self, 6.3s cast, range 40 circle
    AnnihilationEnrage = 31928, // Boss->location, no cast, single-target, if Arcane Sphere doesn't get destroyed in time
    AnnihilationEnrageAOE = 33025, // Helper->self, 6.3s cast, range 40 circle

    ArcaneRevelation1 = 31912, // Boss->location, no cast, single-target, teleport
    ArcaneRevelation2 = 31913, // Boss->self, 3.0s cast, single-target
    BurningChains = 31905, // Helper->player, no cast, single-target

    ChillingCrossVisual = 31922, // IgeyorhmsShade->self, 6.0s cast, single-target
    ChillingCross1 = 31923, // Helper->self, 6.0s cast, range 40 width 5 cross
    ChillingCross2 = 31924, // Helper->self, 6.0s cast, range 40 width 5 cross

    CircleOfIcePrimeVisual1 = 31898, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeVisual2 = 31899, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime = 33021, // Helper->self, 2.0s cast, range 5-40 donut
    FireSpherePrimeVisual1 = 31896, // BurningStar->self, no cast, single-target
    FireSpherePrimeVisual2 = 31897, // BurningStar->self, no cast, single-target
    FireSpherePrime = 33022, // Helper->self, 2.0s cast, range 16 circle

    DarkBlizzardIIIVisual = 31914, // IgeyorhmsShade->self, 6.0s cast, single-target
    DarkBlizzardIII1 = 31915, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII2 = 31916, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII3 = 31917, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII4 = 31918, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII5 = 31919, // Helper->self, 6.0s cast, range 41 20-degree cone

    DarkFireIIVisual = 31920, // LahabreasShade->self, 6.0s cast, single-target
    DarkFireII = 31921, // Helper->players, 6.0s cast, range 6 circle

    Dualstar = 31894, // Boss->self, 4.0s cast, single-target

    EntropicFlame = 32126, // Boss->self, no cast, single-target
    EntropicFlameCast = 31906, // Boss->self, 5.0s cast, single-target
    EntropicFlameTargetSelect = 31907, // Helper->player, no cast, single-target
    EntropicFlameVisual = 32555, // Helper->self, no cast, range 50 width 8 rect, line stack

    FusionPrime = 31895, // Boss->self, 3.0s cast, single-target

    HeightOfChaos = 31911, // Boss->player, 5.0s cast, range 5 circle

    ShadowFlare1 = 31910, // Boss->self, 5.0s cast, range 40 circle
    ShadowFlare2 = 31925, // IgeyorhmsShade->self, 5.0s cast, range 40 circle
    ShadowFlare3 = 31926, // LahabreasShade->self, 5.0s cast, range 40 circle

    UniversalManipulationTeleport = 31419, // Boss->location, no cast, single-target
    UniversalManipulation = 31900, // Boss->self, 5.0s cast, range 40 circle
    UniversalManipulation2 = 33044 // Boss->player, no cast, single-target
}

public enum SID : uint
{
    AncientCircle = 3534, // none->player, extra=0x0 Player targeted donut AOE
    AncientFrost = 3506, // none->player, extra=0x0 Stack marker
    Bleeding = 2088, // Boss->player, extra=0x0
    BurningChains1 = 3505, // none->player, extra=0x0
    BurningChains2 = 769, // none->player, extra=0x0
    DarkWhispers = 3535, // none->player, extra=0x0 Spread marker
    Untargetable = 2056 // Boss->Boss, extra=0x231, before limitbreak phase
}

public enum IconID : uint
{
    Tankbuster = 343, // player
    AncientCircle = 384, // player
    DarkWhispers = 139, // player
    AncientFrost = 161, // player
    BurningChains = 97, // player
    DarkFire = 311 // player
}

public enum TetherID : uint
{
    StarTether = 110, // FrozenStar/BurningStar->FrozenStar/BurningStar
    BurningChains = 9, // player->player
    ArcaneSphere = 197 // ArcaneSphere->Boss
}

class AncientCircle(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeDonut(10, 20), (uint)IconID.AncientCircle, AID.AncientCircle, 8, true);

class DarkWhispers(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    // regular spread component won't work because this is self targeted
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DarkWhispers)
            AddSpread(actor, WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AncientDarkness)
            Spreads.Clear();
    }
}

class AncientFrost(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AncientFrost, AID.AncientFrost, 6, 5, 4, 4);
class ShadowFlare(BossModule module) : Components.RaidwideCast(module, AID.ShadowFlare1);
class ShadowFlareLBPhase(BossModule module) : Components.RaidwideCast(module, AID.ShadowFlare2, "Raidwide x2");
class Annihilation(BossModule module) : Components.RaidwideCast(module, AID.AnnihilationAOE);
class UniversalManipulation(BossModule module) : Components.RaidwideCast(module, AID.UniversalManipulation, "Raidwide + Apply debuffs for later");

class HeightOfChaos(BossModule module) : Components.BaitAwayCast(module, AID.HeightOfChaos, new AOEShapeCircle(5), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class AncientEruption(BossModule module) : Components.StandardAOEs(module, AID.AncientEruption, 5);

class ChillingCross(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCross(40, 2.5f));
class ChillingCross1(BossModule module) : ChillingCross(module, AID.ChillingCross1);
class ChillingCross2(BossModule module) : ChillingCross(module, AID.ChillingCross2);

class DarkBlizzard(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE1(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII1);
class DarkBlizzardIIIAOE2(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII2);
class DarkBlizzardIIIAOE3(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII3);
class DarkBlizzardIIIAOE4(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII4);
class DarkBlizzardIIIAOE5(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII5);

class DarkFireII(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkFireII, 6);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, AID.BurningChains, 15);
class EntropicFlame(BossModule module) : Components.SimpleLineStack(module, 4, 50, AID.EntropicFlameTargetSelect, AID.EntropicFlameCast, 0);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5, 40);
    private static readonly AOEShapeCircle circle = new(16);

    private readonly List<AOEInstance> _aoesLongTether = [];
    private readonly List<AOEInstance> _aoesShortTether = [];
    private static readonly WPos _frozenStarShortTether = new(230, 86);
    private static readonly WPos _frozenStarLongTether = new(230, 92);
    private static readonly WPos _donut = new(230, 79);
    private static readonly WPos _circle1 = new(241, 79);
    private static readonly WPos _circle2 = new(219, 79);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = _aoesShortTether.Count > 0 ? _aoesShortTether : _aoesLongTether;
        foreach (var aoe in aoes)
            yield return new AOEInstance(aoe.Shape, aoe.Origin, default, aoe.Activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        var activation1 = WorldState.FutureTime(11.8f);
        var activation2 = WorldState.FutureTime(14.8f);
        if ((OID)actor.OID == OID.FrozenStar)
        {
            if (actor.Position == _frozenStarLongTether)
            {
                _aoesShortTether.Add(new(circle, _circle1, default, activation1));
                _aoesShortTether.Add(new(circle, _circle2, default, activation1));
                _aoesLongTether.Add(new(donut, _donut, default, activation2));
            }
            else if (actor.Position == _frozenStarShortTether)
            {
                _aoesShortTether.Add(new(donut, _donut, default, activation1));
                _aoesLongTether.Add(new(circle, _circle1, default, activation2));
                _aoesLongTether.Add(new(circle, _circle2, default, activation2));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CircleOfIcePrime or AID.FireSpherePrime)
        {
            NumCasts++;
            if (_aoesShortTether.Count != 0)
                _aoesShortTether.RemoveAt(0);
            else
                _aoesLongTether.Clear();
        }
    }
}

class D064AscianPrimeStates : StateMachineBuilder
{
    public D064AscianPrimeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkWhispers>()
            .ActivateOnEnter<AncientFrost>()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<ShadowFlareLBPhase>()
            .ActivateOnEnter<Annihilation>()
            .ActivateOnEnter<HeightOfChaos>()
            .ActivateOnEnter<DarkBlizzardIIIAOE1>()
            .ActivateOnEnter<DarkBlizzardIIIAOE2>()
            .ActivateOnEnter<DarkBlizzardIIIAOE3>()
            .ActivateOnEnter<DarkBlizzardIIIAOE4>()
            .ActivateOnEnter<DarkBlizzardIIIAOE5>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<ChillingCross1>()
            .ActivateOnEnter<ChillingCross2>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<DarkFireII>()
            .ActivateOnEnter<UniversalManipulation>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<AncientCircle>()
            .ActivateOnEnter<EntropicFlame>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3823)]
public class D064AscianPrime(WorldState ws, Actor primary) : BossModule(ws, primary, new(230, 80), new ArenaBoundsCircle(21))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ArcaneSphere), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 2,
                OID.ArcaneSphere => 1,
                _ => 0
            };
        }
    }
}
