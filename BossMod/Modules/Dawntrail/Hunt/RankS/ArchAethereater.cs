namespace BossMod.Dawntrail.Hunt.RankS.ArchAethereater;

public enum OID : uint
{
    Boss = 0x4544, // R9.600, x1
}

public enum AID : uint
{
    AutoAttack = 39517, // Boss->player, no cast, single-target
    AethermodynamicsIO = 39839, // Boss->self, 5.0s cast, range 40 circle, raidwide + stay/move debuffs (in -> out)
    AethermodynamicsOI = 39840, // Boss->self, 5.0s cast, range 40 circle, raidwide + stay/move debuffs (out -> in)
    AethermodynamicsROLI = 39511, // Boss->self, 5.0s cast, range 40 circle, raidwide + stay/move debuffs (right/out <-> left/in)
    AethermodynamicsLORI = 39512, // Boss->self, 5.0s cast, range 40 circle, raidwide + stay/move debuffs (left/out <-> right/in)
    SoullessStreamLI = 39507, // Boss->self, 5.0s cast, range 40 180-degree cone (left cleave -> in)
    SoullessStreamRO = 39508, // Boss->self, 5.0s cast, range 40 180-degree cone (right cleave -> out)
    SoullessStreamLO = 39509, // Boss->self, 5.0s cast, range 40 180-degree cone (left cleave -> out)
    SoullessStreamRI = 39510, // Boss->self, 5.0s cast, range 40 180-degree cone (right cleave -> in)
    FireFirst = 39829, // Boss->self, 5.0s cast, range 15 circle
    BlizzardFirst = 39830, // Boss->self, 5.0s cast, range 6-40 donut
    FireSecond = 39837, // Boss->self, 5.0s cast, range 15 circle
    BlizzardSecond = 39838, // Boss->self, 5.0s cast, range 6-40 donut
    FireCleaveFirstL = 39800, // Boss->self, 1.0s cast, range 15 circle
    BlizzardCleaveFirstL = 39801, // Boss->self, 1.0s cast, range 6-40 donut
    FireCleaveFirstR = 39802, // Boss->self, 1.0s cast, range 15 circle
    BlizzardCleaveFirstR = 39803, // Boss->self, 1.0s cast, range 6-40 donut
    FireCleaveSecond = 39513, // Boss->self, 1.0s cast, range 15 circle
    BlizzardCleaveSecond = 39514, // Boss->self, 1.0s cast, range 6-40 donut
    Obliterate = 39515, // Boss->players, 5.0s cast, range 6 circle stack
    Meltdown = 39516, // Boss->self, 3.0s cast, range 40 width 10 rect
    Reset = 19277, // Boss->self, no cast, single-target, visual (happens when boss resets?..)
}

public enum SID : uint
{
    Heatstroke = 4141, // Boss->player, extra=0x0
    ColdSweats = 4142, // Boss->player, extra=0x0
    Pyretic = 960, // none->player, extra=0x0
    FreezingUp = 2540, // none->player, extra=0x0
    DeepFreeze = 3519, // none->player, extra=0x0
}

public enum IconID : uint
{
    Obliterate = 62, // player
}

// note: for very first cast, we could predict aoe sequence in advance (not that it matters much...)
class AethermodynamicsIO(BossModule module) : Components.RaidwideCast(module, AID.AethermodynamicsIO, "Raidwide + stay/move debuffs");
class AethermodynamicsOI(BossModule module) : Components.RaidwideCast(module, AID.AethermodynamicsOI, "Raidwide + stay/move debuffs");
class AethermodynamicsROLI(BossModule module) : Components.RaidwideCast(module, AID.AethermodynamicsROLI, "Raidwide + stay/move debuffs");
class AethermodynamicsLORI(BossModule module) : Components.RaidwideCast(module, AID.AethermodynamicsLORI, "Raidwide + stay/move debuffs");

class AethermodynamicsStayMove(BossModule module) : Components.StayMove(module, 5)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var state = StateForStatus(status);
        if (state.Requirement != Requirement.None)
            SetState(Raid.FindSlot(actor.InstanceID), state);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var state = StateForStatus(status);
        if (state.Requirement != Requirement.None)
            ClearState(Raid.FindSlot(actor.InstanceID), state.Priority);
    }

    private PlayerState StateForStatus(ActorStatus status) => (SID)status.ID switch
    {
        SID.Heatstroke => new(Requirement.Stay, status.ExpireAt),
        SID.Pyretic => new(Requirement.Stay, WorldState.CurrentTime, 1),
        SID.ColdSweats => new(Requirement.Move, status.ExpireAt.AddSeconds(1)),
        SID.FreezingUp => new(Requirement.Move, WorldState.CurrentTime, 1),
        _ => default
    };
}

// note: we can predict full sequence accurately on first cast, but we don't really care, since the casts are long
class FireBlizzardSoullessStream(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(15);
    private static readonly AOEShapeDonut _shapeIn = new(6, 40);
    private static readonly AOEShapeCone _shapeCleave = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireFirst:
            case AID.FireSecond:
                _aoes.Add(new(_shapeOut, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.BlizzardFirst:
            case AID.BlizzardSecond:
                _aoes.Add(new(_shapeIn, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.SoullessStreamLI:
            case AID.SoullessStreamRI:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeIn, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 2.6f)));
                break;
            case AID.SoullessStreamLO:
            case AID.SoullessStreamRO:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeOut, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 2.6f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FireFirst or AID.BlizzardFirst or AID.FireSecond or AID.BlizzardSecond or AID.SoullessStreamLI or AID.SoullessStreamRO or AID.SoullessStreamLO or AID.SoullessStreamRI
            or AID.FireCleaveFirstL or AID.BlizzardCleaveFirstL or AID.FireCleaveFirstR or AID.BlizzardCleaveFirstR or AID.FireCleaveSecond or AID.BlizzardCleaveSecond && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class Obliterate(BossModule module) : Components.StackWithCastTargets(module, AID.Obliterate, 6, 4);
class Meltdown(BossModule module) : Components.StandardAOEs(module, AID.Meltdown, new AOEShapeRect(40, 5));

class ArchAethereaterStates : StateMachineBuilder
{
    public ArchAethereaterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AethermodynamicsIO>()
            .ActivateOnEnter<AethermodynamicsOI>()
            .ActivateOnEnter<AethermodynamicsROLI>()
            .ActivateOnEnter<AethermodynamicsLORI>()
            .ActivateOnEnter<AethermodynamicsStayMove>()
            .ActivateOnEnter<FireBlizzardSoullessStream>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Meltdown>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13406)]
public class ArchAethereater(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
