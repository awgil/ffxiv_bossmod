namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ThornedCatharsis : Components.RaidwideCast
{
    public ThornedCatharsis(BossModule module) : base(module, AID._Weaponskill_ThornedCatharsis)
    {
        KeepOnPhaseChange = true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(16));
