namespace BossMod.Endwalker.Savage.P2SHippokampos;

// note: if activated together with ChannelingFlow, it does not target next flow arrows
class TaintedFlood : Components.CastCounter
{
    private BitMask _ignoredTargets;

    private const float _radius = 6;

    public TaintedFlood(BossModule module) : base(module, ActionID.MakeSpell(AID.TaintedFloodAOE))
    {
        var flow = module.FindComponent<ChannelingFlow>();
        if (flow != null)
        {
            _ignoredTargets = Raid.WithSlot().WhereSlot(flow.SlotActive).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[slot])
        {
            // player is not a target of flood, so just make sure he is not clipped by others
            if (Raid.WithSlot().ExcludedFromMask(_ignoredTargets).InRadius(actor.Position, _radius).Any())
                hints.Add("GTFO from flood!");
        }
        else
        {
            // player is target of flood => make sure no one is in range
            if (Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any())
                hints.Add("Spread!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[pcSlot])
        {
            foreach ((_, var actor) in Raid.WithSlot().ExcludedFromMask(_ignoredTargets))
            {
                Arena.Actor(actor, ArenaColor.Danger);
                Arena.AddCircle(actor.Position, _radius, ArenaColor.Danger);
            }
        }
        else
        {
            Arena.AddCircle(pc.Position, _radius, ArenaColor.Danger);
            foreach (var player in Raid.WithoutSlot().Exclude(pc))
                Arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
    }
}
