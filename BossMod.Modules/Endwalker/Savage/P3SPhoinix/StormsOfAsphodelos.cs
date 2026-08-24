namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to storms of asphodelos mechanics
class StormsOfAsphodelos(BossModule module) : BossComponent(module)
{
    private readonly AOEShapeCone _windsAOE = new(50, 30.Degrees());
    private readonly AOEShapeCircle _beaconAOE = new(6);
    private readonly List<Actor> _twisterTargets = [];
    private BitMask _tetherTargets;
    private BitMask _bossTargets;
    private BitMask _closeToTetherTarget;
    private BitMask _hitByMultipleAOEs;

    public override void Update()
    {
        _twisterTargets.Clear();
        _tetherTargets = _bossTargets = _closeToTetherTarget = _hitByMultipleAOEs = new();

        // we determine failing players, trying to take two reasonable tactics in account:
        // either two tanks immune and soak everything, or each player is hit by one mechanic
        // for now, we consider tether target to be a "tank"
        int[] aoesPerPlayer = new int[PartyState.MaxPartySize];

        foreach ((int i, var player) in Raid.WithSlot(true).WhereActor(x => x.Tether.Target == Module.PrimaryActor.InstanceID))
        {
            _tetherTargets.Set(i);

            ++aoesPerPlayer[i];
            foreach ((int j, var other) in Raid.WithSlot().InRadiusExcluding(player, _beaconAOE.Radius))
            {
                ++aoesPerPlayer[j];
                _closeToTetherTarget.Set(j);
            }
        }

        foreach ((int i, var player) in Raid.WithSlot().SortedByRange(Module.PrimaryActor.Position).Take(3))
        {
            _bossTargets.Set(i);
            foreach ((int j, var other) in FindPlayersInWinds(Module.PrimaryActor, player))
            {
                ++aoesPerPlayer[j];
            }
        }

        foreach (var twister in Module.Enemies(OID.DarkblazeTwister))
        {
            var target = Raid.WithoutSlot().Closest(twister.Position);
            if (target == null)
                continue; // there are no alive players - target list will be left empty

            _twisterTargets.Add(target);
            foreach ((int j, var other) in FindPlayersInWinds(twister, target))
            {
                ++aoesPerPlayer[j];
            }
        }

        for (int i = 0; i < aoesPerPlayer.Length; ++i)
            if (aoesPerPlayer[i] > 1)
                _hitByMultipleAOEs.Set(i);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank)
        {
            if (!_tetherTargets[slot])
            {
                hints.Add("Intercept tether!");
            }
            if (_hitByMultipleAOEs[slot])
            {
                hints.Add("Press invul!");
            }
        }
        else
        {
            if (_tetherTargets[slot])
            {
                hints.Add("Pass the tether!");
            }
            if (_hitByMultipleAOEs[slot])
            {
                hints.Add("GTFO from aoes!");
            }
        }
        if (_closeToTetherTarget[slot])
        {
            hints.Add("GTFO from tether!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach ((int i, var player) in Raid.WithSlot())
        {
            if (_tetherTargets[i])
            {
                _beaconAOE.Draw(Arena, player);
            }
            if (_bossTargets[i] && player.Position != Module.PrimaryActor.Position)
            {
                _windsAOE.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(player.Position - Module.PrimaryActor.Position));
            }
        }

        foreach (var (twister, target) in Module.Enemies(OID.DarkblazeTwister).Zip(_twisterTargets))
        {
            _windsAOE.Draw(Arena, twister.Position, Angle.FromDirection(target.Position - twister.Position));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var twister in Module.Enemies(OID.DarkblazeTwister))
        {
            Arena.Actor(twister, ArenaColor.Enemy, true);
        }

        foreach ((int i, var player) in Raid.WithSlot())
        {
            bool tethered = _tetherTargets[i];
            if (tethered)
                Arena.AddLine(Module.PrimaryActor.Position, player.Position, player.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger);
            bool active = tethered || _bossTargets[i] || _twisterTargets.Contains(player);
            bool failing = (_hitByMultipleAOEs | _closeToTetherTarget)[i];
            Arena.Actor(player, active ? ArenaColor.Danger : (failing ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric));
        }
    }

    private IEnumerable<(int, Actor)> FindPlayersInWinds(Actor origin, Actor target)
    {
        return Raid.WithSlot().InShape(_windsAOE, origin.Position, Angle.FromDirection(target.Position - origin.Position));
    }
}
