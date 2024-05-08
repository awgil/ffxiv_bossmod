namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretDjinn;

public enum OID : uint
{
    Boss = 0x300F, //R=3.48
    BossAdd = 0x3010, //R=1.32
    BossHelper = 0x233C,
    BonusAddKeeperOfKeys = 0x3034, // R3.230
}

public enum AID : uint
{
    AutoAttack = 23185, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BossAdd/BonusAddKeeperOfKeys->player, no cast, single-target
    Gust = 21655, // Boss->location, 3.0s cast, range 6 circle
    ChangelessWinds = 21657, // Boss->self, 3.0s cast, range 40 width 8 rect, knockback 10, source forward
    WhirlingGaol = 21654, // Boss->self, 4.0s cast, range 40 circle, knockback 25 away from source
    Whipwind = 21656, // Boss->self, 5.0s cast, range 55 width 40 rect, knockback 25, source forward
    GentleBreeze = 21653, // BossAdd->self, 3.0s cast, range 15 width 4 rect

    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // 3034->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // 3034->self, 4.0s cast, range 11 circle
    Scoop = 21768, // 3034->self, 4.0s cast, range 15 120-degree cone
}

class Gust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Gust), 6);
class ChangelessWinds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChangelessWinds), new AOEShapeRect(40, 4));
class ChangelessWindsKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ChangelessWinds), 10, shape: new AOEShapeRect(40, 4), kind: Kind.DirForward, stopAtWall: true);
class Whipwind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Whipwind), new AOEShapeRect(55, 20));
class WhipwindKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Whipwind), 25, shape: new AOEShapeRect(55, 20), kind: Kind.DirForward, stopAtWall: true);
class GentleBreeze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GentleBreeze), new AOEShapeRect(15, 2));
class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WhirlingGaol), "Raidwide + Knockback");
class WhirlingGaolKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WhirlingGaol), 25, stopAtWall: true);
class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class DjinnStates : StateMachineBuilder
{
    public DjinnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<ChangelessWindsKB>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<WhipwindKB>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<WhirlingGaol>()
            .ActivateOnEnter<WhirlingGaolKB>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddKeeperOfKeys).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9788)]
public class Djinn(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(19))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddKeeperOfKeys))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddKeeperOfKeys => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
