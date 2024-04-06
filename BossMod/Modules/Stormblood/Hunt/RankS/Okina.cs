namespace BossMod.Stormblood.Hunt.RankS.Okina;

public enum OID : uint
{
    Boss = 0x1AB1, // R=8.0
};

public enum AID : uint
{
    AutoAttack = 872, // 1AB1->player, no cast, single-target
    Hydrocannon = 7990, // 1AB1->self, no cast, range 22+R width 6 rect
    ElectricSwipe = 7991, // 1AB1->self, 2,5s cast, range 17+R 60-degree cone, applies paralysis
    BodySlam = 7993, // 1AB1->location, 4,0s cast, range 10 circle, knockback 20, away from source
    ElectricWhorl = 7996, // 1AB1->self, 4,0s cast, range 8-60 donut
    Expulsion = 7995, // 1AB1->self, 3,0s cast, range 6+R circle, knockback 30, away from source
    Immersion = 7994, // 1AB1->self, 3,0s cast, range 60+R circle
    RubyTide = 7992, // Boss->self, 2,0s cast, single-target, boss gives itself Dmg up buff
};

class Hydrocannon : Components.SelfTargetedAOEs
{
    public Hydrocannon() : base(ActionID.MakeSpell(AID.Hydrocannon), new AOEShapeRect(30, 3)) { }
}

class ElectricWhorl : Components.SelfTargetedAOEs
{
    public ElectricWhorl() : base(ActionID.MakeSpell(AID.ElectricWhorl), new AOEShapeDonut(8, 60)) { }
}

class Expulsion : Components.SelfTargetedAOEs
{
    public Expulsion() : base(ActionID.MakeSpell(AID.Expulsion), new AOEShapeCircle(14)) { }
}

class ExpulsionKB : Components.KnockbackFromCastTarget
{
    public ExpulsionKB() : base(ActionID.MakeSpell(AID.Expulsion), 30, shape: new AOEShapeCircle(14)) { }
}

class ElectricSwipe : Components.SelfTargetedAOEs
{
    public ElectricSwipe() : base(ActionID.MakeSpell(AID.ElectricSwipe), new AOEShapeCone(25, 30.Degrees())) { }
}

class BodySlam : Components.LocationTargetedAOEs
{
    public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), 10) { }
}

class BodySlamKB : Components.KnockbackFromCastTarget
{
    public BodySlamKB() : base(ActionID.MakeSpell(AID.BodySlam), 20, shape: new AOEShapeCircle(10)) { }
}

class Immersion : Components.RaidwideCast
{
    public Immersion() : base(ActionID.MakeSpell(AID.Immersion)) { }
}

class RubyTide : Components.CastHint
{
    public RubyTide() : base(ActionID.MakeSpell(AID.RubyTide), "Applies damage buff to self") { }
}

class OkinaStates : StateMachineBuilder
{
    public OkinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<ElectricSwipe>()
            .ActivateOnEnter<ElectricWhorl>()
            .ActivateOnEnter<Expulsion>()
            .ActivateOnEnter<ExpulsionKB>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<RubyTide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 5984)]
public class Okina : SimpleBossModule
{
    public Okina(WorldState ws, Actor primary) : base(ws, primary) { }
}
