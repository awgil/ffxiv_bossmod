namespace BossMod.Heavensward.Dungeon.D04Vault.D043SerCharibert;

public enum OID : uint
{
    Boss = 0x1056, // R2.200, SerCharibert 
    DuskKnight = 0x1058, // R2.000, x?
    DawnKnight = 0x1057, // R2.000, x?
    HolyFlame = 0x1059, // R1.500, x?
    Charibert = 0xF71EE, // R0.500, x?, EventNpc type
    Helper2 = 0xD25,  // "DawnKnight"?
    Helper = 0x233C, // x3
}
public enum AID : uint
{
    Fire = 4143, // 1056->player, no cast, single-target
    AltarCandle = 4144, // 1056->player, no cast, single-target
    Heavensflame2 = 4145, // 1056->self, 2.5s cast, single-target
    Heavensflame = 4146, // D25->location, 3.0s cast, range 5 circle
    HolyChain = 4147, // 1056->self, 2.0s cast, single-target
    HolyChain2 = 4148, // D25->self, no cast, ???
    AltarPyre = 4149, // 1056->location, 3.0s cast, range 80 circle
    BlackKnightsTour = 4153, // 1058->self, 3.0s cast, range 40+R width 4 rect
    WhiteKnightsTour = 4152, // 1057->self, 3.0s cast, range 40+R width 4 rect
    PureOfHeart = 4151, // 1056->location, no cast, range 80 circle
    TurretCharge = 4155, // D25->player, no cast, single-target
    TurretCharge2 = 4154, // D25->player, no cast, single-target
    SacredFlame = 4156, // 1059->self, no cast, range 80+R circle
}
public enum GID : uint
{
    BurningChains = 769,
}
public enum TetherID : uint
{
    HolyChain = 9,
}

class Heavensflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Heavensflame), 5);
class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, ActionID.MakeSpell(AID.HolyChain2), 13);
class AltarPyre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AltarPyre));
class BlackKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlackKnightsTour), new AOEShapeRect(40, 2));
class WhiteKnightsTour(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhiteKnightsTour), new AOEShapeRect(40, 2));
class PureOfHeart(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PureOfHeart));
class SacredFlame(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SacredFlame));
class March(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _knights = [];
    private static readonly AOEShapeRect rect = new(8, 2, -1);
    private static readonly AOEShapeCircle circ = new(2.5f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _knights)
        {
            yield return new AOEInstance(rect, b.Position);
            yield return new AOEInstance(circ, b.Position) with { Color = ArenaColor.Danger };
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight && !actor.Position.AlmostEqual(Module.Center, 5))
        {
            _knights.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight)
        {
            _knights.Remove(actor);
        }
    }
};
class AddsModule(BossModule module) : Components.Adds(module, (uint)OID.HolyFlame)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.HolyFlame => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
};
class D043SerCharibertStates : StateMachineBuilder
{
    public D043SerCharibertStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AltarPyre>()
            .ActivateOnEnter<Heavensflame>()
            .ActivateOnEnter<HolyChain>()
            .ActivateOnEnter<BlackKnightsTour>()
            .ActivateOnEnter<WhiteKnightsTour>()
            .ActivateOnEnter<PureOfHeart>()
            .ActivateOnEnter<SacredFlame>()
            .ActivateOnEnter<March>()
            .ActivateOnEnter<AddsModule>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 4142)]
public class D043SerCharibert(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 4), new ArenaBoundsRect(20, 20));
