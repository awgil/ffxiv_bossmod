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
    TurretCharge = 4155, // D25->player, no cast, single-target
    TurretCharge2 = 4154, // D25->player, no cast, single-target
}
public enum GID : uint
{
    BurningChains = 769,
}
public enum TetherID : uint
{
    HolyChain = 9,
}

class Heavensflame(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame, 5);
class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, AID.HolyChain2, 13);
class AltarPyre(BossModule module) : Components.RaidwideCast(module, AID.AltarPyre);
class BlackKnightsTour(BossModule module) : Components.StandardAOEs(module, AID.BlackKnightsTour, new AOEShapeRect(40, 2));
class WhiteKnightsTour(BossModule module) : Components.StandardAOEs(module, AID.WhiteKnightsTour, new AOEShapeRect(40, 2));
class March : Components.PersistentVoidzone
{
    private readonly List<Actor> _knights = [];

    public March(BossModule module) : base(module, 2.5f, _ => [], 15)
    {
        Sources = _ => _knights;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight && !actor.Position.AlmostEqual(Module.Center, 5))
            _knights.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.DawnKnight or OID.DuskKnight)
            _knights.Remove(actor);
    }
}
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
            .ActivateOnEnter<March>()
            .ActivateOnEnter<AddsModule>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 4142)]
public class D043SerCharibert(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 4), new ArenaBoundsRect(20, 20));
