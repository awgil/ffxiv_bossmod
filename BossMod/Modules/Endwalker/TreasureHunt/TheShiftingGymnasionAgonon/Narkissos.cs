namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.Narkissos;

public enum OID : uint
{
    Boss = 0x3D48, //R=8.0
    BossHelper = 0x233C,
    BonusAddLampas = 0x3D4D, //R=2.001, bonus loot adds
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
}

public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target
    FetchingFulgence = 32332, // Boss->self, 4.0s cast, range 40 circle, Gaze, Vegetal Vapors
    PotentPerfume = 32333, // BossHelper->location, 4.0s cast, range 8 circle, high damage, Vegetal Vapours
    Lash = 32330, // Boss->player, 5.0s cast, single-target, tank buster
    SapShower = 32335, // Boss->self, no cast, single-target
    SapShower2 = 32336, // BossHelper->location, 6.5s cast, range 8 circle, high damage, Vegetal Vapours
    ExtensibleTendrils = 32339, // Boss->self, 5.0s cast, range 25 width 6 cross
    AutoAttack = 870, // 3D4E->player, no cast, single-target
    RockHard = 32340, // BossHelper->player, 5.5s cast, range 6 circle
    HeavySmash = 32317, // 3D4E->location, 3.0s cast, range 6 circle
    BeguilingGas = 32331, // Boss->self, 5.0s cast, range 40 circle, Temporary Misdirection
    Brainstorm = 32334, // Boss->self, 5.0s cast, range 40 circle, Forced March debuffs
    PutridBreath = 32338, // Boss->self, 4.0s cast, range 25 90-degree cone
}

public enum SID : uint
{
    VegetalVapours = 3467, // Boss/BossHelper->player, extra=0x2162 (description: Overcome and quite unable to act.)
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x2/0x4/0x8
    RightFace = 1961, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
}

class Brainstorm(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<SapShower>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class FetchingFulgence(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.FetchingFulgence));
class Lash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Lash));
class PotentPerfume(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PotentPerfume), 8);

class SapShowerTendrilsHint(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SapShower2)
        {
            active = true;
            ++NumCasts;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SapShower2)
            active = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
        {
            if (NumCasts is <= 4 and > 0)
                hints.Add("Circles resolve before cross");
            if (NumCasts > 4)
                hints.Add("Circles resolve before cross, aim forced march into cross");
        }
    }
}

class SapShower : Components.LocationTargetedAOEs
{
    public SapShower(BossModule module) : base(module, ActionID.MakeSpell(AID.SapShower2), 8)
    {
        Color = ArenaColor.Danger;
    }
}

class ExtensibleTendrils(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExtensibleTendrils), new AOEShapeCross(25, 3));
class PutridBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PutridBreath), new AOEShapeCone(25, 45.Degrees()));
class RockHard(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.RockHard), 6);
class BeguilingGas(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.BeguilingGas), "Raidwide + Temporary Misdirection");
class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class NarkissosStates : StateMachineBuilder
{
    public NarkissosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Brainstorm>()
            .ActivateOnEnter<Lash>()
            .ActivateOnEnter<FetchingFulgence>()
            .ActivateOnEnter<PotentPerfume>()
            .ActivateOnEnter<SapShower>()
            .ActivateOnEnter<SapShowerTendrilsHint>()
            .ActivateOnEnter<ExtensibleTendrils>()
            .ActivateOnEnter<PutridBreath>()
            .ActivateOnEnter<RockHard>()
            .ActivateOnEnter<BeguilingGas>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAddLampas).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12029)]
public class Narkissos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAddLampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddLyssa))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddLampas => 3,
                OID.BonusAddLyssa => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
