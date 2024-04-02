namespace BossMod.ReplayAnalysis;

// note: there are tons of 'unexpected' effect results that we don't do much about, at least now:
// - SGE kardia reapplication: caster won't get confirmation for ApplyStatusEffectSource (initial application has ER)
// - AST draw, earthly star: buffs are granted immediately, no ER
// - 100% overheal has no ER?..
// - damage which does not affect HP (e.g. holmgang at 1 hp) has no ER
// - 0 damage has no ER
// - stuff like actor disappearing right after cast event will have no ER
class EffectResultMispredict
{
    private List<(Replay r, Replay.Action a)> _unexpected = new();

    public EffectResultMispredict(List<Replay> replays, bool showMissing)
    {
        foreach (var r in replays)
        {
            foreach (var a in r.Actions)
            {
                bool unexpected = false;

                foreach (var t in a.Targets)
                {
                    bool expectConfirmSource = false;
                    bool expectConfirmTarget = false;
                    foreach (var eff in t.Effects)
                    {
                        if (eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage or ActionEffectType.Heal or ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource or ActionEffectType.RecoveredFromStatusEffect)
                        {
                            if (t.Target == a.Source)
                                expectConfirmSource = expectConfirmTarget = true;
                            else if (eff.AtSource)
                                expectConfirmSource = true;
                            else
                                expectConfirmTarget = true;
                        }
                    }
                    bool haveConfirmSource = t.ConfirmationSource != default;
                    bool haveConfirmTarget = t.ConfirmationTarget != default;
                    if (IsRelevant(showMissing, expectConfirmSource, haveConfirmSource) || IsRelevant(showMissing, expectConfirmTarget, haveConfirmTarget))
                    {
                        unexpected = true;
                        break;
                    }
                }

                if (unexpected)
                    _unexpected.Add((r, a));
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var e in tree.Nodes(_unexpected, e => new($"{e.r.Path} @ {e.a.Timestamp:O}: #{e.a.GlobalSequence} {e.a.ID} {ReplayUtils.ParticipantString(e.a.Source, e.a.Timestamp)} -> {ReplayUtils.ParticipantString(e.a.MainTarget, e.a.Timestamp)}")))
        {
            foreach (var t in tree.Nodes(e.a.Targets, t => new(ReplayUtils.ActionTargetString(t, e.a.Timestamp))))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }
    }

    private bool IsRelevant(bool showMissing, bool expect, bool have) => showMissing ? (expect && !have) : (have && !expect);
}
