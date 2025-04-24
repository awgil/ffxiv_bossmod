namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D032Wrecker;

public enum OID : uint
{
    Boss = 0x33E9, // R=6.0
    QueerBubble = 0x3731, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherSiphonFire = 25145, // Boss->self, 3.0s cast, single-target
    AetherSiphonWater = 25146, // Boss->self, 3.0s cast, single-target
    AetherSprayFire = 25147, // Boss->location, 7.0s cast, range 30, raidwide, be in bubble
    AetherSprayWater = 25148, // Boss->location, 7.0s cast, range 30, knockback 13 away from source
    MeaninglessDestruction = 25153, // Boss->self, 5.0s cast, range 100 circle
    PoisonHeartVisual = 25151, // Boss->self, 5.0s cast, single-target
    PoisonHeartStack = 27851, // Helper->players, 5.0s cast, range 6 circle
    TotalWreck = 25154, // Boss->player, 5.0s cast, single-target
    UnholyWater = 27852, // Boss->self, 3.0s cast, single-target, spawns bubbles
    Withdraw = 27847 // 3731->player, 1.0s cast, single-target, pull 30 between centers
}

class QueerBubble(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> _aoes = [];
    private static readonly AOEShapeCircle circle = new(2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            foreach (var a in _aoes.Where(x => !x.IsDead))
                yield return new(circle, a.Position, default, default, Module.FindComponent<AetherSprayFire>()!.Active ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.QueerBubble)
            _aoes.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.QueerBubble && _aoes.Count > 0)
            _aoes.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.Withdraw)
            _aoes.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<AetherSprayFire>()!.Active)
        {
            var forbiddenInverted = new List<Func<WPos, bool>>();
            foreach (var a in _aoes)
                forbiddenInverted.Add(ShapeContains.InvertedCircle(a.Position, 2.5f));
            var activation = Module.CastFinishAt(Module.FindComponent<AetherSprayFire>()!.Casters[0].CastInfo);
            if (forbiddenInverted.Count > 0)
                hints.AddForbiddenZone(p => forbiddenInverted.All(f => f(p)), activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class MeaninglessDestruction(BossModule module) : Components.RaidwideCast(module, AID.MeaninglessDestruction);
class PoisonHeartStack(BossModule module) : Components.StackWithCastTargets(module, AID.PoisonHeartStack, 6, 4, 4);
class TotalWreck(BossModule module) : Components.SingleTargetCast(module, AID.TotalWreck);
class AetherSprayWater(BossModule module) : Components.RaidwideCast(module, AID.AetherSprayWater);
class AetherSprayFire(BossModule module) : Components.RaidwideCast(module, AID.AetherSprayFire, "Go into a bubble! (Raidwide)");
class AetherSprayWaterKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AetherSprayWater, 13)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<QueerBubble>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
    private static readonly Dictionary<object, object> cache = [];
    public static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster)
    {
        if (cache.TryGetValue((rotatebydegrees, origin, caster), out var cachedResult))
            return (WPos)cachedResult;
        var x = MathF.Cos(rotatebydegrees * Angle.DegToRad) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * Angle.DegToRad) * (caster.Z - origin.Z);
        var z = MathF.Sin(rotatebydegrees * Angle.DegToRad) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * Angle.DegToRad) * (caster.Z - origin.Z);
        var result = new WPos(origin.X + x, origin.Z + z);
        cache[(rotatebydegrees, origin, caster)] = result;
        return result;
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, bool>>();
        var source = Sources(slot, actor).FirstOrDefault();
        if (Module.FindComponent<QueerBubble>()!.ActiveAOEs(slot, actor).Any() && source != default)
        {
            forbidden.Add(ShapeContains.InvertedCircle(Arena.Center, 7));
            for (var i = 0; i < 6; i++)
                if (Module.Enemies(OID.QueerBubble).Where(x => x.Position.AlmostEqual(RotateAroundOrigin(i * 60, Arena.Center, x.Position), 1) && Module.FindComponent<QueerBubble>()!._aoes.Contains(x)) != null)
                    forbidden.Add(ShapeContains.Cone(Arena.Center, 20, i * 60.Degrees(), 10.Degrees()));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Any(f => f(p)), source.Activation);
        }
    }
}

class D032WreckerStates : StateMachineBuilder
{
    public D032WreckerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<QueerBubble>()
            .ActivateOnEnter<AetherSprayWater>()
            .ActivateOnEnter<AetherSprayWaterKB>()
            .ActivateOnEnter<AetherSprayFire>()
            .ActivateOnEnter<TotalWreck>()
            .ActivateOnEnter<PoisonHeartStack>()
            .ActivateOnEnter<MeaninglessDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus, LTS), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10718)]
public class D032Wrecker(WorldState ws, Actor primary) : BossModule(ws, primary, new(-295, -354), new ArenaBoundsCircle(20));
