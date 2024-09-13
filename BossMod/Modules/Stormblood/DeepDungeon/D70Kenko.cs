namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D70Kenko;

public enum OID : uint
{
    Boss = 0x23EB, // R6.000, x1
    Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type
    InnerspacePuddle = 0x1E9829, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Devour = 12204, // Boss->location, no cast, range 4+R ?-degree cone
    HoundOutOfHell = 12206, // Boss->player, 5.0s cast, width 14 rect charge
    Innerspace = 12207, // Boss->player, 3.0s cast, single-target
    PredatorClaws = 12205, // Boss->self, 3.0s cast, range 9+R ?-degree cone
    Slabber = 12203, // Boss->location, 3.0s cast, range 8 circle
    Ululation = 12208, // Boss->self, 3.0s cast, range 80+R circle
}

public enum SID : uint
{
    Minimum = 438, // none->player, extra=0x32
    Stun = 149, // Boss->player, extra=0x0
    FleshWound = 264, // Boss->player, extra=0x0

}

public enum IconID : uint
{
    HoundOutOfHellTarget = 1, // player
}

class HoundOutofHell(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.HoundOutOfHell), 7);
class PredatorClaws(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PredatorClaws), new AOEShapeCone(9, 45.Degrees()));
class Slabber(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Slabber), 8, "GTFO from the puddle!");
class Utilation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ululation), "Raidwide, Do not be in the shrink puddle!");

class D70KenkoStates : StateMachineBuilder
{
    public D70KenkoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HoundOutofHell>()
            .ActivateOnEnter<PredatorClaws>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Utilation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public class D70Kenko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25));

