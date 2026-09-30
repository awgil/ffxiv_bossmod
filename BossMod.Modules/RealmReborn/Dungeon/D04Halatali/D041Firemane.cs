namespace BossMod.RealmReborn.Dungeon.D04Halatali.D041Firemane;

public enum OID : uint
{
    Boss = 0x4643,
    Helper = 0x233C
}

public enum AID : uint
{
    Fire = 40055, // Boss->player, no cast, single-target
    FireII = 40592, // Boss->location, 4.0s cast, range 5 circle

    FireflowVisual = 40587, // Boss->self, 6.0s cast, single-target
    Fireflow1 = 40588, // Helper->self, 6.0s cast, range 60 45-degree cone
    Fireflow2 = 40589, // Helper->self, 9.0s cast, range 60 45-degree cone

    BurningBoltVisual = 40590, // Boss->self, 5.0s cast, single-target
    BurningBolt = 40591 // Helper->player, 5.0s cast, single-target
}

class FireII(BossModule module) : Components.StandardAOEs(module, AID.FireII, 5);
class Fireflow1(BossModule module) : Components.StandardAOEs(module, AID.Fireflow1, new AOEShapeCone(60, 22.5f.Degrees()));
class Fireflow2(BossModule module) : Components.StandardAOEs(module, AID.Fireflow2, new AOEShapeCone(60, 22.5f.Degrees()));
class BurningBolt(BossModule module) : Components.SingleTargetCast(module, AID.BurningBolt);

class D041FiremaneStates : StateMachineBuilder
{
    public D041FiremaneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Fireflow1>()
            .ActivateOnEnter<Fireflow2>()
            .ActivateOnEnter<BurningBolt>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1194)]
public class D041Firemane(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
