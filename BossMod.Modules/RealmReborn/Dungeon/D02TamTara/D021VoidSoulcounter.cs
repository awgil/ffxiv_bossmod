namespace BossMod.RealmReborn.Dungeon.D02TamTara.D021VoidSoulcounter;

public enum OID : uint
{
    Boss = 0x237 // R1.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    Enthunder = 430, // Boss->self, 1.5s cast, single target
    DarkOrbs = 911, // Boss->player, no cast, single target
    Condemnation = 912 // Boss->self, 2.5s cast, range 6+R (7) 90-degree cone aoe
}

class Condemnation(BossModule module) : Components.StandardAOEs(module, AID.Condemnation, new AOEShapeCone(7, 45.Degrees()));

class D021VoidSoulcounterStates : StateMachineBuilder
{
    public D021VoidSoulcounterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Condemnation>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 2, NameID = 455)]
public class D021VoidSoulcounter(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
