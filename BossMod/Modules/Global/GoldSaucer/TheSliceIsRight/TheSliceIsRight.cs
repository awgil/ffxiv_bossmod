namespace BossMod.Global.GoldSaucer.SliceIsRight;

public enum OID : uint
{
    Boss = 0x25AB, //R=1.80 Yojimbo
    Daigoro = 0x25AC, //R=2.50
    Bamboo = 0x25AD, //R=0.50
    Helper = 0x1EAEDF,
    Somethingoutsidethearena = 0x2BC8, //not sure what this does, spawns multiple times before the minigame starts, but casts something during the minigame, probably just for visuals
    HelperCupPhase1 = 0x1EAEB7,
    HelperCupPhase2 = 0x1EAEB6,
    HelperCupPhase3 = 0x1EAE9D,
    HelperSingleRect = 0x1EAE99,
    HelperDoubleRect = 0x1EAE9A,
    HelperCircle = 0x1EAE9B,
    Pileofgold = 0x1EAE9C,
};

public enum AID : uint
{
    Yoyimbodoesstuff1 = 19070, // 25AB->self, no cast, single-target
    Yoyimbodoesstuff2 = 18331, // 25AB->self, no cast, single-tat
    Yoyimbodoesstuff3 = 18329, // 25AB->self, no cast, single-target
    Yoyimbodoesstuff4 = 18332, // 25AB->self, no cast, single-target
    Yoyimbodoesstuff5 = 18328, // 25AB->location, no cast, single-target
    Yoyimbodoesstuff6 = 18326, // 25AB->self, 3.0s cast, single-target
    Yoyimbodoesstuff7 = 18339, // 25AB->self, 3.0s cast, single-target
    Yoyimbodoesstuff8 = 18340, // 25AB->self, no cast, single-target
    Yoyimbodoesstuff9 = 19026, // 25AB->self, no cast, single-target
    Somethingoutsidethearena = 18338, // 2BC8->self, no cast, single-target
    BambooSplit = 18333, // 25AD->self, 0.7s cast, range 28 width 5 rect
    BambooCircleFall = 18334, // 25AD->self, 0.7s cast, range 11 circle 
    BambooSpawn = 18327, // 25AD->self, no cast, range 3 circle
    FirstGilJump = 18335, // 25AC->location, 2.5s cast, width 7 rect charge
    NextGilJump = 18336, // 25AC->location, 1.5s cast, width 7 rect charge
    BadCup = 18337, // 25AC->self, 1.0s cast, range 15+R 120-degree cone
};

class BambooSplits : Components.GenericAOEs
{
    private readonly List<Actor> _doublesidedsplit = [];
    private readonly List<Actor> _singlesplit = [];
    private readonly List<Actor> _circle = [];
    private readonly List<Actor> _doublesidedsplitToberemoved = [];
    private readonly List<Actor> _singlesplitToberemoved = [];
    private readonly List<Actor> _circleToberemoved = [];
    private readonly List<Actor> _bamboospawn = [];
    private static readonly AOEShapeRect rectdouble = new(28, 2.5f, 28);
    private static readonly AOEShapeRect rectsingle = new(28, 2.5f);
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly AOEShapeCircle bamboospawn = new(3);
    private DateTime _activation;
    private DateTime _time;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var b in _doublesidedsplit)
            yield return new(rectdouble, b.Position, b.Rotation + 90.Degrees(), _activation.AddSeconds(7));
        foreach (var b in _singlesplit)
            yield return new(rectsingle, b.Position, b.Rotation + 90.Degrees(), _activation.AddSeconds(7));
        foreach (var b in _circle)
            yield return new(circle, b.Position, b.Rotation, _activation.AddSeconds(7));
        foreach (var b in _bamboospawn)
            yield return new(bamboospawn, b.Position); //activation time varies a lot (depending on the set?), just avoid entirely
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.HelperCircle or OID.HelperDoubleRect or OID.HelperSingleRect)
            _bamboospawn.Add(actor);
    }

    public override void Update(BossModule module)
    {
        if (_time != default && module.WorldState.CurrentTime > _time)
        {
            _time = default;
            _circle.RemoveAll(_circleToberemoved.Contains);
            _circleToberemoved.Clear();
            _singlesplit.RemoveAll(_singlesplitToberemoved.Contains);
            _singlesplitToberemoved.Clear();
            _doublesidedsplit.RemoveAll(_doublesidedsplitToberemoved.Contains);
            _doublesidedsplitToberemoved.Clear();
        }
    }

    public override void OnActorEAnim(BossModule module, Actor actor, uint state)
    {
        if (state == 0x00010002) //bamboo gets activated, technically we could draw the AOEs before, but then we could see different sets overlap
        {
            if ((OID)actor.OID == OID.HelperCircle && !_circle.Contains(actor))
                _circle.Add(actor);
            if ((OID)actor.OID == OID.HelperSingleRect && !_singlesplit.Contains(actor))
                _singlesplit.Add(actor);
            if ((OID)actor.OID == OID.HelperDoubleRect && !_doublesidedsplit.Contains(actor))
                _doublesidedsplit.Add(actor);
            _activation = module.WorldState.CurrentTime.AddSeconds(7);
        }
        if (state == 0x00040008) //bamboo deactivation animation, spell casts end about 0.75s later
        {
            if ((OID)actor.OID == OID.HelperCircle)
                _circleToberemoved.Add(actor);
            if ((OID)actor.OID == OID.HelperSingleRect)
                _singlesplitToberemoved.Add(actor);
            if ((OID)actor.OID == OID.HelperDoubleRect)
                _doublesidedsplitToberemoved.Add(actor);
            _time = module.WorldState.CurrentTime.AddSeconds(0.75f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_bamboospawn.Count > 0 && (AID)spell.Action.ID == AID.BambooSpawn)
            _bamboospawn.RemoveAt(0);
    }
}

class DaigoroFirstGilJump : Components.ChargeAOEs
{
    public DaigoroFirstGilJump() : base(ActionID.MakeSpell(AID.FirstGilJump), 3.5f) { }
}

class DaigoroNextGilJump : Components.ChargeAOEs
{
    public DaigoroNextGilJump() : base(ActionID.MakeSpell(AID.NextGilJump), 3.5f) { }
}

class DaigoroBadCup : Components.SelfTargetedAOEs
{
    public DaigoroBadCup() : base(ActionID.MakeSpell(AID.BadCup), new AOEShapeCone(17.5f, 60.Degrees())) { }
}

class TheSliceIsRightStates : StateMachineBuilder
{
    public TheSliceIsRightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BambooSplits>()
            .ActivateOnEnter<DaigoroFirstGilJump>()
            .ActivateOnEnter<DaigoroNextGilJump>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.GoldSaucer, GroupID = 181, NameID = 9066)]
public class TheSliceIsRight : BossModule
{
    public TheSliceIsRight(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(70.5f, -36), 15)) { }

    protected override bool CheckPull() { return PrimaryActor != null; }
}
