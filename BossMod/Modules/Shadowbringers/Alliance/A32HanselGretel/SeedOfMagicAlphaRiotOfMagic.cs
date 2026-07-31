namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

[SkipLocalsInit]
sealed class RiotOfMagicSeedOfMagicAlpha(BossModule module) : Components.CastStackSpread(module, (uint)AID.RiotOfMagic, (uint)AID.SeedOfMagicAlpha, 5f, 5f, 18, 18, true)
{
    private BitMask forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == SpreadAction && WorldState.Actors.Find(spell.TargetID) is Actor spreadTarget) // stack and spreads can appear in any order during the same frame
        {
            forbidden.Set(Raid.FindSlot(spell.TargetID));
            if (Stacks.Count != 0)
            {
                Stacks.Ref(0).ForbiddenPlayers = forbidden;
            }
            AddSpread(spreadTarget, Module.CastFinishAt(spell));
        }
        else if (id == StackAction && WorldState.Actors.Find(spell.TargetID) is Actor stackTarget)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell), forbidden);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == StackAction)
        {
            forbidden = default;
        }
    }
}
