namespace BossMod.Heavensward.FATE.Darkscale3;

public enum OID : uint
{
    Boss = 0x12BB,
    Helper = 0x233C,
}

class Darkscale3States : StateMachineBuilder
{
    public Darkscale3States(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 870, NameID = 3983)]
//public class Darkscale3(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
