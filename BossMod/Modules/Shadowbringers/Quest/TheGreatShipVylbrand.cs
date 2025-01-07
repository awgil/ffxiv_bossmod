namespace BossMod.Shadowbringers.Quest.TheGreatShipVylbrand;

public enum OID : uint
{
    Boss = 0x3107
}

public enum AID : uint
{
    _Ability_ = 23486, // 3108->self, no cast, single-target
    _Spell_Stone = 21588, // 3101->30FD, 1.0s cast, single-target
    _AutoAttack_Attack = 6499, // 3103/3107->player/30FD, no cast, single-target
    _AutoAttack_Attack1 = 6497, // 3100/3102/3104->player/30FD, no cast, single-target
    _Weaponskill_Breakthrough = 22948, // 3104->30FA, 11.0s cast, width 8 rect charge
    _Ability_1 = 22947, // 3104->location, no cast, single-target
    _Weaponskill_10TrolleyWallop = 22950, // 3104->self, 6.0s cast, range 40 60-degree cone
    _Weaponskill_10TrolleyTap = 23362, // 3104->self, 3.5s cast, range 8 120-degree cone
    _Weaponskill_10TrolleyTorque = 22949, // 3104->self, 6.0s cast, range 16 circle
    _Weaponskill_ExplosiveChemistry = 23497, // 318C/3106->self, 12.0s cast, single-target
    _Weaponskill_SelfDestruct = 22952, // 318C/3106->self, no cast, range 6 circle
    _Weaponskill_SelfDestruct1 = 23501, // 3187->self, 3.5s cast, range 10 circle
    _Weaponskill_SelfDestruct2 = 23500, // 3106->self, no cast, single-target
    _Weaponskill_Quakedown = 22953, // 3107->location, no cast, range 60 circle
    _Weaponskill_Excavate = 23132, // 3107->self, 17.0s cast, single-target
    _Weaponskill_Excavate1 = 22954, // 3107->30FC, no cast, width 6 rect charge
    _Weaponskill_KoboldDrill = 22967, // 3107->player, 4.0s cast, single-target
    _Weaponskill_Bulldoze = 22955, // 3107->location, 8.0s cast, width 6 rect charge
    _Weaponskill_Bulldoze1 = 22957, // 233C->location, 8.0s cast, width 6 rect charge
    _Weaponskill_Bulldoze2 = 22956, // 3107->location, no cast, width 6 rect charge
    _Weaponskill_TunnelShaker = 22958, // 3107->self, 5.0s cast, single-target
    _Weaponskill_TunnelShaker1 = 22959, // 233C->self, 5.0s cast, range 60 30-degree cone
    _Weaponskill_StrataSmasher = 22960, // 3107->location, no cast, range 60 circle
    _Weaponskill_Uplift = 22961, // 233C->self, 6.0s cast, range 10 circle
    _Weaponskill_Uplift1 = 22962, // 233C->self, 8.0s cast, range 10-20 donut
    _Weaponskill_Uplift2 = 22963, // 233C->self, 10.0s cast, range 20-30 donut
}

class Torque(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_10TrolleyTorque), new AOEShapeCircle(16));
class Tap(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_10TrolleyTap), new AOEShapeCone(8, 60.Degrees()));
class Wallop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_10TrolleyWallop), new AOEShapeCone(40, 30.Degrees()));
class Bulldoze(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bulldoze), 3);
class Bulldoze2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Bulldoze1), 3);
class TunnelShaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TunnelShaker1), new AOEShapeCone(60, 15.Degrees()));
class Uplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_Uplift)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_Uplift => 0,
            AID._Weaponskill_Uplift1 => 1,
            AID._Weaponskill_Uplift2 => 2,
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69551)]
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
