namespace BossMod.Endwalker.Hunt.RankA.FanAil;

public enum OID : uint
{
    Boss = 0x35C1, // R5.040, x1
}

public enum AID : uint
{
    Divebomb = 27373, // Boss->players, 5.0s cast, range 30 width 11 rect
    DivebombDisappear = 27374, // Boss->location, no cast, single-target
    DivebombReappear = 27375, // Boss->self, 1.0s cast, single-target
    LiquidHell = 27376, // Boss->location, 3.0s cast, range 6 circle
    Plummet = 27378, // Boss->self, 4.0s cast, range 8 90-degree cone
    DeathSentence = 27379, // Boss->player, 5.0s cast, single-target
    CycloneWing = 27380, // Boss->self, 5.0s cast, range 35 circle
    AutoAttack = 27381, // Boss->player, no cast, single-target
}

// TODO: ok, this needs investigation...
class Divebomb(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.Divebomb), new AOEShapeRect(30, 5.5f));

class LiquidHell(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LiquidHell), 6);
class Plummet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Plummet), new AOEShapeCone(8, 45.Degrees()));
class DeathSentence(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DeathSentence));
class CycloneWing(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CycloneWing));

class FanAilStates : StateMachineBuilder
{
    public FanAilStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Divebomb>()
            .ActivateOnEnter<LiquidHell>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<DeathSentence>()
            .ActivateOnEnter<CycloneWing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10633)]
public class FanAil(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
