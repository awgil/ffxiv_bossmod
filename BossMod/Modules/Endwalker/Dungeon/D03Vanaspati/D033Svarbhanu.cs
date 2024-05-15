namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D033Svarbhanu;

public enum OID : uint
{
    Boss = 0x33EB, // R=7.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AetherialDisruption = 25160, // Boss->self, 7.0s cast, single-target
    ChaoticPulse = 27489, // Boss->self, no cast, single-target
    ChaoticUndercurrent1 = 25162, // Helper->self, no cast, range 40 width 10 rect
    ChaoticUndercurrent2 = 25163, // Helper->self, no cast, range 40 width 10 rect
    CosmicKissAOE = 25167, // Helper->location, 3.0s cast, range 6 circle
    CosmicKissSpread = 25168, // Helper->player, 8.0s cast, range 6 circle
    CosmicKissKnockback = 25169, // Helper->location, 6.0s cast, range 100 circle knockback 13
    CrumblingSky = 25166, // Boss->self, 3.0s cast, single-target
    FlamesOfDecay = 25170, // Boss->self, 5.0s cast, range 40 circle
    GnashingOfTeeth = 25171, // Boss->player, 5.0s cast, single-target
    Unknown1 = 25161, // Boss->self, no cast, single-target
    Unknown2 = 25164, // Helper->self, no cast, single-target
    Unknown3 = 25374, // Helper->self, no cast, range 50 width 10 rect Knockback
    Unknown4 = 25165, // Helper->self, no cast, single-target
}

public enum SID : uint
{
    Bleeding1 = 3077, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    Bleeding2 = 3078, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon_101 = 101, // player
    Icon_218 = 218, // player
}

class CosmicKissSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.CosmicKissSpread), 6);
class CosmicKissAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CosmicKissAOE), 6);
class CosmicKissKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.CosmicKissKnockback), 13, shape: new AOEShapeCircle(100));
class FlamesOfDecay(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlamesOfDecay));
class GnashingOfTeeth(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.GnashingOfTeeth));

class D033SvarbhanuStates : StateMachineBuilder
{
    public D033SvarbhanuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CosmicKissSpread>()
            .ActivateOnEnter<CosmicKissAOE>()
            .ActivateOnEnter<CosmicKissKnockback>()
            .ActivateOnEnter<FlamesOfDecay>()
            .ActivateOnEnter<GnashingOfTeeth>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10719)]
public class D033Svarbhanu(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, -157), new ArenaBoundsSquare(20));
