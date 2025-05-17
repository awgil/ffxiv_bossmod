namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

// initial aoe + tethers
class Tangle(BossModule module) : Components.StandardAOEs(module, AID.Tangle, new AOEShapeCircle(6))
{
    public int NumTethers { get; private set; }
    private readonly Actor?[] _tethers = new Actor?[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tether = _tethers[pcSlot];
        if (tether != null)
        {
            Arena.AddCircle(tether.Position, 8, ArenaColor.Object);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (Raid.TryFindSlot(source, out var slot) && target != null)
            {
                _tethers[slot] = target;
                ++NumTethers;
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle)
        {
            if (Raid.TryFindSlot(source.InstanceID, out var slot))
            {
                _tethers[slot] = null;
                --NumTethers;
            }
        }
    }
}
