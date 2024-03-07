namespace BossMod.Endwalker.FATE.Daivadipa
{
    public enum OID : uint
    {
        Boss = 0x356D, // R=8.0
        OrbOfImmolation = 0x3570, //R=1.0
        OrbOfImmolation2 = 0x356F, //R=1.0
        OrbOfConflagration = 0x3572, //R=1.0
        OrbOfConflagration2 = 0x3571, //R=1.0
        Helper1 = 0x3573, //R=0.5
        Helper2 = 0x3574, //R=0.5
        Helper3 = 0x3575, //R=0.5
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Drumbeat = 26510, // Boss->player, 5,0s cast, single-target
        LeftwardTrisula = 26508, // Boss->self, 7,0s cast, range 65 180-degree cone
        RightwardParasu = 26509, // Boss->self, 7,0s cast, range 65 180-degree cone
        Lamplight = 26497, // Boss->self, 2,0s cast, single-target
        LoyalFlame = 26499, // Boss->self, 5,0s cast, single-target, blue first
        LoyalFlame2 = 26498, // Boss->self, 5,0s cast, single-target, red first
        LitPath1 = 26501, // OrbOfImmolation->self, 1,0s cast, range 50 width 10 rect
        LitPath2 = 26500, // OrbOfImmolation2->self, 1,0s cast, range 50 width 10 rect
        CosmicWeave = 26513, // Boss->self, 4,0s cast, range 18 circle
        YawningHells = 26511, // Boss->self, no cast, single-target
        YawningHells2 = 26512, // Helper1->location, 3,0s cast, range 8 circle
        ErrantAkasa = 26514, // Boss->self, 5,0s cast, range 60 90-degree cone
        InfernalRedemption = 26517, // Boss->self, 5,0s cast, single-target
        InfernalRedemption2 = 26518, // Helper3->location, no cast, range 60 circle
        IgnitingLights = 26503, // Boss->self, 2,0s cast, single-target
        IgnitingLights2 = 26502, // Boss->self, 2,0s cast, single-target
        Burn = 26507, // OrbOfConflagration->self, 1,0s cast, range 10 circle
        Burn2 = 26506, // OrbOfConflagration2->self, 1,0s cast, range 10 circle
        KarmicFlames = 26515, // Boss->self, 5,5s cast, single-target
        KarmicFlames2 = 26516, // Helper2->location, 5,0s cast, range 50 circle, damage fall off, safe distance should be about 20
        DivineCall = 27080, // Boss->self, 4,0s cast, range 65 circle, forced backwards march
        DivineCall2 = 26520, // Boss->self, 4,0s cast, range 65 circle, forced right march
        DivineCall3 = 27079, // Boss->self, 4,0s cast, range 65 circle, forced forward march
        DivineCall4 = 26519, // Boss->self, 4,0s cast, range 65 circle, forced left march
    };

    public enum SID : uint
    {
        Hover = 1515, // none->OrbOfImmolation2, extra=0x64
        AboutFace = 1959, // Boss->player, extra=0x0
        RightFace = 1961, // Boss->player, extra=0x0
        ForwardMarch = 1958, // Boss->player, extra=0x0
        LeftFace = 1960, // Boss->player, extra=0x0
        ForcedMarch = 1257, // Boss->player, extra=0x2/0x8/0x1/0x4
    };


    class DaivadipaStates : StateMachineBuilder
    {
        public DaivadipaStates(BossModule module) : base(module)
        {
            TrivialPhase();
                // .ActivateOnEnter<BlizzardBreath>()
                // .ActivateOnEnter<FlamesOfTheApocalypse>()
                // .ActivateOnEnter<MindBlast>()
                // .ActivateOnEnter<Megaflare>()
                // .ActivateOnEnter<TidalWave>()
                // .ActivateOnEnter<WindSlash>()
                // .ActivateOnEnter<Windwinder>()
                // .ActivateOnEnter<CivilizationBuster1>()
                // .ActivateOnEnter<CivilizationBuster2>()
                // .ActivateOnEnter<Touchdown>()
                // .ActivateOnEnter<PillarImpact>()
                // .ActivateOnEnter<PillarPierce>()
                // .ActivateOnEnter<AncientAevis>()
                // .ActivateOnEnter<HeadlongRush>()
                // .ActivateOnEnter<Aether>();
        }
    }

    [ModuleInfo(FateID = 1763)]
    public class Daivadipa : BossModule
    {
        public Daivadipa(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-608, 811), 24.5f)) { }
    }
}
