namespace BossMod.Global.Crucible.BorgnyTheVenomous;

public enum OID : uint
{
    Boss = 0x4CD8,
    Helper = 0x233C,
}

class BorgnyTheVenomousStates : StateMachineBuilder
{
    public BorgnyTheVenomousStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14628, BitmapType = BossModuleInfo.BitmapType.Enabled)]
public class BorgnyTheVenomous(WorldState ws, Actor primary) : BossModule(ws, primary, new(920, -420), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = new(20, Utils.LoadResource<RelSimplifiedComplexPolygon>("BossMod.Global.Crucible.Master1.FinalBoss.json"));
}

