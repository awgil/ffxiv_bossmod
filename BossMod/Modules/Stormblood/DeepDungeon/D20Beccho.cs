namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D20Beccho;

public enum OID : uint
{
    Boss = 0x23E7, // R3.000, x1
    ChokeshinAdds = 0x23E8, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Fragility = 11901, // ChokeshinAdds->self, 3.0s cast, range 8 circle
    NeuroSquama = 11900, // Boss->self, 3.0s cast, range 50 circle // look away from boss
    Proboscis = 11898, // Boss->player, no cast, single-target
    PsychoSquama = 11899, // Boss->self, 3.0s cast, range 50+R 90-degree cone // opening attack
}

class PsychoSquamaAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PsychoSquama), new AOEShapeCone(53, 45.Degrees()));
class NeuroSquamaLookAway(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.NeuroSquama));
class FragilityAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Fragility), new AOEShapeCircle(8));

class D20BecchoStates : StateMachineBuilder
{
    public D20BecchoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychoSquamaAOE>()
            .ActivateOnEnter<NeuroSquamaLookAway>()
            .ActivateOnEnter<FragilityAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 541, NameID = 7481)]
public class D20Beccho(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(23.7f));
