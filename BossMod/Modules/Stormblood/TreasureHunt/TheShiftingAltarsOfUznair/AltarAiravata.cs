namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarAiravata;

public enum OID : uint
{
    Boss = 0x2543, //R=4.75
    BonusAddAltarMatanga = 0x2545, // R3.420
    BonusAddGoldWhisker = 0x2544, // R0.540
    BossHelper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // 2544->player, no cast, single-target
    AutoAttack2 = 872, // Boss,Matanaga->player, no cast, single-target
    Huff = 13371, // Boss->player, 3.0s cast, single-target
    HurlBoss = 13372, // Boss->location, 3.0s cast, range 6 circle
    Buffet = 13374, // Boss->player, 3.0s cast, single-target, knockback 20 forward
    SpinBoss = 13373, // Boss->self, 4.0s cast, range 30 120-degree cone
    BarbarousScream = 13375, // Boss->self, 3.5s cast, range 14 circle

    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    BuffetTarget = 23, // player
}

class HurlBoss(BossModule module) : Components.StandardAOEs(module, AID.HurlBoss, 6);
class SpinBoss(BossModule module) : Components.StandardAOEs(module, AID.SpinBoss, new AOEShapeCone(30, 60.Degrees()));
class BarbarousScream(BossModule module) : Components.StandardAOEs(module, AID.BarbarousScream, new AOEShapeCircle(13));
class Huff(BossModule module) : Components.SingleTargetDelayableCast(module, AID.Huff);

class Buffet(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Buffet, 20, kind: Kind.DirForward, stopAtWall: true)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BuffetTarget)
        {
            targeted = true;
            target = actor;
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor) => target == actor ? base.Sources(slot, actor) : [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Buffet)
            targeted = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
        {
            hints.Add("Bait away!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 18));
    }
}

class Buffet2(BossModule module) : Components.BaitAwayCast(module, AID.Buffet, new AOEShapeCone(30, 60.Degrees()), true) //Boss jumps on player and does a cone attack, this is supposed to predict the position of the cone attack
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in CurrentBaits)
            if (b.Target.InstanceID != actor.InstanceID && CurrentBaits.Count > 0)
                hints.AddForbiddenZone(b.Shape, b.Target.Position + (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * (Module.PrimaryActor.Position - b.Target.Position).Normalized(), b.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaitsOn(pc))
        {
            bait.Shape.Outline(Arena, bait.Target.Position + (bait.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * (Module.PrimaryActor.Position - bait.Target.Position).Normalized(), bait.Rotation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!IgnoreOtherBaits)
            foreach (var bait in ActiveBaitsNotOn(pc))
                if (AlwaysDrawOtherBaits || IsClippedBy(pc, bait))
                    bait.Shape.Draw(Arena, bait.Target.Position + (bait.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * (Module.PrimaryActor.Position - bait.Target.Position).Normalized(), bait.Rotation);
    }
}

class RaucousScritch(BossModule module) : Components.StandardAOEs(module, AID.RaucousScritch, new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.StandardAOEs(module, AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, AID.Spin, new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class AiravataStates : StateMachineBuilder
{
    public AiravataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HurlBoss>()
            .ActivateOnEnter<SpinBoss>()
            .ActivateOnEnter<BarbarousScream>()
            .ActivateOnEnter<Huff>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Buffet2>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddGoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7601)]
public class Airavata(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAddGoldWhisker))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddGoldWhisker => 3,
                OID.BonusAddAltarMatanga => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
