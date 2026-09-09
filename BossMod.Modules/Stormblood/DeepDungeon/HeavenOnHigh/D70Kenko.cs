namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D70Kenko;

public enum OID : uint
{
    Boss = 0x23EB, // R6.000, x1
    Puddle = 0x1E9829
}

public enum AID : uint
{
    PredatorClaws = 12205, // Boss->self, 3.0s cast, range 9+R ?-degree cone
    Slabber = 12203, // Boss->location, 3.0s cast, range 8 circle
    Innerspace = 12207, // Boss->player, 3.0s cast, single-target
    Ululation = 12208, // Boss->self, 3.0s cast, range 80+R circle
    HoundOutOfHell = 12206, // Boss->player, 5.0s cast, width 14 rect charge
    Devour = 12204, // Boss->location, no cast, range 4+R ?-degree cone
}

public enum SID : uint
{
    Minimum = 438, // none->player, extra=0x32
    Stun = 149, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Prey = 1, // player->self
}

class PredatorClaws(BossModule module) : Components.StandardAOEs(module, AID.PredatorClaws, new AOEShapeCone(15, 60.Degrees()));
class Slabber(BossModule module) : Components.StandardAOEs(module, AID.Slabber, 8);
class Ululation(BossModule module) : Components.RaidwideCast(module, AID.Ululation);
class HoundOutOfHell(BossModule module) : Components.BaitAwayChargeCast(module, AID.HoundOutOfHell, 7);
class InnerspacePredict(BossModule module) : Components.SpreadFromCastTargets(module, AID.Innerspace, 3);
class Innerspace(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(3);

    private int _prey = -1;
    public DateTime InvertResolveAt;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in Module.Enemies(OID.Puddle).Where(p => p.EventState != 7))
        {
            yield return slot == _prey && InvertResolveAt != default
                ? new(_shape, p.Position, Activation: InvertResolveAt, Inverted: true)
                : new(_shape, p.Position);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Prey && Raid.TryFindSlot(actor, out var slot))
            _prey = slot;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HoundOutOfHell)
            InvertResolveAt = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Devour)
            InvertResolveAt = default;
    }
}

class D70KenkoStates : StateMachineBuilder
{
    public D70KenkoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PredatorClaws>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Innerspace>()
            .ActivateOnEnter<InnerspacePredict>()
            .ActivateOnEnter<Ululation>()
            .ActivateOnEnter<HoundOutOfHell>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public class D70Kenko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(24));
