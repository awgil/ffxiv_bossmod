namespace BossMod.Endwalker.FATE.Chi
{
    public enum OID : uint
    {
        Boss = 0x34CB, // R=16.0
        Helper1 = 0x34CC, //R=0.5
        Helper2 = 0x34CD, //R=0.5
        Helper3 = 0x361E, //R=0.5
        Helper4 = 0x364C, //R=0.5
        Helper5 = 0x364D, //R=0.5
        Helper6 = 0x3505, //R=0.5
    };

    public enum AID : uint
    {
        AutoAttack = 25952, // Boss->player, no cast, single-target
        AssaultCarapace = 25954, // Boss->self, 5,0s cast, range 120 width 32 rect
        AssaultCarapace2 = 25173, // Boss->location, 8,0s cast, range 120 width 32 rect
        Carapace_RearGuns2dot0A = 25958, // Boss->self, 8,0s cast, range 120 width 32 rect
        Carapace_ForeArms2dot0A = 25957, // Boss->self, 8,0s cast, range 120 width 32 rect
        AssaultCarapace3 = 25953, // Boss->self, 5,0s cast, range 16-60 donut
        Carapace_ForeArms2dot0B = 25955, // Boss->self, 8,0s cast, range 16-60 donut
        Carapace_RearGuns2dot0B = 25956, // Boss->self, 8,0s cast, range 16-60 donut
        ForeArms = 25959, // Boss->self, 6,0s cast, range 45 180-degree cone
        ForeArms2 = 26523, // Boss->location, 6,0s cast, range 45 180-degree cone
        Hellburner = 25971, // Boss->self, no cast, single-target, circle tankbuster
        Hellburner2 = 25972, // Helper1->players, 5,0s cast, range 5 circle
        FreeFallBombs = 25967, // Boss->self, no cast, single-target
        FreeFallBombs2 = 25968, // Helper1->location, 3,0s cast, range 6 circle
        MissileShower = 25969, // Boss->self, 4,0s cast, single-target
        MissileShower2 = 25970, // Helper2->self, no cast, range 30 circle
        RearGuns = 25962, // Boss->self, 6,0s cast, range 45 180-degree cone
        RearGuns2 = 26524, // Boss->location, 6,0s cast, range 45 180-degree cone
        RearGuns_ForeArms2dot0 = 25963, // Boss->self, 6,0s cast, range 45 180-degree cone
        ForeArms_RearGuns2dot0 = 25960, // Boss->self, 6,0s cast, range 45 180-degree cone
        ForeArms2dot0 = 25961, // Boss->self, no cast, range 45 180-degree cone
        RearGuns2dot0 = 25964, // Boss->self, no cast, range 45 180-degree cone
        Teleport = 25155, // Boss->location, no cast, single-target, boss teleports mid
        BunkerBuster = 25975, // Boss->self, 3,0s cast, single-target
        BunkerBuster2 = 25101, // Helper3->self, 10,0s cast, range 20 width 20 rect
        BunkerBuster3 = 25976, // Helper6->self, 12,0s cast, range 20 width 20 rect
        BouncingBomb = 27484, // Boss->self, 3,0s cast, single-target
        BouncingBomb2 = 27485, // Helper4->self, 5,0s cast, range 20 width 20 rect
        BouncingBomb3 = 27486, // Helper5->self, 1,0s cast, range 20 width 20 rect
        ThermobaricExplosive = 25965, // Boss->self, 3,0s cast, single-target
        ThermobaricExplosive2 = 25966, // Helper1->location, 10,0s cast, range 55 circle, damage fall off AOE
    };

    public enum IconID : uint
    {
        Tankbuster = 243, // player
        BunkerbusterTelegraph = 292, // Helper3/Helper6
    };


    class ChiStates : StateMachineBuilder
    {
        public ChiStates(BossModule module) : base(module)
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

    [ModuleInfo(FateID = 1855)]
    public class Chi : BossModule
    {
        public Chi(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(650, 0), 30)) { }
    }
}
