namespace BossMod.Heavensward.Dungeon.D04TheVault.D043SerCharibert;

public enum OID : uint
{
    Boss = 0x1056, // R2.200, x1
    Helper = 0xD25, // R0.500, x13, mixed types
    Charibert = 0xF71EE, // R0.500, x0 (spawn during fight), EventNpc type
    DawnKnight = 0x1057, // R2.000, x0 (spawn during fight)
    DuskKnight = 0x1058, // R2.000, x0 (spawn during fight)
    HolyFlame = 0x1059, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    Visual1 = 4121, // Boss->self, no cast, single-target
    Visual2 = 4120, // Boss->self, no cast, single-target

    AutoAttack = 4143, // Boss->player, no cast, single-target
    AltarCandle = 4144, // Boss->player, no cast, single-target tankbuster

    HeavensflameTelegraph = 4145, // Boss->self, 2.5s cast, single-target
    HeavensflameAOE = 4146, // Helper->location, 3.0s cast, range 5 circle ground targetted aoe
    HolyChainTelegraph = 4147, // Boss->self, 2.0s cast, single-target
    HolyChainPlayerTether = 4148, // Helper->self, no cast, ??? tether

    AltarPyre = 4149, // Boss->location, 3.0s cast, range 80 circle raidwide
    StartLimitBreakPhase = 4150, // Boss->self, no cast, single-target
    SacredFlame = 4156, // HolyFlame->self, no cast, range 80+R circle raidewide, "enrage" for each flame not killed within 30s
    PureOfHeart = 4151, // Boss->location, no cast, range 80 circle raidewide

    WhiteKnightsTour = 4152, // DawnKnight->self, 3.0s cast, range 40+R width 4 rect
    BlackKnightsTour = 4153, // DuskKnight->self, 3.0s cast, range 40+R width 4 rect

    TurretChargeDawnKnight = 4154, // Helper->player, no cast, only triggers if inside hitbox
    TurretChargeRestDuskKnight = 4155, // Helper->player, no cast, only triggers if inside hitbox
}

public enum TetherID : uint
{
    HolyChain = 9, // player->player
}

class WhiteKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhiteKnightsTour), new AOEShapeRect(40, 2));
class BlackKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlackKnightsTour), new AOEShapeRect(40, 2));
class AltarPyre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AltarPyre));

class HeavensflameAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavensflameAOE), 5);
class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, ActionID.MakeSpell(AID.HolyChainPlayerTether));

class TurretTour(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _knights = [];
    private static readonly AOEShapeCircle circle = new(2);
    private static readonly AOEShapeRect rect = new(6, 2, 2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _knights)
        {
            yield return new(rect, c.Position + 2 * c.Rotation.ToDirection(), c.Rotation);
            yield return new(circle, c.Position, c.Rotation, Color: ArenaColor.Danger);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight && !actor.Position.AlmostEqual(Module.Center, 10))
            _knights.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight)
            _knights.Remove(actor);
    }
}

class D043SerCharibertStates : StateMachineBuilder
{
    public D043SerCharibertStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteKnightsTour>()
            .ActivateOnEnter<BlackKnightsTour>()
            .ActivateOnEnter<AltarPyre>()
            .ActivateOnEnter<HeavensflameAOE>()
            .ActivateOnEnter<HolyChain>()
            .ActivateOnEnter<TurretTour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3642)]
public class D043SerCharibert(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 4.1f), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HolyFlame), ArenaColor.Object);
    }
}
