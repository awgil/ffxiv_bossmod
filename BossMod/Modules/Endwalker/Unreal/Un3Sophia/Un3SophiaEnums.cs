namespace BossMod.Endwalker.Unreal.Un3Sophia
{
    public enum OID : uint
    {
        Boss = 0x3E14, // R5.000, x1
        AionTeleos = 0x3E15, // R5.000, spawn during fight (copies)
        Barbelo = 0x3E16, // R5.000, x1 (detachable head)
        Demiurge2 = 0x3E17, // R0.750, spawn during fight
        Demiurge3 = 0x3E18, // R0.750, spawn during fight
        Demiurge1 = 0x3E19, // R0.750, spawn during fight
        Helper1 = 0x3E7A, // R0.500, x8
        Helper2 = 0x3E7B, // R0.500, x17, and more spawn during fight
        ArenaTilt = 0x1EA12C, // R2.000, x1, EventObj type
        RingOfPain = 0x1EA199, // R0.500, EventObj type, spawn during fight, voidzone
    };

    public enum AID : uint
    {
        AutoAttackDemi1 = 870, // Demiurge1->player, no cast, single-target
        AutoAttackDemi3 = 871, // Demiurge3->player, no cast, single-target
        AutoAttackDemi2 = 872, // Demiurge2->player, no cast, single-target
        AutoAttackBoss = 32189, // Boss->player, no cast, single-target
        RemoveLowerHead = 32157, // Boss->self, no cast, single-target, visual (remove head under boss)
        RestoreLowerHead = 32158, // Boss->self, no cast, single-target, visual (restore head under boss)

        Execute = 32164, // Boss->self, 5.0s cast, single-target, visual (copies execute stored mechanics)
        Duplicate = 32165, // AionTeleos->Boss, 3.0s cast, single-target, visual (copy mechanic cast by boss later)
        ThunderDonut = 32166, // Boss->self, 3.0s cast, range 5-15+R donut
        ExecuteDonut = 32167, // AionTeleos->self, 5.0s cast, range 5-15+R donut
        Aero = 32168, // Boss->self, 3.0s cast, range 5+R circle
        ExecuteAero = 32169, // AionTeleos->self, 5.0s cast, range 5+R circle
        ThunderCone = 32170, // Boss->self, 3.0s cast, range 15+R 90-degree cone
        ExecuteCone = 32171, // AionTeleos->self, 5.0s cast, range 15+R 90-degree cone
        DischordantDamnation1 = 32172, // Helper2->self, no cast, range 40 circle, bleed on raid if 77's stack
        DischordantCleansing1 = 32173, // Helper2->player, no cast, single-target, bleed on non-paired with icon 77
        DischordantDamnation2 = 32174, // Helper2->self, no cast, range 40 circle, infirmity on raid if 78's stack
        DischordantCleansing2 = 32175, // Helper2->player, no cast, single-target, infirmity on non-paired with icon 78
        DivineSpark = 32176, // Demiurge2->self, 4.0s cast, range 50 circle gaze (confuse if hit)
        GnosticRant = 32178, // Demiurge3->self, 4.0s cast, range 40+R 270-degree cone
        GnosticSpear = 32179, // Demiurge3->self, 3.0s cast, range 20+R width 4 rect
        RingOfPain = 32180, // Demiurge3->self, 3.0s cast, range 5 circle voidzone
        VerticalKenoma = 32181, // Demiurge1->self, 2.0s cast, single-target, front/back parry
        HorizontalKenoma = 32182, // Demiurge1->self, 2.0s cast, single-target, sides parry
        Revengeance = 32183, // Helper2->player, no cast, single-target, vuln + knockback 10 if hitting parry
        Infusion = 32159, // Demiurge1->players, 6.0s cast, width 10 rect charge, should be shared, knockback 15 if too far (?) from center line
        CloudyHeavens = 32184, // Boss->self, 3.0s cast, range 50+R circle raidwide
        LightDewShort = 32185, // Barbelo->self, 2.0s cast, range 50+R width 18 rect
        LightDewLong = 32186, // Barbelo->self, 7.0s cast, range 50+R width 18 rect
        Onrush = 32187, // Boss->self, 3.0s cast, range 50+R width 16 rect
        Gnosis = 32188, // Barbelo->self, 3.0s cast, range 50+R circle knockback 25
        ArmsOfWisdom = 32190, // Boss->player, 4.0s cast, single-target, tankbuster with knockback 5
        Cintamani = 32191, // Boss->self, no cast, range 45+R circle, raidwide

        Quasar1 = 32155, // Boss->self, 2.7s cast, single-target, visual
        ScalesOfWisdomStart = 32156, // Boss->self, no cast, range 60 circle, visual (phase transition)
        QuasarKnockbackMicro = 32160, // Helper1->player, no cast, single-target, knockback 9 in actor's direction (??? seen cast on dead people)
        Quasar2 = 32161, // Boss->self, 3.0s cast, single-target
        QuasarLight = 32162, // Helper2->location, 7.2s cast, range 1 circle, visual (red/normal meteor)
        QuasarHeavy = 32163, // Helper2->location, 7.2s cast, range 1 circle, visual (blue/heavy meteor)
        QuasarTilt = 32192, // Helper2->self, no cast, range 80 circle, visual
        QuasarProximity1 = 32193, // Helper2->location, 5.0s cast, range 15 circle with ~15 falloff
        QuasarProximity2 = 32194, // Helper2->location, 5.0s cast, range 30 circle with ~25 falloff
        QuasarKnockbackShort = 32195, // Helper1->player, no cast, single-target, knockback 28 in actor's direction
        QuasarKnockbackLong = 32196, // Helper1->player, no cast, single-target, knockback 37 in actor's direction
        ScalesOfWisdomRaidwide = 32198, // Helper2->self, no cast, range 80+R circle, raidwide

        Enrage = 32197, // Boss->self, no cast, range 45+R circle
    };

    public enum IconID : uint
    {
        Pairs1 = 77, // player (dunno whether white or black)
        Pairs2 = 78, // player (dunno whether white or black)
    };
}
