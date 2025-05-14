using BossMod.Modules.Stormblood.Foray;

namespace BossMod.Stormblood.Foray.BaldesionArsenal.AbsoluteVirtue;

public enum OID : uint
{
    Boss = 0x25DC, // R5.400, x1
    Helper = 0x2628, // R0.500, x18
    RelativeVirtue = 0x25DD, // R5.400, x3
    DarkAurora = 0x25E0, // R1.000, x0 (spawn during fight)
    BrightAurora = 0x25DF, // R1.000, x0 (spawn during fight)
    DarkHole = 0x1EAA49,
    BrightHole = 0x1EAA4A,
    AernsWynav = 0x25DE, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14532, // Boss->player, no cast, single-target
    Meteor = 14233, // Boss->self, 4.0s cast, range 60 circle
    Eidos = 14214, // Boss->self, 2.0s cast, single-target, changes aspect (light/dark)
    AstralRaysSmall = 14220, // Helper->self, 8.0s cast, range 8 circle
    AstralRaysBig = 14221, // Helper->self, 8.0s cast, range 8 circle
    UmbralRaysSmall = 14222, // Helper->self, 8.0s cast, range 8 circle
    UmbralRaysBig = 14223, // Helper->self, 8.0s cast, range 8 circle
    HostileAspect = 14219, // Boss->self, 8.0s cast, single-target
    MedusaJavelin = 14235, // Boss->self, 3.0s cast, range 60+R 90-degree cone
    BrightAurora = 14217, // Helper->self, 3.0s cast, range 30 width 100 rect
    DarkAurora = 14218, // Helper->self, 3.0s cast, range 30 width 100 rect
    AuroralWind = 14234, // Boss->players, 5.0s cast, range 5 circle
    TurbulentAether = 14224, // Boss->self, 3.0s cast, single-target
    BrightExplosion = 14225, // BrightAurora->self, no cast, range 6 circle
    DarkExplosion = 14226, // DarkAurora->self, no cast, range 6 circle
    ExplosiveImpulseClone = 14227, // RelativeVirtue->self, 5.0s cast, range 60 circle, falloff at 18 probably
    ExplosiveImpulse = 14228, // Boss->self, 5.0s cast, range 60 circle, falloff at 15 probably
    BrightAuroraClone = 14230, // Helper->self, 5.0s cast, range 30 width 100 rect
    DarkAuroraClone = 14231, // Helper->self, 5.0s cast, range 30 width 100 rect
    Explosion = 14676, // 25DE->self, 8.0s cast, range 60 circle
    MeteorEnrage = 14700, // Boss->self, 10.0s cast, range 60 circle
}

public enum SID : uint
{
    AstralEssence = 1710, // Boss->Helper/Boss, extra=0x0
    UmbralEssence = 1711, // Boss->Helper/Boss, extra=0x0
    AlteredStates = 1387, // none->Helper, extra=0x46
    UmbralCloak = 1713, // none->player, extra=0x0
    AstralCloak = 1712, // none->player, extra=0x0
}

public enum TetherID : uint
{
    DarkTether = 1, // DarkAurora->player
    BrightTether = 2, // BrightAurora->player
}

class Meteor(BossModule module) : Components.RaidwideCast(module, AID.Meteor);
class AuroralWind(BossModule module) : Components.BaitAwayCast(module, AID.AuroralWind, new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);
class MedusaJavelin(BossModule module) : Components.StandardAOEs(module, AID.MedusaJavelin, new AOEShapeCone(65, 45.Degrees()));
class AstralRays(BossModule module) : Components.StandardAOEs(module, AID.AstralRaysSmall, new AOEShapeCircle(8));
class UmbralRays(BossModule module) : Components.StandardAOEs(module, AID.UmbralRaysSmall, new AOEShapeCircle(8));
class AstralRaysBig(BossModule module) : Components.StandardAOEs(module, AID.AstralRaysBig, new AOEShapeCircle(15));
class UmbralRaysBig(BossModule module) : Components.StandardAOEs(module, AID.UmbralRaysBig, new AOEShapeCircle(15));
class ExplosiveImpulse(BossModule module) : Components.StandardAOEs(module, AID.ExplosiveImpulse, new AOEShapeCircle(18));
class ExplosiveImpulseClone(BossModule module) : Components.StandardAOEs(module, AID.ExplosiveImpulseClone, new AOEShapeCircle(18));

class Aurora(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(30, 50), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BrightAurora:
            case AID.BrightAuroraClone:
                if (caster.FindStatus(SID.AstralEssence) != null)
                    Casters.Add(caster);
                break;
            case AID.DarkAurora:
            case AID.DarkAuroraClone:
                if (caster.FindStatus(SID.UmbralEssence) != null)
                    Casters.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BrightAurora or AID.DarkAurora or AID.BrightAuroraClone or AID.DarkAuroraClone)
            Casters.Remove(caster);
    }
}

class Balls(BossModule module) : BossComponent(module)
{
    // balls gain Sprint status and tower disappears ~21.85s after tether

    enum Color
    {
        None,
        Dark,
        Light
    }

    private readonly List<(Actor Source, Actor Target, Color Color)> Tethers = [];
    private readonly List<Actor> Towers = [];

    private readonly Color[] TetherColors = Utils.MakeArray(PartyState.MaxPartySize, Color.None);

    private const float TowerRadius = 1.5f;
    private DateTime Deadline;

    private IEnumerable<(Actor Tower, Color Color)> AllTowers => Towers.Select(t => (t, t.OID == (uint)OID.BrightHole ? Color.Light : Color.Dark));
    private IEnumerable<Actor> SafeTowers(int pcSlot) => TetherColors[pcSlot] switch
    {
        Color.None => [],
        var c => AllTowers.Where(t => t.Color != c).Select(t => t.Tower)
    };

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.DarkTether or TetherID.BrightTether)
        {
            if (Deadline == default)
                Deadline = WorldState.FutureTime(21.85f);

            var actor = WorldState.Actors.Find(tether.Target);
            if (actor != null)
            {
                var color = tether.ID == (uint)TetherID.DarkTether ? Color.Dark : Color.Light;
                Tethers.Add((source, actor, color));
                if (Raid.TryFindSlot(actor, out var slot))
                    TetherColors[slot] = color;
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        Tethers.RemoveAll(t => t.Source == source);
        if (Raid.TryFindSlot(tether.Target, out var slot))
            TetherColors[slot] = default;

        if (Tethers.Count == 0)
            Deadline = default;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BrightHole or OID.DarkHole)
            Towers.Add(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID is OID.BrightHole or OID.DarkHole && state == 0x00040008)
            Towers.Remove(actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, tar, _) in Tethers)
        {
            Arena.AddLine(src.Position, tar.Position, ArenaColor.PlayerGeneric);
            Arena.ZoneCircle(src.Position, 1, ArenaColor.AOE);
        }

        var tetherColor = TetherColors[pcSlot];
        var tethered = tetherColor != Color.None;

        foreach (var (t, tcol) in AllTowers)
        {
            Arena.AddCircle(t.Position, TowerRadius, tethered && tcol != tetherColor ? ArenaColor.Safe : ArenaColor.Danger);
            foreach (var (_, ptar, _) in Tethers)
                if (ptar.Position.InCircle(t.Position, TowerRadius))
                {
                    if (ptar == pc)
                        Arena.AddCircle(ptar.Position, 6, ArenaColor.Danger);
                    else
                        Arena.ZoneCircle(ptar.Position, 6, ArenaColor.AOE);
                }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TetherColors[slot] != Color.None)
            hints.Add("Go to opposite color tower!", !SafeTowers(slot).Any(t => actor.Position.InCircle(t.Position, TowerRadius)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (TetherColors[slot] != Color.None)
        {
            var safeTowers = SafeTowers(slot).Select(t => ShapeContains.Donut(t.Position, TowerRadius, 100)).ToList();
            if (safeTowers.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(safeTowers), Deadline);
        }

        // don't go to the same tower as another baiter
        var otherBaits = Tethers.Where(t => t.Target != actor).Select(t => ShapeContains.Circle(t.Target.Position, 6)).ToList();
        if (otherBaits.Count > 0)
            hints.AddForbiddenZone(ShapeContains.Union(otherBaits), DateTime.MaxValue);
    }
}

class Wyvern(BossModule module) : Components.Adds(module, (uint)OID.AernsWynav, 1);
class MeteorEnrage(BossModule module) : Components.CastHint(module, AID.MeteorEnrage, "Enrage!", true);

class AbsoluteVirtueStates : StateMachineBuilder
{
    public AbsoluteVirtueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<AstralRays>()
            .ActivateOnEnter<AstralRaysBig>()
            .ActivateOnEnter<UmbralRays>()
            .ActivateOnEnter<UmbralRaysBig>()
            .ActivateOnEnter<MedusaJavelin>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<Aurora>()
            .ActivateOnEnter<Balls>()
            .ActivateOnEnter<ExplosiveImpulse>()
            .ActivateOnEnter<ExplosiveImpulseClone>()
            .ActivateOnEnter<Wyvern>()
            .ActivateOnEnter<MeteorEnrage>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7976)]
public class AbsoluteVirtue(WorldState ws, Actor primary) : BAModule(ws, primary, new(-175, 314), new ArenaBoundsCircle(30))
{
    // people like to early pull AV to be funny, so check if we have at least a BA low man run worth of people in the arena
    protected override bool CheckPull() => PrimaryActor.InCombat && WorldState.Actors.Where(p => p.Type == ActorType.Player).Count(a => Bounds.Contains(a.Position - Center)) > 6;
}
