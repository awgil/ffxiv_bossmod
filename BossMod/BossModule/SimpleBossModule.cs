namespace BossMod;

// base class for simple boss modules (hunts, fates, dungeons, etc.)
// these always center map around PC
public abstract class SimpleBossModule(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(30))
{
    private WPos _prevFramePathfindCenter;

    public override bool CheckReset() => !PrimaryActor.InCombat;

    protected override void UpdateModule()
    {
        Arena.Center = WorldState.Party.Player()?.Position ?? default;
        // we don't want to change pathfinding map origin every time player slightly moves, it makes movement jittery
        // instead, (a) quantize origin and (b) only update it when player moves sufficiently far away
        if (!_prevFramePathfindCenter.AlmostEqual(Arena.Center, 5))
            _prevFramePathfindCenter = Arena.Center.Rounded();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PathfindMapCenter = _prevFramePathfindCenter;
        hints.PathfindMapBounds = AIHints.DefaultBounds;
    }
}
