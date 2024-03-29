namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to inversive chlamys mechanic (tethers)
// note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
class InversiveChlamys : BossComponent
{
    private bool _assigned = false;
    private BitMask _tetherForbidden;
    private BitMask _tetherTargets;
    private BitMask _tetherInAOE;

    private static readonly float _aoeRange = 5;

    public bool TethersActive => _tetherTargets.Any();

    public override void Update(BossModule module)
    {
        if (!_assigned)
        {
            var coils = module.FindComponent<BeloneCoils>();
            if (coils == null)
            {
                // assign from bloodrake tethers
                _tetherForbidden = module.Raid.WithSlot().Tethered(TetherID.Bloodrake).Mask();
                _assigned = true;
            }
            else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
            {
                // assign from coils (note that it happens with some delay)
                _tetherForbidden = module.Raid.WithSlot().WhereActor(coils.IsValidSoaker).Mask();
                _assigned = true;
            }
        }

        _tetherTargets = _tetherInAOE = new();
        if (_tetherForbidden.None())
            return;

        foreach ((int i, var player) in module.Raid.WithSlot().Tethered(TetherID.Chlamys))
        {
            _tetherTargets.Set(i);
            _tetherInAOE |= module.Raid.WithSlot().InRadiusExcluding(player, _aoeRange).Mask();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_tetherForbidden.None())
            return;

        if (!_tetherForbidden[slot])
        {
            // we should be grabbing tethers
            if (_tetherTargets.None())
            {
                hints.Add("Tethers: prepare to intercept", false);
            }
            else if (!_tetherTargets[slot])
            {
                hints.Add("Tethers: intercept!");
            }
            else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRange).Any())
            {
                hints.Add("Tethers: GTFO from others!");
            }
            else
            {
                hints.Add("Tethers: OK", false);
            }
        }
        else
        {
            // we should be passing tethers
            if (_tetherTargets.None())
            {
                hints.Add("Tethers: prepare to pass", false);
            }
            else if (_tetherTargets[slot])
            {
                hints.Add("Tethers: pass!");
            }
            else if (_tetherInAOE[slot])
            {
                hints.Add("Tethers: GTFO from aoe!");
            }
            else
            {
                hints.Add("Tethers: avoid", false);
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var forbidden = module.Raid.WithSlot(true).IncludedInMask(_tetherForbidden).FirstOrDefault().Item2;
        if (forbidden != null)
        {
            hints.Add($"Intercept: {(forbidden.Role is Role.Tank or Role.Healer ? "DD" : "Tanks/Healers")}");
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_tetherTargets.None())
            return;

        var failingPlayers = _tetherForbidden & _tetherTargets;
        foreach ((int i, var player) in module.Raid.WithSlot())
        {
            bool failing = failingPlayers[i];
            bool inAOE = _tetherInAOE[i];
            arena.Actor(player, failing ? ArenaColor.Danger : (inAOE ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric));

            if (player.Tether.ID == (uint)TetherID.Chlamys)
            {
                arena.AddLine(player.Position, module.PrimaryActor.Position, failing ? ArenaColor.Danger : ArenaColor.Safe);
                arena.AddCircle(player.Position, _aoeRange, ArenaColor.Danger);
            }
        }
    }
}
