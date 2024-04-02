namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class Thundercall : Components.GenericAOEs
{
    private List<Actor> _orbs = new();
    private Actor? _safeOrb;
    private Actor? _miniTarget;
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeCircle _shapeSmall = new(8);
    private static readonly AOEShapeCircle _shapeLarge = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.Actors(_orbs, ArenaColor.Object, true);
        if (_miniTarget != null)
            arena.AddCircle(_miniTarget.Position, 3, ArenaColor.Danger);
        if (_safeOrb != null)
            arena.AddCircle(_safeOrb.Position, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NHumbleHammerAOE or AID.SHumbleHammerAOE)
        {
            _orbs.AddRange(module.Enemies(OID.NBallOfLevin));
            _orbs.AddRange(module.Enemies(OID.SBallOfLevin));
            WDir center = new();
            foreach (var o in _orbs)
                center += o.Position - module.Bounds.Center;
            _safeOrb = _orbs.Farthest(module.Bounds.Center + center);
            _miniTarget = module.WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NHumbleHammerAOE:
            case AID.SHumbleHammerAOE:
                _miniTarget = null;
                _safeOrb = null;
                foreach (var o in _orbs)
                    _aoes.Add(new(spell.Targets.Any(t => t.ID == o.InstanceID) ? _shapeSmall : _shapeLarge, o.Position, default, module.WorldState.CurrentTime.AddSeconds(4.2f)));
                break;
            case AID.NShockSmall:
            case AID.NShockLarge:
            case AID.SShockSmall:
            case AID.SShockLarge:
                _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
                ++NumCasts;
                break;
        }
    }
}

class Flintlock : Components.GenericWildCharge
{
    public Flintlock() : base(4) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NHumbleHammerAOE:
            case AID.SHumbleHammerAOE:
                Source = module.PrimaryActor;
                foreach (var (slot, player) in module.Raid.WithSlot(true))
                    PlayerRoles[slot] = spell.MainTargetID == player.InstanceID ? PlayerRole.Target : player.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst; // TODO: or should it be 'avoid'?
                break;
            case AID.NFlintlockAOE:
            case AID.SFlintlockAOE:
                ++NumCasts;
                Source = null;
                break;
        }
    }
}
