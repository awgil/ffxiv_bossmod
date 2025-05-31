#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Foray.CriticalEngagement.LionRampant;

public enum OID : uint
{
    Boss = 0x46DC, // R4.600, x1
    Helper = 0x233C, // R0.500, x22 (spawn during fight), Helper type
    _Gen_LionRampant = 0x46B4, // R1.000, x4
    _Gen_AnimatedDoll = 0x45D2, // R0.500, x5 (spawn during fight)
    _Gen_AnimatedDoll1 = 0x45D1, // R0.500, x3 (spawn during fight)
    _Gen_RadiantBeacon = 0x46DD, // R3.000, x0 (spawn during fight)
    _Gen_LightSprite = 0x46DF, // R1.760, x0 (spawn during fight)
    _Gen_OchreStone = 0x46E0, // R2.700, x0 (spawn during fight)
    _Gen_RadiantRoundel = 0x46DE, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Spell_ = 41400, // _Gen_LionRampant->self, no cast, range ?-60 donut
    _Weaponskill_FearsomeGlint = 41411, // Boss->self, 6.0s cast, range 60 90-degree cone
    _Spell_AugmentationOfBeacons = 41401, // Boss->self, 3.0s cast, single-target
    _Spell_AetherialRay = 41402, // Helper->self, no cast, range 28 width 10 rect
    _Weaponskill_Shockwave = 41412, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Shockwave1 = 41414, // Helper->self, no cast, ???
    _Weaponskill_Shockwave2 = 41413, // Boss->self, no cast, single-target
    _Spell_AugmentationOfRoundels = 41404, // Boss->self, 3.0s cast, single-target
    _Spell_AugmentationOfStones = 41405, // Boss->self, 3.0s cast, single-target
    _Spell_FallingRock = 41409, // Boss->self, 3.0s cast, single-target
    _Spell_FallingRock1 = 41410, // Helper->location, 4.0s cast, range 10 circle
    _Spell_Flatten = 30787, // Boss->self, 5.0s cast, single-target
    _Spell_Flatten1 = 41408, // _Gen_OchreStone->self, 5.0s cast, single-target
    _Spell_Decompress = 41407, // Helper->self, 5.0s cast, range 12 circle
    _Spell_Flatten2 = 41406, // _Gen_OchreStone->self, 5.0s cast, single-target
    _Spell_BrightPulse = 41403, // Helper->location, no cast, range 12 circle
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0272_laser_0c = 311, // _Gen_LionRampant->_Gen_RadiantBeacon
    _Gen_Tether_chn_m0272_laser_1c = 312, // _Gen_LionRampant->_Gen_RadiantBeacon
}

class FearsomeGlint(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_FearsomeGlint, new AOEShapeCone(60, 45.Degrees()));

class LionRampantStates : StateMachineBuilder
{
    public LionRampantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FearsomeGlint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13809, DevOnly = true)]
public class LionRampant(WorldState ws, Actor primary) : BossModule(ws, primary, new(636, -54), new ArenaBoundsCircle(24.5f))
{
    public override bool DrawAllPlayers => true;
}
