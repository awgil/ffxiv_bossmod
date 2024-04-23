namespace BossMod.Global.MaskedCarnivale.Stage30.Act1;

public enum OID : uint
{
    Boss = 0x2C67, //R=2.0
    Bomb = 0x2C68, // R=0.4
    Helper = 0x233C,
}

public enum AID : uint
{
    MagicDrain = 18890, // Boss->self, 3,0s cast, single-target
    AnkleGraze = 18846, // Boss->player, 3,0s cast, single-target
    HyperdriveFirst = 18836, // Boss->location, 5,0s cast, range 5 circle
    HyperdriveRest = 18837, // Helper->location, 2,5s cast, range 5 circle
    Hyperdrive = 18893, // Boss->self, no cast, single-target, between Hyperdrive casts
    Teleport = 18848, // Boss->location, no cast, ???
    MagitekExplosive = 18849, // Boss->self, 3,0s cast, single-target
    RubberBullet = 18847, // Boss->player, 4,0s cast, single-target, knockback 20 away from source
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

class Hints2(BossModule module) : BossComponent(module)
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

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will have phases where all magic damage gets reflected.\nExuviation, a melee ability and fire, wind and ice spells are recommended.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Take no damage, use all 6 magic elements,\nuse all 3 melee types, kill all 3 clones in act3 and finish faster than ideal time.", false);
    }
}

class Stage30Act1States : StateMachineBuilder
{
    public Stage30Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HyperdriveFirst>()
            .ActivateOnEnter<HyperdriveRest>()
            .ActivateOnEnter<AnkleGraze>()
            .ActivateOnEnter<RubberBullet>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<MagicDrain>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 1)]
public class Stage30Act1 : BossModule
{
    public Stage30Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }
}
