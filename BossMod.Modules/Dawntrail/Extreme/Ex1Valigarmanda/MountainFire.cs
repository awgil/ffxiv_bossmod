namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class MountainFire(BossModule module) : Components.GenericTowers(module, AID.MountainFireTower)
{
    private BitMask _nonTanks = module.Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
    private BitMask _lastSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(caster.Position, 3, forbiddenSoakers: _nonTanks | _lastSoakers));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _lastSoakers.Reset();
            foreach (var t in spell.Targets)
                _lastSoakers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class MountainFireCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly MountainFire? _tower = module.FindComponent<MountainFire>();
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(40, 165.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // show aoe only if not (or not allowed to) soak the tower
        if (_aoe != null && ((_tower?.Towers.Any(t => t.ForbiddenSoakers[slot]) ?? false) || !actor.Position.InCircle(_aoe.Value.Origin, 3)))
            yield return _aoe.Value;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MountainFireTower)
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MountainFireConeAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
