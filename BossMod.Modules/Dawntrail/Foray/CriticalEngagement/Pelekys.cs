namespace BossMod.Dawntrail.Foray.CriticalEngagement.Pelekys;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x40, Helper type
    Boss = 0x4BCA, // R7.000, x1
    Pelekys = 0x4BCC, // R0.500, x1
    Unk = 0x4BCD, // R2.000, x0 (spawn during fight)

    Venom = 0x1EBFED
}

public enum AID : uint
{
    DeathWall = 47214, // 4BCC->self, no cast, range 25-30 donut
    AutoAttack = 50850, // Boss->player, no cast, single-target
    AcridRainCast = 47231, // Boss->self, 5.0s cast, single-target
    AcridRain = 47232, // Helper->self, no cast, ???
    CentralGardeningCast = 47218, // Boss->self, 5.0s cast, single-target
    CentralGardening = 47220, // Helper->self, 6.0s cast, range 52 width 10 rect
    SideGardeningCast = 47219, // Boss->self, 5.0s cast, single-target
    SideGardening1 = 47221, // Helper->self, 6.0s cast, range 26 180-degree cone
    SideGardening2 = 49729, // Helper->self, 6.0s cast, range 26 180-degree cone
    NoxiousNectarCast = 49730, // Boss->self, 3.0s cast, single-target
    NoxiousNectar1 = 49885, // Boss->self, no cast, single-target
    NoxiousNectar2 = 47215, // Boss->self, no cast, single-target
    VenomPuddleCast = 47216, // Helper->self, 4.8s cast, range 2 circle
    VenomPuddleInstant = 47217, // Helper->self, no cast, range 2 circle
    PollenLureCast = 47222, // Boss->self, 4.0s cast, single-target
    Devour = 47223, // Boss->self, 7.0s cast, range 10 circle
    PoisonHeartCast = 47229, // Boss->self, 4.0s cast, single-target
    PoisonHeart = 47230, // Helper->location, 3.0s cast, range 5 circle
    VenomMistBoss1 = 47225, // Boss->self, 5.0s cast, single-target
    VenomMistBoss2 = 47226, // Boss->self, 5.0s cast, single-target
    VenomMistBoss3 = 47227, // Boss->self, 5.0s cast, single-target
    VenomMist1 = 50547, // Helper->self, 6.0s cast, range 30 90-degree cone
    VenomMist2 = 50549, // Helper->self, 6.0s cast, range 30 90-degree cone
    VenomMist3 = 50548, // Helper->self, 6.0s cast, range 30 90-degree cone
    VenomMist4 = 47228, // Helper->self, 6.0s cast, range 30 90-degree cone
}

public enum SID : uint
{
    Toxicosis = 4379, // Helper->player, extra=0x0
    Poison = 5425, // Helper->player, extra=0x0
    UnkBoss = 2552, // none->Boss, extra=0x3F2/0x3F3
    UnkAdds = 2056, // none->4BCD, extra=0x3C2
}

class AcridRain(BossModule module) : Components.RaidwideCastDelay(module, AID.AcridRainCast, AID.AcridRain, 1.2f);
class CentralGardening(BossModule module) : Components.StandardAOEs(module, AID.CentralGardening, new AOEShapeRect(52, 5));
class SideGardening(BossModule module) : Components.GroupedAOEs(module, [AID.SideGardening1, AID.SideGardening2], new AOEShapeCone(26, 90.Degrees()));

class VenomPuddle(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<Puddle> _puddles = [];

    class Puddle
    {
        public required Actor Actor;
        public int NumCasts;
        // telegraphed puddle cast
        public DateTime Spawn;
        // eventobj animation
        public DateTime GrowStart;
        // most recent instant cast on this puddle
        public DateTime PrevCast;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // max size hint
        foreach (var p in _puddles.Take(2))
            yield return new(new AOEShapeCircle(22), p.Actor.Position, p.Actor.Rotation, p.Spawn.AddSeconds(10));

        // explicit hint for next venom cast
        foreach (var p in _puddles)
        {
            if (p.GrowStart == default)
                yield return new(new AOEShapeCircle(2), p.Actor.Position, p.Actor.Rotation, p.Spawn, ArenaColor.Danger);
            else
            {
                var nextActive = (p.NumCasts > 0 ? p.PrevCast : p.GrowStart).AddSeconds(1.1f);

                yield return new(new AOEShapeCircle(2 + 2.5f * (p.NumCasts + 1)), p.Actor.Position, p.Actor.Rotation, nextActive, ArenaColor.Danger);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VenomPuddleCast)
            _puddles.Add(new() { Actor = caster, Spawn = Module.CastFinishAt(spell) });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.VenomPuddleInstant)
        {
            var ix = _puddles.FindIndex(p => p.Actor.Position.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
            {
                var puddle = _puddles[ix];
                puddle.PrevCast = WorldState.CurrentTime;
                if (++puddle.NumCasts >= 8)
                    _puddles.RemoveAt(ix);
            }
            else
                ReportError($"No puddle near {caster.Position}");
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.Venom)
        {
            if (state == 0x00040008)
            {
                var ix = _puddles.FindIndex(p => p.Actor.Position.AlmostEqual(actor.Position, 1));
                if (ix >= 0)
                    _puddles[ix].GrowStart = WorldState.CurrentTime;
            }
        }
    }
}

class Devour(BossModule module) : Components.StandardAOEs(module, AID.Devour, 10);
class PoisonHeart(BossModule module) : Components.StandardAOEs(module, AID.PoisonHeart, 5);
class VenomMist(BossModule module) : Components.GroupedAOEs(module, [AID.VenomMist1, AID.VenomMist2, AID.VenomMist3, AID.VenomMist4], new AOEShapeCone(30, 45.Degrees()));

class PelekysStates : StateMachineBuilder
{
    public PelekysStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcridRain>()
            .ActivateOnEnter<CentralGardening>()
            .ActivateOnEnter<SideGardening>()
            .ActivateOnEnter<VenomPuddle>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<PoisonHeart>()
            .ActivateOnEnter<VenomMist>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14747)]
public class Pelekys(WorldState ws, Actor primary) : CEModule(ws, primary, new(-870, -560), new ArenaBoundsCircle(25));

