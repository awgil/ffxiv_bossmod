namespace BossMod.Endwalker.Savage.P11SThemis
{
    public enum OID : uint
    {
        Boss = 0x3EF2, // R11.000, x1
        Helper = 0x233C, // R0.500, x28
        IllusoryThemis = 0x3EF3, // R11.000, x4 (clone)
        JuryOverrulingProteanHelper = 0x3EF4, // R11.000, x8
        MirrorLight = 0x3EF5, // R1.000, x4
        MirrorDark = 0x3EF6, // R1.000, x4
        SphereLight = 0x3EF7, // R1.000, x2
        SphereDark = 0x3EF8, // R1.000, x2
        ArcaneCylinder = 0x3EF9, // R1.000, x3
    };

    public enum AID : uint
    {
        AutoAttack = 33332, // Boss->player, no cast, single-target
        Teleport = 34431, // Boss->location, no cast, single-target
        Eunomia = 33323, // Boss->self, 5.0s cast, range 50 circle, raidwide with bleed
        Dike = 33325, // Boss->self, 7.0s cast, single-target, visual (tankbuster)
        DikeSecond = 33326, // Boss->self, no cast, single-target, visual (second hit)
        DikeAOE1Primary = 33327, // Helper->player, 7.0s cast, single-target (at MT)
        DikeAOE1Secondary = 33328, // Helper->player, 7.0s cast, single-target (at OT)
        DikeAOE2Primary = 33329, // Helper->player, no cast, single-target (at now-MT)
        DikeAOE2Secondary = 33330, // Helper->player, no cast, single-target (at now-OT)
        Styx = 33303, // Boss->players, 5.0s cast, range 6 circle stack
        StyxAOE = 33304, // Boss->players, no cast, range 6 circle stack (secondary hits)
        InevitableLaw = 33276, // Helper->players, no cast, range 6 circle, 4-man stacks on healers
        InevitableSentence = 33277, // Helper->players, no cast, range 3 circle, 2-man stacks on dps

        JuryOverrulingLight = 33254, // Boss->self, 6.0s cast, single-target, visual (proteans + baited circles + party stacks)
        JuryOverrulingDark = 33255, // Boss->self, 6.0s cast, single-target, visual (proteans + baited donuts + pair stacks)
        JuryOverrulingProteanLight = 33256, // JuryOverrulingProteanHelper->self, no cast, range 50 width 8 rect
        JuryOverrulingProteanDark = 33257, // JuryOverrulingProteanHelper->self, no cast, range 50 width 8 rect
        IllusoryGlare = 33258, // Helper->self, 3.1s cast, range 5 circle
        IllusoryGloom = 33259, // Helper->self, 3.1s cast, range 2-9 donut

        UpheldOverrulingLight = 34771, // Boss->self, 7.3s cast, single-target, visual (full stack + out + party stacks)
        UpheldOverrulingDark = 34772, // Boss->self, 7.3s cast, single-target, visual (aoe tankbuster + in + pair stacks)
        UpheldOverrulingAOELight = 33266, // Boss->players, no cast, range 6 circle, full party stack
        UpheldOverrulingAOEDark = 33267, // Boss->players, no cast, range 13 circle, tankbuster + knockback 50
        LightburstBoss = 33270, // Helper->self, 3.5s cast, range 13 circle
        DarkPerimeterBoss = 33271, // Helper->self, 3.5s cast, range 8-50 donut

        DivisiveOverrulingSoloLight = 33260, // Boss->self, 6.3s cast, single-target, visual (out + out + party stacks)
        DivisiveOverrulingSoloDark = 33261, // Boss->self, 6.3s cast, single-target, visual (out + in + pair stacks)
        DivisiveOverrulingSoloAOE = 33262, // Helper->self, 8.2s cast, range 46 width 16 rect (first 'out')
        DivineRuinationSolo = 33263, // Helper->self, 10.8s cast, range 46 width 26 rect (second 'out')
        RipplesOfGloomSoloR = 33264, // Helper->self, 10.8s cast, range 46 width 16 rect (second 'in', right side)
        RipplesOfGloomSoloL = 33265, // Helper->self, 10.8s cast, range 46 width 16 rect (second 'in', left side)

        ArcaneRevelationMirrorsLight = 33293, // Boss->self, 5.0s cast, single-target, visual (mirrors light)
        ArcaneRevelationMirrorsDark = 33294, // Boss->self, 5.0s cast, single-target, visual (mirrors dark)
        ArcheLight = 33299, // MirrorLight->self, 5.5s cast, range 50 width 10 rect
        ArcheDark = 33300, // MirrorDark->self, 5.5s cast, range 50 width 10 rect
        DismissalOverrulingLight = 34692, // Boss->self, 5.0s cast, single-target, visual (knockback + out + party stacks)
        DismissalOverrulingDark = 34693, // Boss->self, 5.0s cast, single-target, visual (knockback + in + pair stacks)
        DismissalOverrulingLightAOE = 34694, // Helper->self, 5.5s cast, range 40 circle, knockback 11
        DismissalOverrulingDarkAOE = 34695, // Helper->self, 5.5s cast, range 40 circle, knockback 11
        InnerLight = 34696, // Helper->self, 10.5s cast, range 13 circle
        OuterDark = 34697, // Helper->self, 10.5s cast, range 8-50 donut

        ShadowedMessengers = 33305, // Boss->self, 4.0s cast, single-target, visual (clones)
        DivisiveRulingLight = 33306, // IllusoryThemis->self, 8.7s cast, single-target, visual (light clone)
        DivisiveRulingDark = 33307, // IllusoryThemis->self, 8.7s cast, single-target, visual (dark clone)
        DivisiveRulingAOE = 33308, // Helper->self, 9.2s cast, range 46 width 16 rect (first 'out')
        DivineRuinationClone = 33309, // Helper->self, 12.4s cast, range 46 width 26 rect (second 'out')
        RipplesOfGloomCloneR = 33310, // Helper->self, 12.4s cast, range 46 width 16 rect (second 'in', right side)
        RipplesOfGloomCloneL = 33311, // Helper->self, 12.4s cast, range 46 width 16 rect (second 'in', left side)
        DivisiveOverrulingBossLight = 34739, // Boss->self, 7.8s cast, single-target, visual (together with clones, light version)
        DivisiveOverrulingBossDark = 34740, // Boss->self, 7.8s cast, single-target, visual (together with clones, dark version)
        DivisiveOverrulingBossAOE = 34741, // Helper->self, 9.7s cast, range 46 width 16 rect
        DivineRuinationBoss = 34742, // Helper->self, 12.3s cast, range 46 width 26 rect
        RipplesOfGloomBossR = 34743, // Helper->self, 12.3s cast, range 46 width 16 rect
        RipplesOfGloomBossL = 34744, // Helper->self, 12.3s cast, range 46 width 16 rect
        UpheldRulingLight = 34768, // IllusoryThemis->self, 10.8s cast, single-target, visual (light tether, full stack)
        UpheldRulingDark = 34769, // IllusoryThemis->self, 10.8s cast, single-target, visual (dark tether, tank gtfo)
        UpheldRulingAOELight = 33312, // IllusoryThemis->players, no cast, range 6 circle, 7-man stack
        UpheldRulingAOEDark = 33313, // IllusoryThemis->players, no cast, range 13 circle, tankbuster
        LightburstClone = 33316, // Helper->self, 3.5s cast, range 13 circle
        DarkPerimeterClone = 33317, // Helper->self, 3.5s cast, range 8-50 donut

        Lightstream = 33283, // Boss->self, 4.0s cast, single-target, visual (rotating orbs)
        LightstreamVisual = 33285, // Helper->self, 4.0s cast, single-target, ???
        LightstreamAOEFirst = 33287, // ArcaneCylinder->self, 8.0s cast, range 50 width 10 rect
        LightstreamAOERest = 33288, // ArcaneCylinder->self, no cast, range 50 width 10 rect

        DarkAndLight = 33278, // Boss->self, 4.0s cast, single-target, visual (tethers)
        KatakrisisLight = 33279, // Helper->player, no cast, single-target, damage when tethers fail
        KatakrisisDark = 33280, // Helper->player, no cast, single-target, damage when tethers fail
        ArcaneRevelationSpheresLight = 33295, // Boss->self, 5.0s cast, single-target
        ArcaneRevelationSpheresDark = 33296, // Boss->self, 5.0s cast, single-target
        UnluckyLotLight = 33301, // SphereLight->self, 5.5s cast, range 15 circle
        UnluckyLotDark = 33302, // SphereDark->self, 5.5s cast, range 15 circle
        EmissarysWill = 33282, // Boss->self, 4.0s cast, range 50 circle, kill fails

        DarkCurrent = 33284, // Boss->self, 4.0s cast, single-target, visual (rotating circle aoes)
        DarkCurrentVisual = 33286, // Helper->self, 4.0s cast, single-target, ???
        DarkCurrentAOEFirst = 33289, // Helper->self, 7.0s cast, range 8 circle
        DarkCurrentAOERest = 33290, // Helper->self, no cast, range 8 circle
        BlindingLight = 33321, // Boss->self, 5.0s cast, single-target, visual (spreads)
        BlindingLightAOE = 33322, // Helper->player, 5.0s cast, range 6 circle spread

        LetterOfTheLaw = 34770, // Boss->self, 4.0s cast, single-target, visual (clones, orbs, mirrors, jumps, towers, knockback, party/pair stacks)
        TwofoldRevelationLight = 33297, // Boss->self, 5.0s cast, single-target, visual (mirrors + orbs light)
        TwofoldRevelationDark = 33298, // Boss->self, 5.0s cast, single-target, visual (mirrors + orbs dark)
        HeartOfJudgment = 33318, // Boss->self, 3.0s cast, single-target, visual (towers)
        Explosion = 33319, // Helper->self, no cast, range 4 circle tower
        MassiveExplosion = 33320, // Helper->self, no cast, range 50 circle unsoaked tower

        UltimateVerdict = 33324, // Boss->self, 10.0s cast, range 50 circle enrage
    };

    public enum IconID : uint
    {
        Dike = 475, // player
        Styx = 305, // player
        RotateCW = 156, // ArcaneCylinder
        RotateCCW = 157, // ArcaneCylinder
        BlindingLight = 466, // player
    };

    public enum TetherID : uint
    {
        UpheldOverruling = 249, // Boss/IllusoryThemis->player
        LightLightGood = 236, // player->player (dist > ~20)
        LightLightBad = 237, // player->player (dist < ~20)
        DarkDarkGood = 238, // player->player
        DarkDarkBad = 239, // player->player
        DarkLightGood = 240, // player->player (dist < ~7)
        DarkLightBad = 241, // player->player (dist > ~7)
        //_Gen_Tether_245 = 245, // player->player - happens when one of the tethered player dies?
    };
}
