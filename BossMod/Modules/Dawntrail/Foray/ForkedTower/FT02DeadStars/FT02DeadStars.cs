namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DecisiveBattleAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();
    private readonly Actor?[] _casters = new Actor?[3];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in ActiveCasters())
            yield return new AOEInstance(new AOEShapeCircle(35), c.CastInfo!.LocXZ, Activation: Module.CastFinishAt(c.CastInfo));
    }

    public IEnumerable<Actor> ActiveCasters()
    {
        var active = new BitMask(0b111);
        switch (_config.PlayerAlliance)
        {
            case ForkedTowerConfig.Alliance.A:
            case ForkedTowerConfig.Alliance.D1:
                active.Clear(0);
                break;
            case ForkedTowerConfig.Alliance.B:
            case ForkedTowerConfig.Alliance.E2:
                active.Clear(1);
                break;
            case ForkedTowerConfig.Alliance.C:
            case ForkedTowerConfig.Alliance.F3:
                active.Clear(2);
                break;
        }
        foreach (var bit in active.SetBits())
            if (_casters[bit] is { } c)
                yield return c;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_DecisiveBattle or AID._Ability_DecisiveBattle1 or AID._Ability_DecisiveBattle2)
        {
            var ix = caster.Position.Z < Arena.Center.Z ? 1 : caster.Position.X < Arena.Center.X ? 0 : 2;
            _casters[ix] = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_DecisiveBattle or AID._Ability_DecisiveBattle1 or AID._Ability_DecisiveBattle2)
        {
            var ix = Array.FindIndex(_casters, c => c == caster);
            if (ix >= 0)
                _casters[ix] = null;
        }
    }
}

class DecisiveBattle(BossModule module) : Components.GenericInvincible(module)
{
    private readonly int[] _playerAssignments = Utils.MakeArray(PartyState.MaxPartySize, -1);

    private Actor? _triton;
    private Actor? _nereid;
    private Actor? _phobos;

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
            SID._Gen_TritonicGravity => 0,
            SID._Gen_NereidicGravity => 1,
            SID._Gen_PhobosicGravity => 2,
            _ => -1
        };
        if (ix >= 0 && Raid.TryFindSlot(actor, out var slot))
            _playerAssignments[slot] = ix;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_TritonicGravity or SID._Gen_NereidicGravity or SID._Gen_PhobosicGravity && Raid.TryFindSlot(actor, out var slot))
            _playerAssignments[slot] = -1;
    }
}

class NoisomeNuisance(BossModule module) : Components.StandardAOEs(module, AID._Ability_NoisomeNuisance, 6);

class Ooze(BossModule module) : Components.GenericAOEs(module)
{
    private readonly int[] _states = new int[PartyState.MaxPartySize];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var ele = Math.Sign(_states.BoundSafeAt(slot, 0));

        foreach (var aoe in _predicted.Take(2))
        {
            var safe = ele == -aoe.Element;
            yield return new AOEInstance(new AOEShapeCircle(22), aoe.Center, default, aoe.Predicted, safe ? ArenaColor.SafeFromAOE : ArenaColor.AOE, Risky: !safe);
        }
    }

    record struct Cast(WPos Center, int Element, DateTime Predicted);

    private readonly List<Cast> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var resolve = WorldState.FutureTime(9.1f + _predicted.Count / 2 * 3);
        switch ((AID)spell.Action.ID)
        {
            case AID.FrozenFalloutFireCast:
                _predicted.Add(new(caster.Position, 1, resolve));
                break;
            case AID.FrozenFalloutIceCast:
                _predicted.Add(new(caster.Position, -1, resolve));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_)
        {
            if (_predicted[0].Center.AlmostEqual(caster.Position, 1))
                _predicted.RemoveAt(0);
            else if (_predicted[1].Center.AlmostEqual(caster.Position, 1))
                _predicted.RemoveAt(1);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_NovaOoze or SID._Gen_IceOoze && Raid.TryFindSlot(actor, out var slot))
        {
            var extra = (int)status.Extra;
            if ((SID)status.ID == SID._Gen_IceOoze)
                extra = -extra;
            _states[slot] = extra;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_NovaOoze or SID._Gen_IceOoze && Raid.TryFindSlot(actor, out var slot))
            _states[slot] = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var st = _states.BoundSafeAt(slot);
        if (st != 0)
        {
            var stacks = Math.Abs(st);
            var ele = st < 0 ? "Ice" : "Fire";
            hints.Add($"{ele} {stacks}", false);
        }
    }
}

class Vengeful(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_VengefulBlizzardIII, AID._Spell_VengefulFireIII, AID._Spell_VengefulBioIII], new AOEShapeCone(60, 60.Degrees()));

class FT02DeadStarsStates : StateMachineBuilder
{
    public FT02DeadStarsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DecisiveBattleAOE>()
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<NoisomeNuisance>()
            .ActivateOnEnter<Ooze>()
            .ActivateOnEnter<Vengeful>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13738)]
public class FT02DeadStars(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, 360), new ArenaBoundsCircle(30))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;

    public override bool DrawAllPlayers => true;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Triton), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Nereid), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Phobos), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GaseousNereid), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GaseousPhobos), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FrozenTriton), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FrozenPhobos), ArenaColor.Enemy);
    }
}

