namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D132MagitekCore;

public enum OID : uint
{
    Boss = 0x31AC, // R2.300, x1
    MagitekMissile = 0x31B2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    StableCannon = 23700, // Helper->self, no cast, range 60 width 10 rect
    DefensiveReaction = 23710, // Boss->self, 5.0s cast, range 60 circle
    Aethershot = 23708, // TelotekSkyArmor->location, 4.0s cast, range 6 circle
    GroundToGroundBallistic = 23703, // Helper->location, 5.0s cast, range 40 circle
    Exhaust = 23705, // MarkIITelotekColossus->self, 4.0s cast, range 40 width 7 rect
    ExplosiveForce = 23704, // MagitekMissile->player, no cast, single-target
    W2TonzeMagitekMissile = 23701, // Helper->location, 5.0s cast, range 12 circle
}

class DefensiveReaction(BossModule module) : Components.RaidwideCast(module, AID.DefensiveReaction);
class Aethershot(BossModule module) : Components.StandardAOEs(module, AID.Aethershot, 6);
class Exhaust(BossModule module) : Components.StandardAOEs(module, AID.Exhaust, new AOEShapeRect(40, 3.5f));

class C2TonzeMagitekMissile(BossModule module) : Components.StandardAOEs(module, AID.W2TonzeMagitekMissile, 12);

class StableCannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos[] Cannons = [new(-185, 28.3f), new(-175, 28.3f), new(-165, 28.3f)];

    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 8 and <= 10)
        {
            switch (state)
            {
                case 0x00200010:
                    aoes.Add(new AOEInstance(new AOEShapeRect(60, 5), Cannons[index - 8], default, WorldState.FutureTime(12.6f)));
                    break;
                case 0x00040004:
                    aoes.Clear();
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StableCannon)
            aoes.RemoveAt(0);
    }
}

class GroundToGroundBallistic(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GroundToGroundBallistic, 10, stopAtWall: true)
{
    private StableCannon? cannons;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        cannons ??= Module.FindComponent<StableCannon>();

        if (cannons == null)
            return false;

        return cannons.ActiveAOEs(slot, actor).Any(e => e.Check(pos));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count == 0)
            return;

        var aoes = cannons?.ActiveAOEs(slot, actor).ToList();
        if (aoes == null)
            return;

        var source = Casters[0].CastInfo!.LocXZ;
        hints.AddForbiddenZone(p =>
        {
            var dist = (p - source).Normalized();
            var proj = Arena.ClampToBounds(p + dist * 10);
            return aoes.Any(e => e.Check(proj));
        }, Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class Launchpad(BossModule module) : BossComponent(module)
{
    private bool active;

    private static readonly WPos Position = new(-175, 30);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0D)
        {
            switch (state)
            {
                case 0x00020001:
                    active = true;
                    Arena.Center = MagitekCore.CombinedCenter;
                    Arena.Bounds = MagitekCore.CombinedBounds;
                    break;
                case 0x00080004:
                    active = false;
                    Arena.Center = MagitekCore.DefaultCenter;
                    Arena.Bounds = MagitekCore.DefaultBounds;
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active && actor.PosRot.Y < -18)
            hints.GoalZones.Add(p => 15 - (p - Position).Length());
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && actor.PosRot.Y < -18)
            hints.Add("Go to launchpad!", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (active && pc.PosRot.Y < -18)
            Arena.ZoneCircle(Position, 2, ArenaColor.SafeFromAOE);
    }
}

class MagitekMissile(BossModule module) : BossComponent(module)
{
    private const float Radius = 1.5f;
    private readonly List<Actor> Missiles = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.MagitekMissile)
            Missiles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        Missiles.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosiveForce)
            Missiles.Remove(caster);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var m in Missiles)
            Arena.ZoneCircle(m.Position, Radius, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var m in Missiles)
            hints.AddForbiddenZone(ShapeContains.Capsule(m.Position, m.Rotation, 7, Radius), WorldState.FutureTime(1.5f));
    }
}

class MagitekCoreStates : StateMachineBuilder
{
    public MagitekCoreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StableCannon>()
            .ActivateOnEnter<GroundToGroundBallistic>()
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<C2TonzeMagitekMissile>()
            .ActivateOnEnter<DefensiveReaction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10076)]
public class MagitekCore(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultBounds)
{
    public static readonly WPos DefaultCenter = new(-175, 43);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(14.6f);

    private static readonly (WPos, ArenaBoundsCustom) DoubleBounds = MakeCombined();
    public static readonly WPos CombinedCenter = DoubleBounds.Item1;
    public static readonly ArenaBounds CombinedBounds = DoubleBounds.Item2;

    public static (WPos, ArenaBoundsCustom) MakeCombined()
    {
        var ground = CurveApprox.Rect(new WDir(0, 13.75f), new WDir(14.6f, 0), new WDir(0, 14.6f));
        var platform = CurveApprox.Rect(new WDir(0, -20.75f), new WDir(7.5f, 0), new WDir(0, 7.5f));
        var clipper = new PolygonClipper();
        return (new(-175, 29.25f), new(29, clipper.Union(new(ground), new(platform))));
    }

    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
