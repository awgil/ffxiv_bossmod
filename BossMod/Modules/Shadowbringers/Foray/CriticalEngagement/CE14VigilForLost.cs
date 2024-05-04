namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE14VigilForLost;

public enum OID : uint
{
    Boss = 0x2DBD, // R8.000, x1
    Helper = 0x233C, // R0.500, x11
    MagitekBit = 0x2F58, // R0.660, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    LightLeap = 20146, // Boss->location, 4.0s cast, range 10 circle aoe
    AreaBombardment = 20147, // Boss->self, 5.0s cast, single-target, visual (main mechanic start)
    ChemicalMissile = 20148, // Helper->self, 4.0s cast, range 12 circle aoe
    TailMissile = 20149, // Boss->self, 5.0s cast, single-target, visual (big aoe)
    TailMissileAOE = 20150, // Helper->self, 8.0s cast, range 30 circle aoe
    Shockwave = 20151, // Boss->self, 6.0s cast, range 16 circle aoe
    ExplosiveFlare = 20152, // Helper->self, 3.0s cast, range 10 circle aoe
    CripplingBlow = 20153, // Boss->player, 4.0s cast, single-target, tankbuster
    PlasmaField = 20154, // Boss->self, 4.0s cast, range 60 circle, raidwide
    Explosion = 21266, // Helper->self, 7.0s cast, range 6 circle tower
    MassiveExplosion = 21267, // Helper->self, no cast, range 60 circle, failed tower
    MagitekRay = 21268, // MagitekBit->self, 2.5s cast, range 50 width 4 rect
}

class LightLeap(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LightLeap), 10);
class ChemicalMissile(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChemicalMissile), new AOEShapeCircle(12));
class TailMissile(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailMissileAOE), new AOEShapeCircle(30));
class Shockwave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shockwave), new AOEShapeCircle(16));
class ExplosiveFlare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExplosiveFlare), new AOEShapeCircle(10));
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class PlasmaField(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PlasmaField));
class Towers(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Explosion), 6);
class MagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRay), new AOEShapeRect(50, 2));

class CE14VigilForLostStates : StateMachineBuilder
{
    public CE14VigilForLostStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightLeap>()
            .ActivateOnEnter<ChemicalMissile>()
            .ActivateOnEnter<TailMissile>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ExplosiveFlare>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<PlasmaField>()
            .ActivateOnEnter<Towers>()
            .ActivateOnEnter<MagitekRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 3)] // bnpcname=9396
public class CE14VigilForLost(WorldState ws, Actor primary) : BossModule(ws, primary, new(451, 830), new ArenaBoundsCircle(30));
