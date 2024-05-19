using BossMod.Endwalker.Hunt.RankS.KerShroud;

namespace BossMod.Heavensward.Dungeon.D06Aetherochemical.D064AscianPrime;

public enum OID : uint
{
    Boss = 0x3DA7, // R3.800, x1
    LahabreasShade = 0x3DAB, // R3.500, x1
    IgeyorhmsShade = 0x3DAA, // R3.500, x1
    Helper = 0x233C, // R0.500, x23, 523 type

    FrozenStar = 0x3DA8, // R1.500, x0 (spawn during fight)
    BurningStar = 0x3DA9, // R1.500, x0 (spawn during fight)
    ArcaneSphere = 0x3DAC, // R7.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    AncientCircle = 31901, // Helper->self, no cast, range 5-20 donut Player targeted donut AOE
    AncientDarkness = 31903, // Helper->self, no cast, range 6 circle

    AncientEruption = 31908, // Boss->self, 5.0s cast, single-target
    AncientEruptionAOE = 31909, // Helper->location, 5.0s cast, range 5 circle

    AncientFrost = 31904, // Helper->players, no cast, range 6 circle

    Annihilation = 31927, // Boss->location, no cast, single-target
    AnnihilationAOE = 33024, // Helper->self, 6.3s cast, range 40 circle
    AnnihilationEnrage = 31928, // Boss->location, no cast, single-target, if Arcane Sphere doesn't get destroyed in time
    AnnihilationEnrageAOE = 33025, // Helper->self, 6.3s cast, range 40 circle

    ArcaneRevelation1 = 31912, // Boss->location, no cast, single-target, teleport
    ArcaneRevelation2 = 31913, // Boss->self, 3.0s cast, single-target
    BurningChains = 31905, // Helper->player, no cast, single-target

    ChillingCross = 31922, // IgeyorhmsShade->self, 6.0s cast, single-target
    ChillingCrossAOE1 = 31923, // Helper->self, 6.0s cast, range 40 width 5 cross
    ChillingCrossAOE2 = 31924, // Helper->self, 6.0s cast, range 40 width 5 cross

    CircleOfIcePrime1 = 31898, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime2 = 31899, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeAOE = 33021, // Helper->self, 2.0s cast, range 5-40 donut

    DarkBlizzardIII = 31914, // IgeyorhmsShade->self, 6.0s cast, single-target
    DarkBlizzardIIIAOE1 = 31915, // Helper->self, 6.0s cast, range 41 ?-degree cone
    DarkBlizzardIIIAOE2 = 31916, // Helper->self, 6.0s cast, range 41 ?-degree cone
    DarkBlizzardIIIAOE3 = 31917, // Helper->self, 6.0s cast, range 41 ?-degree cone
    DarkBlizzardIIIAOE4 = 31918, // Helper->self, 6.0s cast, range 41 ?-degree cone
    DarkBlizzardIIIAOE5 = 31919, // Helper->self, 6.0s cast, range 41 ?-degree cone

    DarkFireII = 31920, // LahabreasShade->self, 6.0s cast, single-target
    DarkFireIIAOE = 31921, // Helper->players, 6.0s cast, range 6 circle

    Dualstar = 31894, // Boss->self, 4.0s cast, single-target

    EntropicFlame = 31907, // Helper->player, no cast, single-target
    EntropicFlame1 = 32126, // Boss->self, no cast, single-target
    EntropicFlame2 = 31906, // Boss->self, 5.0s cast, single-target
    EntropicFlame3 = 32555, // Helper->self, no cast, range 50 width 8 rect // Line Stack

    FireSpherePrime1 = 31896, // BurningStar->self, no cast, single-target
    FireSpherePrime2 = 31897, // BurningStar->self, no cast, single-target
    FireSpherePrimeAOE = 33022, // Helper->self, 2.0s cast, range 16 circle

    FusionPrime = 31895, // Boss->self, 3.0s cast, single-target

    HeightOfChaos = 31911, // Boss->player, 5.0s cast, range 5 circle

    ShadowFlare1 = 31910, // Boss->self, 5.0s cast, range 40 circle//
    ShadowFlare2 = 31925, // IgeyorhmsShade->self, 5.0s cast, range 40 circle
    ShadowFlare3 = 31926, // LahabreasShade->self, 5.0s cast, range 40 circle

    UniversalManipulationTeleport = 31419, // Boss->location, no cast, single-target
    UniversalManipulation = 31900, // Boss->self, 5.0s cast, range 40 circle
    UniversalManipulation2 = 33044, // Boss->player, no cast, single-target
}

public enum SID : uint
{
    AncientCircle = 3534, // none->player, extra=0x0 Player targeted donut AOE
    AncientFrost = 3506, // none->player, extra=0x0 Stack marker
    Bleeding = 2088, // Boss->player, extra=0x0
    BurningChains1 = 3505, // none->player, extra=0x0
    BurningChains2 = 769, // none->player, extra=0x0
    DarkWhispers = 3535, // none->player, extra=0x0 Spread marker
    Untargetable = 2056, // Boss->Boss, extra=0x231, before limitbreak phase
}

public enum IconID : uint
{
    Tankbuster = 343, // player
    AncientCircle = 384, // player
    DarkWhispers = 139, // player
    AncientFrost = 161, // player
    BurningChains = 97, // player
    DarkFire = 311, // player
}

public enum TetherID : uint
{
    OrbTether = 110, // FrozenStar/BurningStar->FrozenStar/BurningStar
    BurningChains = 9, // player->player
    ArcaneSphere = 197, // ArcaneSphere->Boss
}

class ShadowFlare1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare1));
class ShadowFlare2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare2));
class ShadowFlare3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare3));
class Annihilation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AnnihilationAOE));
class UniversalManipulation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UniversalManipulation), "Raidwide + Apply debuffs for later");
class HeightOfChaos(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeightOfChaos));
class HeightOfChaosSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HeightOfChaos), 5);
class AncientEruptionAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AncientEruptionAOE), 5);
class AnnihilationAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AnnihilationAOE));
class ChillingCrossAOE1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChillingCrossAOE1), new AOEShapeCross(40, 2.5f));
class ChillingCrossAOE2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChillingCrossAOE2), new AOEShapeCross(40, 2.5f));
class CircleOfIcePrimeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircleOfIcePrimeAOE), new AOEShapeDonut(5, 40));
class DarkBlizzardIIIAOE1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkBlizzardIIIAOE1), new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkBlizzardIIIAOE2), new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkBlizzardIIIAOE3), new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkBlizzardIIIAOE4), new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkBlizzardIIIAOE5), new AOEShapeCone(41, 10.Degrees()));
class DarkFireIIAOE(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkFireIIAOE), 6);
class FireSpherePrimeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireSpherePrimeAOE), new AOEShapeCircle(16));

class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChains));

// TODO: create and use generic 'line stack' component
class EntropicFlame(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? target;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EntropicFlame)
        {
            target = WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(Module.PrimaryActor, target!, new AOEShapeRect(50, 4)));
        }
        if ((AID)spell.Action.ID == AID.EntropicFlame2)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0 && actor != target)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 50, 0, 4));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (!actor.Position.InRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 50, 0, 4))
                hints.Add("Stack!");
            else
                hints.Add("Stack!", false);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in CurrentBaits)
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

class D064AscianPrimeStates : StateMachineBuilder
{
    public D064AscianPrimeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlare1>()
            .ActivateOnEnter<ShadowFlare2>()
            .ActivateOnEnter<ShadowFlare3>()
            .ActivateOnEnter<Annihilation>()
            .ActivateOnEnter<HeightOfChaos>()
            .ActivateOnEnter<DarkBlizzardIIIAOE1>()
            .ActivateOnEnter<DarkBlizzardIIIAOE2>()
            .ActivateOnEnter<DarkBlizzardIIIAOE3>()
            .ActivateOnEnter<DarkBlizzardIIIAOE4>()
            .ActivateOnEnter<DarkBlizzardIIIAOE5>()
            .ActivateOnEnter<AncientEruptionAOE>()
            .ActivateOnEnter<AnnihilationAOE>()
            .ActivateOnEnter<ChillingCrossAOE1>()
            .ActivateOnEnter<ChillingCrossAOE2>()
            .ActivateOnEnter<CircleOfIcePrimeAOE>()
            .ActivateOnEnter<DarkFireIIAOE>()
            .ActivateOnEnter<FireSpherePrimeAOE>()
            .ActivateOnEnter<UniversalManipulation>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<BurningChains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3823)]
public class D064AscianPrime(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(230, 79), 20.25f)];
    private static readonly List<Shape> difference = [new Rectangle(new(230, 99.5f), 20, 0.75f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArcaneSphere))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}