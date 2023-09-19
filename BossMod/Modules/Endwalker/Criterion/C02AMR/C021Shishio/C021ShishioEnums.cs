namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    public enum OID : uint
    {
        NBoss = 0x3F48, // R7.800, x1
        NRaiun = 0x3F49, // R0.800, spawn during fight - small smoke clouds
        NRairin = 0x3F4A, // R1.000, spawn during fight - rings
        NDevilishThrall = 0x3F4B, // R2.000, spawn during fight - cleaving adds
        NHauntingThrall = 0x3F4C, // R2.000, spawn during fight - tethered ghosts

        SBoss = 0x3F4D, // R7.800, x1
        SRaiun = 0x3F4E, // R0.800, spawn during fight - small smoke clouds
        SRairin = 0x3F4F, // R1.000, spawn during fight - rings
        SDevilishThrall = 0x3F50, // R2.000, spawn during fight - cleaving adds
        SHauntingThrall = 0x3F51, // R2.000, spawn during fight - tethered ghosts

        Helper = 0x233C, // R0.500, x20
    };

    public enum AID : uint
    {
        NAutoAttack = 34527, // NBoss->player, no cast, single-target
        SAutoAttack = 34528, // NBoss->player, no cast, single-target
        NTeleport = 33821, // NBoss->location, no cast, single-target, teleport
        STeleport = 33860, // SBoss->location, no cast, single-target, teleport

        NEnkyo = 33818, // NBoss->self, 5.0s cast, range 60 circle, raidwide
        NSplittingCry = 33819, // NBoss->self/players, 5.0s cast, range 60 width 14 rect tankbuster
        NSlither = 33820, // NBoss->self, 2.0s cast, range 25 90-degree cone
        SEnkyo = 33857, // SBoss->self, 5.0s cast, range 60 circle, raidwide
        SSplittingCry = 33858, // SBoss->self/players, 5.0s cast, range 60 width 14 rect tankbuster
        SSlither = 33859, // SBoss->self, 2.0s cast, range 25 90-degree cone

        NStormcloudSummons = 33784, // NBoss->self, 3.0s cast, single-target, visual (summon clouds)
        NSmokeaterFirst = 33785, // NBoss->self, 2.5s cast, single-target, visual (first breath in)
        NSmokeaterRest = 33786, // NBoss->self, no cast, single-target, visual (optional second/third breath in)
        NSmokeaterAbsorb = 33787, // NRaiun->NBoss, no cast, single-target, visual (absorb cloud)
        NRokujoRevelFirst = 33788, // NBoss->self, 7.5s cast, single-target, visual (first line aoe)
        NRokujoRevelRest = 33789, // NBoss->self, no cast, single-target, visual (second/third line aoe)
        NRokujoRevelAOE = 33790, // Helper->self, 8.0s cast, range 60 width 14 rect
        NLeapingLevin1 = 33791, // NRaiun->self, 1.0s cast, range 8 circle
        NLeapingLevin2 = 33792, // NRaiun->self, 1.0s cast, range 12 circle
        NLeapingLevin3 = 33793, // NRaiun->self, 1.0s cast, range 23 circle
        NLightningBolt = 33794, // NBoss->self, 3.0s cast, single-target, visual (start multiple lines)
        NLightningBoltAOE = 33795, // Helper->location, 4.0s cast, range 6 circle
        NCloudToCloud1 = 33796, // NRaiun->self, 2.5s cast, range 100 width 2 rect
        NCloudToCloud2 = 33797, // NRaiun->self, 4.0s cast, range 100 width 6 rect
        NCloudToCloud3 = 33798, // NRaiun->self, 4.0s cast, range 100 width 12 rect
        SStormcloudSummons = 33823, // SBoss->self, 3.0s cast, single-target, visual
        SSmokeaterFirst = 33824, // SBoss->self, 2.5s cast, single-target, visual (first breath in)
        SSmokeaterRest = 33825, // SBoss->self, no cast, single-target, visual (optional second/third breath in)
        SSmokeaterAbsorb = 33826, // SRaiun->SBoss, no cast, single-target, visual (absorb cloud)
        SRokujoRevelFirst = 33827, // SBoss->self, 7.5s cast, single-target, visual (first line aoe)
        SRokujoRevelRest = 33828, // SBoss->self, no cast, single-target, visual (second/third line aoe)
        SRokujoRevelAOE = 33829, // Helper->self, 8.0s cast, range 60 width 14 rect
        SLeapingLevin1 = 33830, // SRaiun->self, 1.0s cast, range 8 circle
        SLeapingLevin2 = 33831, // SRaiun->self, 1.0s cast, range 12 circle
        SLeapingLevin3 = 33832, // SRaiun->self, 1.0s cast, range 23 circle
        SLightningBolt = 33833, // SBoss->self, 3.0s cast, single-target, visual (start multiple lines)
        SLightningBoltAOE = 33834, // Helper->location, 4.0s cast, range 6 circle
        SCloudToCloud1 = 33835, // SRaiun->self, 2.5s cast, range 100 width 2 rect
        SCloudToCloud2 = 33836, // SRaiun->self, 4.0s cast, range 100 width 6 rect
        SCloudToCloud3 = 33837, // SRaiun->self, 4.0s cast, range 100 width 12 rect

        NNoblePursuitFirst = 33799, // NBoss->location, 8.0s cast, width 12 rect charge
        NNoblePursuitRest = 33800, // NBoss->location, no cast, width 12 rect charge
        NLevinburst = 33801, // NRairin->self, no cast, range 10 width 100 rect
        SNoblePursuitFirst = 33838, // SBoss->location, 8.0s cast, width 12 rect charge
        SNoblePursuitRest = 33839, // SBoss->location, no cast, width 12 rect charge
        SLevinburst = 33840, // SRairin->self, no cast, range 10 width 100 rect

        NUnnaturalWail = 33815, // NBoss->self, 3.0s cast, single-target, visual (spread/stack debuffs)
        NUnnaturalAilment = 33816, // Helper->players, no cast, range 6 circle spread
        NUnnaturalForce = 33817, // Helper->players, no cast, range 6 circle 2-man stack
        NHauntingCry = 33802, // NBoss->self, 3.0s cast, single-target, visual (spawn adds)
        NRightSwipe = 33803, // NDevilishThrall->self, 10.0s cast, range 40 180-degree cone
        NLeftSwipe = 33804, // NDevilishThrall->self, 10.0s cast, range 40 180-degree cone
        NEyeOfTheThunderVortexFirst = 33811, // NBoss->self, 5.2s cast, range 15 circle
        NEyeOfTheThunderVortexSecond = 33812, // NBoss->self, no cast, range 8-30 donut
        NVortexOfTheThunderEyeFirst = 33813, // NBoss->self, 5.2s cast, range 8-30 donut
        NVortexOfTheThunderEyeSecond = 33814, // NBoss->self, no cast, range 15 circle
        SUnnaturalWail = 33854, // SBoss->self, 3.0s cast, single-target, visual (spread/stack debuffs)
        SUnnaturalAilment = 33855, // Helper->players, no cast, range 6 circle spread
        SUnnaturalForce = 33856, // Helper->players, no cast, range 6 circle 2-man stack
        SHauntingCry = 33841, // SBoss->self, 3.0s cast, single-target, visual (spawn adds)
        SRightSwipe = 33842, // SDevilishThrall->self, 10.0s cast, range 40 180-degree cone
        SLeftSwipe = 33843, // SDevilishThrall->self, 10.0s cast, range 40 180-degree cone
        SEyeOfTheThunderVortexFirst = 33850, // SBoss->self, 5.2s cast, range 15 circle
        SEyeOfTheThunderVortexSecond = 33851, // SBoss->self, no cast, range 8-30 donut
        SVortexOfTheThunderEyeFirst = 33852, // SBoss->self, 5.2s cast, range 8-30 donut
        SVortexOfTheThunderEyeSecond = 33853, // SBoss->self, no cast, range 15 circle

        NReisho = 33805, // Helper->self, no cast, range 6 circle
        NVengefulSouls = 33806, // NBoss->self, 15.0s cast, single-target, visual (towers/defamations)
        NVermilionAura = 33807, // Helper->self, 15.0s cast, range 4 circle tower
        NStygianAura = 33808, // Helper->players, 15.0s cast, range 15 circle spread
        NUnmitigatedExplosion = 33809, // Helper->self, 1.0s cast, range 60 circle unsoaked tower
        SReisho = 33844, // Helper->self, no cast, range 6 circle
        SVengefulSouls = 33845, // SBoss->self, 15.0s cast, single-target, visual (towers/defamations)
        SVermilionAura = 33846, // Helper->self, 15.0s cast, range 4 circle tower
        SStygianAura = 33847, // Helper->players, 15.0s cast, range 15 circle spread
        SUnmitigatedExplosion = 33848, // Helper->self, 1.0s cast, range 60 circle unsoaked tower

        NThunderVortex = 33810, // NBoss->self, 5.0s cast, range 8-30 donut
        SThunderVortex = 33849, // SBoss->self, 5.0s cast, range 8-30 donut

        NEnrage = 33822, // NBoss->self, 10.0s cast, range 60 circle enrage
        SEnrage = 33861, // SBoss->self, 10.0s cast, range 60 circle enrage
        NEnrageRepeat = 33981, // NBoss->self, no cast, range 60 circle
        SEnrageRepeat = 33982, // SBoss->self, no cast, range 60 circle
    };

    public enum SID : uint
    {
        ScatteredWailing = 3563, // *Boss->player, extra=0x0
        IntensifiedWailing = 3564, // *Boss->player, extra=0x0
        //_Gen_ = 2056, // none->*DevilishThrall, extra=0xE1
    };
}
