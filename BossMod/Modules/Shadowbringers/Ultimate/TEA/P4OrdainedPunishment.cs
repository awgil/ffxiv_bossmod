namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4OrdainedCapitalPunishment(BossModule module) : Components.GenericSharedTankbuster(module, ActionID.MakeSpell(AID.OrdainedCapitalPunishmentAOE), 4)
{
    public override void Update()
    {
        Target = Source != null ? WorldState.Actors.Find(Source.TargetID) : null;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OrdainedCapitalPunishment)
        {
            Source = caster;
            Activation = spell.NPCFinishAt.AddSeconds(3.1f);
        }
    }
}

// TODO: dedicated tankbuster component with tankswap hint
class P4OrdainedPunishment(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.OrdainedPunishment), 5);
