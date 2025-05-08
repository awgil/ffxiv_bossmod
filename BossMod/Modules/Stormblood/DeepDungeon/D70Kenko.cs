namespace BossMod.Stormblood.DeepDungeon.D70Kenko;

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
class InnerspacePredict(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? Next;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Next);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Innerspace)
            Next = new(new AOEShapeCircle(3), WorldState.Actors.Find(spell.MainTargetID)!.Position, default, WorldState.FutureTime(1.6f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Puddle)
            Next = null;
    }
}
// TODO only apply invert logic to marked player, will probably need to rewrite component
class Innerspace(BossModule module) : Components.PersistentInvertibleVoidzone(module, 3, m => m.Enemies(OID.Puddle).Where(p => p.EventState != 7))
{
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
class Ululation(BossModule module) : Components.RaidwideCast(module, AID.Ululation);
class HoundOutOfHell(BossModule module) : Components.BaitAwayChargeCast(module, AID.HoundOutOfHell, 7);

class KenkoStates : StateMachineBuilder
{
    public KenkoStates(BossModule module) : base(module)
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public class Kenko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(24));
