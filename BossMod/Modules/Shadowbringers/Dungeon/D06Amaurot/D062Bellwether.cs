namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D062Bellwether;

public enum OID : uint
{
    Boss = 0x27B8, // R=6.0-12.0
    TerminusFlesher = 0x27B9, // R=3.3
    TerminusCrier = 0x27BD, // R=1.0
    TerminusSprinter = 0x2879, // R=1.96
    TerminusDetonator = 0x27BA, // R=2.0
    TerminusShriver = 0x27BC, // R=1.35
    TerminusRoiler = 0x27BB, // R=1.2
    TerminusBeholder = 0x27BE, // R=1.32
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/TerminusSprinter/TerminusCrier->player, no cast, single-target
    AutoAttack2 = 6499, // TerminusFlesher/TerminusBeholder->player, no cast, single-target
    AutoAttack3 = 18013, // TerminusDetonator->player, no cast, single-target
    ShrillShriek = 15567, // Boss->self, 3.0s cast, range 50 circle
    Aetherspike = 15571, // TerminusSprinter->self, 4.0s cast, range 40 width 8 rect
    SelfDestruct = 15570, // TerminusDetonator->self, no cast, range 6 circle
    IllWill = 15573, // TerminusRoiler->player, no cast, single-target
    SicklyFlame = 15588, // TerminusShriver->player, 2.0s cast, single-target
    Comet = 15572, // 18D6->location, 4.0s cast, range 4 circle
    SicklyInferno = 16765, // TerminusShriver->location, 3.0s cast, range 5 circle
    Burst = 15569, // Boss->self, no cast, range 50 circle, raidwide on boss death
    BurstEnrage = 15568, // Boss->self, 45.0s cast, single-target, enrage cast
    ExplosionEnrage = 15919, // Boss->self, no cast, range 50 circle, enrage
}

class ShrillShriek(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShrillShriek));
class Aetherspike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Aetherspike), new AOEShapeRect(40, 4));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);
class SicklyInferno(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SicklyInferno), 5);
class Burst(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.BurstEnrage), "Enrage!", true);

class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
            if (actor.Role is Role.Melee or Role.Tank && Module.PrimaryActor.IsTargetable)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
    }
}

class D062BellwetherStates : StateMachineBuilder
{
    public D062BellwetherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<ShrillShriek>()
            .ActivateOnEnter<Aetherspike>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<SicklyInferno>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 341, NameID = 8202)]
public class D062Bellwether(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(60, -361), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(60, -341), 20, 1)];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusRoiler))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusShriver))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusFlesher))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusDetonator))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusBeholder))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusCrier))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.TerminusSprinter))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}