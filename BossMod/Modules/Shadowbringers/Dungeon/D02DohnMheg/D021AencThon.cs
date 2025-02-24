namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    _Weaponskill_Hydrofall = 8871, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Hydrofall1 = 8893, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_LaughingLeap = 8852, // Boss->location, 4.0s cast, range 4 circle
    _Weaponskill_LaughingLeap1 = 8840, // Boss->players, no cast, range 4 circle
    _Weaponskill_Landsblood = 7822, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_Landsblood1 = 7899, // Boss->self, no cast, range 40 circle
    _Weaponskill_Geyser = 8800, // Helper->self, no cast, range 6 circle
}

class CandyCane(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_CandyCane));
class Hydrofall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Hydrofall1), 6);
class LaughingLeap(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LaughingLeap), 4);
class LaughingLeap2(BossModule module) : Components.StackWithIcon(module, 62, ActionID.MakeSpell(AID._Weaponskill_LaughingLeap1), 4, 5.15f);
class Landsblood(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Landsblood));

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
        if (spell.Action.ID == (uint)AID._Weaponskill_Geyser)
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
            .ActivateOnEnter<LaughingLeap2>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<Geyser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class AencThonLordOfTheLingeringGaze(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 30), new ArenaBoundsCircle(19.5f));

