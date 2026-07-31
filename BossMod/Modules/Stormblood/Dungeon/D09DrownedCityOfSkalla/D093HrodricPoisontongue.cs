namespace BossMod.Stormblood.Dungeon.D09DrownedCityOfSkalla.D093HrodricPoisontongue;

public enum OID : uint
{
    Boss = 0x1FAE, // R5.520, x1
    Helper = 0x18D6, // R0.500, x4
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    RustingClaw = 9825, // Boss->self, 5.0s cast, range 8+R 90?-degree cone
    WordsOfWoe = 9826, // Boss->self, 3.0s cast, range 45+R width 6 rect
    TailDrive = 9827, // Boss->self, 5.0s cast, range 30+R 120?-degree cone
    TheSpin = 9828, // Boss->self, 5.0s cast, range 40+R circle, ~15y falloff
    EyeOfTheFire = 9829, // Boss->self, 3.0s cast, range 40 circle
    Jump = 9830, // Boss->self, no cast, single-target
    RingOfChaos = 9831, // Helper->self, no cast, range 10?-20 donut
    CrossOfChaos = 9832, // Helper->self, no cast, range 50 width 8 cross
    CircleOfChaos = 9833, // Helper->self, no cast, range 6 circle
}

public enum IconID : uint
{
    Gravity = 28, // player->self
    Ring = 121, // player->self
    Cross = 122, // player->self
}

class RustingClaw(BossModule module) : Components.StandardAOEs(module, AID.RustingClaw, new AOEShapeCone(13.52f, 45.Degrees()));
class TailDrive(BossModule module) : Components.StandardAOEs(module, AID.TailDrive, new AOEShapeCone(35.52f, 60.Degrees()));
class TheSpin(BossModule module) : Components.ProximityAOEs(module, AID.TheSpin, 15);
class RingOfChaos(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeDonut(10, 20), (uint)IconID.Ring, AID.RingOfChaos, centerAtTarget: true);
class EyeOfTheFire(BossModule module) : Components.CastGaze(module, AID.EyeOfTheFire);
class CircleOfChaos(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Gravity, AID.CircleOfChaos, 5.1f, true);
class CrossOfChaos(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCross(50, 4), (uint)IconID.Cross, AID.CrossOfChaos, 5.1f, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);

        for (var i = 0; i < CurrentBaits.Count; i++)
            CurrentBaits.Ref(i).IgnoreRotation = true;
    }
}
class WordsOfWoe(BossModule module) : Components.StandardAOEs(module, AID.WordsOfWoe, new AOEShapeRect(50.52f, 3));

class D093HrodricPoisontongueStates : StateMachineBuilder
{
    public D093HrodricPoisontongueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<RingOfChaos>()
            .ActivateOnEnter<EyeOfTheFire>()
            .ActivateOnEnter<CircleOfChaos>()
            .ActivateOnEnter<CrossOfChaos>()
            .ActivateOnEnter<WordsOfWoe>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 279, NameID = 6910)]
public class D093HrodricPoisontongue(WorldState ws, Actor primary) : BossModule(ws, primary, new(479, 4), new ArenaBoundsCircle(19.5f));

