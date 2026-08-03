namespace BossMod.Endwalker.Savage.P2SHippokampos;

// note: if activated together with ChannelingFlow, it does not target next flow arrows
class TaintedFlood : Components.CastCounter
{
    private BitMask _ignoredTargets;

    private const float _radius = 6;

    public TaintedFlood(BossModule module) : base(module, (uint)AID.TaintedFloodAOE)
    {
        var flow = module.FindComponent<ChannelingFlow>();
        if (flow != null)
        {
            _ignoredTargets = Raid.WithSlot(false, true, true).WhereSlot(flow.SlotActive).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[slot])
        {
            // player is not a target of flood, so just make sure he is not clipped by others
            if (Raid.WithSlot(false, true, true).ExcludedFromMask(_ignoredTargets).InRadius(actor.Position, _radius).Any())
                hints.Add("GTFO from flood!");
        }
        else
        {
            // player is target of flood => make sure no one is in range
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _radius).Any())
                hints.Add("Spread!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[pcSlot])
        {
            foreach ((_, var actor) in Raid.WithSlot(false, true, true).ExcludedFromMask(_ignoredTargets))
            {
                Arena.Actor(actor, Colors.Danger);
                Arena.ZoneCircleOutline(actor.Position, _radius, Colors.Danger);
            }
        }
        else
        {
            Arena.ZoneCircleOutline(pc.Position, _radius, Colors.Danger);
            foreach (var player in Raid.WithoutSlot(false, true, true).Exclude(pc))
                Arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? Colors.PlayerInteresting : Colors.PlayerGeneric);
        }
    }
}
