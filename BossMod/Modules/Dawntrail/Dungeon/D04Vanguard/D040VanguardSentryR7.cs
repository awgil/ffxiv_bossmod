namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardSentryR7;

public enum OID : uint
{
    Boss = 0x4479, //R=2.34
    SentryR7 = 0x41D8, //R=2.34
}

public enum AID : uint
{
    AutoAttack = 36403, // SentryR7/Boss->player, no cast, single-target
    Swoop = 38051, // SentryR7/Boss->location, 4.0s cast, width 5 rect charge
    FloaterTurn = 38451, // SentryR7/Boss->self, 4.0s cast, range 4-10 donut
    SpinningAxle = 39018, // Boss->self, 4.0s cast, range 6 circle
}

class Swoop(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Swoop), 2.5f);
class FloaterTurn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FloaterTurn), new AOEShapeDonut(4, 10));
class SpinningAxle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpinningAxle), new AOEShapeCircle(6));

class D040VanguardSentryR7States : StateMachineBuilder
{
    public D040VanguardSentryR7States(D040VanguardSentryR7 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<FloaterTurn>()
            .ActivateOnEnter<SpinningAxle>()
            .Raw.Update = () => module.SentryR7.All(e => e.IsDeadOrDestroyed) && module.SentryR72.All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12778, SortOrder = 1)]
public class D040VanguardSentryR7 : BossModule
{
    private static readonly List<WPos> arenacoords = [new(-90.423f, 263.603f), new(-90.23f, 280.48f), new(-82.081f, 280.704f), new(-82.235f, 288.316f), new(-89.289f, 288.557f), new(-89.909f, 302.169f),
    new(-90.005f, 307.76f), new(-74.234f, 318.768f), new(-48.199f, 318.908f), new(-47.511f, 333.532f), new(-97.889f, 334.552f), new(-97.019f, 331.294f), new(-109.076f, 316.539f), new(-108.71f, 287.42f),
    new(-117.732f, 287.449f), new(-118.057f, 273.564f), new(-109.382f, 273.519f), new(-109.449f, 263.178f)];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex([new PolygonCustom(arenacoords)], MapResolution: 0.35f);
    public readonly IReadOnlyList<Actor> SentryR7;
    public readonly IReadOnlyList<Actor> SentryR72;

    public D040VanguardSentryR7(WorldState ws, Actor primary) : base(ws, primary, arena.Center, arena)
    {
        SentryR7 = Enemies(OID.Boss);
        SentryR72 = Enemies(OID.SentryR7);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(SentryR7, ArenaColor.Enemy);
        Arena.Actors(SentryR72, ArenaColor.Enemy);
    }
}
