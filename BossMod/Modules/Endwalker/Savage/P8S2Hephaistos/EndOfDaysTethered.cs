namespace BossMod.Endwalker.Savage.P8S2;

class EndOfDaysTethered : BossComponent
{
    private List<(Actor source, Actor target)> _tethers = new(); // enemy -> player

    private static readonly AOEShapeRect _shape = new(60, 5);

    public bool Active => _tethers.Count > 0;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var tetheredCaster = _tethers.FirstOrDefault(t => t.target == actor).source;
        if (tetheredCaster == null)
            return; // non-tethered players shouldn't need to worry about this mechanic

        if (module.Raid.WithoutSlot().Exclude(actor).InShape(_shape, tetheredCaster).Any())
            hints.Add("Bait away from raid!");
        if (_tethers.Any(t => t.target != actor && _shape.Check(actor.Position, t.source)))
            hints.Add("Move away from other baits!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var t in _tethers)
            _shape.Draw(arena, t.source);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var t in _tethers)
            arena.AddLine(t.source.Position, t.target.Position, ArenaColor.Danger);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.IllusoryHephaistosMovable)
        {
            var target = module.WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EndOfDaysMovable)
            _tethers.RemoveAll(e => e.source == caster);
    }
}
