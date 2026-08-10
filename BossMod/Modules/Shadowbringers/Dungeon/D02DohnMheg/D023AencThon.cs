namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D031AencThon;

public enum OID : uint
{
    Boss = 0xF14, // R=2.5-6.875
    LiarsLyre = 0xF63, // R=2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Teleport = 13206, // Boss->location, no cast, single-target

    CripplingBlow = 13732, // Boss->player, 4.0s cast, single-target
    VirtuosicCapriccio = 13708, // Boss->self, 5.0s cast, range 80+R circle
    ImpChoir = 13552, // Boss->self, 4.0s cast, range 80+R circle
    ToadChoir = 13551, // Boss->self, 4.0s cast, range 17+R 150-degree cone

    FunambulistsFantasia = 13498, // Boss->self, 4.0s cast, single-target, changes arena to planks over a chasm
    FunambulistsFantasiaPull = 13519, // Helper->self, 4.0s cast, range 50 circle, pull 50, between hitboxes

    ChangelingsFantasia = 13521, // Boss->self, 3.0s cast, single-target
    ChangelingsFantasia2 = 13522, // Helper->self, 1.0s cast, single-target

    Malaise = 13549, // Boss->self, no cast, single-target
    BileBombardment = 13550, // Helper->location, 4.0s cast, range 8 circle
    CorrosiveBileFirst = 13547, // Boss->self, 4.0s cast, range 18+R 120-degree cone
    CorrosiveBileRest = 13548, // Helper->self, no cast, range 18+R 120-degree cone
    FlailingTentaclesVisual = 13952, // Boss->self, 5.0s cast, single-target
    FlailingTentacles = 13953, // Helper->self, no cast, range 32+R width 7 rect

    Finale = 15723, // LiarsLyre->self, 60.0s cast, single-target
    FinaleEnrage = 13520 // Boss->self, 60.0s cast, range 80+R circle
}

sealed class VirtuosicCapriccio(BossModule module) : Components.RaidwideCast(module, (uint)AID.VirtuosicCapriccio, "Raidwide + Bleed");
sealed class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow);
sealed class ImpChoir(BossModule module) : Components.CastGaze(module, (uint)AID.ImpChoir);
sealed class ToadChoir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToadChoir, new AOEShapeCone(19.5f, 75f.Degrees()));
sealed class BileBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BileBombardment, 8f);

sealed class FunambulistsFantasia(BossModule module) : BossComponent(module)
{
    private bool chasmArena;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FunambulistsFantasia)
        {
            Arena.Bounds = new ArenaBoundsCustom(D033AencThon.GetUnion(), [.. D033AencThon.GetDifference(), new Rectangle(new(-128.5f, -244f), 20f, 10f)],
            [new PolygonCustom([new(-142.32f, -234f), new(-140.533f, -245.712f), new(-129.976f, -241.934f), new(-113.76f, -243.889f),
            new(-113.87f, -244.775f), new(-125.28f, -249.556f), new(-123.83f, -254f), new(-124.66f, -254f), new(-126.205f, -249.744f), new(-126.421f, -249.072f),
            new(-115.56f, -244.512f), new(-129.954f, -242.795f), new(-141.178f, -246.795f), new(-143.12f, -234f)])], 0.25f);
            chasmArena = true;
        }
        else if (spell.Action.ID == (uint)AID.Finale)
        {
            Arena.Bounds = D033AencThon.BuildArena().arena;
            chasmArena = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chasmArena && Module.Enemies((uint)OID.LiarsLyre) is var lyre && lyre.Count != 0)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(lyre[0], 1f, 5f));
        }
    }
}

sealed class Finale(BossModule module) : Components.CastHint(module, (uint)AID.Finale, "Enrage, destroy the Liar's Lyre!", true);

sealed class CorrosiveBile(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(24.875f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CorrosiveBileFirst)
        {
            _aoe = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CorrosiveBileFirst:
            case (uint)AID.CorrosiveBileRest:
                if (++NumCasts == 6)
                {
                    _aoe = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class FlailingTentacles(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCross cross = new(38.875f, 3.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlailingTentaclesVisual)
        {
            _aoe = [new(cross, spell.LocXZ, Module.PrimaryActor.Rotation + 45f.Degrees(), Module.CastFinishAt(spell, 1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlailingTentaclesVisual:
            case (uint)AID.FlailingTentacles:
                if (++NumCasts == 5)
                {
                    _aoe = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class D033AencThonStates : StateMachineBuilder
{
    public D033AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirtuosicCapriccio>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<ImpChoir>()
            .ActivateOnEnter<ToadChoir>()
            .ActivateOnEnter<BileBombardment>()
            .ActivateOnEnter<CorrosiveBile>()
            .ActivateOnEnter<FlailingTentacles>()
            .ActivateOnEnter<FunambulistsFantasia>()
            .ActivateOnEnter<Finale>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649u, NameID = 8146u)]
public sealed class D033AencThon : BossModule
{
    public D033AencThon(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D033AencThon(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static Polygon[] GetUnion() => [new(new(-128.5f, -244f), 19.7f, 40)];
    public static Rectangle[] GetDifference() => [new(new(-128.5f, -224f), 20f, 1.5f)];

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom(GetUnion(), GetDifference());
        return (arena.Center, arena);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        if (count == 0)
        {
            return;
        }
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.LiarsLyre && (actor.Position - e.Actor.Position).LengthSq() > 15f)
            {
                e.Priority = AIHints.Enemy.PriorityInvincible;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LiarsLyre), Colors.Object);
    }
}
