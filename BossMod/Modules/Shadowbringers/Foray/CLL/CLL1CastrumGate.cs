#if DEBUG
namespace BossMod.Shadowbringers.Foray.CLL.CLL1CastrumGate;

public enum OID : uint
{
    Boss = 0x2ED9,
    Helper = 0x233C,
}

class CastrumGateStates : StateMachineBuilder
{
    public CastrumGateStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9441)]
public class CastrumGate(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -177.3f), new ArenaBoundsRect(30, 26.7f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat && WorldState.Party.Player() is { } player && Bounds.Contains(player.Position - Arena.Center);
}
#endif
