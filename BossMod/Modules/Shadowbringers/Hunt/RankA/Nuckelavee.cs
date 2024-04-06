namespace BossMod.Shadowbringers.Hunt.RankA.Nuckelavee;

public enum OID : uint
{
    Boss = 0x288F, // R=3.6
};

public enum AID : uint
{
    AutoAttack = 872, // 288F->player, no cast, single-target
    Torpedo = 16964, // 288F->player, 4,0s cast, single-target, tankbuster on cast event
    BogBody = 16965, // 288F->player, 5,0s cast, range 5 circle, spread, applies bleed that can be dispelled
    Gallop = 16967, // 288F->location, 4,5s cast, rushes to target and casts Spite
    Spite = 18037, // 288F->self, no cast, range 8 circle
};

class Torpedo : Components.SingleTargetDelayableCast
{
    public Torpedo() : base(ActionID.MakeSpell(AID.Torpedo)) { }
}

class BogBody : Components.SpreadFromCastTargets
{
    public BogBody() : base(ActionID.MakeSpell(AID.BogBody), 5) { }
}

class Spite : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Gallop)
            _aoe = new(new AOEShapeCircle(8), spell.LocXZ, activation: spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Spite)
            _aoe = null;
    }
}

class NuckelaveeStates : StateMachineBuilder
{
    public NuckelaveeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Torpedo>()
            .ActivateOnEnter<BogBody>()
            .ActivateOnEnter<Spite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8906)]
public class Nuckelavee(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
