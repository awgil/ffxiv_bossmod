namespace BossMod.Global.PVP.HiddenGorge.GoblinMercenary;

public enum OID : uint
{
    Boss = 0x25FA, //R=2.0
    BossHelper = 0x233C,
};

public enum AID : uint
{
    IronKiss = 14562, // 233C->location, 5,0s cast, range 7 circle
    GobfireShootypopsStart = 14563, // 25FA->self, 5,0s cast, range 30+R width 6 rect
    GobfireShootypops = 14564, // 25FA->self, no cast, range 30+R width 6 rect
    GobspinWhooshdropsTelegraph = 14567, // 233C->self, 1,0s cast, single-target
    Plannyplot = 14558, // 25FA->self, 4,0s cast, single-target
    GobspinWhooshdrops = 14559, // 25FA->self, no cast, range 8 circle, knockback 15 away from source
    GobswipeConklopsTelegraph = 14568, // BossHelper->self, 1,0s cast, single-target
    GobswipeConklops = 14560, // Boss->self, no cast, range 5-30 donut, knockback 15 away from source
    Discharge = 14561, // Boss->self, no cast, single-target
};

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167, // Boss
};

class GobspinSwipe : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GobspinWhooshdropsTelegraph)
            _aoe = new(new AOEShapeCircle(8), module.PrimaryActor.Position, activation: spell.NPCFinishAt.AddSeconds(4));
        if ((AID)spell.Action.ID == AID.GobswipeConklopsTelegraph)
            _aoe = new(new AOEShapeDonut(5, 30), module.PrimaryActor.Position, activation: spell.NPCFinishAt.AddSeconds(4));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GobspinWhooshdrops or AID.GobswipeConklops)
            _aoe = null;
    }
}

class Knockbacks : Components.Knockback
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GobspinWhooshdropsTelegraph)
            _knockback = new(module.PrimaryActor.Position, 15, spell.NPCFinishAt.AddSeconds(4), new AOEShapeCircle(8));
        if ((AID)spell.Action.ID == AID.GobswipeConklopsTelegraph)
            _knockback = new(module.PrimaryActor.Position, 15, spell.NPCFinishAt.AddSeconds(4), new AOEShapeDonut(5, 30));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GobspinWhooshdrops or AID.GobswipeConklops)
            _knockback = null;
    }
}

class GobfireShootypops : Components.GenericRotatingAOE
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new AOEShapeRect(32, 3);

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -60.Degrees(),
            IconID.RotateCCW => 60.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(module, actor);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GobfireShootypopsStart)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
        if (_rotation != default)
            InitIfReady(module, caster);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GobfireShootypopsStart or AID.GobfireShootypops)
            AdvanceSequence(0, module.WorldState.CurrentTime);
    }

    private void InitIfReady(BossModule module, Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 1, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class IronKiss : Components.LocationTargetedAOEs
{
    public IronKiss() : base(ActionID.MakeSpell(AID.IronKiss), 7) { }
}

class GoblinMercenaryStates : StateMachineBuilder
{
    public GoblinMercenaryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<GobspinSwipe>()
            .ActivateOnEnter<Knockbacks>()
            .ActivateOnEnter<GobfireShootypops>()
            .Raw.Update = () => module.PrimaryActor.IsDead || !module.PrimaryActor.IsTargetable;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 599, NameID = 7906)]
public class GoblinMercenary : BossModule
{
    public GoblinMercenary(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 0)) { }

    protected override void UpdateModule()
    {
        if (Enemies(OID.Boss).Any(e => e.Position.AlmostEqual(new(0, -125), 1)))
            Arena.Bounds = new ArenaBoundsSquare(new(0, -124.5f), 16); //Note: the arena doesn't seem to be a perfect square, but it seems close enough
        if (Enemies(OID.Boss).Any(e => e.Position.AlmostEqual(new(0, 144.5f), 1)))
            Arena.Bounds = new ArenaBoundsCircle(new(0, 144.5f), 30); //Note: the arena doesn't seem to be a perfect circle, but this seems good enough
    }
}
