namespace BossMod.RealmReborn.Dungeon.D08Qarn.D081Teratotaur;
// note: this boss' radar pops up at the start because it's close to the arena
// no mechanism to fix that yet

public enum OID : uint
{
    Boss = 0x477A, // R2.240, x1
    DungWespe = 0x477B, // R0.400, x0 (spawn during fight)
    Platform1 = 0x1E87E2, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform2 = 0x1E87E3, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform3 = 0x1E87E4, // x1, EventObj type; eventstate 0 if active, 7 if inactive
}

public enum AID : uint
{
    AutoAttackBoss = 870, // Boss->player, no cast, single-target
    Triclip = 42231, // Boss->player, 5.0s cast, single-target tankbuster
    Mow = 42232, // Boss->self, 2.5s cast, range 6+R 120-degree cone aoe
    FrightfulRoar = 42233, // Boss->self, 3.0s cast, range 6 circle aoe
    MortalRay = 42229, // Boss->self, 3.0s cast, raidwide doom debuff

    AutoAttackWespe = 871, // DungWespe->player, no cast, single-target
    FinalSting = 919, // DungWespe->player, 3.0s cast
}

public enum SID : uint
{
    Doom = 1970, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, //Player->self
}

class Triclip(BossModule module) : Components.SingleTargetCast(module, AID.Triclip, "Tankbuster");
class Mow(BossModule module) : Components.StandardAOEs(module, AID.Mow, new AOEShapeCone(8.24f, 60.Degrees()));
class FrightfulRoar(BossModule module) : Components.StandardAOEs(module, AID.FrightfulRoar, new AOEShapeCircle(6));

// TODO:
// A: priority stun to stop MR cast?
// auto-stun doesn't exist yet.
// B: second MR cast drops first pad after a sec or two; delay move?
// no, this is on a timer between casts; it only happens to align.
// would have to count time active & say "don't move if active/until inactive = say 2s or less."
// C: runs off pad as soon as Doom drops; looks weird, only cuts like 200ms of downtime.
// could delay to look more natural; maybe unnecessary-paranoiac.
class MortalRay(BossModule module) : BossComponent(module)
{
    private BitMask _dooms;
    private readonly Actor?[] _platforms = [null, null, null];

    // note: cleanse area seems slightly smaller than it looks visually.
    private static readonly AOEShapeCircle _platformShape = new(1);

    private Actor? ActivePlatform => _platforms.FirstOrDefault(a => a != null && a.EventState == 0);

    public override void Update()
    {
        _platforms[0] ??= Module.Enemies(OID.Platform1).FirstOrDefault();
        _platforms[1] ??= Module.Enemies(OID.Platform2).FirstOrDefault();
        _platforms[2] ??= Module.Enemies(OID.Platform3).FirstOrDefault();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_dooms[slot])
            hints.Add("Go to glowing platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_dooms[slot])
        {
            var target = ActivePlatform;
            if (target != null)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(target.Position, _platformShape.Radius), actor.FindStatus(SID.Doom)!.Value.ExpireAt);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_dooms[pcSlot])
            _platformShape.Draw(Arena, ActivePlatform, ArenaColor.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _dooms.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _dooms.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class MortalRayHint(BossModule module) : Components.CastInterruptHint(module, AID.MortalRay, canBeInterrupted: false, canBeStunned: true);

class D081TeratotaurStates : StateMachineBuilder
{
    public D081TeratotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Triclip>()
            .ActivateOnEnter<Mow>()
            .ActivateOnEnter<FrightfulRoar>()
            .ActivateOnEnter<MortalRay>()
            .ActivateOnEnter<MortalRayHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1567)]
public class D081Teratotaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-70, -60), new ArenaBoundsSquare(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.DungWespe => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    // TODO: draw pads as AOEShapeRect while inactive?
    // DrawArenaForeground seems preferred over DrawEnemies; bit inconsistent.
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.DungWespe))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
