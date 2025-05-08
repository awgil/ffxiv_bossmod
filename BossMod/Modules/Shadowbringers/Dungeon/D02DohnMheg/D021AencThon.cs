namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2,
    Helper = 0x233C,
}

public enum AID : uint
{
    CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    Hydrofall = 8893, // Helper->location, 3.0s cast, range 6 circle
    LaughingLeap = 8852, // Boss->location, 4.0s cast, range 4 circle
    LaughingLeapStack = 8840, // Boss->players, no cast, range 4 circle
    Landsblood = 7822, // Boss->self, 3.0s cast, range 40 circle
    Geyser = 8800, // Helper->self, no cast, range 6 circle
}

class CandyCane(BossModule module) : Components.SingleTargetCast(module, AID.CandyCane);
class Hydrofall(BossModule module) : Components.StandardAOEs(module, AID.Hydrofall, 6);
class LaughingLeap(BossModule module) : Components.StandardAOEs(module, AID.LaughingLeap, 4);
class LaughingLeapStack(BossModule module) : Components.StackWithIcon(module, 62, AID.LaughingLeapStack, 4, 5.15f);
class Landsblood(BossModule module) : Components.RaidwideCast(module, AID.Landsblood);

class Geyser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> Geysers = [];

    private readonly List<WDir> Geysers1 = [new(0, -16), new(-9, 15)];
    private readonly List<WDir> Geysers2 = [new(0, 5), new(-9, -15), new(7, -7)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Geysers;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x100020)
        {
            var geysers = actor.OID switch
            {
                0x1EAAA1 => Geysers1,
                0x1EAAA2 => Geysers2,
                _ => []
            };
            Geysers.AddRange(geysers.Select(d =>
            {
                var center = d.Rotate(-actor.Rotation) + actor.Position;
                return new AOEInstance(new AOEShapeCircle(6), center, default, WorldState.FutureTime(5.1f));
            }));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Geyser)
            Geysers.RemoveAll(g => g.Origin.AlmostEqual(caster.Position, 1));
    }
}

class AencThonLordOfTheLingeringGazeStates : StateMachineBuilder
{
    public AencThonLordOfTheLingeringGazeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandyCane>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<LaughingLeapStack>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<Geyser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class AencThonLordOfTheLingeringGaze(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 30), new ArenaBoundsCircle(19.5f));
