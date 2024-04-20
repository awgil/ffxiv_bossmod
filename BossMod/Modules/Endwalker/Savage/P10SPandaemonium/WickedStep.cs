namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class WickedStep(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly Actor?[] _towers = [null, null];

    private const float _towerRadius = 4;
    private const float _knockbackRadius = 36;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var s in _towers.Where(s => s?.Position.InCircle(actor.Position, _towerRadius) ?? false))
            yield return new(s!.Position, _knockbackRadius, s!.CastInfo!.NPCFinishAt);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        bool soaking = _towers.Any(t => t?.Position.InCircle(actor.Position, _towerRadius) ?? false);
        bool shouldSoak = actor.Role == Role.Tank;
        if (soaking != shouldSoak)
            hints.Add(shouldSoak ? "Soak the tower!" : "GTFO from tower!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var t in _towers)
            if (t != null)
                Components.GenericTowers.DrawTower(Arena, t.Position, _towerRadius, pc.Role == Role.Tank);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var index = ActionToIndex(spell.Action);
        if (index >= 0)
            _towers[index] = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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
