namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    public enum OID : uint
    {
        Boss = 0x3135, // R4.000, x1
        SeekerAvatar = 0x3136, // R4.000, x7
        Helper = 0x233C, // R0.500, x16
        AetherialOrb = 0x3137, // R2.000, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack1 = 6497, // Boss->player, no cast, single-target
        AutoAttack2 = 6499, // Boss->player, no cast, single-target (fist form)
        Teleport = 23194, // Boss/SeekerAvatar->location, no cast, single-target, teleport

        VerdantTempest = 23251, // Boss->self, 5.0s cast, range 50 circle, visual (raidwide)
        VerdantTempestAOE = 23347, // Helper->self, no cast, ???, raidwide (2 instances hitting half of raid each)
        ManifestAvatar = 23258, // Boss->self, 3.0s cast, single-target, visual (summon clone)

        VerdantPathKatana = 23191, // Boss->self, 3.0s cast, single-target, visual (switch to katana, cross aoe)
        ActOfMercy = 23247, // Boss->self, no cast, range 50 width 8 cross aoe
        FirstMercy = 23393, // Boss->self, 3.0s cast, single-target, visual (mercy hint)
        SecondMercy = 23394, // Boss->self, 3.0s cast, single-target, visual (mercy hint)
        ThirdMercy = 23395, // Boss->self, 3.0s cast, single-target, visual (mercy hint)
        FourthMercy = 23396, // Boss->self, 3.0s cast, single-target, visual (mercy hint)
        MercyFourfold = 23224, // Boss->self, 2.0s cast, single-target, visual (mercy resolve)
        MercyFourfoldAOE = 23444, // Helper->self, 1.7s cast, range 50 180-degree cone aoe
        MercyFourfold1 = 23225, // Boss->self, no cast, range 50 180-degree cone, visual (~0.1s after aoe ends with -45deg rotation)
        MercyFourfold2 = 23226, // Boss->self, no cast, range 50 180-degree cone, visual (~0.1s after aoe ends with -135deg rotation)
        MercyFourfold3 = 23227, // Boss->self, no cast, range 50 180-degree cone, visual (~0.1s after aoe ends with +45deg rotation)
        MercyFourfold4 = 23228, // Boss->self, no cast, range 50 180-degree cone, visual (~0.1s after aoe ends with +135deg rotation)
        SeasonsOfMercy = 23239, // SeekerAvatar/Boss->self, 5.0s cast, single-target, visual (crisscross + gaze + big aoe)
        MercifulBreeze = 23240, // Helper->self, 2.5s cast, range 50 width 5 rect aoe (crisscross)
        MercifulMoon = 23241, // AetherialOrb->self, no cast, range 50 circle gaze
        MercifulBlooms = 23242, // Helper->self, 9.0s cast, range 4 circle aoe (real radius is 20 due to influence up)
        MercifulArc = 23252, // Boss->self, no cast, range 12 ?-degree cone cleave

        VerdantPathSword = 23192, // Boss->self, 3.0s cast, single-target, visual (switch to sword, side aoes)
        BalefulSwathe = 23248, // Boss->self, no cast, single-target, visual (side aoes)
        BalefulSwatheAOE = 23249, // Helper->self, no cast, range 50 ?-degree cone (doesn't really look like cone...)
        BalefulOnslaught = 23690, // Boss->self, 4.0s cast, single-target, visual (tankbuster, shareable or skipping closest target)
        BalefulOnslaughtAOE1 = 23253, // Boss->self, no cast, range 10 ?-degree cone tankbuster (shareable/invulable)
        BalefulOnslaughtAOE2 = 23254, // Boss->self, no cast, range 10 ?-degree cone tankbuster (solo, skipping closest target)
        PhantomEdge = 23229, // Boss->self, 4.0s cast, single-target, visual (applies status changing some effects)
        ScorchingShackle = 23243, // Helper->self, no cast, ??? (happens if chains aren't broken in time)
        BalefulBlade1 = 23230, // Boss->self, 8.0s cast, range 30 circle, visual (knockback, 'blockable' variant)
        BalefulBlade2 = 23231, // Boss->self, 8.0s cast, range 30 circle, visual (knockback, 'unblockable' variant)
        BalefulBladeAOE1 = 23338, // Helper->self, no cast, ???, LOSable knockback 30
        BalefulBladeAOE2 = 23339, // Helper->self, no cast, ???, knockback 30
        BalefulComet = 23255, // SeekerAvatar->self, no cast, range 12 circle aoe (clone jumps before firestorm)
        BalefulFirestorm = 23256, // SeekerAvatar->self, 1.0s cast, range 50 width 20 rect aoe

        VerdantPathFist = 23193, // Boss->self, 3.0s cast, single-target, visual (switch to fists, line stack)
        IronImpact = 23250, // Boss->self, no cast, range 50 width 8 rect line stack
        IronRose = 23257, // SeekerAvatar->self, 3.5s cast, range 50 width 8 rect aoe
        IronSplitter = 23232, // Boss/SeekerAvatar->self, 5.0s cast, single-target, visual (tiles/sands)
        IronSplitterTile1 = 23233, // Helper->self, no cast, range 4 circle
        IronSplitterTile2 = 23234, // Helper->self, no cast, range 8-12 donut
        IronSplitterTile3 = 23235, // Helper->self, no cast, range 16-20 donut
        IronSplitterSand1 = 23236, // Helper->self, no cast, range 4-8 donut
        IronSplitterSand2 = 23237, // Helper->self, no cast, range 12-16 donut
        IronSplitterSand3 = 23238, // Helper->self, no cast, range 20-25 donut
        DeadIron = 23244, // SeekerAvatar->self, 4.0s cast, single-target, visual (earthshakers)
        DeadIronAOE = 23245, // Helper->self, no cast, range 50 30-degree cone earthshaker
        DeadIronSecond = 23364, // SeekerAvatar->self, no cast, single-target, visual (second earthshakers, without cast)
    };

    public enum SID : uint
    {
        Mercy = 2056, // none->Boss, extra=0xF7 (-45 deg)/0xF8 (-135 deg)/0xF9 (+45deg)/0xFA (+135 deg)
        //MercifulAir = 2489, // Boss->SeekerAvatar/Boss, extra=0x194
        //BalefulAir = 2490, // Boss->Boss/SeekerAvatar, extra=0x195
        //IronAir = 2491, // Boss->Boss/SeekerAvatar, extra=0x196
        //PhantomEdge = 2488, // Boss->Boss, extra=0x0
        //BurningChains = 769, // none->player, extra=0x0
        //AreaOfInfluenceUp = 1749, // none->Helper, extra=0x10
    };

    public enum TetherID : uint
    {
        //_Gen_Tether_128 = 128, // player->player
        DeadIron = 138, // player->SeekerAvatar
    };
}
