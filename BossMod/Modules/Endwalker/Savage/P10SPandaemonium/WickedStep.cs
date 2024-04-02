namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class WickedStep : Components.Knockback
{
    private Actor?[] _towers = { null, null };

    private static readonly float _towerRadius = 4;
    private static readonly float _knockbackRadius = 36;

    public WickedStep() : base(ignoreImmunes: true) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        foreach (var s in _towers.Where(s => s?.Position.InCircle(actor.Position, _towerRadius) ?? false))
            yield return new(s!.Position, _knockbackRadius, s!.CastInfo!.NPCFinishAt);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);

        bool soaking = _towers.Any(t => t?.Position.InCircle(actor.Position, _towerRadius) ?? false);
        bool shouldSoak = actor.Role == Role.Tank;
        if (soaking != shouldSoak)
            hints.Add(shouldSoak ? "Soak the tower!" : "GTFO from tower!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var t in _towers)
            if (t != null)
                Components.GenericTowers.DrawTower(arena, t.Position, _towerRadius, pc.Role == Role.Tank);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var index = ActionToIndex(spell.Action);
        if (index >= 0)
            _towers[index] = caster;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var index = ActionToIndex(spell.Action);
        if (index >= 0)
        {
            _towers[index] = null;
            ++NumCasts;
        }
    }

    private int ActionToIndex(ActionID aid) => (AID)aid.ID switch
    {
        AID.WickedStepAOE1 => 0,
        AID.WickedStepAOE2 => 1,
        _ => -1
    };
}
