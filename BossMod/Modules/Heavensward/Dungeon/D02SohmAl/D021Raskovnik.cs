namespace BossMod.Heavensward.Dungeon.D02SohmAl.D021Raskovnik;

public enum OID : uint
{
    Boss = 0xE8F, // R3.680, x1
    Boss2 = 0x1B2, // ?
    DravanianHornet = 0x13C2, // R0.400, x?
    Helper = 0x233C, // Helper
}

public enum AID : uint
{
    AutoAttack = 872, // F1E/F1D/E8F/1030/F22/F24->player, no cast, single-target
    BloodyCaress = 3793, // E8F->self, no cast, range 8+R ?-degree cone
    AcidRain = 3794, // E8F->self, 4.0s cast, single-target
    AcidRain2 = 3799, // 1B2->location, 3.5s cast, range 6 circle
    SweetScent = 5013, // E8F->self, 3.0s cast, single-target
    FloralTrap = 5009, // E8F->self, no cast, range 45+R ?-degree cone
    FlowerDevour = 5010, // E8F->self, 3.0s cast, range 8 circle
}

class BloodyCaress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BloodyCaress), new AOEShapeCone(8, 45.Degrees()));
class AcidRain(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AcidRain2), 6);
class SweetScent(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(45, 45.Degrees()), 46, ActionID.MakeSpell(AID.FloralTrap));
class FlowerDevour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlowerDevour), new AOEShapeCircle(8));
class Adds(BossModule module) : Components.Adds(module, (uint)OID.DravanianHornet);

class D021RaskovnikStates : StateMachineBuilder
{
    public D021RaskovnikStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<SweetScent>()
            .ActivateOnEnter<FlowerDevour>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3791)]
public class D021Raskovnik(WorldState ws, Actor primary) : BossModule(ws, primary, new(-127, 168), new ArenaBoundsCircle(23f));
