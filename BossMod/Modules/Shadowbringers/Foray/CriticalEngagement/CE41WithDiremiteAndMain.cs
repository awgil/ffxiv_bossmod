namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE41WithDiremiteAndMain;

public enum OID : uint
{
    Boss = 0x31CC, // R7.200, x1
    DimCrystal = 0x31CD, // R1.600, spawn during fight
    CorruptedCrystal = 0x31CE, // R1.600, spawn during fight
    SandSphere = 0x31CF, // R4.000, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 24090, // Boss->location, no cast, single-target

    Shardfall = 24071, // Boss->self, 2.0s cast, single-target, visual
    CrystallineFracture = 24072, // CorruptedCrystal/DimCrystal->self, 3.0s cast, range 4 circle aoe (cast on spawn)
    ResonantFrequencyDim = 24073, // DimCrystal->self, 3.0s cast, range 6 circle, self-destruct, cast early if crystal is hit by shardstrike
    ResonantFrequencyCorrupted = 24074, // CorruptedCrystal->self, 3.0s cast, range 6 circle, self-destruct, cast early if crystal is hit by shardstrike
    ResonantFrequencyDimStinger = 24075, // DimCrystal->self, no cast, single-target, self-destruct, after aetherial stingers
    ResonantFrequencyCorruptedStinger = 24076, // CorruptedCrystal->self, no cast, single-target, self-destruct, after crystalline stingers
    CrystallineStingers = 24077, // Boss->self, 5.0s cast, range 60 circle, hide behind dim
    AetherialStingers = 24078, // Boss->self, 5.0s cast, range 60 circle, hide behind corrupted
    SandSphere = 24079, // Boss->self, 5.0s cast, single-target, visual
    Subduction = 24080, // SandSphere->self, 4.0s cast, range 8 circle aoe with knockback 10
    Earthbreaker = 24081, // Boss->self, 5.0s cast, single-target, visual
    EarthbreakerAOE1 = 24082, // Helper->self, 5.0s cast, range 10 circle
    EarthbreakerAOE2 = 24083, // Helper->self, 3.0s cast, range 10-20 donut
    EarthbreakerAOE3 = 24084, // Helper->self, 3.0s cast, range 20-30 donut

    CrystalNeedle = 24085, // Boss->player, 5.0s cast, single-target, tankbuster
    Shardstrike = 24086, // Boss->self, 2.0s cast, single-target, visual
    ShardstrikeAOE = 24087, // Helper->players, 5.0s cast, range 5 circle spread
    Hailfire = 24088, // Boss->self, 8.0s cast, single-target, visual
    HailfireAOE = 24089 // Boss->self, no cast, range 40 width 4 rect aoe
}

public enum IconID : uint
{
    Shardstrike = 96, // player
    Hailfire1 = 79, // player
    Hailfire2 = 80, // player
    Hailfire3 = 81, // player
    Hailfire4 = 82 // player
}

class CrystallineFracture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrystallineFracture, 4f);
class ResonantFrequencyDim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ResonantFrequencyDim, 6f);
class ResonantFrequencyCorrupted(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ResonantFrequencyCorrupted, 6f);

class CrystallineStingers(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.CrystallineStingers, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var crystals = Module.Enemies((uint)OID.DimCrystal);
        var count = crystals.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = crystals[i];
            if (!c.IsDead)
                actors.Add(c);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

class AetherialStingers(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.AetherialStingers, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var crystals = Module.Enemies((uint)OID.CorruptedCrystal);
        var count = crystals.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = crystals[i];
            if (!c.IsDead)
                actors.Add(c);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

class Subduction(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Subduction, 8f);

class Earthbreaker(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EarthbreakerAOE1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.EarthbreakerAOE1 => 0,
                (uint)AID.EarthbreakerAOE2 => 1,
                (uint)AID.EarthbreakerAOE3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class CrystalNeedle(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CrystalNeedle);
class Shardstrike(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShardstrikeAOE, 5f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            var crystals = Module.Enemies(CE41WithDiremiteAndMain.Crystals);
            var count = crystals.Count;
            if (count == 0)
                return;
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
                forbidden[i] = new SDCircle(crystals[i].Position, 6.6f);
            if (forbidden.Length != 0)
                hints.AddForbiddenZone(new SDUnion(forbidden), Spreads[0].Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!IsSpreadTarget(pc))
            return;
        var crystals = Module.Enemies(CE41WithDiremiteAndMain.Crystals);
        var count = crystals.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = crystals[i];
            Arena.ZoneCircleOutline(a.Position, a.HitboxRadius);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsSpreadTarget(actor))
            hints.Add("Spread, avoid intersecting cage hitboxes!");
    }
}

class Hailfire(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(40f, 2f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && spell.Action.ID == (uint)AID.HailfireAOE)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Hailfire1 and <= (uint)IconID.Hailfire4)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect, WorldState.FutureTime(8.2d + (iconID - (uint)IconID.Hailfire1) * 2.1d)));
            CurrentBaits.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }
}

class CE41WithDiremiteAndMainStates : StateMachineBuilder
{
    public CE41WithDiremiteAndMainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrystallineFracture>()
            .ActivateOnEnter<ResonantFrequencyDim>()
            .ActivateOnEnter<ResonantFrequencyCorrupted>()
            .ActivateOnEnter<CrystallineStingers>()
            .ActivateOnEnter<AetherialStingers>()
            .ActivateOnEnter<Subduction>()
            .ActivateOnEnter<Earthbreaker>()
            .ActivateOnEnter<CrystalNeedle>()
            .ActivateOnEnter<Shardstrike>()
            .ActivateOnEnter<Hailfire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 21)] // bnpcname=9969
public class CE41WithDiremiteAndMain(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly uint[] Crystals = [(uint)OID.DimCrystal, (uint)OID.CorruptedCrystal];
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-220f, 530f), 29.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        var crystals = Enemies(Crystals);
        var count = crystals.Count;
        var filteredcrystals = new List<Actor>(count);

        for (var i = 0; i < count; ++i)
        {
            var c = crystals[i];
            if (!c.IsDead)
                filteredcrystals.Add(c);
        }
        Arena.Actors(filteredcrystals, Colors.Object, true);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
