namespace BossMod.Stormblood.Hunt.RankS.BoneCrawler;

public enum OID : uint
{
    Boss = 0x1AB5, // R=6.2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    HeatBreath = 7924, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    RipperClaw = 7920, // Boss->self, 2.1s cast, range 5+R 90-degree cone
    WildCharge = 7921, // Boss->location, 3.5s cast, width 8 rect charge
    TailSmash = 7918, // Boss->self, 3.0s cast, range 12+R 90-degree cone
    VolcanicHowl = 7917, // Boss->self, no cast, range 50 circle, raidwide, applies Haste to self
    TailSwing = 7919, // Boss->self, 2.1s cast, range 10 circle, knockback 20, away from source
    HotCharge = 7922, // Boss->location, 2.5s cast, width 12 rect charge
    Haste = 7926, // Boss->self, no cast, single-target, boss applies Haste to self
    BoneShaker = 7925, // Boss->self, no cast, range 30+R circle, raidwide player stun
}

class HeatBreath(BossModule module) : Components.StandardAOEs(module, AID.HeatBreath, new AOEShapeCone(14.2f, 45.Degrees()));
class RipperClaw(BossModule module) : Components.StandardAOEs(module, AID.RipperClaw, new AOEShapeCone(11.2f, 45.Degrees()));
class WildCharge(BossModule module) : Components.ChargeAOEs(module, AID.WildCharge, 4);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, AID.HotCharge, 6);
class TailSwing(BossModule module) : Components.StandardAOEs(module, AID.TailSwing, new AOEShapeCircle(10));
class TailSwingKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TailSwing, 20, shape: new AOEShapeCircle(10));
class TailSmash(BossModule module) : Components.StandardAOEs(module, AID.TailSmash, new AOEShapeCone(18.2f, 45.Degrees()));

class BoneCrawlerStates : StateMachineBuilder
{
    public BoneCrawlerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBreath>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<WildCharge>()
            .ActivateOnEnter<HotCharge>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<TailSwingKB>()
            .ActivateOnEnter<TailSmash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 5988)]
public class BoneCrawler(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
