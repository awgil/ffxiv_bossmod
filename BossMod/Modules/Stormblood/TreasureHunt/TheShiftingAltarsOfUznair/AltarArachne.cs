namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarArachne;

public enum OID : uint
{
    Boss = 0x253B, //R=7.0
    Helper = 0x253C, //R=0.5
    Helper2 = 0x2565, //R=0.5
    BossHelper = 0x233C,
    BonusAdd_AltarMatanga = 0x2545, // R3.420
    BonusAdd_GoldWhisker = 0x2544, // R0.540
};

public enum AID : uint
{
    AutoAttack = 870, // Boss/2544->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdd_AltarMatanga->player, no cast, single-target
    DarkSpike = 13342, // Boss->player, 3,0s cast, single-target
    SilkenSpray = 13455, // Boss->self, 2,5s cast, range 17+R 60-degree cone
    FrondAffeared = 13784, // Boss->self, 3,0s cast, range 60 circle, gaze, applies hysteria
    Implosion = 13343, // Boss->self, 4,0s cast, range 50+R circle
    Earthquake1 = 13346, // 253C/2565->self, 3,5s cast, range 10+R circle
    Earthquake2 = 13345, // 253C/2565->self, 3,5s cast, range 10-20 donut
    Earthquake3 = 13344, // 253C/2565->self, 3,5s cast, range 20-30 donut
    unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
};

class DarkSpike : Components.SingleTargetDelayableCast
{
    public DarkSpike() : base(ActionID.MakeSpell(AID.DarkSpike)) { }
}

class FrondAffeared : Components.CastGaze
{
    public FrondAffeared() : base(ActionID.MakeSpell(AID.FrondAffeared)) { }
}

class SilkenSpray : Components.SelfTargetedAOEs
{
    public SilkenSpray() : base(ActionID.MakeSpell(AID.SilkenSpray), new AOEShapeCone(24, 30.Degrees())) { }
}

class Implosion : Components.RaidwideCast
{
    public Implosion() : base(ActionID.MakeSpell(AID.Implosion)) { }
}

class Earthquake1 : Components.SelfTargetedAOEs
{
    public Earthquake1() : base(ActionID.MakeSpell(AID.Earthquake1), new AOEShapeCircle(10.5f)) { }
}

class Earthquake2 : Components.SelfTargetedAOEs
{
    public Earthquake2() : base(ActionID.MakeSpell(AID.Earthquake2), new AOEShapeDonut(10, 20)) { }
}

class Earthquake3 : Components.SelfTargetedAOEs
{
    public Earthquake3() : base(ActionID.MakeSpell(AID.Earthquake3), new AOEShapeDonut(20, 30)) { }
}

class RaucousScritch : Components.SelfTargetedAOEs
{
    public RaucousScritch() : base(ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees())) { }
}

class Hurl : Components.LocationTargetedAOEs
{
    public Hurl() : base(ActionID.MakeSpell(AID.Hurl), 6) { }
}

class Spin : Components.Cleave
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAdd_AltarMatanga) { }
}

class ArachneStates : StateMachineBuilder
{
    public ArachneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkSpike>()
            .ActivateOnEnter<FrondAffeared>()
            .ActivateOnEnter<SilkenSpray>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<Earthquake1>()
            .ActivateOnEnter<Earthquake2>()
            .ActivateOnEnter<Earthquake3>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7623)]
public class Arachne : BossModule
{
    public Arachne(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAdd_GoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAdd_AltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_GoldWhisker => 3,
                OID.BonusAdd_AltarMatanga => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
