namespace BossMod.RealmReborn.Dungeon.D08Qarn.D081Teratotaur;

public enum OID : uint
{
    Boss = 0x6D9, // x1
    DungWespe = 0x6DA, // spawn during fight
    Platform1 = 0x1E87E2, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform2 = 0x1E87E3, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform3 = 0x1E87E4, // x1, EventObj type; eventstate 0 if active, 7 if inactive
}

public enum AID : uint
{
    AutoAttackBoss = 870, // Boss->player, no cast
    Triclip = 1414, // Boss->self, no cast, range 5.25 width 4 rect cleave
    Mow = 1413, // Boss->self, 2.5s cast, range 8.25 120-degree cone aoe
    FrightfulRoar = 933, // Boss->self, 3.0s cast, range 8.25 aoe
    MortalRay = 934, // Boss->self, 1.5s cast, raidwide doom debuff

    AutoAttackWespe = 871, // DungWespe->player, no cast
    FinalSting = 919, // DungWespe->player, 3.0s cast
}

public enum SID : uint
{
    Doom = 210, // Boss->player, extra=0x0
}

class Triclip(BossModule module) : Components.Cleave(module, AID.Triclip, new AOEShapeRect(5.25f, 2));
class Mow(BossModule module) : Components.StandardAOEs(module, AID.Mow, new AOEShapeCone(8.25f, 60.Degrees()));
class FrightfulRoar(BossModule module) : Components.StandardAOEs(module, AID.FrightfulRoar, new AOEShapeCircle(8.25f));

class MortalRay(BossModule module) : BossComponent(module)
{
    private BitMask _dooms;
    private readonly Actor?[] _platforms = [null, null, null];

    private static readonly AOEShapeCircle _platformShape = new(2);

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

class D081TeratotaurStates : StateMachineBuilder
{
    public D081TeratotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Triclip>()
            .ActivateOnEnter<Mow>()
            .ActivateOnEnter<FrightfulRoar>()
            .ActivateOnEnter<MortalRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1567)]
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
}
