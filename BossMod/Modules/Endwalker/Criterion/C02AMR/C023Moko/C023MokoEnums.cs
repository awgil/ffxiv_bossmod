namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko
{
    public enum OID : uint
    {
        NBoss = 0x3F89, // R6.000, x1
        NMokoShadow = 0x3F8A, // R6.000, x8
        NAshigaruKyuhei = 0x3F8B, // R1.000, x6
        NOniClaw = 0x3F8C, // R13.200, x6
        NAncientKatana = 0x3F8D, // R1.500, x2

        SBoss = 0x3F8E, // R6.000, x1
        SMokoShadow = 0x3F8F, // R6.000, x8
        SAshigaruKyuhei = 0x3F90, // R1.000, x6
        SOniClaw = 0x3F91, // R13.200, x6
        SAncientKatana = 0x3F92, // R1.500, x2

        Helper = 0x233C, // R0.500, x20
        SoldierOfDeath = 0x1EB8C9, // R0.500, EventObj type, spawn during fight - soldiers of death mechanic
    };

    public enum AID : uint
    {
        AutoAttack = 34055, // *Boss->player, no cast, single-target
        Teleport = 34223, // Boss->location, no cast, single-target
        //_Weaponskill_ = 34591, // *Boss->self, no cast, single-target, ???
        StopMoving = 34592, // *Boss->self, no cast, single-target, visual (???)

        NKenkiRelease = 34272, // NBoss->self, 5.0s cast, range 60 circle, raidwide
        NLateralSlice = 34275, // NBoss->self/player, 5.0s cast, range 40 ?-degree cone
        SKenkiRelease = 34316, // SBoss->self, 5.0s cast, range 60 circle, raidwide
        SLateralSlice = 34317, // SBoss->self/player, 5.0s cast, range 40 ?-degree cone

        NTripleKasumiGiriOutFrontFirst = 34224, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutRightFirst = 34225, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutBackFirst = 34226, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutLeftFirst = 34227, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInFrontFirst = 34234, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInRightFirst = 34235, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInBackFirst = 34236, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInLeftFirst = 34237, // NBoss->self, 12.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutFrontRest = 34228, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutRightRest = 34229, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutBackRest = 34230, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriOutLeftRest = 34231, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInFrontRest = 34238, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInRightRest = 34239, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInBackRest = 34240, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NTripleKasumiGiriInLeftRest = 34241, // NBoss->self, 1.0s cast, range 60 270-degree cone
        NUnboundSpirit = 34232, // Helper->self, no cast, range 6 circle (kasumi-giri out)
        NAzureCoil = 34233, // Helper->self, no cast, range 6-40 donut (kasumi-giri in)
        STripleKasumiGiriOutFrontFirst = 34276, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutRightFirst = 34277, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutBackFirst = 34278, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutLeftFirst = 34279, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriInFrontFirst = 34286, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriInRightFirst = 34287, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriInBackFirst = 34288, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriInLeftFirst = 34289, // SBoss->self, 12.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutFrontRest = 34280, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutRightRest = 34281, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutBackRest = 34282, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriOutLeftRest = 34283, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriInFrontRest = 34290, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriInRightRest = 34291, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriInBackRest = 34292, // SBoss->self, 1.0s cast, range 60 270-degree cone
        STripleKasumiGiriInLeftRest = 34293, // SBoss->self, 1.0s cast, range 60 270-degree cone
        SUnboundSpirit = 34284, // Helper->self, no cast, range 6 circle (kasumi-giri out)
        SAzureCoil = 34285, // Helper->self, no cast, range 6-40 donut (kasumi-giri in)

        NScarletAuspice = 34257, // NBoss->self, 5.0s cast, range 6 circle
        SScarletAuspice = 34304, // SBoss->self, 5.0s cast, range 6 circle
        BoundlessScarlet = 34201, // *Boss->self, 2.4s cast, single-target, visual (expanding lines)
        NBoundlessScarletAOE = 34258, // Helper->self, 3.0s cast, range 60 width 10 rect
        NBoundlessScarletExplosion = 34259, // Helper->self, 12.0s cast, range 60 width 30 rect (expanded line)
        SBoundlessScarletAOE = 34305, // Helper->self, 3.0s cast, range 60 width 10 rect
        SBoundlessScarletExplosion = 34306, // Helper->self, 12.0s cast, range 60 width 30 rect (expanded line)
        InvocationOfVengeance = 34267, // *Boss->self, 3.0s cast, single-target, visual (tether)
        FleetingIaiGiri = 34242, // *Boss->self, 9.0s cast, single-target, visual (jumping cleave)
        FleetingIaiGiriJump = 34243, // *Boss->location, no cast, single-target, teleport dist 3 behind target
        NFleetingIaiGiriFront = 34244, // Boss->self, 1.0s cast, range 60 270-degree cone
        NFleetingIaiGiriRight = 34245, // Boss->self, 1.0s cast, range 60 270-degree cone
        NFleetingIaiGiriLeft = 34246, // Boss->self, 1.0s cast, range 60 270-degree cone
        SFleetingIaiGiriFront = 34294, // Boss->self, 1.0s cast, range 60 270-degree cone
        SFleetingIaiGiriRight = 34295, // Boss->self, 1.0s cast, range 60 270-degree cone
        SFleetingIaiGiriLeft = 34296, // Boss->self, 1.0s cast, range 60 270-degree cone
        NVengefulFlame = 34268, // Helper->players, no cast, range 3 circle spread
        NVengefulPyre = 34269, // Helper->players, no cast, range 3 circle 2-man stack
        SVengefulFlame = 34312, // Helper->players, no cast, range 3 circle spread
        SVengefulPyre = 34313, // Helper->players, no cast, range 3 circle 2-man stack

        ShadowTwin = 34247, // Boss->self, 3.0s cast, single-target, visual (mechanic start)
        DoubleIaiGiri = 34248, // *MokoShadow->self, 12.0s cast, single-target, visual (jump + cleaves)
        ShadowKasumiGiriJump = 34249, // NMokoShadow->location, no cast, single-target
        NShadowKasumiGiriFrontFirst = 34250, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriRightFirst = 34251, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriBackFirst = 34252, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriLeftFirst = 34253, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriFrontFirst = 34297, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriRightFirst = 34298, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriBackFirst = 34299, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriLeftFirst = 34300, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriFrontSecond = 34499, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriRightSecond = 34500, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriBackSecond = 34501, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NShadowKasumiGiriLeftSecond = 34502, // NMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriFrontSecond = 34507, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriRightSecond = 34508, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriBackSecond = 34509, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        SShadowKasumiGiriLeftSecond = 34510, // SMokoShadow->self, 1.0s cast, range 23 270-degree cone
        NMoonlessNight = 34270, // NBoss->self, 3.0s cast, range 60 circle, raidwide
        SMoonlessNight = 34314, // SBoss->self, 3.0s cast, range 60 circle, raidwide
        FarEdge = 34264, // *Boss->self, 6.0s cast, single-target, visual (accursed edge on 2 farthest)
        NearEdge = 34265, // *Boss->self, 6.0s cast, single-target (accursed edge on 2 closest)
        NAccursedEdge = 34266, // NAncientKatana->self, no cast, range 6 circle
        SAccursedEdge = 34311, // SAncientKatana->self, no cast, range 6 circle
        // TODO: clarify meaning, add remaining spells incl. savage variants...
        NClearout1 = 35873, // NOniClaw->self, 6.0s cast, range 22 180-degree cone
        NClearout2 = 35879, // NOniClaw->self, no cast, range 22 180-degree cone
        NClearout3 = 34271, // NOniClaw->self, no cast, range 22 180-degree cone

        NAzureAuspice = 34260, // NBoss->self, 5.0s cast, range ?-40 donut
        SAzureAuspice = 34307, // SBoss->self, 5.0s cast, range ?-40 donut
        BoundlessAzure = 34205, // *Boss->self, 2.4s cast, single-target, visual (splitting lines)
        NBoundlessAzureAOE = 34261, // Helper->self, 3.0s cast, range 60 width 10 rect
        NUpwellFirst = 34262, // Helper->self, 7.0s cast, range 60 width 10 rect
        NUpwellRest = 34263, // Helper->self, no cast, range 60 width 5 rect
        SBoundlessAzureAOE = 34308, // Helper->self, 3.0s cast, range 60 width 10 rect
        SUpwellFirst = 34309, // Helper->self, 7.0s cast, range 60 width 10 rect
        SUpwellRest = 34310, // Helper->self, no cast, range 60 width 5 rect

        SoldiersOfDeath = 34195, // *Boss->self, 3.0s cast, single-target, visual (mechanic start)
        NIronRainFirst = 34255, // NAshigaruKyuhei->location, 15.0s cast, range 10 circle
        NIronStormFirst = 34256, // NAshigaruKyuhei->location, 15.0s cast, range 20 circle
        NIronRainSecond = 34727, // NAshigaruKyuhei->location, 1.0s cast, range 10 circle
        NIronStormSecond = 34728, // NAshigaruKyuhei->location, 1.0s cast, range 20 circle
        SIronRainFirst = 34302, // SAshigaruKyuhei->location, 15.0s cast, range 10 circle
        SIronStormFirst = 34303, // SAshigaruKyuhei->location, 15.0s cast, range 20 circle
        SIronRainSecond = 34729, // SAshigaruKyuhei->location, 1.0s cast, range 10 circle
        SIronStormSecond = 34730, // SAshigaruKyuhei->location, 1.0s cast, range 20 circle
        //_Weaponskill_KenkiRelease = 34254, // NMokoShadow->self, 3.0s cast, range 60 circle

        Enrage = 34273, // *Boss->self, 10.0s cast, range 60 circle
    };

    public enum SID : uint
    {
        Giri = 2970, // none->*Boss/*MokoShadow, extra=0x248 (front)/0x249 (right)/0x24A (back)/0x24B (left)/0x24C (out-front)/0x24D (out-right)/0x24E (out-back)/0x24F (out-left)/0x250 (in-front)/0x251 (in-right)/0x252 (in-back)/0x253 (in-left)
        RatAndMouse = 3609, // none->player, extra=0x0, jump target
        VengefulFlame = 3610, // none->player, extra=0x0, spread
        VengefulPyre = 3611, // none->player, extra=0x0, stack
        //_Gen_ = 2193, // none->*Boss/*MokoShadow, extra=0x268/0x267/0x266/0xE1
        //_Gen_ = 2056, // none->*OniClaw/*Boss/*AshigaruKyuhei, extra=0x257/0x26C/0x26D/0x1E8/0x5E
        //_Gen_Bind = 2518, // *AncientKatana->player, extra=0x0
    };

    public enum TetherID : uint
    {
        RatAndMouse = 17, // *Boss/*MokoShadow->player
        //_Gen_Tether_1 = 1, // *MokoShadow->*MokoShadow
    };
}
