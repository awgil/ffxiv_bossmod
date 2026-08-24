namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to large bird tethers
// TODO: simplify and make more robust, e.g. in case something goes wrong and bird dies without tether update
class BirdTether(BossModule module) : BossComponent(module)
{
    public int NumFinishedChains { get; private set; }
    private readonly (Actor?, Actor?, int)[] _chains = new (Actor?, Actor?, int)[4]; // actor1, actor2, num-charges
    private BitMask _playersInAOE;

    private const float _chargeHalfWidth = 3;
    private const float _chargeMinSafeDistance = 30;

    public override void Update()
    {
        _playersInAOE.Reset();
        var birdsLarge = Module.Enemies(OID.SunbirdLarge);
        for (int i = 0; i < Math.Min(birdsLarge.Count, _chains.Length); ++i)
        {
            if (_chains[i].Item3 == 2)
                continue; // this is finished

            var bird = birdsLarge[i];
            if (_chains[i].Item1 == null && bird.Tether.Target != 0)
            {
                _chains[i].Item1 = WorldState.Actors.Find(bird.Tether.Target); // first target found
            }
            if (_chains[i].Item2 == null && (_chains[i].Item1?.Tether.Target ?? 0) != 0)
            {
                _chains[i].Item2 = WorldState.Actors.Find(_chains[i].Item1!.Tether.Target); // second target found
            }
            if (_chains[i].Item3 == 0 && _chains[i].Item1 != null && bird.Tether.Target == 0)
            {
                _chains[i].Item3 = 1; // first charge (bird is no longer tethered to anyone)
            }
            if (_chains[i].Item3 == 1 && (_chains[i].Item1?.Tether.Target ?? 0) == 0)
            {
                _chains[i].Item3 = 2;
                ++NumFinishedChains;
                continue;
            }

            // find players hit by next bird charge
            var nextTarget = _chains[i].Item3 > 0 ? _chains[i].Item2 : _chains[i].Item1;
            if (nextTarget != null && nextTarget.Position != bird.Position)
            {
                var fromTo = nextTarget.Position - bird.Position;
                float len = fromTo.Length();
                fromTo /= len;
                foreach ((int j, var player) in Raid.WithSlot().Exclude(nextTarget))
                {
                    if (player.Position.InRect(bird.Position, fromTo, len, 0, _chargeHalfWidth))
                    {
                        _playersInAOE.Set(j);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var birdsLarge = Module.Enemies(OID.SunbirdLarge);
        foreach ((var bird, (var p1, var p2, int numCharges)) in birdsLarge.Zip(_chains))
        {
            if (numCharges == 2)
                continue;

            var nextTarget = numCharges > 0 ? p2 : p1;
            if (actor == nextTarget)
            {
                // check that tether is 'safe'
                var tetherSource = numCharges > 0 ? p1 : bird;
                if (tetherSource?.Tether.ID != (uint)TetherID.LargeBirdFar)
                {
                    hints.Add("Too close!");
                }
            }
        }

        if (_playersInAOE[slot])
        {
            hints.Add("GTFO from charge zone!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // draw aoe zones for imminent charges, except one towards player
        var birdsLarge = Module.Enemies(OID.SunbirdLarge);
        foreach ((var bird, (var p1, var p2, int numCharges)) in birdsLarge.Zip(_chains))
        {
            if (numCharges == 2)
                continue;

            var nextTarget = numCharges > 0 ? p2 : p1;
            if (nextTarget != null && nextTarget != pc && nextTarget.Position != bird.Position)
            {
                var fromTo = nextTarget.Position - bird.Position;
                float len = fromTo.Length();
                Arena.ZoneRect(bird.Position, fromTo / len, len, 0, _chargeHalfWidth, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw all birds and all players
        var birdsLarge = Module.Enemies(OID.SunbirdLarge);
        foreach (var bird in birdsLarge)
            Arena.Actor(bird, ArenaColor.Enemy);
        foreach ((int i, var player) in Raid.WithSlot())
            Arena.Actor(player, _playersInAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

        // draw chains containing player
        foreach ((var bird, (var p1, var p2, int numCharges)) in birdsLarge.Zip(_chains))
        {
            if (numCharges == 2)
                continue;

            if (p1 == pc)
            {
                // bird -> pc -> other
                if (numCharges == 0)
                {
                    Arena.AddLine(bird.Position, pc.Position, (bird.Tether.ID == (uint)TetherID.LargeBirdFar) ? ArenaColor.Safe : ArenaColor.Danger);
                    if (p2 != null)
                    {
                        Arena.AddLine(pc.Position, p2.Position, (pc.Tether.ID == (uint)TetherID.LargeBirdFar) ? ArenaColor.Safe : ArenaColor.Danger);
                    }

                    if (bird.Position != Module.Center)
                    {
                        var safespot = bird.Position + (Module.Center - bird.Position).Normalized() * _chargeMinSafeDistance;
                        Arena.AddCircle(safespot, 1, ArenaColor.Safe);
                    }
                }
                // else: don't care, charge to pc already happened
            }
            else if (p2 == pc && p1 != null)
            {
                // bird -> other -> pc
                if (numCharges == 0)
                {
                    Arena.AddLine(bird.Position, p1.Position, (bird.Tether.ID == (uint)TetherID.LargeBirdFar) ? ArenaColor.Safe : ArenaColor.Danger);
                    Arena.AddLine(p1.Position, pc.Position, (p1.Tether.ID == (uint)TetherID.LargeBirdFar) ? ArenaColor.Safe : ArenaColor.Danger);

                    Arena.AddCircle(bird.Position, 1, ArenaColor.Safe); // draw safespot near bird
                }
                else
                {
                    Arena.AddLine(bird.Position, pc.Position, (p1.Tether.ID == (uint)TetherID.LargeBirdFar) ? ArenaColor.Safe : ArenaColor.Danger);

                    if (bird.Position != Module.Center)
                    {
                        var safespot = bird.Position + (Module.Center - bird.Position).Normalized() * _chargeMinSafeDistance;
                        Arena.AddCircle(safespot, 1, ArenaColor.Safe);
                    }
                }
            }
        }
    }
}
