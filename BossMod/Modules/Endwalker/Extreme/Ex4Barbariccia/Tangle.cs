namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

// initial aoe + tethers
class Tangle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tangle, 6f)
{
    public int NumTethers;
    private readonly Actor?[] _tethers = new Actor?[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tether = _tethers[pcSlot];
        if (tether != null)
        {
            Arena.ZoneCircleOutline(tether.Position, 8f, Colors.Object);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            var target = WorldState.Actors.Find(tether.Target);
            if (slot >= 0 && target != null)
            {
                _tethers[slot] = target;
                ++NumTethers;
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            if (slot >= 0)
            {
                _tethers[slot] = null;
                --NumTethers;
            }
        }
    }
}
