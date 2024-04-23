namespace BossMod.Global.MaskedCarnivale.Stage30.Act2;

public enum OID : uint
{
    Boss = 0x2C6A, //R=2.0
    IceBoulder = 0x2CC6, // R=2.0
    FireVoidzone = 0x1E8D9B,
    Helper = 0x233C,
}
public enum AID : uint
{
    LawOfTheTorch = 18838, // Boss->self, 3,0s cast, range 34 20-degree cone
    LawOfTheTorch2 = 18839, // Helper->self, 3,0s cast, range 34 20-degree cone
    Teleport = 18848, // Boss->location, no cast, ???
    Swiftsteel = 18842, // Boss->self, 5,0s cast, range 100 circle, knockback 10, away from source
    Swiftsteel2 = 18843, // Helper->location, 8,8s cast, range 4 circle
    Swiftsteel3 = 18844, // Helper->self, 8,8s cast, range 8-20 donut
    SparksteelVisual = 18893, // Boss->self, no cast, single-target
    Sparksteel1 = 18840, // Boss->location, 3,0s cast, range 6 circle, spawns voidzone
    Sparksteel2 = 18841, // Helper->location, 4,0s cast, range 8 circle
    Sparksteel3 = 18897, // Helper->location, 6,0s cast, range 8 circle
    Shattersteel = 19027, // Boss->self, 5,0s cast, range 8 circle
    SphereShatter = 18986, // IceBoulder->self, no cast, range 10 circle
}

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

class Stage30Act2States : StateMachineBuilder
{
    public Stage30Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LawOfTheTorch>()
            .ActivateOnEnter<LawOfTheTorch2>()
            .ActivateOnEnter<Swiftsteel>()
            .ActivateOnEnter<Swiftsteel2>()
            .ActivateOnEnter<Swiftsteel3>()
            .ActivateOnEnter<Sparksteel1>()
            .ActivateOnEnter<Sparksteel2>()
            .ActivateOnEnter<Sparksteel3>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shattersteel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 2)]
public class Stage30Act2(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(100, 100), 16));
