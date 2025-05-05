namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ThornedCatharsis : Components.RaidwideCast
{
    public ThornedCatharsis(BossModule module) : base(module, AID.ThornedCatharsis)
    {
        KeepOnPhaseChange = true;
    }
}

class AlexandrianBanishII(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AlexandrianBanishII, AID._Spell_AlexandrianBanishII1, 5, 5.8f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861, PlanLevel = 100)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(16));
