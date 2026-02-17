using BossMod.Interfaces;

namespace BossMod.Mocks;

sealed class MockWorldStateSync(WorldState ws) : IWorldStateSync
{
    bool _firstTick;

    public void Dispose() { }
    public void Update(TimeSpan prevFramePerf)
    {
        if (_firstTick)
            return;

        _firstTick = true;

        ws.Execute(new ActorState.OpCreate(0xEEEE0000, 0, 200, 0, "Player", 0, ActorType.Player, Class.WAR, 100, new(100, 0, 105, MathF.PI), 0.5f, new ActorHPMP(100_000, 120_000, 5_000, 8_000, 10_000), true, true, 0, 0));
        ws.Execute(new PartyState.OpModify(0, new(0xFFFF0000, 0xEEEE0000, false, "Player")));

        // trigger striking dummy bossmodule activation
        ws.Execute(new ActorState.OpCreate(0xEEEE0001, (uint)StrikingDummy.OID.Boss, 0, 0, "Striking Dummy", 541, ActorType.Enemy, Class.None, 1, new(100, 0, 100, 0), 1, new(100, 100, 0, 0, 0), true, false, 0, 0));

        ws.Execute(new ActorState.OpTarget(0xEEEE0000, 0xEEEE0001));
    }
}
