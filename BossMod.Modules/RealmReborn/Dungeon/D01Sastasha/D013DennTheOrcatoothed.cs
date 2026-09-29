namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D013DennTheOrcatoothed;

public enum OID : uint
{
    Boss = 0x4B6, // R?, Denn the Orcatoothed (NameID 1206)
    BaleenGuard = 0x4B7, // R?, NameID 1207 (邓恩的护卫)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    TrueThrust = 75, // Boss->player, no cast, single-target
    Hydroball = 556, // Boss->self, 3.5s cast, range 6 90-degree cone (silence)
}

class Hydroball(BossModule module) : Components.StandardAOEs(module, AID.Hydroball, new AOEShapeCone(6, 45.Degrees()));
class BaleenGuards(BossModule module) : Components.Adds(module, (uint)OID.BaleenGuard, 1);

class D013DennTheOrcatoothedStates : StateMachineBuilder
{
    public D013DennTheOrcatoothedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydroball>()
            .ActivateOnEnter<BaleenGuards>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1206)]
public class D013DennTheOrcatoothed(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
