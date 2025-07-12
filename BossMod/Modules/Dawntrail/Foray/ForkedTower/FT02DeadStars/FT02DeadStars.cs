namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DeathWall : Components.GenericAOEs
{
    private DateTime _activation;

    public DeathWall(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active && _activation != default)
            yield return new(new AOEShapeDonut(30, 40), Arena.Center, Activation: _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DecisiveBattle3)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EBDAC)
        {
            Active = true;
            Arena.Bounds = new ArenaBoundsCircle(30);
        }
    }
}

class SliceNDice(BossModule module) : Components.BaitAwayCast(module, AID.SliceNDice, new AOEShapeCone(70, 45.Degrees()));

class NoisomeNuisance(BossModule module) : Components.GroupedAOEs(module, [AID.NoisomeNuisance, AID.IceboundBuffoon, AID.BlazingBelligerent], new AOEShapeCircle(6));

class VengefulCone(BossModule module) : Components.GroupedAOEs(module, [AID.VengefulBlizzardIII, AID.VengefulFireIII, AID.VengefulBioIII], new AOEShapeCone(60, 60.Degrees()));

class VengefulConeHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Source, DateTime Activation)> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(new AOEShapeCone(60, 60.Degrees()), s.Source, Angle.FromDirection(Arena.Center - s.Source), s.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BossJump)
        {
            NumCasts++;
            _sources.Add((spell.TargetXZ, WorldState.FutureTime(9.9f)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.VengefulBioIII or AID.VengefulFireIII or AID.VengefulBlizzardIII)
            _sources.Clear();
    }
}

class Firestrike(BossModule module) : Components.MultiLineStack(module, 5, 70, AID.FirestrikeTarget, AID.FirestrikeRect, 5.2f);
class Firestrike2(BossModule module) : Components.MultiLineStack(module, 5, 70, AID.FirestrikeTarget2, AID.FirestrikeRect, 5.2f)
{
    private BitMask _forbiddenTargets;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.FirestrikeTarget2)
            for (var i = 0; i < Stacks.Count; i++)
                Stacks.Ref(i).ForbiddenPlayers |= _forbiddenTargets;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.SliceNDice && Raid.TryFindSlot(spell.TargetID, out var slot))
        {
            _forbiddenTargets.Set(slot);
            for (var i = 0; i < Stacks.Count; i++)
                Stacks.Ref(i).ForbiddenPlayers.Set(slot);
        }
    }
}

class Multiboss(BossModule module) : Components.Adds(module, (uint)OID.DeadStars, 1);

// 5.2s after second cast, spreads go off
class CollateralJet(BossModule module) : Components.GroupedAOEs(module, [AID.CollateralGasJet, AID.CollateralColdJet, AID.CollateralHeatJet], new AOEShapeCone(40, 30.Degrees()), maxCasts: 3);

class CollateralBall : Components.GenericStackSpread
{
    public int NumCasts;

    public CollateralBall(BossModule module) : base(module, alwaysShowSpreads: true)
    {
        Spreads.AddRange(WorldState.Actors.Where(a => a.Type == ActorType.Player && !a.IsDead).Select(a => new Spread(a, 4, WorldState.FutureTime(5.2f))));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CollateralBioball or AID.CollateralFireball or AID.CollateralIceball)
        {
            NumCasts++;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}

class SixHandedFistfight(BossModule module) : Components.GenericAOEs(module, AID.SixHandedFistfightAOE)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(12), Arena.Center, Activation: _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }
}
class SixHandedRaidwide(BossModule module) : Components.RaidwideCastDelay(module, AID.SixHandedFistfightAOE, AID.SixHandedRaidwide, 0.5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13738, PrimaryActorOID = (uint)OID.Triton, PlanLevel = 100)]
public class FT02DeadStars(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, 360), new ArenaBoundsCircle(35))
{
    private Actor? _nereid;
    private Actor? _phobos;
    private Actor? _multiboss;
    private Actor? _fireNereid;
    private Actor? _firePhobos;
    private Actor? _iceTriton;
    private Actor? _icePhobos;

    public Actor? Triton() => PrimaryActor;
    public Actor? Nereid() => _nereid;
    public Actor? Phobos() => _phobos;
    public Actor? FireNereid() => _fireNereid;
    public Actor? FirePhobos() => _firePhobos;
    public Actor? IceTriton() => _iceTriton;
    public Actor? IcePhobos() => _icePhobos;
    public Actor? Multiboss() => _multiboss;
    public Actor? DeathWall { get; private set; }

    public override bool DrawAllPlayers => true;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_nereid, ArenaColor.Enemy);
        Arena.Actor(_phobos, ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        DeathWall ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.DeathWallHelper).FirstOrDefault() : null;
        _nereid ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Nereid).FirstOrDefault() : null;
        _phobos ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Phobos).FirstOrDefault() : null;
        _icePhobos ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FrozenPhobos).FirstOrDefault() : null;
        _iceTriton ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FrozenTriton).FirstOrDefault() : null;
        _fireNereid ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.GaseousNereid).FirstOrDefault() : null;
        _firePhobos ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.GaseousPhobos).FirstOrDefault() : null;
        _multiboss ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.DeadStars).FirstOrDefault() : null;
    }
}

