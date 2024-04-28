namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class Thundercall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _orbs = [];
    private Actor? _safeOrb;
    private Actor? _miniTarget;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeSmall = new(8);
    private static readonly AOEShapeCircle _shapeLarge = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_orbs, ArenaColor.Object, true);
        if (_miniTarget != null)
            Arena.AddCircle(_miniTarget.Position, 3, ArenaColor.Danger);
        if (_safeOrb != null)
            Arena.AddCircle(_safeOrb.Position, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NHumbleHammerAOE or AID.SHumbleHammerAOE)
        {
            _orbs.AddRange(Module.Enemies(OID.NBallOfLevin));
            _orbs.AddRange(Module.Enemies(OID.SBallOfLevin));
            WDir center = new();
            foreach (var o in _orbs)
                center += o.Position - Module.Center;
            _safeOrb = _orbs.Farthest(Module.Center + center);
            _miniTarget = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NHumbleHammerAOE:
            case AID.SHumbleHammerAOE:
                _miniTarget = null;
                _safeOrb = null;
                foreach (var o in _orbs)
                    _aoes.Add(new(spell.Targets.Any(t => t.ID == o.InstanceID) ? _shapeSmall : _shapeLarge, o.Position, default, WorldState.FutureTime(4.2f)));
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

class Flintlock(BossModule module) : Components.GenericWildCharge(module, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NHumbleHammerAOE:
            case AID.SHumbleHammerAOE:
                Source = Module.PrimaryActor;
                foreach (var (slot, player) in Raid.WithSlot(true))
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
