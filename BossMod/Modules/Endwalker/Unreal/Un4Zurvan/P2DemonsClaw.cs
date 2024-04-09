namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P2DemonsClawKnockback(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.DemonsClaw), true)
{
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            yield return new(_caster.Position, 17, _caster.CastInfo.NPCFinishAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = null;
    }
}

class P2DemonsClawWaveCannon(BossModule module) : Components.GenericWildCharge(module, 5, ActionID.MakeSpell(AID.WaveCannonShared))
{
    public Actor? Target { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = caster;
            foreach (var (slot, player) in Raid.WithSlot())
            {
                PlayerRoles[slot] = player == Target ? PlayerRole.Target : PlayerRole.Share;
            }
        }
        else if ((AID)spell.Action.ID == AID.DemonsClaw)
        {
            Target = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Source = null;
    }
}
