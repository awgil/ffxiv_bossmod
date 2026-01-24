namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x22-43, Helper type
    _Gen_Lindwurm = 0x4B02, // R5.000, x1
    Boss = 0x4AFB, // R13.800, x1
    RedOrb = 0x4B00, // R1.800, x0 (spawn during fight)
    GreenOrb = 0x4B01, // R1.800, x0 (spawn during fight)
    _Gen_Lindwurm1 = 0x4AFE, // R0.000-0.500, x2, Part type
    _Gen_BloodVessel = 0x4AFF, // R0.800, x0 (spawn during fight)
    _Gen_Lindwurm4 = 0x4B27, // R1.000, x0 (spawn during fight), Helper type
    _Gen_Lindwurm5 = 0x4AFD, // R1.000, x0 (spawn during fight), Helper type
    _Gen_Lindwurm6 = 0x4AFC, // R4.000, x0 (spawn during fight), Helper type
    _Gen_Lindwurm7 = 0x4B28, // R1.000, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 46291, // 4AFE->player, no cast, single-target
    _Weaponskill_TheFixer = 46295, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_ = 47044, // Boss->self, no cast, single-target
    _Weaponskill_MortalSlayer = 46229, // Boss->self, 12.0s cast, single-target
    _Spell_MortalSlayer = 46230, // 4B01->players, no cast, range 6 circle
    _Spell_MortalSlayer1 = 46232, // 4B00->players, no cast, range 6 circle
    _Weaponskill_1 = 47045, // Boss->self, no cast, single-target
    _Weaponskill_2 = 47579, // Boss->self, no cast, single-target
    _Weaponskill_GrotesquerieAct1 = 48829, // Boss->self, 3.0s cast, single-target
    _Weaponskill_RavenousReach = 46185, // Boss->self, 1.0+10.7s cast, single-target
    _Weaponskill_3 = 46235, // 4AFC->self, no cast, single-target
    _Spell_PhagocyteSpotlight = 46238, // Helper->location, 3.0s cast, range 5 circle
    _Weaponskill_RavenousReach1 = 46954, // 4AFC->self, no cast, single-target
    _Weaponskill_4 = 46190, // Boss->self, no cast, single-target
    _Weaponskill_RavenousReach2 = 46237, // Helper->self, 10.6s cast, range 35 120-degree cone
    _Spell_HemorrhagicProjection = 46255, // Helper->self, no cast, range 60 30-degree cone
    _Spell_DramaticLysis = 46250, // Helper->location, no cast, range 6 circle
    _Spell_FourthWallFusion = 46254, // Helper->location, no cast, range 6 circle
    _Spell_Burst = 46239, // Helper->location, 2.5s cast, range 12 circle
    _Spell_FourthWallFusion1 = 47545, // Helper->players, no cast, range 6 circle
    _Spell_VisceralBurst = 46294, // Helper->players, no cast, range 6 circle
    _Weaponskill_5 = 46234, // 4AFC->self, no cast, single-target
    _Weaponskill_RavenousReach3 = 46953, // 4AFC->self, no cast, single-target
    _Weaponskill_GrotesquerieAct2 = 48830, // Boss->self, 3.0s cast, single-target
    _Spell_PhagocyteSpotlight1 = 46262, // Helper->location, 3.0s cast, range 5 circle
    _Weaponskill_CruelCoil = 46267, // Boss->location, 3.0s cast, single-target
    _Spell_ = 46194, // Helper->self, no cast, range 60 circle
    _Weaponskill_Skinsplitter = 46268, // Boss->self, no cast, single-target
    _Weaponskill_Skinsplitter1 = 46398, // Helper->self, no cast, range ?-13 donut
    _Weaponskill_6 = 46273, // Helper->self, 3.0s cast, range 9 circle
    _Spell_DramaticLysis1 = 46261, // Helper->self, no cast, ???
    _Spell_DramaticLysis2 = 46260, // Helper->location, no cast, range 4 circle
    _Weaponskill_RoilingMass = 46263, // _Gen_BloodVessel->self, 3.0s cast, range 3 circle
    _Weaponskill_RoilingMass1 = 46259, // _Gen_BloodVessel->self, 3.0s cast, range 3 circle
    _Weaponskill_UnmitigatedExplosion = 46258, // Helper->self, no cast, range 60 circle
    _Weaponskill_CruelCoil1 = 46265, // Boss->location, 3.0s cast, single-target
    _Weaponskill_CruelCoil2 = 46266, // Boss->location, 3.0s cast, single-target
    _Weaponskill_CruelCoil3 = 46264, // Boss->location, 3.0s cast, single-target
}

public enum SID : uint
{
    _Gen_ = 3792, // Boss->GreenOrb/RedOrb, extra=0x12/0x16/0x13/0x17/0x14/0x15/0x76/0x18
    _Gen_PoisonResistanceDownII = 3935, // GreenOrb/RedOrb/_Gen_BloodVessel->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_DirectedGrotesquerie = 4976, // none->player, extra=0x0
    DirectedGrotesquerieVisual = 3558, // none->player, extra=0x40F/0x40D/0x40E/0x40C
    _Gen_BurstingGrotesquerie = 4761, // Helper->player, extra=0x0
    _Gen_SharedGrotesquerie = 4762, // none->player, extra=0x0
    _Gen_2 = 3913, // none->Boss, extra=0x444
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper->player, extra=0x0
    _Gen_SustainedDamage = 4149, // Helper->player, extra=0x1/0x2
    BondsA = 4752, // none->player, extra=0x0
    UnbreakableA = 4753, // none->player, extra=0x0
    BondsB = 4754, // none->player, extra=0x0
    UnbreakableB = 4755, // none->player, extra=0x0
    _Gen_SecondInLine = 3005, // none->player, extra=0x0
    _Gen_FourthInLine = 3451, // none->player, extra=0x0
    _Gen_ThirdInLine = 3006, // none->player, extra=0x0
    _Gen_FirstInLine = 3004, // none->player, extra=0x0
    _Gen_Bind = 2518, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_com_share3t = 161, // player->self
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    _Gen_Icon_x6rc_cellchain_01x = 657, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_x6rc_cell_01x = 366, // player->player
}
