namespace BossMod.Endwalker.Ultimate.DSW2
{
    public enum OID : uint
    {
        Boss = 0x313C, // king thordan - p2
        SerZephirin = 0x3130, // x1 - ??
        SerAdelphel = 0x3139, // x1 - p1, p2
        SerGrinnaux = 0x313A, // x1 - p1, p2
        SerCharibert = 0x313B, // x1 - ??
        SerJanlenoux = 0x3158, // x1 - p2
        SerVellguine = 0x3159, // x1 - p2
        SerPaulecrain = 0x315A, // x1 - p2
        SerIgnasse = 0x315B, // x1 - p2
        SerHermenost = 0x315C, // x1 - p2
        SerGuerrique = 0x315D, // x1 - p2
        SerHaumeric = 0x315E, // x1 - p2
        SerNoudenet = 0x315F, // x1 - p2
        //_Gen_Actor_3316 = 0x3316, // x1
        Helper = 0x233C, // x24
    };

    public enum AID : uint
    {
        AutoAttack = 25531, // Boss->mt, no cast, range 10 ?-degree cone
        Teleport = 25540, // Boss->location, no cast

        AscalonsMercyConcealed = 25544, // Boss->self, 3.0s cast, visual
        AscalonsMercyConcealedAOE = 25545, // Helper->self, 1.5s cast, range 50 30-degree (?) cone
        AscalonsMight = 25541, // Boss->self, no cast, range 50 60-degree (?) cone tankbuster

        KnightsOfTheRound = 25581, // Boss->self, no cast, visual
        StrengthOfTheWard = 25555, // Boss->self, 4.0s cast, visual
        LightningStorm = 25548, // Boss->self, 5.7s cast, visual
        LightningStormAOE = 25549, // Helper->player, no cast, range 5 aoe
        SpiralThrust = 25556, // SerIgnasse/SerVellguine/SerPaulecrain->self, 6.0s cast, range 52 width 16 rect aoe
        HeavyImpact = 25557, // SerGuerrique->self, 4.3s cast, visual
        HeavyImpactHit1 = 25558, // Helper->self, 6.0s cast, range 6 aoe
        HeavyImpactHit2 = 25559, // Helper->self, no cast, range 6-12 donut
        HeavyImpactHit3 = 25560, // Helper->self, no cast, range 12-18 donut
        HeavyImpactHit4 = 25561, // Helper->self, no cast, range 18-24 donut
        HeavyImpactHit5 = 25562, // Helper->self, no cast, range 24-30 donut

        DragonsRage = 25550, // Boss->self, 4.7s cast, visual
        DragonsRageAOE = 25551, // Helper->players, no cast, range 8 shared aoe
        DimensionalCollapse = 25563, // SerGrinnaux->self, 8.0s cast, visual (growing void zones)
        DimensionalCollapseAOE = 25564, // Helper->location, 9.0s cast, range 10 aoe
        SkywardLeap = 25565, // SerIgnasse/SerVellguine/SerPaulecrain->player, no cast, range 24 aoe on player with blue mark
        Conviction = 25566, // SerHermenost->self, 8.2s cast, visual towers
        ConvictionAOE = 25567, // Helper->location, 11.0s cast, range 3 aoe, soaked towers
        EternalConviction = 25568, // Helper->self, no cast, raidwide from unsoaked towers
        HolyShieldBash = 25297, // SerJanlenoux/SerAdelphel->tethered player, no cast, width 8 rect tankbuster
        HolyShieldBashAOE = 25579, // Helper->self, no cast, range 6 ??-degree cone, ??
        HolyBladedanceVisual = 25298, // SerJanlenoux/SerAdelphel->self, no cast, visual
        HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, multiple hits)

        AncientQuaga = 25542, // Boss->self, 6.0s cast, raidwide
        HeavenlyHeel = 25543, // Boss->player, 4.0s cast, tankbuster forcing tankswap
    };

    public enum SID : uint
    {
        None = 0,
    }

    public enum TetherID : uint
    {
        None = 0,
        HolyShieldBash = 84, // SerJanlenoux/SerAdelphel->player
    }

    public enum IconID : uint
    {
        None = 0,
        SkywardLeap = 330, // player
    }
}
