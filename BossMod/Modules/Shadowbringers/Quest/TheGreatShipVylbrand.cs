namespace BossMod.Shadowbringers.Quest.TheGreatShipVylbrand;

public enum OID : uint
{
    Boss = 0x3107
}

public enum AID : uint
{
    W10TrolleyWallop = 22950, // 3104->self, 6.0s cast, range 40 60-degree cone
    W10TrolleyTap = 23362, // 3104->self, 3.5s cast, range 8 120-degree cone
    W10TrolleyTorque = 22949, // 3104->self, 6.0s cast, range 16 circle
    Bulldoze = 22955, // 3107->location, 8.0s cast, width 6 rect charge
    Bulldoze1 = 22957, // 233C->location, 8.0s cast, width 6 rect charge
    TunnelShaker1 = 22959, // 233C->self, 5.0s cast, range 60 30-degree cone
    Uplift = 22961, // 233C->self, 6.0s cast, range 10 circle
    Uplift1 = 22962, // 233C->self, 8.0s cast, range 10-20 donut
    Uplift2 = 22963, // 233C->self, 10.0s cast, range 20-30 donut
}

class Torque(BossModule module) : Components.StandardAOEs(module, AID.W10TrolleyTorque, new AOEShapeCircle(16));
class Tap(BossModule module) : Components.StandardAOEs(module, AID.W10TrolleyTap, new AOEShapeCone(8, 60.Degrees()));
class Wallop(BossModule module) : Components.StandardAOEs(module, AID.W10TrolleyWallop, new AOEShapeCone(40, 30.Degrees()));
class Bulldoze(BossModule module) : Components.ChargeAOEs(module, AID.Bulldoze, 3);
class Bulldoze2(BossModule module) : Components.ChargeAOEs(module, AID.Bulldoze1, 3);
class TunnelShaker(BossModule module) : Components.StandardAOEs(module, AID.TunnelShaker1, new AOEShapeCone(60, 15.Degrees()));
class Uplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Uplift)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.Uplift => 0,
            AID.Uplift1 => 1,
            AID.Uplift2 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2)))
            ReportError($"unexpected order {order}");
    }
}

class BombTether : Components.BaitAwayTethers
{
    private DateTime? Activation;

    public BombTether(BossModule module) : base(module, new AOEShapeCircle(6), 97)
    {
        CenterAtTarget = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != null)
            hints.AddForbiddenZone(new AOEShapeDonut(1.5f, 100), new(9.15f, -8.44f), activation: Activation.Value);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Intercept tether!", CurrentBaits.Any(b => b.Target != actor));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        if (tether.ID == TID)
            Activation = WorldState.FutureTime(15);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        base.OnUntethered(source, tether);
        if (tether.ID == TID)
            Activation = null;
    }
}

public class SecondOrderRocksplitterStates : StateMachineBuilder
{
    public SecondOrderRocksplitterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bulldoze2>()
            .ActivateOnEnter<TunnelShaker>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<Torque>()
            .ActivateOnEnter<Tap>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<BombTether>()
            .Raw.Update = () => Module.WorldState.CurrentCFCID != 764;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69551)]
public class SecondOrderRocksplitter(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(27))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.InteractWithTarget = Enemies(0x1EB0F7).FirstOrDefault(x => x.IsTargetable);

        foreach (var e in hints.PotentialTargets)
            if (e.Actor.OID == 0x3106)
                e.Priority = AIHints.Enemy.PriorityPointless;
    }
}
