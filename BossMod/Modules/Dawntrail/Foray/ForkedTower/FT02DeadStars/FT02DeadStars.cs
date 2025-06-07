namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DeathWall(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active && _activation != default)
            yield return new(new AOEShapeDonut(30, 40), Arena.Center, Activation: _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_DecisiveBattle)
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

class SliceNDice(BossModule module) : Components.BaitAwayCast(module, AID._Ability_SliceNDice, new AOEShapeCone(70, 45.Degrees()));

class NoisomeNuisance(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_NoisomeNuisance, AID._Ability_IceboundBuffoon], new AOEShapeCircle(6));

class VengefulCone(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_VengefulBlizzardIII, AID._Spell_VengefulFireIII, AID._Spell_VengefulBioIII], new AOEShapeCone(60, 60.Degrees()));

class VengefulConeHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Source, DateTime Activation)> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(new AOEShapeCone(60, 60.Degrees()), s.Source, Angle.FromDirection(Arena.Center - s.Source), s.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_)
        {
            NumCasts++;
            _sources.Add((spell.TargetXZ, WorldState.FutureTime(9.9f)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_VengefulBioIII or AID._Spell_VengefulFireIII or AID._Spell_VengefulBlizzardIII)
            _sources.Clear();
    }
}

class Firestrike(BossModule module) : Components.MultiLineStack(module, 5, 70, AID._Ability_Firestrike, AID._Ability_Firestrike2, 5.2f);
class Firestrike2(BossModule module) : Components.MultiLineStack(module, 5, 70, AID._Ability_Firestrike3, AID._Ability_Firestrike2, 5.2f)
{
    private BitMask _forbiddenTargets;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID._Ability_Firestrike3)
            for (var i = 0; i < Stacks.Count; i++)
                Stacks.Ref(i).ForbiddenPlayers |= _forbiddenTargets;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID._Ability_SliceNDice && Raid.TryFindSlot(spell.TargetID, out var slot))
        {
            _forbiddenTargets.Set(slot);
            for (var i = 0; i < Stacks.Count; i++)
                Stacks.Ref(i).ForbiddenPlayers.Set(slot);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13738)]
public class FT02DeadStars(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, 360), new ArenaBoundsCircle(35))
{
    private Actor? _triton;
    private Actor? _nereid;
    private Actor? _phobos;

    public Actor? Triton() => _triton;
    public Actor? Nereid() => _nereid;
    public Actor? Phobos() => _phobos;

    protected override bool CheckPull() => PrimaryActor.InCombat;

    public override bool DrawAllPlayers => true;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_triton, ArenaColor.Enemy);
        Arena.Actor(_nereid, ArenaColor.Enemy);
        Arena.Actor(_phobos, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GaseousNereid), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GaseousPhobos), ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        _triton ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Triton).FirstOrDefault() : null;
        _nereid ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Nereid).FirstOrDefault() : null;
        _phobos ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Phobos).FirstOrDefault() : null;
    }
}

