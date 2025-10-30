namespace BossMod.Dawntrail.Foray.CriticalEngagement.DeathClaw;

public enum OID : uint
{
    Boss = 0x46C4, // R3.600, x1
    Helper = 0x233C, // R0.500, x12, Helper type
    DeathWallHelper = 0x483F, // R0.500, x1
    PhantomClaw = 0x46C5, // R2.925, x5
    Clawmarks1 = 0x46C6, // R1.000, x0 (spawn during fight)
    Clawmarks2 = 0x46C7, // R1.000, x0 (spawn during fight)
    Clawmarks3 = 0x46C8, // R1.000, x0 (spawn during fight)

    SkulkingOrders = 0x1EBCF0
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    DeathWall = 41308, // DeathWallHelper->self, no cast, ???
    DirtyNails = 41332, // Boss->player, 5.0s cast, single-target
    Clawmarks = 41309, // Boss->self, 5.0s cast, single-target
    SlashVisual = 41312, // PhantomClaw->self, no cast, single-target
    LethalNailsFast = 41315, // Clawmarks1/Clawmarks2->self, 2.0s cast, range 60 width 7 rect
    LethalNailsMid = 41316, // Clawmarks1/Clawmarks2/Clawmarks3->self, 4.0s cast, range 60 width 7 rect
    LethalNailsSlow = 41317, // Clawmarks2/Clawmarks3->self, 6.0s cast, range 60 width 7 rect
    VerticalCrosshatch = 41323, // Boss->self, 5.0s cast, single-target
    HorizontalCrosshatch = 41324, // Boss->self, 5.0s cast, single-target
    RakingScratch = 41325, // Helper->self, no cast, range 50 90-degree cone
    SkulkingOrdersSingle = 41326, // Boss->self, 7.0s cast, single-target
    ClawingShadow = 41327, // PhantomClaw->self, no cast, range 50 90-degree cone
    ClawingShadowVisual = 41328, // Helper->self, 1.0s cast, range 50 90-degree cone
    SkulkingOrdersDouble = 41329, // Boss->self, 7.0s cast, single-target
    VerticalCrosshatchSlow = 41330, // Boss->self, 7.5s cast, single-target
    HorizontalCrosshatchSlow = 41331, // Boss->self, 7.5s cast, single-target
    TheGripOfPoisonCast = 41333, // Boss->self, 4.0s cast, single-target
    TheGripOfPoison = 41334, // Helper->self, no cast, ???
    ThreefoldMarks = 41310, // Boss->self, 5.0s cast, single-target
    ManifoldMarks = 41311, // Boss->self, 5.0s cast, single-target
}

class DirtyNails(BossModule module) : Components.SingleTargetCast(module, AID.DirtyNails);
class TheGripOfPoison(BossModule module) : Components.RaidwideCast(module, AID.TheGripOfPoisonCast);

class Clawmarks(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Claw, DateTime AppearAt, int Order)> _casters = [];
    private readonly List<uint> _appearOrder = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var order = _casters.Count > 0 ? _casters[0].Order : -1;
        return _casters.TakeWhile(c => c.Order == order).Select(c => new AOEInstance(new AOEShapeRect(60, 3.5f), c.Claw.Position, c.Claw.Rotation, c.AppearAt.AddSeconds(CastDelay(c.Order))));
    }

    private static float CastDelay(int order) => order switch
    {
        0 => 6.9f,
        1 => 9.1f,
        2 => 11.1f,
        _ => 0
    };

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Clawmarks1 or OID.Clawmarks2 or OID.Clawmarks3)
        {
            var order = _appearOrder.IndexOf(actor.OID);
            if (order == -1)
            {
                order = _appearOrder.Count;
                _appearOrder.Add(actor.OID);
            }
            _casters.Add((actor, WorldState.CurrentTime, order));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LethalNailsFast or AID.LethalNailsMid or AID.LethalNailsSlow)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Claw == caster);
            if (_casters.Count == 0)
                _appearOrder.Clear();
        }
    }
}

class Crosshatch(BossModule module) : Components.GenericAOEs(module, AID.RakingScratch)
{
    private readonly List<AOEInstance> _predicted = [];

    private static readonly AOEShape Shape = new AOEShapeCone(50, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VerticalCrosshatch:
            case AID.VerticalCrosshatchSlow:
                Predict(default, spell);
                break;
            case AID.HorizontalCrosshatch:
            case AID.HorizontalCrosshatchSlow:
                Predict(90.Degrees(), spell);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count == 4)
        {
            // preposition along edge of aoe to make subsequent dodge easier
            var cross = ShapeContains.Cross(Arena.Center, 45.Degrees(), 50, 3);
            hints.AddForbiddenZone(p => !cross(p), _predicted[0].Activation.AddSeconds(2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Rotation.Normalized().AlmostEqual(spell.Rotation, 0.1f));
        }
    }

    private void Predict(Angle rotation, ActorCastInfo spell)
    {
        var src = spell.LocXZ;
        _predicted.Add(new(Shape, src, spell.Rotation + rotation, Module.CastFinishAt(spell, 0.1f)));
        _predicted.Add(new(Shape, src, spell.Rotation + rotation + 180.Degrees(), Module.CastFinishAt(spell, 0.1f)));
        _predicted.Add(new(Shape, src, spell.Rotation + rotation + 90.Degrees(), Module.CastFinishAt(spell, 2.1f)));
        _predicted.Add(new(Shape, src, spell.Rotation + rotation + 270.Degrees(), Module.CastFinishAt(spell, 2.1f)));
    }
}

class SkulkingOrders(BossModule module) : Components.GenericAOEs(module, AID.ClawingShadow)
{
    private readonly List<(Actor Caster, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeCone(50, 45.Degrees()), c.Caster.Position, c.Caster.Rotation, c.Activation)).Take(2);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
            _casters.Add((actor, WorldState.FutureTime(_casters.Count < 2 ? 8.1f : 10.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Caster.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

class DeathClawStates : StateMachineBuilder
{
    public DeathClawStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DirtyNails>()
            .ActivateOnEnter<TheGripOfPoison>()
            .ActivateOnEnter<Clawmarks>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<SkulkingOrders>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13656)]
public class DeathClaw(WorldState ws, Actor primary) : BossModule(ws, primary, new(681, 534), new ArenaBoundsSquare(21))
{
    public override bool DrawAllPlayers => true;
}

