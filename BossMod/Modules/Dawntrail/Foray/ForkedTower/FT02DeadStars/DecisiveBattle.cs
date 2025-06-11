namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DecisiveBattleAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();
    private readonly Actor?[] _casters = new Actor?[3];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var showAll = _config.PlayerAlliance == ForkedTowerConfig.Alliance.None;

        foreach (var c in ActiveCasters())
            yield return new AOEInstance(new AOEShapeCircle(35), c.CastInfo!.LocXZ, Activation: Module.CastFinishAt(c.CastInfo), Risky: !showAll);
    }

    public IEnumerable<Actor> ActiveCasters()
    {
        var active = new BitMask(0b111);
        switch (_config.PlayerAlliance.Group3())
        {
            case 1:
                active.Clear(0);
                break;
            case 2:
                active.Clear(1);
                break;
            case 3:
                active.Clear(2);
                break;
        }
        foreach (var bit in active.SetBits())
            if (_casters[bit] is { } c)
                yield return c;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DecisiveBattle3 or AID.DecisiveBattle2 or AID.DecisiveBattle1)
        {
            var ix = caster.Position.Z < Arena.Center.Z ? 1 : caster.Position.X < Arena.Center.X ? 0 : 2;
            _casters[ix] = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DecisiveBattle3 or AID.DecisiveBattle2 or AID.DecisiveBattle1)
        {
            var ix = Array.FindIndex(_casters, c => c == caster);
            if (ix >= 0)
                _casters[ix] = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_config.PlayerAlliance == ForkedTowerConfig.Alliance.None)
        {
            if (_casters.Count(c => c != null && actor.Position.InCircle(c.Position, 35)) > 1)
                hints.Add("Hit by too many AOEs!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_config.PlayerAlliance == ForkedTowerConfig.Alliance.None && _casters.Any(c => c != null))
        {
            var active = _casters.Where(c => c != null).Select(c => c!).ToList();
            hints.AddForbiddenZone(p => active.Count(a => p.InCircle(a.Position, 35)) != 1, Module.CastFinishAt(active[0].CastInfo));
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class DecisiveBattle : Components.GenericInvincible
{
    private readonly int[] _playerAssignments = Utils.MakeArray(PartyState.MaxPartySize, -1);

    private Actor? _triton;
    private Actor? _nereid;
    private Actor? _phobos;

    public DecisiveBattle(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public bool Active => _playerAssignments.Any(p => p >= 0);

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.Triton:
                _triton = actor;
                break;
            case OID.Nereid:
                _nereid = actor;
                break;
            case OID.Phobos:
                _phobos = actor;
                break;
        }
    }

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var assignment = _playerAssignments.BoundSafeAt(slot, -1);
        if (assignment >= 0)
        {
            if (assignment != 0 && _triton != null)
                yield return _triton;
            if (assignment != 1 && _nereid != null)
                yield return _nereid;
            if (assignment != 2 && _phobos != null)
                yield return _phobos;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var ix = (SID)status.ID switch
        {
            SID.TritonicGravity => 0,
            SID.NereidicGravity => 1,
            SID.PhobosicGravity => 2,
            _ => -1
        };
        if (ix >= 0 && Raid.TryFindSlot(actor, out var slot))
            _playerAssignments[slot] = ix;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.TritonicGravity or SID.NereidicGravity or SID.PhobosicGravity && Raid.TryFindSlot(actor, out var slot))
            _playerAssignments[slot] = -1;
    }
}
