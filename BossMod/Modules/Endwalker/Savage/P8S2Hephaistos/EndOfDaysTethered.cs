namespace BossMod.Endwalker.Savage.P8S2;

class EndOfDaysTethered(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor source, Actor target)> _tethers = []; // enemy -> player

    private static readonly AOEShapeRect _shape = new(60, 5);

    public bool Active => _tethers.Count > 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var tetheredCaster = _tethers.FirstOrDefault(t => t.target == actor).source;
        if (tetheredCaster == null)
            return; // non-tethered players shouldn't need to worry about this mechanic

        if (Raid.WithoutSlot().Exclude(actor).InShape(_shape, tetheredCaster).Any())
            hints.Add("Bait away from raid!");
        if (_tethers.Any(t => t.target != actor && _shape.Check(actor.Position, t.source)))
            hints.Add("Move away from other baits!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            _shape.Draw(Arena, t.source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            Arena.AddLine(t.source.Position, t.target.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.IllusoryHephaistosMovable)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EndOfDaysMovable)
            _tethers.RemoveAll(e => e.source == caster);
    }
}
