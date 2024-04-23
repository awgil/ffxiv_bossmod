namespace BossMod.Stormblood.Ultimate.UWU;

class P4ViscousAetheroplasmApply(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ViscousAetheroplasmApply), new AOEShapeCircle(2), (uint)OID.UltimaWeapon, originAtTarget: true);

// TODO: if aetheroplasm target is the same as homing laser target, assume it is being soaked solo; consider merging these two components
class P4ViscousAetheroplasmResolve(BossModule module) : Components.UniformStackSpread(module, 4, 0, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HomingLasers)
        {
            // update avoid target to homing laser target
            BitMask avoid = new();
            avoid.Set(Raid.FindSlot(spell.TargetID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = avoid;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ViscousAetheroplasmApply:
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target != null)
                    AddStack(target, default, Raid.WithSlot(true).WhereActor(a => a.InstanceID != spell.MainTargetID && a.Role == Role.Tank).Mask());
                break;
            case AID.ViscousAetheroplasmResolve:
                Stacks.Clear();
                break;
            case AID.HomingLasers:
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers.Reset();
                break;
        }
    }
}

class P5ViscousAetheroplasmTriple(BossModule module) : Components.UniformStackSpread(module, 4, 0, 8)
{
    public int NumCasts { get; private set; }
    private readonly List<(Actor target, DateTime resolve)> _aetheroplasms = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
        {
            _aetheroplasms.Add((actor, status.ExpireAt));
            _aetheroplasms.SortBy(a => a.resolve);
            UpdateStackTargets();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ViscousAetheroplasmResolve)
        {
            ++NumCasts;
            _aetheroplasms.RemoveAll(a => a.target.InstanceID == spell.MainTargetID);
            UpdateStackTargets();
        }
    }

    private void UpdateStackTargets()
    {
        Stacks.Clear();
        if (_aetheroplasms.Count > 0)
            AddStack(_aetheroplasms[0].target, _aetheroplasms[0].resolve);
    }
}
