namespace BossMod.Dawntrail.Dungeon.D07StrayboroughDeadwalk.D071HisRoyalHeadnessLeonoggI;

public enum OID : uint
{
    Boss = 0x4183, // R3.600, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    LittleLadyNogginette = 0x41BD, // R1.000, x0 (spawn during fight)
    LittleLordNoggington = 0x41BB, // R1.000, x0 (spawn during fight)
    NobleNoggin = 0x4205, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    MaliciousMist = 36529, // Boss->self, 5.0s cast, range 50 circle, raidwide + arena shrink
    FallingNightmareVisual = 36526, // Boss->self, 3.0s cast, single-target, visual
    FallingNightmare1 = 36532, // NobleNoggin->self, 2.0s cast, range 2 circle
    FallingNightmare2 = 36536, // NobleNoggin->self, 1.0s cast, range 2 circle
    MorbidFascination = 36528, // Boss->self, no cast, single-target
    TeamSpirit = 36527, // Boss->self, 3.0s cast, single-target, visual (spawn dolls)
    SpiritedChargeVisual = 36598, // Boss->self, 3.0s cast, single-target, visual
    SpiritedChargeStart = 36533, // doll->self, no cast, single-target
    SpiritedCharge = 36534, // Helper->self, no cast, range 2 width 1 rect
    Overattachment = 36535, // doll->player, no cast, single-target
    EvilSchemeVisual = 39682, // Boss->self, 6.0s cast, single-target, visual (exaflare)
    EvilSchemeFirst = 39683, // Helper->self, 6.0s cast, range 4 circle
    EvilSchemeRest = 39684, // Helper->self, no cast, range 4 circle
    LoomingNightmareVisual = 39685, // Boss->self, 5.0s cast, single-target, visual (chase)
    LoomingNightmareFirst = 39686, // Helper->self, 2.0s cast, range 4 circle
    LoomingNightmareRest = 39687, // Helper->self, no cast, range 4 circle
    ScreamVisual1 = 36530, // Boss->self, 5.0s cast, single-target, visual
    ScreamVisual2 = 36541, // Boss->self, no cast, single-target, visual
    Scream = 36531, // Helper->self, 5.0s cast, range 20 60-degree cone
}

public enum IconID : uint
{
    LoomingNightmare = 197, // player
}

class MaliciousMist(BossModule module) : Components.RaidwideCast(module, AID.MaliciousMist);

class MistDeathwall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MaliciousMist && Arena.Bounds.Radius > 14)
            _aoe = new(new AOEShapeDonut(14, 20), Arena.Center, default, Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = D071HisRoyalHeadnessLeonoggI.DefaultBounds;
        }
    }
}

class LoomingNightmare(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), AID.LoomingNightmareFirst, AID.LoomingNightmareRest, 3, 1.6f, 5)
{
    private int _numChases;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst && ++_numChases > 2)
            MaxCasts = 4;
        base.OnCastStarted(caster, spell);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Chasers.Find(c => c.Target == actor) is { } chaser)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 13.5f), chaser.NextActivation);
    }
}

class FallingNightmare(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Values;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1 && (OID)actor.OID == OID.NobleNoggin)
            _aoes[actor.InstanceID] = new(new AOEShapeCircle(2), actor.Position, default, WorldState.FutureTime(3));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FallingNightmare1 or AID.FallingNightmare2)
            _aoes[caster.InstanceID] = new(new AOEShapeCircle(2), caster.Position, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FallingNightmare1 or AID.FallingNightmare2)
        {
            ++NumCasts;
            _aoes.Remove(caster.InstanceID);
        }
    }

    public override void OnActorDestroyed(Actor actor) => _aoes.Remove(actor.InstanceID);
}

class SpiritedCharge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _dolls = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _dolls.Select(d => new AOEInstance(new AOEShapeRect(6, 1), d.Position, d.Rotation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritedChargeStart)
            _dolls.Add(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E3C)
            _dolls.Remove(actor);
    }

    public override void OnActorDestroyed(Actor actor) => _dolls.Remove(actor);
}

class EvilScheme(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EvilSchemeFirst)
            Lines.Add(new() { Next = caster.Position, Advance = 4 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell, 1.6f), TimeToMove = 1.5f, ExplosionsLeft = 5, MaxShownExplosions = 5 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EvilSchemeFirst or AID.EvilSchemeRest)
        {
            ++NumCasts;
            var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
            {
                ReportError($"Failed to find line for {caster.InstanceID:X}");
                return;
            }
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class Scream(BossModule module) : Components.StandardAOEs(module, AID.Scream, new AOEShapeCone(20, 30.Degrees()), maxCasts: 4);

class D071HisRoyalHeadnessLeonoggIStates : StateMachineBuilder
{
    public D071HisRoyalHeadnessLeonoggIStates(BossModule module) : base(module)
    {
        // TODO: Morbid Fascination and Scream never seen (two clears)
        TrivialPhase()
            .ActivateOnEnter<MaliciousMist>()
            .ActivateOnEnter<MistDeathwall>()
            .ActivateOnEnter<Scream>()
            .ActivateOnEnter<SpiritedCharge>()
            .ActivateOnEnter<FallingNightmare>()
            .ActivateOnEnter<EvilScheme>()
            .ActivateOnEnter<LoomingNightmare>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 13073)]
public class D071HisRoyalHeadnessLeonoggI(ModuleInit init) : BossModule(init, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(0, 150);
    public static readonly ArenaBoundsCircle StartingBounds = new(19.5f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(14);
}
