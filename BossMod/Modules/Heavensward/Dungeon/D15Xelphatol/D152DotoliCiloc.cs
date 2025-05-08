namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D151DotoliCiloc;

public enum OID : uint
{
    Boss = 0x179F, // R1.980, x?
    Whirlwind = 0x17A0, // R1.000, x?
}

public enum AID : uint
{
    OnLow = 6606, // Boss->self, 4.0s cast, range 9+R 120-degree cone
    OnHigh = 6607, // Boss->self, 3.0s cast, range 50+R circle
    DarkWings = 32556, // Boss->player, no cast, range 6 circle
    Swiftfeather = 6609, // Boss->self, 3.0s cast, single-target
    Stormcoming = 32557, // Boss->location, 4.0s cast, range 6 circle
    TerribleFlurry = 6610, // _Gen_Whirlwind->self, no cast, range 6 circle
}

class Stormcoming(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.Stormcoming, m => m.Enemies(OID.Whirlwind).Where(w => w.EventState != 7), 0);
class Swiftfeather(BossModule module) : Components.GenericBaitAway(module, AID.Swiftfeather)
{
    // 2.3f delay
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(caster.TargetID)!, new AOEShapeCone(11, 60.Degrees()), Module.CastFinishAt(spell, 2.3f)));

        if ((AID)spell.Action.ID == AID.OnLow)
            CurrentBaits.Clear();
    }
}
class OnLow(BossModule module) : Components.StandardAOEs(module, AID.OnLow, new AOEShapeCone(11, 60.Degrees()));
class OnHigh(BossModule module) : Components.Knockback(module, AID.OnHigh)
{
    private readonly List<Actor> Casters = [];
    private static readonly Angle[] Walls = [default, 90.Degrees(), 180.Degrees(), 270.Degrees()];
    private BitMask BlockedWalls;
    private IEnumerable<Angle> UnblockedWalls => Walls.Where((_, i) => !BlockedWalls[i]);
    private const float WallHalfAngle = MathF.PI / 16;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, CheckWall(actor.Position) ? 19 - (c.Position - actor.Position).Length() : 30, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Whirlwind)
        {
            for (var i = 0; i < Walls.Length; i++)
            {
                if (Module.PrimaryActor.AngleTo(actor).AlmostEqual(Walls[i], WallHalfAngle))
                {
                    BlockedWalls.Set(i);
                    break;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.Whirlwind)
            BlockedWalls.Reset();
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => base.DestinationUnsafe(slot, actor, pos) || !UnblockedWalls.Any(w => Module.PrimaryActor.AngleTo(actor).AlmostEqual(w, WallHalfAngle));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var zones = UnblockedWalls.Select(w => ShapeContains.InvertedCone(Arena.Center, 30, w, WallHalfAngle.Radians())).ToList();
        hints.AddForbiddenZone(ShapeContains.Intersection(zones), Module.CastFinishAt(Casters[0].CastInfo));
    }

    private bool CheckWall(WPos pos) => Walls.Any(w => Angle.FromDirection(pos - Arena.Center).AlmostEqual(w, WallHalfAngle));
}
class Walls(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => [new AOEInstance(new AOEShapeDonut(20, 30), Arena.Center)];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        DrawWall(default);
        DrawWall(90.Degrees());
        DrawWall(180.Degrees());
        DrawWall(270.Degrees());
    }

    private void DrawWall(Angle angle)
    {
        var delta = MathF.PI / 16;
        Arena.PathArcTo(Arena.Center, 20, angle.Rad + delta, angle.Rad - delta);
        Arena.PathStroke(false, ArenaColor.Border);
    }
}

class DotoliCilocStates : StateMachineBuilder
{
    public DotoliCilocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OnLow>()
            .ActivateOnEnter<OnHigh>()
            .ActivateOnEnter<Walls>()
            .ActivateOnEnter<Swiftfeather>()
            .ActivateOnEnter<Stormcoming>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5269, Contributors = "xan")]
public class DotoliCiloc(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, new ArenaBoundsCircle(25))
{
    // position of boss casting On High (knockback from arena center)
    public static readonly WPos DefaultCenter = new(245.289f, 13.626f);
}
