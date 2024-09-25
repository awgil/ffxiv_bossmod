namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D20Spurge;

public enum OID : uint
{
    Boss = 0x169F, // R3.600, x1
    PalaceHornet = 0x1763, // R0.400, x0 (spawn during fight)
    Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackHornet = 6498, // PalaceHornet->player, no cast, single-target

    AcidMist = 6422, // Boss->self, 3.0s cast, range 6+R circle
    BloodyCaress = 6421, // Boss->self, no cast, range 8+R 120-degree cone
    GoldDust = 6423, // Boss->location, 3.0s cast, range 8 circle
    Leafstorm = 6424, // Boss->self, 3.0s cast, range 50 circle
    RottenStench = 6425, // Boss->self, 3.0s cast, range 45+R width 12 rect
}

class AcidMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AcidMist), new AOEShapeCircle(9.6f));
class BloodyCaress(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.BloodyCaress), new AOEShapeCone(11.6f, 60.Degrees()), activeWhileCasting: false);
class GoldDust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldDust), 8);
class Leafstorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Leafstorm));
class RottenStench(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RottenStench), new AOEShapeRect(47.6f, 6));

class D20SpurgeStates : StateMachineBuilder
{
    public D20SpurgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<RottenStench>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 175, NameID = 4999)]
public class D20Spurge(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(24));
