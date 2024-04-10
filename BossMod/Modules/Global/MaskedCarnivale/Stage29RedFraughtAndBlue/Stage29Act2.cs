namespace BossMod.Global.MaskedCarnivale.Stage29.Act2;

public enum OID : uint
{
    Boss = 0x2C5D, //R=3.0
    FireTornado = 0x2C60, // R=4.0
    LeftHand = 0x2C5F, // R=1.3
    RightHand = 0x2C5E, // R=1.8
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // 2C5D->player, no cast, single-target
    AutoAttack2 = 18980, // 2C5F->player, no cast, single-target
    AutoAttack3 = 18976, // 2C5E->player, no cast, single-target
    Unknown = 18975, // 2C5D->self, no cast, single-target
    ProteanWave = 18971, // 2C5D->self, 3,0s cast, range 39 30-degree cone, knockback 40, away from source (only first wave)
    ProteanWave2 = 18972, // 2C5D->self, no cast, range 39 30-degree cone, hits player position, but can be dodged. angle snapshot sometime between cast event of wave 1 and wave 2, not sure if its possible to predict accurately
    ProteanWave3 = 18984, // 2C60->self, 3,0s cast, range 50 30-degree cone
    Throttle = 18964, // 2C5D->player, no cast, range 5 circle
    FerrofluidKB = 18963, // 2C5D->self, 5,0s cast, range 80 circle, knockback 6, away from source
    FluidConvection = 18974, // 2C5D->self, no cast, range 12-40 donut
    FerrofluidAttract = 18962, // Boss->self, 5,0s cast, range 80 circle, pull 6, between centers
    FluidDynamic = 18973, // Boss->self, no cast, range 6 circle
    FluidBallVisual = 18968, // 2C5D->self, 3,0s cast, single-target
    FluidBall = 18969, // 233C->location, 3,0s cast, range 5 circle
    WateryGrasp = 19028, // 2C5D->self, 5,0s cast, single-target, calls left/right hand, happens at 70% max hp
    FluidStrike = 18981, // 2C5F->self, no cast, range 12 90-degree cone
    FluidStrike2 = 18977, // 2C5E->self, no cast, range 12 90-degree cone
    BigSplashFirst = 18965, // 2C5D->self, 8,0s cast, range 80 circle, knockback 25, away from source, use diamondback to survive
    BigSplashRepeat = 18966, // 2C5D->self, no cast, range 80 circle, knockback 25, away from source
    Cascade = 19022, // 2C5D->self, 4,0s cast, range 80 circle, raidwide, summons water tornados
    Unwind = 18985, // 2C60->self, 5,0s cast, range 80 circle, damage fall of AOE, about 10 distance is fine
    FluidSwing = 18961, // 2C5D->player, 4,0s cast, range 11 30-degree cone, interruptible, knockback 50, source forward
    Palmistry = 18982, // 2C5F->player, 3,0s cast, single-target, drains 5000 MP
    WashAwayFirst = 18978, // 2C5E->self, 5,0s cast, range 80 circle, multiple raidwides, add should be killed before cast finishes
    WashAwayRest = 18979, // 2C5E->self, no cast, range 80 circle
}

public enum SID : uint
{
    Throttle = 700, // Boss->player, extra=0x0, player dies when debuff runs out if not cleansed before
    Stun = 149, // Boss->player, extra=0x0
}

class BigSplash() : Components.RaidwideCast(ActionID.MakeSpell(AID.BigSplashFirst), "Diamondback! (Multiple raidwides + knockbacks)");
class BigSplashKB() : Components.KnockbackFromCastTarget(ActionID.MakeSpell(AID.BigSplashFirst), 25);
class Cascade() : Components.RaidwideCast(ActionID.MakeSpell(AID.Cascade), "Raidwide + Tornados spawn");
class WateryGrasp() : Components.CastHint(ActionID.MakeSpell(AID.WateryGrasp), "Spawns hands. Focus left hand first.");
class Throttle() : Components.CastHint(ActionID.MakeSpell(AID.Throttle), "Prepare to use Excuviation to remove debuff");
class FluidSwing() : Components.CastInterruptHint(ActionID.MakeSpell(AID.FluidSwing));
class FluidSwingKnockback() : Components.KnockbackFromCastTarget(ActionID.MakeSpell(AID.FluidSwing), 50, kind: Kind.DirForward);
class ProteanWave() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.ProteanWave), new AOEShapeCone(39, 15.Degrees()));
class ProteanWave3() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.ProteanWave3), new AOEShapeCone(39, 15.Degrees()));

class KnockbackPull : Components.Knockback
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FerrofluidKB)
            _knockback = new(module.PrimaryActor.Position, 6, spell.NPCFinishAt);
        if ((AID)spell.Action.ID == AID.FerrofluidAttract)
            _knockback = new(module.PrimaryActor.Position, 6, spell.NPCFinishAt, Kind: Kind.TowardsOrigin);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FerrofluidKB or AID.FerrofluidAttract)
            _knockback = null;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<FluidConvectionDynamic>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}

class Unwind() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Unwind), new AOEShapeCircle(10));
class FluidBall() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.FluidBall), 5);

class FluidConvectionDynamic : Components.GenericAOEs
{
    private static readonly AOEShapeDonut donut = new(10, 40);
    private static readonly AOEShapeCircle circle = new(6);
    private DateTime _activation;
    private AOEShape? shape;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (shape != default)
            yield return new(shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        // boss can move after cast started, so we can't use aoe instance, since that would cause outdated position data to be used
        if ((AID)spell.Action.ID == AID.FerrofluidKB)
        {
            shape = donut;
            _activation = spell.NPCFinishAt;
        }
        if ((AID)spell.Action.ID == AID.FerrofluidAttract)
        {
            shape = circle;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FluidConvection or AID.FluidDynamic)
            shape = null;
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var lefthand = module.Enemies(OID.LeftHand).Where(x => !x.IsDead).FirstOrDefault();
        var righthand = module.Enemies(OID.RightHand).Where(x => !x.IsDead).FirstOrDefault();
        if (lefthand != null)
            hints.Add($"{lefthand.Name} will drain all your MP, kill it fast!");
        if (lefthand == null && righthand != null)
            hints.Add($"{righthand.Name} will do multiple raidwides, kill it fast!");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var doomed = actor.FindStatus(SID.Throttle); //it is called throttle, but works exactly like any cleansable doom
        if (doomed != null)
            hints.Add("You were doomed! Cleanse it with Exuviation.");
    }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"{module.PrimaryActor.Name} will cast Throttle on you which needs to be\ncleansed with Excuviation. It will also spawn two hands which need to be\nkilled asap. Focus the left hand first because it will drain all your MP.");
    }
}

class Stage29Act2States : StateMachineBuilder
{
    public Stage29Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<FluidSwingKnockback>()
            .ActivateOnEnter<BigSplash>()
            .ActivateOnEnter<BigSplashKB>()
            .ActivateOnEnter<Cascade>()
            .ActivateOnEnter<WateryGrasp>()
            .ActivateOnEnter<Throttle>()
            .ActivateOnEnter<ProteanWave>()
            .ActivateOnEnter<ProteanWave3>()
            .ActivateOnEnter<KnockbackPull>()
            .ActivateOnEnter<FluidBall>()
            .ActivateOnEnter<Unwind>()
            .ActivateOnEnter<FluidConvectionDynamic>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 698, NameID = 9241)]
public class Stage29Act2 : BossModule
{
    public Stage29Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.LeftHand))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.RightHand))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}