namespace BossMod.Dawntrail.Hunt.RankS.CrystalIncarnation;

public enum OID : uint
{
    Boss = 0x4571, // R2.400, x1
}

public enum AID : uint
{
    AutoAttack = 39622, // Boss->player, no cast, single-target
    FireII = 39623, // Boss->location, 5.0s cast, range 5 circle
    BlizzardII = 39624, // Boss->self, 5.0s cast, range 40 45-degree cone
}

class FireII(BossModule module) : Components.StandardAOEs(module, AID.FireII, 5);
class BlizzardII(BossModule module) : Components.StandardAOEs(module, AID.BlizzardII, new AOEShapeCone(40, 22.5f.Degrees()));

class CrystalIncarnationStates : StateMachineBuilder
{
    public CrystalIncarnationStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<BlizzardII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13407)]
public class CrystalIncarnation(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
