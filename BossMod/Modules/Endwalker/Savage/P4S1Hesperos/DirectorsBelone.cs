namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to director's belone (debuffs) mechanic
// note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
class DirectorsBelone : BossComponent
{
    private bool _assigned = false;
    private BitMask _debuffForbidden;
    private BitMask _debuffTargets;
    private BitMask _debuffImmune;

    private static readonly float _debuffPassRange = 3; // not sure about this...

    public override void Update(BossModule module)
    {
        if (!_assigned)
        {
            var coils = module.FindComponent<BeloneCoils>();
            if (coils == null)
            {
                // assign from bloodrake tethers
                _debuffForbidden = module.Raid.WithSlot().Tethered(TetherID.Bloodrake).Mask();
                _assigned = true;
            }
            else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
            {
                // assign from coils (note that it happens with some delay)
                _debuffForbidden = module.Raid.WithSlot().WhereActor(coils.IsValidSoaker).Mask();
                _assigned = true;
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_debuffForbidden.None())
            return;

        if (!_debuffForbidden[slot])
        {
            // we should be grabbing debuff
            if (_debuffTargets.None())
            {
                // debuffs not assigned yet => spread and prepare to grab
                bool stacked = module.Raid.WithoutSlot().InRadiusExcluding(actor, _debuffPassRange).Any();
                hints.Add("Debuffs: spread and prepare to handle!", stacked);
            }
            else if (_debuffImmune[slot])
            {
                hints.Add("Debuffs: failed to handle");
            }
            else if (_debuffTargets[slot])
            {
                hints.Add("Debuffs: OK", false);
            }
            else
            {
                hints.Add("Debuffs: grab!");
            }
        }
        else
        {
            // we should be passing debuff
            if (_debuffTargets.None())
            {
                bool badStack = module.Raid.WithSlot().Exclude(slot).IncludedInMask(_debuffForbidden).OutOfRadius(actor.Position, _debuffPassRange).Any();
                hints.Add("Debuffs: stack and prepare to pass!", badStack);
            }
            else if (_debuffTargets[slot])
            {
                hints.Add("Debuffs: pass!");
            }
            else
            {
                hints.Add("Debuffs: avoid", false);
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var forbidden = module.Raid.WithSlot(true).IncludedInMask(_debuffForbidden).FirstOrDefault().Item2;
        if (forbidden != null)
        {
            hints.Add($"Stack: {(forbidden.Role is Role.Tank or Role.Healer ? "Tanks/Healers" : "DD")}");
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_debuffTargets.None())
            return;

        var failingPlayers = _debuffForbidden & _debuffTargets;
        foreach ((int i, var player) in module.Raid.WithSlot())
        {
            arena.Actor(player, failingPlayers[i] ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RoleCall:
                _debuffTargets.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Miscast:
                _debuffImmune.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RoleCall:
                _debuffTargets.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Miscast:
                _debuffImmune.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CursedCasting1 or AID.CursedCasting2)
            _debuffForbidden.Reset();
    }
}
