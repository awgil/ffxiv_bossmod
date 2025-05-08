namespace BossMod.Shadowbringers.Hunt.RankS.Tarchia;

public enum OID : uint
{
    Boss = 0x2873, // R=9.86
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    WakeUp = 18103, // Boss->self, no cast, single-target, visual for waking up from sleep
    WildHorn = 18026, // Boss->self, 3.0s cast, range 17 120-degree cone
    BafflementBulb = 18029, // Boss->self, 3.0s cast, range 40 circle, pull 50 between hitboxes, temporary misdirection
    ForestFire = 18030, // Boss->self, 5.0s cast, range 40 circle, damage fall off AOE, hard to tell optimal distance because logs are polluted by vuln stacks, guessing about 15
    MightySpin = 18028, // Boss->self, 3.0s cast, range 14 circle
    MightySpin2 = 18093, // Boss->self, no cast, range 14 circle, after 1s after boss wakes up and 4s after every Groundstorm
    Trounce = 18027, // Boss->self, 4.0s cast, range 40 60-degree cone
    MetamorphicBlast = 18031, // Boss->self, 4.0s cast, range 40 circle
    Groundstorm = 18023, // Boss->self, 5.0s cast, range 5-40 donut
}

class WildHorn(BossModule module) : Components.StandardAOEs(module, AID.WildHorn, new AOEShapeCone(17, 60.Degrees()));
class Trounce(BossModule module) : Components.StandardAOEs(module, AID.Trounce, new AOEShapeCone(40, 30.Degrees()));
class Groundstorm(BossModule module) : Components.StandardAOEs(module, AID.Groundstorm, new AOEShapeDonut(5, 40));
class MightySpin(BossModule module) : Components.StandardAOEs(module, AID.MightySpin, new AOEShapeCircle(14));
class ForestFire(BossModule module) : Components.StandardAOEs(module, AID.ForestFire, new AOEShapeCircle(15));
class BafflementBulb(BossModule module) : Components.CastHint(module, AID.BafflementBulb, "Pull + Temporary Misdirection -> Donut -> Out");
class MetamorphicBlast(BossModule module) : Components.RaidwideCast(module, AID.MetamorphicBlast);

class MightySpin2(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeCircle circle = new(14);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default || NumCasts == 0)
            yield return new(circle, Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Groundstorm)
            _activation = Module.CastFinishAt(spell, 4);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MightySpin2 or AID.Trounce or AID.AutoAttack or AID.MightySpin or AID.WildHorn or AID.Groundstorm or AID.BafflementBulb or AID.MetamorphicBlast or AID.Groundstorm) //everything but Mightyspin2 is a failsafe incase player joins fight/starts replay record late and Numcasts is 0 because of it
            ++NumCasts;
        if ((AID)spell.Action.ID == AID.MightySpin2)
            _activation = default;
    }
}

class TarchiaStates : StateMachineBuilder
{
    public TarchiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MetamorphicBlast>()
            .ActivateOnEnter<MightySpin>()
            .ActivateOnEnter<WildHorn>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<BafflementBulb>()
            .ActivateOnEnter<Groundstorm>()
            .ActivateOnEnter<ForestFire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8900)]
public class Tarchia : SimpleBossModule
{
    public Tarchia(WorldState ws, Actor primary) : base(ws, primary)
    {
        ActivateComponent<MightySpin2>();
    }
}
