using BossMod.Interfaces;

namespace BossMod.Mocks;

internal class MockWorldStateFactory : IWorldStateFactory
{
    public RealWorld Create()
    {
        var rw = new RealWorld(0, "unknown");

        // player actor (most windows auto-hide if the player isn't present)
        rw.Execute(new ActorState.OpCreate(0xEEEE0000, 0, 200, 0, "Player", 0, ActorType.Player, Class.WAR, 100, new(100, 0, 105, MathF.PI), 0.5f, new ActorHPMP(100_000, 120_000, 5_000, 8_000, 10_000), true, true, 0, 0));
        rw.Execute(new PartyState.OpModify(0, new(0xFFFF0000, 0xEEEE0000, false, "Player")));

        // trigger striking dummy bossmodule activation
        rw.Execute(new ActorState.OpCreate(0xEEEE0001, (uint)StrikingDummy.OID.Boss, 0, 0, "Striking Dummy", 541, ActorType.Enemy, Class.None, 1, new(100, 0, 100, 0), 1, new(100, 100, 0, 0, 0), true, false, 0, 0));

        rw.Execute(new ActorState.OpTarget(0xEEEE0000, 0xEEEE0001));

        return rw;
    }
}
