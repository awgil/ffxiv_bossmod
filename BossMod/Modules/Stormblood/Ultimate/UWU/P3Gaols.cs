namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: add sludge voidzones?..
class P3Gaols : Components.GenericAOEs
{
    public enum State { None, TargetSelection, Fetters, Done }

    public State CurState { get; private set; }
    private BitMask _targets;

    private static readonly AOEShapeCircle _freefireShape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (CurState == State.Fetters && !_targets[slot])
            foreach (var (_, target) in module.Raid.WithSlot(true).IncludedInMask(_targets))
                yield return new(_freefireShape, target.Position);
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurState == State.TargetSelection && _targets.Any())
        {
            var hint = string.Join(" > ", Service.Config.Get<UWUConfig>().P3GaolPriorities.Resolve(module.Raid).Where(i => _targets[i.slot]).OrderBy(i => i.group).Select(i => module.Raid[i.slot]?.Name));
            hints.Add($"Gaols: {hint}");
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Fetters)
        {
            if (CurState == State.TargetSelection)
            {
                CurState = State.Fetters;
                _targets.Reset(); // note that if target dies, fetters is applied to random player
            }
            _targets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.RockThrowBoss:
            case AID.RockThrowHelper:
                CurState = State.TargetSelection;
                _targets.Set(module.Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.FreefireGaol:
                var (closestSlot, closestPlayer) = module.Raid.WithSlot(true).IncludedInMask(_targets).Closest(caster.Position);
                if (closestPlayer != null)
                {
                    _targets.Clear(closestSlot);
                    if (_targets.None())
                        CurState = State.Done;
                }
                break;
        }
    }
}
