namespace BossMod.Shadowbringers.Hunt.RankA.Huracan;

public enum OID : uint
{
    Boss = 0x28B5, // R=4.9
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WindsEnd = 17494, // Boss->player, no cast, single-target
    WinterRain = 17497, // Boss->location, 4,0s cast, range 6 circle
    Windburst = 18042, // Boss->self, no cast, range 80 width 10 rect
    SummerHeat = 17499, // Boss->self, 4,0s cast, range 40 circle
    DawnsEdge = 17495, // Boss->self, 3,5s cast, range 15 width 10 rect
    SpringBreeze = 17496, // Boss->self, 3,5s cast, range 80 width 10 rect
    AutumnWreath = 17498, // Boss->self, 4,0s cast, range 10-20 donut
};

class SpringBreeze : Components.SelfTargetedAOEs
{
    public SpringBreeze() : base(ActionID.MakeSpell(AID.SpringBreeze), new AOEShapeRect(40, 5, 40)) { }
}

class SummerHeat : Components.RaidwideCast
{
    public SummerHeat() : base(ActionID.MakeSpell(AID.SummerHeat)) { }
}

class Combos : Components.GenericAOEs
{
    private static readonly AOEShapeDonut donut = new(10, 20);
    private static readonly AOEShapeCircle circle = new(6);
    private static readonly AOEShapeRect rect = new(15, 5);
    private static readonly AOEShapeRect rect2 = new(40, 5, 40);
    private DateTime _activation;
    private Angle _rotation;
    private AOEShape? _shape;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default && _shape != null)
        {
            if (NumCasts == 0)
            {
                yield return new(_shape, module.PrimaryActor.Position, _rotation, _activation, ArenaColor.Danger);
                yield return new(rect2, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation.AddSeconds(3.1f), risky: false);
            }
            if (NumCasts == 1)
                yield return new(rect2, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation.AddSeconds(3.1f), ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AutumnWreath)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
            _shape = donut;
        }
        if ((AID)spell.Action.ID is AID.DawnsEdge)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
            _shape = rect;
        }
        if ((AID)spell.Action.ID is AID.WinterRain)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
            _shape = circle;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AutumnWreath or AID.DawnsEdge or AID.WinterRain)
            ++NumCasts;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Windburst)
        {
            NumCasts = 0;
            _activation = default;
            _shape = null;
        }
    }
}

class HuracanStates : StateMachineBuilder
{
    public HuracanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringBreeze>()
            .ActivateOnEnter<SummerHeat>()
            .ActivateOnEnter<Combos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8912)]
public class Huracan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
