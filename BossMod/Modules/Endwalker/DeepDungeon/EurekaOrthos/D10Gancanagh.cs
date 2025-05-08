namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D10Gancanagh;

public enum OID : uint
{
    Boss = 0x3D54, // R1.800, x?
    PachypodiumMine = 0x3D55, // R1.500, x9
}

public enum AID : uint
{
    Attack = 6499, // Boss->player, no cast, single-target // direct auto attack
    AuthoritativeShriek = 31477, // Boss->self, 3.0s cast, single-target, Activates the PachypodiumMine on the floor
    MandrashockSingle = 31478, // 3D55->self, 5.0s cast, range 10 circle, Floor drones casting at the same time, usually within a pattern
    MandrashockTitan = 32700, // 3D55->self, 8.0s cast, range 10 circle,  Floor drones casting in a titan-formation
    Mandrastorm = 31479, // Boss->self, 5.0s cast, range 60 circle, Proximity AOE from boss ? distance (18.5f seems the the point where it will hit for flat damage)
}

class MandrashockSingle(BossModule module) : Components.StandardAOEs(module, AID.MandrashockSingle, new AOEShapeCircle(10f));
class MandraShockTitan(BossModule module) : Components.StandardAOEs(module, AID.MandrashockTitan, new AOEShapeCircle(10f), 6); // made this 6 at the second to keep the safe spot shown for 3rd hit.
class MandraStorm(BossModule module) : Components.StandardAOEs(module, AID.Mandrastorm, new AOEShapeCircle(18.5f));

class D10GancanaghStates : StateMachineBuilder
{
    public D10GancanaghStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MandrashockSingle>()
            .ActivateOnEnter<MandraShockTitan>()
            .ActivateOnEnter<MandraStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Puni.sh Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 897, NameID = 12240)]
public class D10Gancanagh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(19.5f));
