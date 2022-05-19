namespace BossMod.Endwalker.Ultimate.DSW2
{
    public enum OID : uint
    {
        Boss = 0x313C, // king thordan - p2
        SerZephirin = 0x3130, // x1 - ??
        SerAdelphel = 0x3139, // x1 - ??
        SerGrinnaux = 0x313A, // x1 - ??
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
    };

    public enum SID : uint
    {
        None = 0,
    }

    public enum TetherID : uint
    {
        None = 0,
    }

    public enum IconID : uint
    {
        None = 0,
    }
}
