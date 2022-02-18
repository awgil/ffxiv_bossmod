namespace BossMod.P2S
{
    public enum OID : uint
    {
        Boss = 0x359B,
        CataractHead = 0x359C,
        DissociatedHead = 0x386A,
        Helper = 0x233C, // x24
    };

    public enum AID : uint
    {
        SewageDeluge = 26640, // Boss->Boss
        SpokenCataractSecondary = 26641, // CHead->CHead, half behind is safe
        WingedCataractSecondaryDissoc = 26644, // CHead->CHead, half in front is safe
        WingedCataractSecondary = 26645, // CHead->CHead, half in front is safe
        SpokenCataract = 26647, // Boss->Boss
        WingedCataract = 26648, // Boss->Boss
        CoherenceAOE = 26640, // Boss->n/a, no cast, aoe around tether target
        Coherence = 26651, // Boss->Boss
        CoherenceRay = 26652, //Boss->Boss, no cast, ray on closest target
        ChannelingFlow = 26654, // Boss->Boss
        Crash = 26657, // Helper->Helper, attack after arrows resolve
        KampeosHarma = 26659, // Boss->Boss
        KampeosHarmaChargeBoss = 26660, // Boss->target, no cast
        KampeosHarmaChargeHead = 26661, // CHead->target, no cast, first 3 charges
        KampeosHarmaChargeLast = 26662, // CHead->n/a, no cast, 4th charge
        PredatoryAvarice = 26663, // Boss->Boss
        OminousBubbling = 26666, // Boss->Boss
        OminousBubblingAOE = 26667, // Helper->targets
        Dissociation = 26668, // Boss->Boss
        DissociationAOE = 26670, // DHead->DHead
        Shockwave = 26671, // Boss->impact location
        SewageEruption = 26672, // Boss->Boss
        SewageEruptionAOE = 26673, // Helper->Helper
        DoubledImpact = 26674, // Boss->MT
        MurkyDepths = 26675, // Boss->Boss
        Enrage = 26676, // Boss->Boss
        TaintedFlood = 26679, // Boss->Boss
        TaintedFloodAOE = 26680, // Helper->targets
        ChannelingOverflow = 28098, // Boss->Boss (both 2nd and 3rd arrows)
    };

    public enum SID : uint
    {
        MarkOfTides = 2768, // avarice - gtfo (tank+dd)
        MarkOfDepths = 2769, // avarice - stack (healer)
        MarkFlowN = 2770, // 'fore', points north
        MarkFlowS = 2771, // 'rear', points south
        MarkFlowW = 2772, // 'left', points west
        MarkFlowE = 2773, // 'right', points east
    }

    public enum TetherID : uint
    {
        Coherence = 84,
    }
}
