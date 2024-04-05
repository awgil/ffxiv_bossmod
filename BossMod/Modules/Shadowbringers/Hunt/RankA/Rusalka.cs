namespace BossMod.Shadowbringers.Hunt.RankA.Rusalka;

public enum OID : uint
{
    Boss = 0x2853, // R=3.6
};

public enum AID : uint
{
    AutoAttack = 17364, // Boss->player, no cast, single-target
    Hydrocannon = 17363, // Boss->location, 3,5s cast, range 8 circle
    AetherialSpark = 17368, // Boss->self, 2,5s cast, range 12 width 4 rect
    AetherialPull = 17366, // Boss->self, 4,0s cast, range 30 circle, pull 30 between centers
    Flood = 17369, // Boss->self, no cast, range 8 circle
};

class Hydrocannon : Components.LocationTargetedAOEs
{
    public Hydrocannon() : base(ActionID.MakeSpell(AID.Hydrocannon), 8) { }
}

class AetherialSpark : Components.SelfTargetedAOEs
{
    public AetherialSpark() : base(ActionID.MakeSpell(AID.AetherialSpark), new AOEShapeRect(12, 2)) { }
}

class AetherialPull : Components.KnockbackFromCastTarget
{
    public AetherialPull() : base(ActionID.MakeSpell(AID.AetherialPull), 30, shape: new AOEShapeCircle(30), kind: Kind.TowardsOrigin) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<Flood>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Flood : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialPull)
            _aoe = new(new AOEShapeCircle(8), module.PrimaryActor.Position, activation: spell.NPCFinishAt.AddSeconds(3.6f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Flood)
            _aoe = null;
    }
}

class RusalkaStates : StateMachineBuilder
{
    public RusalkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<AetherialSpark>()
            .ActivateOnEnter<AetherialPull>()
            .ActivateOnEnter<Flood>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8896)]
public class Rusalka(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
