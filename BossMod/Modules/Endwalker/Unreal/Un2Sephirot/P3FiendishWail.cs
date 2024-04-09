namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P3FiendishWail : Components.CastCounter
{
    private BitMask _physResistMask;
    private List<Actor> _towers = new();

    public bool Active => _towers.Count > 0;

    private static readonly float _radius = 5;

    public P3FiendishWail() : base(ActionID.MakeSpell(AID.FiendishWailAOE)) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        bool wantToSoak = _physResistMask.Any() ? _physResistMask[slot] : actor.Role == Role.Tank;
        bool soaking = _towers.InRadius(actor.Position, _radius).Any();
        if (wantToSoak)
            hints.Add("Soak the tower!", !soaking);
        else
            hints.Add("GTFO from tower!", soaking);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _towers)
            arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ForceAgainstMight)
            _physResistMask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _towers.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _towers.Remove(caster);
    }
}
