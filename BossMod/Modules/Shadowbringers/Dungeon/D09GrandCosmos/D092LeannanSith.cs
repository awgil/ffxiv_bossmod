namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D092LeannanSith;

public enum OID : uint
{
    Boss = 0x2C04, // R2.4
    EnslavedLove = 0x2C06, // R3.6
    LoversRing = 0x2C05, // R2.04
    LeannanSeed1 = 0x1EAE9E, // R0.5
    LeannanSeed2 = 0x1EAE9F, // R0.5
    LeannanSeed3 = 0x1EAEA0, // R0.5
    LeannanSeed4 = 0x1EAEA1, // R0.5
    DirtTiles = 0x1EAEC6, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // LoversRing->player, no cast, single-target
    Teleport = 18207, // Boss->location, no cast, ???

    StormOfColor = 18203, // Boss->player, 4.0s cast, single-target
    OdeToLostLove = 18204, // Boss->self, 4.0s cast, range 60 circle
    DirectSeeding = 18205, // Boss->self, 4.0s cast, single-target
    GardenersHymn = 18206, // Boss->self, 14.0s cast, single-target

    ToxicSpout = 18208, // LoversRing->self, 8.0s cast, range 60 circle
    OdeToFarWinds = 18210, // Boss->self, 3.0s cast, single-target
    FarWind = 18211, // Helper->location, 5.0s cast, range 8 circle
    FarWindSpread = 18212, // Helper->player, 5.0s cast, range 5 circle
    OdeToFallenPetals = 18768, // Boss->self, 4.0s cast, range 5-60 donut
    IrefulWind = 18209 // 2C06->self, 13.0s cast, range 40+R width 40 rect, knockback 10, source forward
}

public enum SID : uint
{
    Transporting = 404 // none->player, extra=0x15
}

class OdeToLostLove(BossModule module) : Components.RaidwideCast(module, AID.OdeToLostLove);
class StormOfColor(BossModule module) : Components.SingleTargetCast(module, AID.StormOfColor);
class FarWindSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.FarWindSpread, 5);
class FarWind(BossModule module) : Components.StandardAOEs(module, AID.FarWind, 8);
class OdeToFallenPetals(BossModule module) : Components.StandardAOEs(module, AID.OdeToFallenPetals, new AOEShapeDonut(5, 60));
class IrefulWind(BossModule module) : Components.KnockbackFromCastTarget(module, AID.IrefulWind, 10, kind: Kind.DirForward, stopAtWall: true);

class DirectSeeding(BossModule module) : BossComponent(module)
{
    private IrefulWind? Wind;
    private static readonly WDir[] Tileset = [
        new(-5, -15), new(5, -15), new(-15, -5), new(15, -5),
        new(-15, 5), new(5, 5), new(-5, 15), new(15, 15)
    ];
    private Angle? CurrentTileset;
    private IEnumerable<Actor> Seeds => WorldState.Actors.Where(x => (OID)x.OID is OID.LeannanSeed1 or OID.LeannanSeed2 or OID.LeannanSeed3 or OID.LeannanSeed4);
    private IEnumerable<WPos> TileCenters => CurrentTileset == null ? [] : Tileset.Select(t => t.Rotate(CurrentTileset.Value) + Arena.Center);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x0B)
            return;

        CurrentTileset = state switch
        {
            0x00020001 => default(Angle),
            0x00200010 => 180.Degrees(),
            0x01000080 => 90.Degrees(),
            0x08000400 => 270.Degrees(),
            _ => null
        };
    }

    public override void Update()
    {
        Wind ??= Module.FindComponent<IrefulWind>();
    }

    private WDir WindOffset => Wind?.Casters.FirstOrDefault() is Actor helper ? helper.Rotation.ToDirection() * 10 : default;

    private IEnumerable<Actor> GetDangerSeeds()
    {
        var centers = TileCenters.ToList();
        if (centers.Count == 0)
            return [];

        var off = WindOffset;

        return Seeds.Where(s =>
        {
            var projected = Module.Arena.ClampToBounds(s.Position + off);
            return centers.Any(c => projected.AlmostEqual(c, 5));
        });
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (GetDangerSeeds().Any())
            hints.Add("Move seeds!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(SID.Transporting) == null)
            hints.InteractWithTarget = GetDangerSeeds().MinBy(actor.DistanceToHitbox);
        else
        {
            var off = WindOffset;
            List<Func<WPos, bool>> tiledist = [];
            foreach (var t in TileCenters)
            {
                tiledist.Add(ShapeContains.Rect(t - off, default(Angle), 5, 5, 5));
                // tile is at edge of arena; seed can't be pushed out of it, it will just hit the wall
                if (!Module.Arena.InBounds(t + off))
                    tiledist.Add(ShapeContains.Rect(t, default(Angle), 5, 5, 5));
            }
            var zone = ShapeContains.Union(tiledist);

            if (!zone(actor.Position))
            {
                // normally the position of the seed we're carrying will lag behind our actual position in accordance with standard server latency
                // jumping forces the server to acknowledge our current position (i think???) so we jump as soon as we enter a safe tile and then drop the seed
                hints.WantJump = true;
                if (actor.PosRot.Y > -11)
                    hints.StatusesToCancel.Add(((uint)SID.Transporting, default));
            }
            hints.AddForbiddenZone(zone, WorldState.FutureTime(5));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (CurrentTileset == null)
            return;

        foreach (var t in TileCenters)
            Arena.ZoneRect(t, default(Angle), 5, 5, 5, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var danger = GetDangerSeeds();
        Arena.Actors(danger, ArenaColor.Danger);
        Arena.Actors(Seeds.Except(danger), ArenaColor.PlayerGeneric);
    }
}

class LeannanSithStates : StateMachineBuilder
{
    public LeannanSithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OdeToLostLove>()
            .ActivateOnEnter<StormOfColor>()
            .ActivateOnEnter<FarWindSpread>()
            .ActivateOnEnter<FarWind>()
            .ActivateOnEnter<OdeToFallenPetals>()
            .ActivateOnEnter<IrefulWind>()
            .ActivateOnEnter<DirectSeeding>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9044)]
public class LeannanSith(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -60), new ArenaBoundsSquare(19.5f));
