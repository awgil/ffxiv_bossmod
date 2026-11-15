namespace BossMod.Stormblood.Alliance.A24Yiazmat;

public enum OID : uint
{
    Boss = 0x2253, // R10.000, x1
    Helper = 0x18D6, // R0.500, x24, Helper type
    WindAzer = 0x2255, // R0.800, x0 (spawn during fight)
    Archaeodemon = 0x2257, // R1.950, x0 (spawn during fight)
    Unk1 = 0x2256, // R1.000, x0 (spawn during fight)
    Cyclone = 0x2259, // R1.000, x0 (spawn during fight)
    HeartOfTheDragon = 0x2254, // R3.000, x0 (spawn during fight), Part type

    MagneticLysis = 0x1EA961, // minus field in forward direction
}

public enum AID : uint
{
    AutoAttackBoss = 870, // Boss->player, no cast, single-target
    RakeBusterVisual = 11302, // Boss->player, 4.0s cast, single-target
    RakeBuster = 11598, // 18D6->player, 4.5s cast, range 5 circle
    GustFront = 11300, // 18D6->location, 3.5s cast, range 8 circle
    StoneBreath = 11305, // Boss->self, 7.0s cast, range 60+R 120?-degree cone
    DustStormVisual = 11318, // Boss->self, 3.5s cast, single-target
    DustStorm = 11317, // 18D6->location, no cast, range 30 circle
    WhiteBreath = 11313, // Boss->self, 5.0s cast, range 9?-60 donut
    MagneticLysis = 11306, // Boss->location, 3.5s cast, range 30 circle
    MagneticGenesisVisual = 11307, // 18D6->self, no cast, single-target
    MagneticGenesis1 = 11323, // 18D6->self, no cast, range 0 ???
    MagneticGenesis2 = 11322, // 18D6->self, no cast, range 0 ???
    RakeLineCast = 11324, // Boss->location, 4.0s cast, range 10 circle, visual only
    RakeLineFirst = 11826, // 18D6->self, 4.5s cast, range 10 circle
    RakeJump1 = 11303, // Boss->location, no cast, range 5 circle, visual only
    RakeJump2 = 11304, // Boss->location, no cast, range 5 circle, visual only
    RakeLineRest = 11599, // 18D6->location, no cast, range 10 circle
    Summon = 11319, // Boss->self, 3.0s cast, single-target
    AncientAero = 11320, // 2255->self, 2.5s cast, range 40+R width 6 rect
    DeathStrikeVisual = 11316, // Boss->self, 5.0s cast, single-target
    DeathStrike = 11315, // 18D6->players, no cast, range 5 circle
    CycloneRaidwideCast = 11299, // Boss->self, 7.0s cast, range 30 circle
    CycloneRaidwide = 11481, // 18D6->player, no cast, single-target
    DemonAutos = 872, // 2257->player, no cast, single-target
    FaceOff = 4488, // 2257->player, no cast, single-target
    Turbulence = 11312, // 18D6->self, no cast, range 4 circle
    UnholyDarkness = 9843, // 2257->location, 3.5s cast, range 8 circle
    Karma = 9842, // 2257->self, 3.0s cast, range 30 90-degree cone
    SolarStorm = 11308, // Boss->self, 5.0s cast, range 30 circle
    GrowingThreat = 11314, // Boss->self, 6.0s cast, single-target
    Unk1 = 11614, // 18D6->self, no cast, single-target
    Explosion = 11321, // _Gen_WindAzer->self, 5.0s cast, range 80 circle
}

public enum SID : uint
{
    MagneticLysisPlus = 1550, // none->player, extra=0x0
    MagneticLysisMinus = 1551, // none->player, extra=0x0
    MagneticLevitation = 1552, // none->player, extra=0x64, extra damage from dust storm, instakill from Cyclone
    Petrification = 610, // Boss->player, extra=0x0
    VulnerabilityDown = 350, // none->2257, extra=0x0
    BorneHeart = 1554, // none->Boss, extra=0x0
    Heartless = 1555, // none->Boss, extra=0x0
    DamageUp = 290, // Boss->18D6/Boss, extra=0x0
    Threatened = 1556, // Boss->Boss, extra=0x64
}

public enum IconID : uint
{
    Tankbuster = 415, // player->self
    TurnRight = 133, // Boss->self
    TurnLeft = 134, // Boss->self
    Stack = 62, // player->self
}

public enum TetherID : uint
{
    Wind = 4, // 2255->player
    Demon = 1, // 2257->2257
}

class Rake(BossModule module) : Components.BaitAwayCast(module, AID.RakeBuster, new AOEShapeCircle(5), true);
class GustFront(BossModule module) : Components.StandardAOEs(module, AID.GustFront, 8);
class StoneBreath(BossModule module) : Components.StandardAOEs(module, AID.StoneBreath, new AOEShapeCone(60, 60.Degrees()));
class DustStorm(BossModule module) : Components.RaidwideCastDelay(module, AID.DustStormVisual, AID.DustStorm, 0.9f);
class WhiteBreath(BossModule module) : Components.StandardAOEs(module, AID.WhiteBreath, new AOEShapeDonut(9, 60));
class MagneticLysis(BossModule module) : Components.RaidwideCast(module, AID.MagneticLysis);
class Magnetism(BossModule module) : Components.GenericAOEs(module, AID.DustStorm)
{
    DateTime? _deadlineCast;
    DateTime? _deadlineStatus;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadlineNext = _deadlineCast ?? _deadlineStatus;
        if (deadlineNext is not { } deadline)
            yield break;

        if (deadline > WorldState.FutureTime(8))
            yield break;

        var sign = GetSign(actor);

        if (sign == 0)
            yield break;

        if (Module.Enemies(0x1EA961).FirstOrDefault() is not { } field)
            yield break;

        var danger = sign > 0 ? 180.Degrees() : default;
        yield return new(new AOEShapeCone(60, 90.Degrees()), field.Position, field.Rotation + danger, deadline);
    }

    int GetSign(Actor actor)
    {
        foreach (var stat in actor.Statuses)
        {
            switch ((SID)stat.ID)
            {
                case SID.MagneticLysisPlus:
                    return 1;
                case SID.MagneticLysisMinus:
                    return -1;
            }
        }

        return 0;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DustStormVisual)
            _deadlineCast = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _deadlineCast = null;

        if ((AID)spell.Action.ID is AID.MagneticGenesis1 or AID.MagneticGenesis2)
            _deadlineStatus = null;

        if ((AID)spell.Action.ID == AID.MagneticLysis)
            _deadlineStatus = WorldState.FutureTime(27.8f);
    }
}

class WindAzer(BossModule module) : Components.Adds(module, (uint)OID.WindAzer, forbidDots: true);
class Explosion(BossModule module) : Components.CastHint(module, AID.Explosion, "Kill sprite before enrage!", true);
class AncientAero(BossModule module) : Components.StandardAOEs(module, AID.AncientAero, new AOEShapeRect(40.8f, 3));
class DeathStrike(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.DeathStrike, 5, 5.1f, 6);
class Cyclone(BossModule module) : Components.RaidwideCastDelay(module, AID.CycloneRaidwideCast, AID.CycloneRaidwide, 3.4f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Activation >= WorldState.CurrentTime && actor.Statuses.Any(s => (SID)s.ID is SID.MagneticLysisMinus or SID.MagneticLysisPlus))
            hints.Add("See ya later, nerd!", false);
    }
}
class Archaeodemon(BossModule module) : Components.Adds(module, (uint)OID.Archaeodemon)
{
    private readonly List<(Actor From, Actor To)> _tethered = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == ObjectID))
            target.Priority = target.Actor.FindStatus(SID.VulnerabilityDown) != null ? AIHints.Enemy.PriorityPointless : 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var target in WorldState.Actors.Where(t => t.OID == ObjectID && t.TargetID == actor.InstanceID && t.FindStatus(SID.VulnerabilityDown) != null))
            hints.Add("Separate adds!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (f, t) in _tethered)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(f.Position, t.Position, 0xFF000000, 2);
            Arena.AddLine(f.Position, t.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Demon && WorldState.Actors.Find(tether.Target) is { } target)
            _tethered.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var tt = tether.Target;
        if ((TetherID)tether.ID == TetherID.Demon)
            _tethered.RemoveAll(t => t.From.InstanceID == source.InstanceID && t.To.InstanceID == tt);
    }
}
class UnholyDarkness(BossModule module) : Components.StandardAOEs(module, AID.UnholyDarkness, 8);
class Karma(BossModule module) : Components.StandardAOEs(module, AID.Karma, new AOEShapeCone(30, 45.Degrees()));
class Turbulence(BossModule module) : Components.Voidzone(module, 4, OID.Cyclone, moveHintLength: 10);
class HeartOfTheDragon(BossModule module) : Components.Adds(module, (uint)OID.HeartOfTheDragon);
class SolarStorm(BossModule module) : Components.RaidwideCast(module, AID.SolarStorm);

class A24YiazmatStates : StateMachineBuilder
{
    public A24YiazmatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rake>()
            .ActivateOnEnter<GustFront>()
            .ActivateOnEnter<StoneBreath>()
            .ActivateOnEnter<DustStorm>()
            .ActivateOnEnter<WhiteBreath>()
            .ActivateOnEnter<MagneticLysis>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<RakeLine>()
            .ActivateOnEnter<WindAzer>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<DeathStrike>()
            .ActivateOnEnter<Cyclone>()
            .ActivateOnEnter<Archaeodemon>()
            .ActivateOnEnter<UnholyDarkness>()
            .ActivateOnEnter<Karma>()
            .ActivateOnEnter<Turbulence>()
            .ActivateOnEnter<HeartOfTheDragon>()
            .ActivateOnEnter<SolarStorm>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7070)]
public class A24Yiazmat(ModuleInit init) : BossModule(init, new(800, -400), new ArenaBoundsCircle(29.5f));

