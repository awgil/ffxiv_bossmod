namespace BossMod.Global.MaskedCarnivale.Stage30.Act3;

public enum OID : uint
{
    Boss = 0x2C6C, //R=2.0
    SiegfriedCloneIce = 0x2C6E, //R=2.0
    SiegfriedCloneWind = 0x2C6D, //R=2.0
    SiegfriedCloneFire = 0x2C6F, //R=2.0
    IceBoulder = 0x2CC6, // R=2.0
    FireVoidzone = 0x1E8D9B,
    Bomb = 0x2C68, // R=0.4
    Helper = 0x233C,
}

public enum AID : uint
{
    MagicDrain = 18890, // Boss->self, 3,0s cast, single-target
    Teleport = 18848, // Boss/SiegfriedCloneIce/SiegfriedCloneWind/SiegfriedCloneFire->location, no cast, ???
    MagitekDecoy = 18850, // Boss->self, no cast, single-target, calls clone (Ice->Wind->Fire weakness)
    HyperdriveFirst = 18836, // Boss->location, 5,0s cast, range 5 circle
    Swiftsteel = 18842, // SiegfriedCloneIce/Boss->self, 5,0s cast, range 100 circle
    Swiftsteel2 = 18843, // Helper->location, 8,8s cast, range 4 circle
    Swiftsteel3 = 18844, // Helper->self, 8,8s cast, range ?-20 donut
    LawOfTheTorch = 18838, // Boss/SiegfriedCloneIce/SiegfriedCloneWind/SiegfriedCloneFire->self, 3,0s cast, range 34 ?-degree cone
    LawOfTheTorch2 = 18839, // Helper->self, 3,0s cast, range 34 ?-degree cone
    AnkleGraze = 18846, // Boss->player, 3,0s cast, single-target
    Hyperdrive = 18893, // Boss/SiegfriedCloneWind/SiegfriedCloneFire->self, no cast, single-target
    HyperdriveRest = 18837, // Helper->location, 2,5s cast, range 5 circle
    Sparksteel1 = 18840, // SiegfriedCloneWind/Boss->location, 3,0s cast, range 6 circle, spawns voidzone
    Sparksteel2 = 18841, // Helper->location, 4,0s cast, range 8 circle
    Sparksteel3 = 18897, // Helper->location, 6,0s cast, range 8 circle
    Shattersteel = 19027, // SiegfriedCloneFire/Boss->self, 5,0s cast, range 8 circle
    SphereShatter = 18986, // IceBoulder->self, no cast, range 10 circle
    MagitekExplosive = 18849, // Boss->self, 3,0s cast, single-target
    RubberBullet = 18847, // Boss->player, 4,0s cast, single-target
    Explosion = 18888, // Bomb->self, 3,5s cast, range 8 circle
}

public enum SID : uint
{
    MagitekField = 2166, // Boss->Boss, extra=0x64, reflects magic damage
    MagicVulnerabilityDown = 812, // Boss->Boss, extra=0x0, invulnerable to magic
    Bind = 564, // Boss->player, extra=0x0, dispellable
}

class MagicDrain(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MagicDrain), "Reflect magic damage for 30s");
class HyperdriveFirst(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdriveFirst), 5);
class HyperdriveRest(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdriveRest), 5);
class AnkleGraze(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.AnkleGraze), "Applies bind, prepare to use Excuviation!");
class LawOfTheTorch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LawOfTheTorch), new AOEShapeCone(34, 10.Degrees()));
class LawOfTheTorch2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LawOfTheTorch2), new AOEShapeCone(34, 10.Degrees()));
class Swiftsteel(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Swiftsteel), 10)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Swiftsteel2>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (Module.FindComponent<Swiftsteel3>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}

class Swiftsteel2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Swiftsteel2), 4);
class Swiftsteel3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Swiftsteel3), new AOEShapeDonut(8, 20));
class Sparksteel1(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.Sparksteel1), m => m.Enemies(OID.FireVoidzone).Where(e => e.EventState != 7), 0.8f);

public class Sparksteel2 : Components.LocationTargetedAOEs
{
    public Sparksteel2(BossModule module) : base(module, ActionID.MakeSpell(AID.Sparksteel2), 8)
    {
        Color = ArenaColor.Danger;
    }
}

class Sparksteel3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Sparksteel3), 8)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Sparksteel2)
            Color = ArenaColor.Danger;
        else
            Color = ArenaColor.AOE;
    }    
}

class Shattersteel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shattersteel), new AOEShapeCircle(5));
class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceBoulder)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(8.4f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SphereShatter)
            _aoes.Clear();
    }
}

class RubberBullet(BossModule module) : Components.Knockback(module)
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Bomb)
            _knockback = new(Module.PrimaryActor.Position, 20, WorldState.FutureTime(6.3f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RubberBullet)
            _knockback = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Explosion>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}

class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Bomb)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(8.4f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
            _aoes.Clear();
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var magicabsorb = Module.PrimaryActor.FindStatus(SID.MagitekField);
        if (magicabsorb != null)
            hints.Add($"{Module.PrimaryActor.Name} will reflect all magic damage!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var bound = actor.FindStatus(SID.Bind);
        if (bound != null)
            hints.Add("You were bound! Cleanse it with Exuviation.");
    }
}

class Stage30Act3States : StateMachineBuilder
{
    public Stage30Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HyperdriveFirst>()
            .ActivateOnEnter<HyperdriveRest>()
            .ActivateOnEnter<AnkleGraze>()
            .ActivateOnEnter<LawOfTheTorch>()
            .ActivateOnEnter<LawOfTheTorch2>()
            .ActivateOnEnter<Swiftsteel>()
            .ActivateOnEnter<Swiftsteel2>()
            .ActivateOnEnter<Swiftsteel3>()
            .ActivateOnEnter<MagicDrain>()
            .ActivateOnEnter<Sparksteel1>()
            .ActivateOnEnter<Sparksteel2>()
            .ActivateOnEnter<Sparksteel3>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shattersteel>()
            .ActivateOnEnter<RubberBullet>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 3)]
public class Stage30Act3(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.SiegfriedCloneIce))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.SiegfriedCloneWind))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.SiegfriedCloneFire))
            Arena.Actor(s, ArenaColor.Object);
    }
}
