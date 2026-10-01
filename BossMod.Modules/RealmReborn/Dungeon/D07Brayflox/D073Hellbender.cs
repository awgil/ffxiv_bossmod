namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D073Hellbender;

public enum OID : uint
{
    Boss = 0x1AB, // Hellbender

    Aiatar = 0x1AD, // Aiatar

    QueerBubble = 0x428 // Queer Bubble
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single target
    BogBubble = 980, // Boss->player, no cast, range 6 circle aoe
    PeculiarLight = 982, // Boss->self, 3.5s cast, range 8 circle aoe
    StagnantSpray = 448, // Boss->self, 2.5s cast, range 8 120-degree cone aoe
    Effluvium = 979, // Boss->player, no cast, single target

    AutoAttackAiatar = 870, // Aiatar->player, no cast, single target
    Touchdown = 564, // Aiatar->self, no cast, range 10 circle aoe
    DragonBreath = 29618 // Aiatar->self, 3.0s cast, range 30 width 8 rect
}

public enum SID : uint
{
    Bind = 280
}

class PeculiarLight(BossModule module) : Components.StandardAOEs(module, AID.PeculiarLight, 8);
class StagnantSpray(BossModule module) : Components.StandardAOEs(module, AID.StagnantSpray, new AOEShapeCone(8, 60.Degrees()));
class BogBubble(BossModule module) : Components.Cleave(module, AID.BogBubble, new AOEShapeCircle(6), originAtTarget: true);
class Touchdown(BossModule module) : Components.GenericAOEs(module, AID.Touchdown)
{
    private static readonly AOEShapeCircle _shape = new(10);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Aiatar)
            _aoe = new(_shape, actor.Position, default, WorldState.FutureTime(0.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _aoe = null;
    }
}

class DragonBreath(BossModule module) : Components.StandardAOEs(module, AID.DragonBreath, new AOEShapeRect(30, 4));
class QueerBubble(BossModule module) : Components.Adds(module, (uint)OID.QueerBubble, 1);
class AiatarAdd(BossModule module) : Components.Adds(module, (uint)OID.Aiatar);

class D073HellbenderStates : StateMachineBuilder
{
    public D073HellbenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PeculiarLight>()
            .ActivateOnEnter<StagnantSpray>()
            .ActivateOnEnter<BogBubble>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<DragonBreath>()
            .ActivateOnEnter<QueerBubble>()
            .ActivateOnEnter<AiatarAdd>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.Aiatar).All(a => a.IsDestroyed || !a.IsTargetable);
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1286)]
public class D073Hellbender(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
