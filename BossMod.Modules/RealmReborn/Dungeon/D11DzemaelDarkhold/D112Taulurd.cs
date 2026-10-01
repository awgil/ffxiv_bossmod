namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D112Taulurd;

public enum OID : uint
{
    Boss = 0x4A8B, // R1.560, Taulurd
    DeepvoidSlave = 0x4A8C, // R1.040
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    ElbowDrop = 45572, // Boss->self, 2.7s cast, range 8 90-degree cone
    BossHit45573 = 45573, // Boss->self, no cast // TODO: identify from replay
    BossHit45574 = 45574, // Boss->self, no cast // TODO: identify from replay
    Firewater = 45575, // DeepvoidSlave->location, 3.7s cast, range 5 circle
    SlaveHit = 45576, // DeepvoidSlave->self, no cast // TODO: identify from replay
}

class ElbowDrop(BossModule module) : Components.StandardAOEs(module, AID.ElbowDrop, new AOEShapeCone(8, 45.Degrees()));
class Firewater(BossModule module) : Components.StandardAOEs(module, AID.Firewater, 5);
class Slaves(BossModule module) : Components.Adds(module, (uint)OID.DeepvoidSlave, 1);

class D112TaulurdStates : StateMachineBuilder
{
    public D112TaulurdStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElbowDrop>()
            .ActivateOnEnter<Firewater>()
            .ActivateOnEnter<Slaves>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1415)] // TODO: clear after unnamed hit AIDs identified
public class D112Taulurd(ModuleInit init) : BossModule(init, new(-93, -33), new ArenaBoundsSquare(20));
