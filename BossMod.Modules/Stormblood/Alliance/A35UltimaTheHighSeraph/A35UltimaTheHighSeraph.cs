namespace BossMod.Stormblood.Alliance.A35UltimaTheHighSeraph;

public enum OID : uint
{
    Boss = 0x25A9, // R8.000, x1
    Helper = 0x233C, // R0.500, x41
    BossP1 = 0x2604, // R10.400, x1
    _Gen_Dominion = 0x25AE, // R1.700, x0 (spawn during fight)
    _Gen_AuraciteShard = 0x2639, // R1.250, x0 (spawn during fight)
    _Gen_Dominion1 = 0x25B6, // R1.700, x0 (spawn during fight)
    _Gen_Dominion2 = 0x2689, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 14501, // Boss->player, no cast, single-target
    _Ability_DemiVirgo = 14530, // Boss->self, 3.0s cast, single-target
    _Ability_EastwardMarch = 14494, // Boss->self, 4.0s cast, single-target
    _Ability_EastwardMarch1 = 14496, // Helper->self, no cast, single-target
    _Weaponskill_RayOfLight = 14519, // Helper->self, no cast, range 70 width 10 rect
    _Weaponskill_RayOfLight1 = 14518, // 25AE->self, no cast, single-target
    _Weaponskill_Redemption = 14506, // Boss->player, 4.0s cast, single-target
    _Weaponskill_GrandCross = 14536, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Plummet = 14509, // 2639->self, 3.5s cast, range 15 width 15 rect
    _Weaponskill_GrandCross1 = 14510, // 2639->self, 4.0s cast, range 60 width 15 cross
    _Weaponskill_Penultima = 14524, // 25B6->player, no cast, single-target
    _Weaponskill_Embrace = 14520, // 25AE->self, 3.0s cast, single-target
    _Weaponskill_Embrace1 = 14521, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_Embrace2 = 14522, // Helper->self, no cast, range 7 circle
    _Weaponskill_LifeDrain = 14523, // Helper->player, no cast, single-target
    _Weaponskill_HolyIV = 14490, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_HolyIV1 = 14534, // Boss->self, 3.0s cast, single-target
    _Weaponskill_HolyIV2 = 14491, // Helper->players, 5.5s cast, range 6 circle
    _Ability_Cataclysm = 14500, // Boss->self, no cast, single-target
    _Weaponskill_Shockwave = 14484, // Helper->self, no cast, range 80 circle
    _Weaponskill_Auralight = 14504, // Helper->self, 2.5s cast, range 50 width 10 rect
    _Weaponskill_Auralight1 = 14505, // Helper->self, 2.5s cast, range 25 width 10 rect
    _Weaponskill_Bombardment = 14526, // Helper->location, 2.5s cast, range 6 circle
    _Weaponskill_Holy = 14507, // Helper->location, 2.5s cast, range 2 circle
    _Ability_EastwardMarch2 = 14502, // Boss->self, 4.0s cast, single-target
    _Weaponskill_FlareIV = 14493, // Helper->players, 5.5s cast, range 80 circle
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 202, // 2639->player, extra=0x1
    _Gen_Clashing = 1271, // none->player, extra=0x17E3
    _Gen_Fearless = 1747, // 25B6->player, extra=0x0
    _Gen_Bind = 564, // Helper->player, extra=0x0
    _Gen_OutOfTheAction = 1284, // none->player, extra=0x63E
    _Gen_AccelerationBomb = 1072, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon01i = 198, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
    _Gen_Icon_m0328sp10trg0s = 75, // player->self
    _Gen_Icon_m0361trg_b0t = 87, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_teke01h = 84, // 25AE->player
}

class A35UltimaTheHighSeraphStates : StateMachineBuilder
{
    public A35UltimaTheHighSeraphStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaTheHighSeraph(ModuleInit init) : BossModule(init, new(600, -600), new ArenaBoundsSquare(29.5f));
