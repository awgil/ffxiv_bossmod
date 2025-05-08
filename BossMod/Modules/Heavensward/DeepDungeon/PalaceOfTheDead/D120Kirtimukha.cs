namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D120Kirtimukha;

public enum OID : uint
{
    Boss = 0x1819, // R3.600, x1
    DeepPalaceHornet = 0x1905, // R0.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackAdds = 6498, // DeepPalaceHornet->player, no cast, single-target
    AcidMist = 7134, // Boss->self, 3.0s cast, range 6+R circle
    BloodyCaress = 7133, // Boss->self, no cast, range 8+R 120-degree cone
    FinalSting = 919, // DeepPalaceHornet->player, 3.0s cast, single-target
    GoldDust = 7135, // Boss->location, 3.0s cast, range 8 circle
    Leafstorm = 7136, // Boss->self, 3.0s cast, range 50 circle
    RottenStench = 7137, // Boss->self, 3.0s cast, range 45+R width 12 rect
}

class AcidMist(BossModule module) : Components.StandardAOEs(module, AID.AcidMist, new AOEShapeCircle(9.6f));
class BossAdds(BossModule module) : Components.Adds(module, (uint)OID.DeepPalaceHornet)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            switch ((OID)e.Actor.OID)
            {
                case OID.DeepPalaceHornet:
                    e.Priority = 2;
                    e.ForbidDOTs = true;
                    break;
                case OID.Boss:
                    e.Priority = 1;
                    break;
            }
        }
    }
}
class BloodyCaress(BossModule module) : Components.Cleave(module, AID.BloodyCaress, new AOEShapeCone(11.6f, 60.Degrees()), activeWhileCasting: false);
class FinalSting(BossModule module) : Components.SingleTargetCast(module, AID.FinalSting, "Final sting is being cast! \nKill the add or take 98% of your hp!");
class GoldDust(BossModule module) : Components.StandardAOEs(module, AID.GoldDust, 8);
class Leafstorm(BossModule module) : Components.RaidwideCast(module, AID.Leafstorm);
class RottenStench(BossModule module) : Components.StandardAOEs(module, AID.RottenStench, new AOEShapeRect(47.6f, 6));

class D120KirtimukhaStates : StateMachineBuilder
{
    public D120KirtimukhaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<FinalSting>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<RottenStench>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 210, NameID = 5384)]
public class D120Kirtimukha(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(24));
