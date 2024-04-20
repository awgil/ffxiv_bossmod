namespace BossMod.RealmReborn.Raid.T05Twintania;

// P2 mechanics
class P2Fireball(BossModule module) : BossComponent(module)
{
    public Actor? Target { get; private set; }
    public DateTime ExplosionAt { get; private set; }
    public DateTime NextAt { get; private set; } = module.WorldState.FutureTime(7.5f);

    public const float Radius = 4;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Target == null)
            hints.Add($"Next fireball in ~{(NextAt - WorldState.CurrentTime).TotalSeconds:f1}s");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Target != null)
            Arena.AddCircle(Target.Position, Radius, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireballMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID);
                ExplosionAt = WorldState.FutureTime(4.5f); // seen 4.5 to 5 seconds delay
                NextAt = WorldState.FutureTime(25); // TODO: verify...
                break;
            case AID.FireballAOE:
                Target = null;
                break;
        }
    }
}

class P2Conflagrate(BossModule module) : BossComponent(module)
{
    public Actor? Target { get; private set; }
    public DateTime FettersAt { get; private set; }
    public DateTime NextAt { get; private set; } = module.WorldState.FutureTime(29);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Target == null)
            hints.Add($"Next conflagrate in ~{(NextAt - WorldState.CurrentTime).TotalSeconds:f1}s");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirestormMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID);
                FettersAt = WorldState.FutureTime(2.5f); // seen 2.3 to 2.8 seconds delay
                NextAt = WorldState.FutureTime(35); // TODO: verify...
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Target && (SID)status.ID == SID.Fetters)
            Target = null;
    }
}

class P2AI(BossModule module) : BossComponent(module)
{
    private readonly DeathSentence? _deathSentence = module.FindComponent<DeathSentence>();
    private readonly P2Fireball? _fireball = module.FindComponent<P2Fireball>();
    private readonly P2Conflagrate? _conflagrate = module.FindComponent<P2Conflagrate>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            switch ((OID)e.Actor.OID)
            {
                case OID.Boss:
                    e.Priority = 1;
                    if (_fireball?.Target == null && (_fireball!.NextAt - WorldState.CurrentTime).TotalSeconds > 5 && _conflagrate?.Target == null && (_conflagrate!.NextAt - WorldState.CurrentTime).TotalSeconds > 5)
                        e.DesiredPosition = new(-14, 5); // don't move boss while other mechanics are active or imminent
                    e.DesiredRotation = -90.Degrees();
                    break;
                case OID.Conflagration:
                    e.Priority = 2;
                    hints.AddForbiddenZone(ShapeDistance.Circle(e.Actor.Position, 6)); // TODO: reconsider (e.g. fireball target might want to run inside conflag)
                    break;
            }
        }

        // conflagrate target always wants to run straight into boss center
        // for fireball, we want to be stacked in two groups; currently we have OT/healers on the left and all DD on the right before fireball target is selected
        // after fireball target is selected, we have healers on left, ranged on right, ot/melee on target (right if one of them is a target)
        // TODO: verify that pets really count as targets, this is weird...
        // TODO: if conflagration is active, consider having fireball target run into conflag...
        if (_conflagrate?.Target == actor)
        {
            hints.ForbiddenZones.Clear(); // ignore things like cleave
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, 1), _conflagrate.FettersAt);
        }
        else if (_deathSentence?.TankRole != assignment)
        {
            bool stayLeft = actor.Role switch
            {
                Role.Healer => true,
                Role.Ranged => false,
                _ => _fireball?.Target?.Role == Role.Healer
            };
            // note that boss rotates when casting fireball - we don't want that to affect positioning
            var bossTarget = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
            var dir = bossTarget != null ? Angle.FromDirection(bossTarget.Position - Module.PrimaryActor.Position) : Module.PrimaryActor.Rotation;
            dir += (stayLeft ? 135 : -135).Degrees();
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position + dir.ToDirection() * 8, P2Fireball.Radius * 0.5f), _fireball?.Target != null ? _fireball.ExplosionAt : DateTime.MaxValue);
        }

        if (_fireball?.Target != null)
        {
            bool hitHealers = _fireball.Target.Role == Role.Healer;
            var hitPlayers = Raid.WithSlot().WhereActor(a => a.Role switch
            {
                Role.Healer => hitHealers,
                Role.Ranged => !hitHealers,
                _ => Module.PrimaryActor.TargetID != a.InstanceID
            }).Mask();
            hints.PredictedDamage.Add((hitPlayers, _fireball.ExplosionAt));
        }
    }
}
