namespace BossMod.Endwalker.Hunt.RankS.KerShroud;

public enum OID : uint
{
    Boss = 0x3672, // R2.500, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AccursedPox = 27725, // Boss->location, 5.0s cast, range 8 circle
    EntropicFlame = 27724, // Boss->self, 4.0s cast, range 60 width 8 rect
}

class AccursedPox(BossModule module) : Components.StandardAOEs(module, AID.AccursedPox, 8);

class EntropicFlame(BossModule module) : Components.StandardAOEs(module, AID.EntropicFlame, new AOEShapeRect(60, 4));

class KerShroudStates : StateMachineBuilder
{
    public KerShroudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AccursedPox>()
            .ActivateOnEnter<EntropicFlame>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 10616)]
public class KerShroud(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
