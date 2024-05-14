namespace BossMod.Heavensward.Dungeon.D04TheVault.D043SerCharibert;

public enum OID : uint
{
    Boss = 0x1056, // R2.200, x1
    Helper = 0xD25, // R0.500, x13, mixed types
    Charibert = 0xF71EE, // R0.500, x0 (spawn during fight), EventNpc type
    DawnKnight = 0x1057, // R2.000, x0 (spawn during fight)
    DuskKnight = 0x1058, // R2.000, x0 (spawn during fight)
    HolyFlame = 0x1059, // R1.500, x0 (spawn during fight)
    Actor1e9872 = 0x1E9872, // R2.000, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    Unknown1 = 4121, // Boss->self, no cast, single-target
    Unknown2 = 4120, // Boss->self, no cast, single-target

    AutoAttack = 4143, // Boss->player, no cast, single-target
    AltarCandle = 4144, // Boss->player, no cast, single-target tankbuster

    HeavensflameTelegraph = 4145, // Boss->self, 2.5s cast, single-target
    HeavensflameAOE = 4146, // Helper->location, 3.0s cast, range 5 circle ground targetted aoe
    HolyChainTelegraph = 4147, // Boss->self, 2.0s cast, single-target
    HolyChainPlayerTether = 4148, // Helper->self, no cast, ??? tether

    AltarPyre = 4149, // Boss->location, 3.0s cast, range 80 circle raidwide
    Unknown3 = 4150, // Boss->self, no cast, single-target
    PureOfHeart = 4151, // Boss->location, no cast, range 80 circle raidewide

    WhiteKnightsTour = 4152, // DawnKnight->self, 3.0s cast, range 40+R width 4 rect
    BlackKnightsTour = 4153, // DuskKnight->self, 3.0s cast, range 40+R width 4 rect

    TurretChargeStart = 4154, // Helper->player, no cast, single-target mob march, exoflare?
    TurretChargeRest = 4155, // Helper->player, no cast, single-target mob march, exoflare?
    SacredFlame = 4156, // HolyFlame->self, no cast, range 80+R circle raidewide
}

public enum SID : uint
{
    Slow = 9, // Helper->player, extra=0x0
    BurningChains = 769, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Bleeding = 273, // Helper->player, extra=0x0
}

public enum TetherID : uint
{
    HolyChain = 9, // player->player
}

class WhiteKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhiteKnightsTour), new AOEShapeRect(40, 2));
class BlackKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlackKnightsTour), new AOEShapeRect(40, 2));
class AltarCandle(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.AltarCandle));
class AltarPyre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AltarPyre));
class PureOfHeart(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PureOfHeart));
class SacredFlame(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SacredFlame));
class HeavensflameAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavensflameAOE), 5);
class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, ActionID.MakeSpell(AID.HolyChainPlayerTether));

class DawnKnight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _spirits = module.Enemies(OID.DawnKnight);

    private static readonly AOEShapeCircle _shape = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spirits.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

class DuskKnight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _spirits = module.Enemies(OID.DuskKnight);

    private static readonly AOEShapeCircle _shape = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spirits.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

class D043SerCharibertStates : StateMachineBuilder
{
    public D043SerCharibertStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteKnightsTour>()
            .ActivateOnEnter<BlackKnightsTour>()
            .ActivateOnEnter<AltarCandle>()
            .ActivateOnEnter<AltarPyre>()
            .ActivateOnEnter<PureOfHeart>()
            .ActivateOnEnter<SacredFlame>()
            .ActivateOnEnter<HeavensflameAOE>()
            .ActivateOnEnter<HolyChain>()
            .ActivateOnEnter<DawnKnight>()
            .ActivateOnEnter<DuskKnight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team, Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3642)]
public class D043SerCharibert(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 4), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HolyFlame), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DawnKnight), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DuskKnight), ArenaColor.Enemy);
    }
}
