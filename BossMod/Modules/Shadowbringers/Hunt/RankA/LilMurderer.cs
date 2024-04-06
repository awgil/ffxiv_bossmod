namespace BossMod.Shadowbringers.Hunt.RankA.LilMurderer;

public enum OID : uint
{
    Boss = 0x28B4, // R=2.1
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    GobthunderIII = 17493, // Boss->player, 6,0s cast, range 20 circle, interruptible, applies Lightning Resistance Down II
    GoblinPunch = 17488, // Boss->player, 3,0s cast, single-target
    GobthunderII = 17492, // Boss->location, 4,0s cast, range 8 circle, applies Lightning Resistance Down II
    Gobhaste = 17491, // Boss->self, 3,0s cast, single-target
    GoblinSlash = 17489, // Boss->self, no cast, range 8 circle, sometimes boss uses Gobthunder II on itself, next attack after is this
};

class GoblinSlash : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GobthunderII && spell.LocXZ == caster.Position)
            _aoe = new(new AOEShapeCircle(8), module.PrimaryActor.Position, activation: spell.NPCFinishAt.AddSeconds(2.6f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GoblinSlash)
            _aoe = null;
    }
}

class GobthunderIII : Components.SpreadFromCastTargets
{
    public GobthunderIII() : base(ActionID.MakeSpell(AID.GobthunderIII), 20) { }
}

class GobthunderIIIHint : Components.CastInterruptHint
{
    public GobthunderIIIHint() : base(ActionID.MakeSpell(AID.GobthunderIII)) { }
}

class GoblinPunch : Components.SingleTargetCast
{
    public GoblinPunch() : base(ActionID.MakeSpell(AID.GoblinPunch)) { }
}

class Gobhaste : Components.CastHint
{
    public Gobhaste() : base(ActionID.MakeSpell(AID.Gobhaste), "Attack speed buff") { }
}

class GobthunderII : Components.LocationTargetedAOEs
{
    public GobthunderII() : base(ActionID.MakeSpell(AID.GobthunderII), 8) { }
}

class LilMurdererStates : StateMachineBuilder
{
    public LilMurdererStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoblinPunch>()
            .ActivateOnEnter<GoblinSlash>()
            .ActivateOnEnter<GobthunderII>()
            .ActivateOnEnter<GobthunderIII>()
            .ActivateOnEnter<GobthunderIIIHint>()
            .ActivateOnEnter<Gobhaste>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8911)]
public class LilMurderer(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
