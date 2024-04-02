namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4OrdainedCapitalPunishment : Components.GenericSharedTankbuster
{
    public P4OrdainedCapitalPunishment() : base(ActionID.MakeSpell(AID.OrdainedCapitalPunishmentAOE), 4) { }

    public override void Update(BossModule module)
    {
        Target = Source != null ? module.WorldState.Actors.Find(Source.TargetID) : null;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OrdainedCapitalPunishment)
        {
            Source = caster;
            Activation = spell.NPCFinishAt.AddSeconds(3.1f);
        }
    }
}

// TODO: dedicated tankbuster component with tankswap hint
class P4OrdainedPunishment : Components.SpreadFromCastTargets
{
    public P4OrdainedPunishment() : base(ActionID.MakeSpell(AID.OrdainedPunishment), 5) { }
}
