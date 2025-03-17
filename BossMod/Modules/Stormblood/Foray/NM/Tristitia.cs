namespace BossMod.Stormblood.Foray.NM;

public enum OID : uint
{
    Boss = 0x2740, // R6.000, x1
    Phuabo = 0x2741, // R4.000, x4
}

public enum AID : uint
{
    AutoAttack = 14919, // Boss->player, no cast, single-target
    ShockSpikes = 14920, // Boss->self, 5.0s cast, single-target
    SpineLash = 14921, // Boss->self, 3.0s cast, range 5+R ?-degree cone
    WatergaIII = 14922, // Boss->location, 3.0s cast, range 8 circle
    Dualcast = 15909, // Boss->self, 2.0s cast, single-target
    MightyStrikes = 15910, // Boss->self, 3.0s cast, single-target
    Meteor = 15911, // Boss->self, 5.0s cast, range 40 circle
    AerogaIV = 15912, // Boss->self, 3.5s cast, range 10 circle
    AerogaIVInstant = 15913, // Boss->self, no cast, range 10 circle
    TornadoII = 15914, // Boss->self, 5.0s cast, range ?-40 donut
    TornadoIIInstant = 15915, // Boss->self, no cast, range ?-40 donut
}

public enum SID : uint
{
    ShockSpikes = 199, // Boss->Boss, extra=0x64
    Dualcast = 1798, // Boss->Boss, extra=0x0
    CriticalStrikes = 1797, // Boss->Boss, extra=0x0
    Stun = 2, // Boss->player, extra=0x0
}

class SpineLash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpineLash), new AOEShapeCone(11, 45.Degrees()));
class Waterga(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WatergaIII), 8);
class ShockSpikes(BossModule module) : DispelComponent(module, (uint)SID.ShockSpikes);
class MightyStrikes(BossModule module) : DispelComponent(module, (uint)SID.CriticalStrikes);
class Meteor(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Meteor));
class Aeroga(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AerogaIV), new AOEShapeCircle(10));
class Tornado(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TornadoII), new AOEShapeDonut(5, 40));
class Dualcast(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? cast;
    private bool dual;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(cast);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Dualcast)
            dual = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (dual)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.TornadoII:
                    cast = new(new AOEShapeCircle(10), caster.Position, default, WorldState.FutureTime(3.1f));
                    dual = false;
                    break;
                case AID.AerogaIV:
                    cast = new(new AOEShapeDonut(5, 40), caster.Position, default, WorldState.FutureTime(3.1f));
                    dual = false;
                    break;
            }
        }

        if ((AID)spell.Action.ID is AID.TornadoIIInstant or AID.AerogaIVInstant)
            cast = null;
    }
}

class TristitiaStates : StateMachineBuilder
{
    public TristitiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpineLash>()
            .ActivateOnEnter<Waterga>()
            .ActivateOnEnter<ShockSpikes>()
            .ActivateOnEnter<MightyStrikes>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<Aeroga>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Dualcast>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1422, Contributors = "xan", SortOrder = 12)]
public class Tristitia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-125.7764f, -111.1819f), new ArenaBoundsCircle(80, 1));

