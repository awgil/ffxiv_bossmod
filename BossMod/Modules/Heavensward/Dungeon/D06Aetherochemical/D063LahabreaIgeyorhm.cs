namespace BossMod.Heavensward.Dungeon.D06Aetherochemical.D063LahabreaIgeyorhm;

public enum OID : uint
{
    Boss = 0x3DA3, // R3.500, x1
    Lahabrea = 0x3DA4, // R3.500, x1
    Helper = 0x233C, // R0.500, x12, 523 type

    BurningStar = 0x3DA6, // R1.500, x0 (spawn during fight)
    FrozenStar = 0x3DA5, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 32818, // Boss/Lahabrea->player, no cast, single-target
    Teleport = 32791, // Boss->location, no cast, single-target, teleport to center

    AetherialDivideIgeyorhm = 32686, // Boss->Lahabrea, no cast, single-target, Igeyorhm donates HP to Lahabrea
    AetherialDivideLahabrea = 32685, // Lahabrea->Boss, no cast, single-target, Lahabrea donates HP to Igeyorhm

    CircleOfIce = 31878, // Boss->self, 3.0s cast, single-target
    CircleOfIceAOE = 31879, // FrozenStar->self, 3.0s cast, range 5-15 donut

    CircleOfIcePrime1 = 31881, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime2 = 31882, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeAOE = 33019, // Helper->self, 3.0s cast, range 5-40 donut

    DarkFireII = 32687, // Lahabrea->self, 6.0s cast, single-target
    DarkFireIIAOE = 32688, // Helper->player, 6.0s cast, range 6 circle

    EndOfDays = 31892, // Helper->player, no cast, single-target
    EndOfDays1 = 31891, // Lahabrea->self, 5.0s cast, single-target
    EndOfDays2 = 33029, // Helper->self, no cast, range 50 width 8 rect // line stack

    EsotericFusion1 = 31880, // Boss->self, 3.0s cast, single-target
    EsotericFusion2 = 31888, // Lahabrea->self, 3.0s cast, single-target

    FireSphere = 31886, // Lahabrea->self, 3.0s cast, single-target
    FireSphereAOE = 31887, // BurningStar->self, 3.0s cast, range 8 circle

    FireSpherePrime1 = 31889, // BurningStar->self, no cast, single-target
    FireSpherePrime2 = 31890, // BurningStar->self, no cast, single-target
    FireSpherePrimeAOE = 33020, // Helper->self, 2.0s cast, range 16 circle

    GripOfNight = 32790, // Boss->self, 6.0s cast, range 40 150-degree cone

    ShadowFlare = 31885, // Boss/Lahabrea->self, 5.0s cast, range 40 circle
}

public enum TetherID : uint
{
    OrbTether = 110, // BurningStar/FrozenStar->BurningStar/FrozenStar
}

class ShadowFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare));
class FireSphereAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireSphereAOE), new AOEShapeCircle(9));
class FireSpherePrimeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireSpherePrimeAOE), new AOEShapeCircle(16));
class GripOfNight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GripOfNight), new AOEShapeCone(40, 75.Degrees()));
class CircleOfIceAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircleOfIceAOE), new AOEShapeDonut(5, 15));
class CircleOfIcePrimeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircleOfIcePrimeAOE), new AOEShapeDonut(5, 40));
class DarkFireIIAOE(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkFireIIAOE), 6);

// TODO: create and use generic 'line stack' component
class EndOfDays(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? target;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EndOfDays)
        {
            target = WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(Module.PrimaryActor, target!, new AOEShapeRect(50, 4)));
        }
        if ((AID)spell.Action.ID == AID.EndOfDays2)
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

class D063LahabreaIgeyorhmStates : StateMachineBuilder
{
    public D063LahabreaIgeyorhmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<FireSphereAOE>()
            .ActivateOnEnter<FireSpherePrimeAOE>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<CircleOfIceAOE>()
            .ActivateOnEnter<CircleOfIcePrimeAOE>()
            .ActivateOnEnter<EndOfDays>()
            .ActivateOnEnter<DarkFireIIAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 2143)]
public class D063LahabreaIgeyorhm(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(230, -181), 20.25f)];
    private static readonly List<Shape> difference = [new Rectangle(new(230, -161), 20, 0.75f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Lahabrea))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
