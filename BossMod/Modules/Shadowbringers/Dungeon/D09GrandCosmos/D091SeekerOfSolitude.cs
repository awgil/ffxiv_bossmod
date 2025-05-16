namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D091SeekerOfSolitude;

public enum AID : uint
{
    Shadowbolt = 18281, // Boss->player, 4.0s cast, single-target
    ImmortalAnathema = 18851, // Boss->self, 4.0s cast, range 60 circle
    Tribulation = 18852, // Helper->location, 3.0s cast, range 3 circle
    DarkShock = 18287, // Helper->location, 3.0s cast, range 6 circle
    DarkPulse = 18282, // Boss->players, 5.0s cast, range 6 circle
    DarkWell = 18285, // Helper->player, 5.0s cast, range 5 circle
}

public enum OID : uint
{
    Boss = 0x2C1A,
    Helper = 0x233C,
    MagickedBroom = 0x2C1B,
    DirtPile = 0x1EAEAE
}

class Tribulation(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 3, AID.Tribulation, m => m.Enemies(OID.DirtPile).Where(x => x.EventState != 7), 0);
class ImmortalAnathema(BossModule module) : Components.RaidwideCast(module, AID.ImmortalAnathema);
class DarkPulse(BossModule module) : Components.StackWithCastTargets(module, AID.DarkPulse, 6);
class DarkWell(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkWell, 5);
class DarkShock(BossModule module) : Components.StandardAOEs(module, AID.DarkShock, 6);
class Shadowbolt(BossModule module) : Components.SingleTargetCast(module, AID.Shadowbolt);

// not sure about radius, sweep trigger is incredibly janky
// filter out brooms who are too far outside the arena since they don't affect players and the AOE lingering on minimap is annoying
class Sweep(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.MagickedBroom).Where(b => MathF.Abs(b.Position.X) <= 23.5f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Sources(Module))
        {
            hints.AddForbiddenZone(ShapeContains.Capsule(t.Position, t.Rotation, 2, 4));
            hints.AddForbiddenZone(ShapeContains.Capsule(t.Position, t.Rotation, 6, 4), WorldState.FutureTime(2));
        }
    }
}

class DeepClean(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly float[] CleaningTime = [11f, 12.5f, 15.5f, 17f, 18.5f];
    private const float CleanLinger = 2f; // estimate
    private readonly List<DirtPile> Cleanings = [];

    private record DirtPile(Actor Actor, DateTime CleaningPredicted, DateTime? CleanedAt)
    {
        public DateTime? CleanedAt = CleanedAt;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Cleanings.RemoveAll(c => c.CleanedAt is DateTime dt && dt.AddSeconds(CleanLinger) < WorldState.CurrentTime);

        return Cleanings.Select(p => new AOEInstance(new AOEShapeCircle(6), p.Actor.Position, default, p.CleaningPredicted))
            .Where(a => (a.Activation - WorldState.CurrentTime).TotalSeconds < 5);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DirtPile && Module.Enemies(OID.DirtPile).Count == 5)
            ScheduleAOEs();
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (modelState == 27 && animState1 == 1)
        {
            var c = Cleanings.MinBy(e => (e.Actor.Position - actor.Position).Length());
            c?.CleanedAt = WorldState.CurrentTime;
        }
    }

    private void ScheduleAOEs()
    {
        var dirtOrdered = Module.Enemies(OID.DirtPile).OrderBy(d => d.Position.Z).ToList();
        var brooms = Module.Enemies(OID.MagickedBroom).ToList();

        float DistanceToBroom(Actor dirt)
        {
            var closestBroom = brooms.MinBy(b => MathF.Abs(b.Position.Z - dirt.Position.Z))!;
            return MathF.Abs(closestBroom.Position.X - dirt.Position.X);
        }

        dirtOrdered.SortBy(DistanceToBroom);
        foreach (var (dirt, delay) in dirtOrdered.Zip(CleaningTime))
            Cleanings.Add(new(dirt, WorldState.FutureTime(delay), null));
    }
}

class SeekerOfSolitudeStates : StateMachineBuilder
{
    public SeekerOfSolitudeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeepClean>()
            .ActivateOnEnter<Sweep>()
            .ActivateOnEnter<DarkPulse>()
            .ActivateOnEnter<DarkWell>()
            .ActivateOnEnter<Tribulation>()
            .ActivateOnEnter<ImmortalAnathema>()
            .ActivateOnEnter<DarkShock>()
            .ActivateOnEnter<Shadowbolt>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || module.PrimaryActor.HPMP.CurHP == 1 && !module.PrimaryActor.IsTargetable;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9041)]
public class SeekerOfSolitude(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 187), new ArenaBoundsRect(20.5f, 14.5f));
