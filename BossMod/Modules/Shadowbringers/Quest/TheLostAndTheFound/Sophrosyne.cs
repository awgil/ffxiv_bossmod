namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Sophrosyne;

public enum OID : uint
{
    Boss = 0x29AA,
    Helper = 0x233C,
}

public enum AID : uint
{
    Charge = 16999, // 29AB->29A9, 3.0s cast, width 4 rect charge
}

class Charge(BossModule module) : Components.ChargeAOEs(module, AID.Charge, 2);

class SophrosyneStates : StateMachineBuilder
{
    public SophrosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Charge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8395)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(632, 64.15f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
