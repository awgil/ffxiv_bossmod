namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    public enum OID : uint
    {
        Nymeia = 0x3CE0, // R7.128, x1
        Althyk = 0x3CDF, // R8.960, x1
        Helper = 0x233C, // R0.500, x17
        HydrostasisQuick = 0x3E70, // R0.500, x1
    };

    public enum AID : uint
    {
        AutoAttackAlthyk = 32830, // Althyk->player, no cast, single-target
        AutoAttackNymeia = 32831, // Nymeia->player, no cast, single-target
        TeleportAlthyk = 31306, // Althyk->location, no cast, single-target
        TeleportNymeia = 31307, // Nymeia->location, no cast, single-target
        Philotes = 31285, // Nymeia->Althyk, no cast, single-target, equalize hp
        MythrilGreataxe = 31302, // Althyk->self, 7.0s cast, range 71 60-degree cone
        SpinnersWheel = 31288, // Nymeia->self, 4.5s cast, single-target, visual (debuffs)
        SpinnersWheelArcaneAttraction = 31316, // Helper->self, no cast, range 75 circle (gaze)
        SpinnersWheelAttractionReversed = 31317, // Helper->self, no cast, range 75 circle (inverse gaze)
        TimeAndTide = 31289, // Althyk->self, 10.0s cast, single-target, visual (speed up time)
        Axioma = 31303, // Althyk->self, 5.0s cast, range 72 circle, raidwide + adds dark lines
        Hydroptosis = 31300, // Nymeia->self, 5.0s cast, single-target, visual (spread)
        HydroptosisAOE = 31301, // Helper->players, 5.0s cast, range 6 circle spread
        InexorablePull = 31298, // Althyk->self, 6.0s cast, single-target, visual (kick up everyone not in dark zone)
        InexorablePullAOE = 31299, // Helper->self, 6.7s cast, range 72 circle
        Hydrorythmos = 31295, // Nymeia->self, 5.0s cast, single-target, visual (expanding rect)
        HydrorythmosFirst = 31296, // Helper->self, 5.0s cast, range 50 width 10 rect
        HydrorythmosRest = 31297, // Helper->self, no cast, range 50 width 5 rect
        Petrai = 31304, // Althyk->player, 5.0s cast, single-target, visual (shared tankbuster)
        PetraiAOE = 31305, // Helper->player, no cast, range 6 circle tankbuster
        Hydrostasis = 31290, // Nymeia->self, 4.0s cast, single-target, visual (knockbacks)
        HydrostasisAOE1 = 31291, // Helper->self, 16.0s cast, range 72 circle, knockback 28
        HydrostasisAOE2 = 31292, // Helper->self, 19.0s cast, range 72 circle, knockback 28
        HydrostasisAOE3 = 31293, // Helper->self, 22.0s cast, range 72 circle, knockback 28
        HydrostasisAOE0 = 31294, // Helper->self, 2.0s cast, range 72 circle, knockback 28
        HydrostasisAOEDelayed = 32698, // Helper->self, 11.0s cast, range 72 circle, knockback 28 (happens if althyk is killed before mechanic is resolved)
    };

    public enum SID : uint
    {
        SiblingRevelry = 3389, // none->Althyk/Nymeia, extra=0x0
        ArcaneAttraction = 3385, // none->player, extra=0x0/0x28
        AttractionReversed = 3386, // none->player, extra=0x0
        ArcaneFever = 3387, // none->player, extra=0x0/0x28
        FeverReversed = 3388, // none->player, extra=0x0
        Pyretic = 3522, // none->player, extra=0x0
        FreezingUp = 3523, // none->player, extra=0x0
        Heavy = 2551, // none->player, extra=0x19
    };

    public enum IconID : uint
    {
        SpinnersWheel = 379, // player
        TimeAndTide = 380, // player
        Hydroptosis = 139, // player
        Petrai = 259, // player
    };

    public enum TetherID : uint
    {
        SiblingRevelry = 12, // Althyk->Nymeia
        SpinnersWheelReversed = 220, // player->Nymeia
        SpinnersWheel = 221, // player->Nymeia
        TimeAndTide = 223, // player->Althyk
        HydrostasisQuick = 219, // HydrostasisQuick->Althyk
    };
}
