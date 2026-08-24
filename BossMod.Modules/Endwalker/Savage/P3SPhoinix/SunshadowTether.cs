namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to sunshadow tethers during fountain of fire mechanics
class SunshadowTether(BossModule module) : BossComponent(module)
{
    private readonly HashSet<ulong> _chargedSunshadows = [];
    private BitMask _playersInAOE;

    private const float _chargeHalfWidth = 3;

    public int NumCharges => _chargedSunshadows.Count;

    public override void Update()
    {
        _playersInAOE.Reset();
        foreach (var bird in ActiveBirds())
        {
            ulong targetID = BirdTarget(bird);
            var target = targetID != 0 ? WorldState.Actors.Find(targetID) : null;
            if (target != null && target.Position != bird.Position)
            {
                var dir = (target.Position - bird.Position).Normalized();
                foreach ((int i, var player) in Raid.WithSlot().Exclude(target))
                {
                    if (player.Position.InRect(bird.Position, dir, 50, 0, _chargeHalfWidth))
                    {
                        _playersInAOE.Set(i);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var bird in ActiveBirds())
        {
            ulong birdTarget = BirdTarget(bird);
            if (birdTarget == actor.InstanceID && bird.Tether.ID != (uint)TetherID.LargeBirdFar)
            {
                hints.Add("Too close!");
            }
        }

        if (_playersInAOE[slot])
        {
            hints.Add("GTFO from charge zone!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bird in ActiveBirds())
        {
            ulong targetID = BirdTarget(bird);
            var target = (targetID != 0 && targetID != pc.InstanceID) ? WorldState.Actors.Find(targetID) : null;
            if (target != null && target.Position != bird.Position)
            {
                var dir = (target.Position - bird.Position).Normalized();
                Arena.ZoneRect(bird.Position, dir, 50, 0, _chargeHalfWidth, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!ActiveBirds().Any())
            return;

        // draw all players
        foreach ((int i, var player) in Raid.WithSlot())
            Arena.Actor(player, _playersInAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

        // draw my tether
        var myBird = Module.Enemies(OID.Sunshadow).FirstOrDefault(bird => BirdTarget(bird) == pc.InstanceID);
        if (myBird != null && !_chargedSunshadows.Contains(myBird.InstanceID))
        {
            Arena.AddLine(myBird.Position, pc.Position, myBird.Tether.ID != (uint)TetherID.LargeBirdFar ? ArenaColor.Danger : ArenaColor.Safe);
            Arena.Actor(myBird, ArenaColor.Enemy);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Fireglide)
            _chargedSunshadows.Add(caster.InstanceID);
    }

    private ulong BirdTarget(Actor bird)
    {
        // we don't get tether messages when birds spawn, so use target as a fallback
        // TODO: investigate this... we do get actor-control 503 before spawn, maybe this is related somehow...
        return bird.Tether.Target != 0 ? bird.Tether.Target : bird.TargetID;
    }

    private IEnumerable<Actor> ActiveBirds()
    {
        return Module.Enemies(OID.Sunshadow).Where(bird => !_chargedSunshadows.Contains(bird.InstanceID));
    }
}
