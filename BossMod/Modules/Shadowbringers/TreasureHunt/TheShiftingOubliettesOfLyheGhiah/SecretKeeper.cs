namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretKeeper;

public enum OID : uint
{
    Boss = 0x3023, //R=3.99
    BossAdd = 0x3019, //R=1.8
    BossHelper = 0x233C,
    ResinVoidzone = 0x1E8FC7,
    BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
    BonusAdd_FuathTrickster = 0x3033, // R0.750
};

public enum AID : uint
{
    AutoAttack = 872, // Boss/BonusAdd_Keeper->player, no cast, single-target
    Buffet = 21680, // Boss->self, 3,0s cast, range 11 120-degree cone
    HeavyScrapline = 21681, // Boss->self, 4,0s cast, range 11 circle
    MoldyPhlegm = 21679, // Boss->location, 3,0s cast, range 6 circle
    InhaleBoss = 21677, // Boss->self, 4,0s cast, range 20 120-degree cone
    MoldySneeze = 21678, // Boss->self, no cast, range 12 120-degree cone, heavy dmg, 20 knockback away from source

    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
    Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
    Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
};

class Buffet : Components.SelfTargetedAOEs
{
    public Buffet() : base(ActionID.MakeSpell(AID.Buffet), new AOEShapeCone(11, 60.Degrees())) { }
}

class Inhale : Components.SelfTargetedAOEs
{
    public Inhale() : base(ActionID.MakeSpell(AID.InhaleBoss), new AOEShapeCone(20, 60.Degrees())) { }
}
class InhalePull : Components.KnockbackFromCastTarget
{
    public InhalePull() : base(ActionID.MakeSpell(AID.InhaleBoss), 20, false, 1, new AOEShapeCone(20, 60.Degrees()), Kind.TowardsOrigin, default, true) { }
}

class HeavyScrapline : Components.SelfTargetedAOEs
{
    public HeavyScrapline() : base(ActionID.MakeSpell(AID.HeavyScrapline), new AOEShapeCircle(11)) { }
}

class MoldyPhlegm : Components.PersistentVoidzoneAtCastTarget
{
    public MoldyPhlegm() : base(6, ActionID.MakeSpell(AID.MoldyPhlegm), m => m.Enemies(OID.ResinVoidzone).Where(z => z.EventState != 7), 0) { }
}

class MoldySneeze : Components.Cleave
{
    public MoldySneeze() : base(ActionID.MakeSpell(AID.MoldySneeze), new AOEShapeCone(12, 60.Degrees()), (uint)OID.Boss) { }
}

class Spin : Components.SelfTargetedAOEs
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
}

class Mash : Components.SelfTargetedAOEs
{
    public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
}

class Scoop : Components.SelfTargetedAOEs
{
    public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
}

class KeeperStates : StateMachineBuilder
{
    public KeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Inhale>()
            .ActivateOnEnter<InhalePull>()
            .ActivateOnEnter<HeavyScrapline>()
            .ActivateOnEnter<MoldyPhlegm>()
            .ActivateOnEnter<MoldySneeze>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_FuathTrickster).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9807)]
public class Keeper : BossModule
{
    public Keeper(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAdd_FuathTrickster))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_FuathTrickster => 4,
                OID.BonusAdd_TheKeeperOfTheKeys => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
