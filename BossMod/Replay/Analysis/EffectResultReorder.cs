namespace BossMod.ReplayAnalysis;

class EffectResultReorder
{
    private readonly List<(Replay r, Replay.Participant p, Replay.Action prev, Replay.Action next, bool prevHeal, bool nextHeal)> _reordered = [];

    public EffectResultReorder(List<Replay> replays)
    {
        foreach (var r in replays)
        {
            Dictionary<ulong, (Replay.Action, DateTime, bool)> _lastConfirms = [];
            foreach (var a in r.Actions)
            {
                foreach (var t in a.Targets)
                {
                    bool damageSource = false;
                    bool damageTarget = false;
                    bool healSource = false;
                    bool healTarget = false;
                    foreach (var eff in t.Effects)
                    {
                        if (eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)
                        {
                            if (t.Target == a.Source)
                                damageSource = damageTarget = true;
                            else if (eff.AtSource)
                                damageSource = true;
                            else
                                damageTarget = true;
                        }
                        else if (eff.Type is ActionEffectType.Heal)
                        {
                            if (t.Target == a.Source)
                                healSource = healTarget = true;
                            else if (eff.AtSource)
                                healSource = true;
                            else
                                healTarget = true;
                        }
                    }

                    if ((damageSource || healSource) && t.ConfirmationSource != default)
                    {
                        var lastConfirm = _lastConfirms.GetValueOrDefault(a.Source.InstanceID);
                        if (lastConfirm.Item2 > t.ConfirmationSource)
                            _reordered.Add((r, a.Source, lastConfirm.Item1, a, lastConfirm.Item3, healSource));
                        _lastConfirms[a.Source.InstanceID] = (a, t.ConfirmationSource, healSource);
                    }

                    if ((damageTarget || healTarget) && t.ConfirmationTarget != default)
                    {
                        var lastConfirm = _lastConfirms.GetValueOrDefault(t.Target.InstanceID);
                        if (lastConfirm.Item2 > t.ConfirmationTarget)
                            _reordered.Add((r, t.Target, lastConfirm.Item1, a, lastConfirm.Item3, healTarget));
                        _lastConfirms[t.Target.InstanceID] = (a, t.ConfirmationTarget, healTarget);
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var e in tree.Nodes(_reordered, e => new($"{e.r.Path}: {ReplayUtils.ParticipantString(e.p, e.prev.Timestamp)} {(e.prevHeal ? "heal" : "dmg")} #{e.prev.GlobalSequence} vs {(e.nextHeal ? "heal" : "dmg")} #{e.next.GlobalSequence}")))
        {
            foreach (var n in tree.Node($"Prev: {e.prev.Timestamp:O} {e.prev.ID} {ReplayUtils.ParticipantString(e.prev.Source, e.prev.Timestamp)} -> {ReplayUtils.ParticipantString(e.prev.MainTarget, e.prev.Timestamp)}"))
            {
                foreach (var t in tree.Nodes(e.prev.Targets, t => new(ReplayUtils.ActionTargetString(t, e.prev.Timestamp))))
                {
                    tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
                }
            }
            foreach (var n in tree.Node($"Next: {e.next.Timestamp:O} {e.next.ID} {ReplayUtils.ParticipantString(e.next.Source, e.next.Timestamp)} -> {ReplayUtils.ParticipantString(e.next.MainTarget, e.next.Timestamp)}"))
            {
                foreach (var t in tree.Nodes(e.next.Targets, t => new(ReplayUtils.ActionTargetString(t, e.next.Timestamp))))
                {
                    tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
                }
            }
        }
    }
}
