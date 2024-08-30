namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D10Mojabune;

public enum OID : uint
{
    Boss = 0x23E1, // R2.400, x1
    CloudHelper = 0x1E8FB8, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AmorphousApplause = 11876, // Boss->self, 5.0s cast, range 25+R 180-degree cone
    AutoAttack = 6499, // Boss->player, no cast, single-target
    ConcussiveOscillation = 11878, // Boss->location, 4.0s cast, range 7 circle
    Overtow = 11877, // Boss->location, 3.0s cast, range 60 circle
}

class ConcussiveOscillationAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ConcussiveOscillation), 7, "Get out of the AOE!");
class OvertowKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Overtow), 23.7f, true);
class AmorphousApplauseAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AmorphousApplause), new AOEShapeCone(27.4f, 90.Degrees()));

class D10MojabuneStates : StateMachineBuilder
{
    public D10MojabuneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ConcussiveOscillationAOE>()
            .ActivateOnEnter<OvertowKB>()
            .ActivateOnEnter<AmorphousApplauseAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 540, NameID = 7480)] // cfcid is 540 -> 549 (personal note for self)
public class D10Mojabune(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(23.7f));
