namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class RM12S2TheLindwurmStates : StateMachineBuilder
{
    public RM12S2TheLindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Replication1(id, 10.2f);
        Replication2(id + 0x10000, 11.2f);
        BloodMana(id + 0x20000, 15);
        IdyllicDream(id + 0x30000, 11.2f);
        DoubleSobat(id + 0x40000, 3.2f);
        ArcadianHell(id + 0x50000, 8.4f);
    }

    void Replication1(uint id, float delay)
    {
        Cast(id, AID.ArcadiaAflame, delay, 5, "Raidwide")
            .ActivateOnEnter<ArcadiaAflame>()
            .DeactivateOnExit<ArcadiaAflame>();

        Cast(id + 0x100, AID.Replication, 9.5f, 3)
            .ActivateOnEnter<Replication1SecondBait>()
            .ActivateOnEnter<WingedScourge>()
            .ActivateOnEnter<WingedScourgeSecond>()
            .ActivateOnEnter<MightyMagicTopTierSlamFirstBait>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(k => k.Risky = false);

        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x110, 11.6f, m => m.NumFire > 0, "Fire bait");
        ComponentCondition<WingedScourge>(id + 0x111, 0.8f, w => w.NumCasts > 0, "Cones");
        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x112, 0.3f, m => m.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamFirstBait>()
            .ExecOnExit<SnakingKick>(k => k.Risky = true);

        ComponentCondition<SnakingKick>(id + 0x120, 4.6f, k => k.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<SnakingKick>();

        ComponentCondition<WingedScourge>(id + 0x130, 12.6f, w => w.Casters.Count > 0)
            .DeactivateOnExit<WingedScourgeSecond>();

        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x140, 3.2f, b => b.NumFire > 0, "Fire baits");
        ComponentCondition<WingedScourge>(id + 0x141, 0.8f, w => w.NumCasts > 4, "Cones");
        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x142, 0.3f, b => b.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<Replication1SecondBait>()
            .DeactivateOnExit<WingedScourge>();

        DoubleSobat(id + 0x200, 2.4f);
    }

    void DoubleSobat(uint id, float delay)
    {
        CastStart(id, AID.DoubleSobatBoss1, delay)
            .ActivateOnEnter<DoubleSobatBuster>()
            .ActivateOnEnter<DoubleSobatRepeat>();
        ComponentCondition<DoubleSobatBuster>(id + 1, 5.6f, b => b.NumCasts > 0, "Half-room buster")
            .DeactivateOnExit<DoubleSobatBuster>();
        ComponentCondition<DoubleSobatRepeat>(id + 0x10, 4.6f, b => b.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<EsotericFinisher>()
            .DeactivateOnExit<DoubleSobatRepeat>()
            .ExecOnEnter<EsotericFinisher>(f => f.EnableHints = false)
            .ExecOnExit<EsotericFinisher>(f => f.EnableHints = true);

        ComponentCondition<EsotericFinisher>(id + 0x20, 2.5f, f => f.NumCasts > 0, "Double tankbuster")
            .DeactivateOnExit<EsotericFinisher>();
    }

    void Replication2(uint id, float delay)
    {
        Cast(id, AID.Staging, delay, 3)
            .ActivateOnEnter<Replication2Staging>();
        ComponentCondition<Replication2Staging>(id + 3, 7.9f, r => r.PlayersAssigned, "Player clones appear");
        Cast(id + 0x10, AID.Replication, 3.3f, 3);

        ComponentCondition<Replication2Staging>(id + 0x100, 16.5f, t => t.WurmsAssigned, "Clone tethers");
        CastStart(id + 0x101, AID.FirefallSplashCast, 0.1f)
            .ActivateOnEnter<Replication2FirefallSplash>()
            .ActivateOnEnter<Replication2ScaldingWaves>()
            .ActivateOnEnter<Replication2ManaBurst>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(k => k.Risky = false);

        ComponentCondition<Replication2FirefallSplash>(id + 0x110, 5.9f, r => r.NumCasts > 0, "Boss jumps (spread)")
            .DeactivateOnExit<Replication2FirefallSplash>();
        ComponentCondition<Replication2ScaldingWaves>(id + 0x111, 0.6f, r => r.NumCasts > 0, "Proteans");
        //.DeactivateOnExit<Replication2ScaldingWaves>(); // keep activated since we need to track targets
        ComponentCondition<Replication2ManaBurst>(id + 0x112, 1.4f, r => r.NumCasts > 0, "Defamations")
            .ActivateOnEnter<Replication2HeavySlam>()
            .ActivateOnEnter<Replication2HemorrhagicProjection>()
            .DeactivateOnExit<Replication2ManaBurst>();
        ComponentCondition<Replication2HeavySlam>(id + 0x113, 5.5f, r => r.NumCasts > 0, "Stacks")
            .ExecOnEnter<Replication2HeavySlam>(s => s.EnableHints = true)
            .DeactivateOnExit<Replication2HeavySlam>();
        ComponentCondition<Replication2HemorrhagicProjection>(id + 0x114, 1.8f, p => p.NumCasts > 0, "Cones")
            .ExecOnEnter<Replication2HemorrhagicProjection>(s => s.EnableHints = true)
            .DeactivateOnExit<Replication2HemorrhagicProjection>();

        ComponentCondition<SnakingKick>(id + 0x120, 3.8f, k => k.NumCasts > 0, "Half-room cleave")
            .ExecOnEnter<SnakingKick>(k => k.Risky = true)
            .DeactivateOnExit<SnakingKick>();

        Cast(id + 0x200, AID.Reenactment, 7.2f, 3)
            .ActivateOnEnter<Replication2ReenactmentOrder>()
            .ActivateOnEnter<Replication2ReenactmentAOEs>()
            .ActivateOnEnter<Replication2ReenactmentScaldingWaves>()
            .ActivateOnEnter<Replication2ReenactmentTowers>();

        CastMulti(id + 0x210, [AID.NetherwrathNear, AID.NetherwrathFar], 3.2f, 5)
            .ActivateOnEnter<Replication2TimelessSpite>();
        ComponentCondition<Replication2TimelessSpite>(id + 0x220, 1.2f, s => s.NumCasts > 0, "Close/far stack + reenactment start")
            .DeactivateOnExit<Replication2ScaldingWaves>()
            .DeactivateOnExit<Replication2TimelessSpite>()
            .DeactivateOnExit<Replication2ReenactmentOrder>();
    }

    void BloodMana(uint id, float delay)
    {
        // reenactment casts are 4s apart, but actual AOEs trigger later, varying by mechanic
        // if we put this at the end of rep2 instead of here, it fucks up state transition text
        Timeout(id, delay)
            .DeactivateOnExit<Replication2ReenactmentTowers>()
            .DeactivateOnExit<Replication2ReenactmentScaldingWaves>()
            .DeactivateOnExit<Replication2ReenactmentAOEs>()
            .DeactivateOnExit<Replication2Staging>();

        Cast(id + 1, AID.MutatingCells, 5.1f, 3)
            .ActivateOnEnter<ManaSphere>();
        ComponentCondition<ManaSphere>(id + 3, 1.8f, m => m.HaveDebuff, "Alpha/beta debuffs");

        ComponentCondition<ManaSphere>(id + 0x10, 8.2f, m => m.Spheres.Count > 0, "Shapes appear");
        ComponentCondition<ManaSphere>(id + 0x11, 8.7f, m => m.SwapDone, "Debuffs swap");

        Cast(id + 0x100, AID.BloodWakening, 11.7f, 3)
            .ActivateOnEnter<BloodWakeningReplay>();
        ComponentCondition<BloodWakeningReplay>(id + 0x110, 1.7f, r => r.NumCasts > 0, "AOEs 1");
        ComponentCondition<BloodWakeningReplay>(id + 0x111, 5.1f, r => r.NumCasts >= 12, "AOEs 2")
            .DeactivateOnExit<BloodWakeningReplay>();

        CastMulti(id + 0x200, [AID.NetherworldNear, AID.NetherworldFar], 0, 4.3f)
            .ActivateOnEnter<Netherworld>();
        ComponentCondition<Netherworld>(id + 0x210, 1.3f, n => n.NumCasts > 0, "Close/far stack")
            .DeactivateOnExit<ManaSphere>()
            .DeactivateOnExit<Netherworld>();

        Cast(id + 0x300, AID.ArcadiaAflame, 1.8f, 5, "Raidwide")
            .ActivateOnEnter<ArcadiaAflame>()
            .DeactivateOnExit<ArcadiaAflame>();

        DoubleSobat(id + 0x400, 4.3f);
    }

    void IdyllicDream(uint id, float delay)
    {
        Cast(id, AID.IdyllicDream, delay, 5, "Raidwide")
            .ActivateOnEnter<IdyllicDreamRaidwide>()
            .ActivateOnEnter<IdyllicDreamStaging>()
            .ActivateOnEnter<IdyllicDreamArena>()
            .DeactivateOnExit<IdyllicDreamRaidwide>()
            .ExecOnExit<IdyllicDreamStaging>(s => s.WatchSpawns = false); // N->S clones spawn before 8x staging set does

        Cast(id + 0x10, AID.Staging, 3.2f, 3);

        ComponentCondition<IdyllicDreamStaging>(id + 0x20, 5.4f, r => r.PlayersAssigned, "Player clones appear");

        // red arena -> blue arena
        Cast(id + 0x100, AID.TwistedVision, 5.8f, 4);

        // 3x boss clones spawn along X=0, store casts for later
        Cast(id + 0x110, AID.Replication, 3.1f, 3)
            .ActivateOnEnter<IdyllicDreamPowerGusherSnakingKick>();

        // blue arena -> red arena, boss clones disappear before finishing cast
        Cast(id + 0x120, AID.TwistedVision, 8.5f, 4);

        // 8x boss clones spawn in clockwise paired order
        Cast(id + 0x130, AID.Replication, 3.2f, 3)
            .ExecOnEnter<IdyllicDreamStaging>(s => s.WatchSpawns = true);

        // twisted vision resummons stored AOEs
        // clones pick tethers during cast
        CastStart(id + 0x140, AID.TwistedVision, 15.5f)
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(k => k.Visible = true);
        ComponentCondition<IdyllicDreamStaging>(id + 0x141, 3.9f, t => t.WurmsAssigned, "Clone tethers")
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(k => k.Risky = true);
        CastEnd(id + 0x142, 0.1f);

        CastStart(id + 0x150, AID.LindwurmsMeteor, 3.5f)
            .ActivateOnEnter<LindwurmsMeteor>();
        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x151, 0.9f, k => k.NumCasts > 0, "Stored AOEs");
        CastEnd(id + 0x152, 4.1f, "Raidwide")
            .DeactivateOnExit<LindwurmsMeteor>();

        // platform transform during cast, towers appear on platforms at cast end
        CastStart(id + 0x160, AID.Downfall, 3.1f)
            .ActivateOnEnter<IdyllicDreamElementalMeteor>();
        ComponentCondition<IdyllicDreamArena>(id + 0x161, 0.9f, a => a.State == 1, "Platforms appear");
        CastEnd(id + 0x162, 2.2f);

        Cast(id + 0x200, AID.ArcadianArcanumCast, 3.1f, 3)
            .ActivateOnEnter<ArcadianArcanum>();
        ComponentCondition<ArcadianArcanum>(id + 0x202, 1.3f, a => a.NumCasts > 0, "Random spreads")
            .DeactivateOnExit<ArcadianArcanum>();

        // circle transform, clone mechanics trigger after a delay
        // 359.88 (cast start) -> 370.37 (first) -> 375.32 (second)
        CastStart(id + 0x210, AID.TwistedVision, 2.8f)
            .ActivateOnEnter<IdyllicDreamWurmStackSpread>();
        ComponentCondition<IdyllicDreamArena>(id + 0x211, 5.2f, a => a.State == 0, "Platforms disappear")
            .ExecOnExit<IdyllicDreamWurmStackSpread>(p => p.EnableHints = true);

        ComponentCondition<IdyllicDreamWurmStackSpread>(id + 0x220, 5.2f, w => w.NumCasts == 2, "Clone mechanics start");
        ComponentCondition<IdyllicDreamWurmStackSpread>(id + 0x221, 15, w => w.NumCasts == 8, "Clone mechanics end")
            .ExecOnExit<IdyllicDreamStaging>(s => s.WurmsFinished = true);

        Timeout(id + 0x222, 1.5f).DeactivateOnExit<IdyllicDreamWurmStackSpread>();

        // platform transform, towers appear and activate
        Cast(id + 0x230, AID.TwistedVision, 3.6f, 4)
            .ExecOnEnter<IdyllicDreamArena>(p => p.Predict(8.8f))
            .ExecOnEnter<IdyllicDreamElementalMeteor>(m => m.CreateTowers())
            .ActivateOnEnter<IdyllicDreamLindwurmsDarkII>()
            .ActivateOnEnter<IdyllicDreamWindTower>()
            .ActivateOnEnter<IdyllicDreamHotBlooded>()
            .ActivateOnEnter<IdyllicDreamDoom>()
            .ActivateOnEnter<LindwurmsStoneIII>();

        ComponentCondition<IdyllicDreamArena>(id + 0x232, 1.3f, a => a.State == 1, "Platforms appear");
        ComponentCondition<IdyllicDreamElementalMeteor>(id + 0x233, 3.1f, m => m.NumCasts > 0, "Towers")
            .ExecOnEnter<IdyllicDreamLindwurmsDarkII>(d => d.EnableHints = true)
            .ExecOnEnter<IdyllicDreamElementalMeteor>(d => d.EnableHints = true)
            .DeactivateOnExit<IdyllicDreamElementalMeteor>();

        ComponentCondition<LindwurmsStoneIII>(id + 0x234, 5.7f, s => s.NumCasts > 0, "Delayed puddles")
            .ActivateOnEnter<LindwurmsPortent>()
            //.DeactivateOnExit<IdyllicDreamHotBlooded>() // keep enabled, in case the status lingers for some reason
            .DeactivateOnExit<IdyllicDreamLindwurmsDarkII>()
            .DeactivateOnExit<IdyllicDreamWindTower>()
            .DeactivateOnExit<LindwurmsStoneIII>();

        ComponentCondition<LindwurmsPortent>(id + 0x240, 5, p => p.NumCasts > 0, "Proximity baits")
            .DeactivateOnExit<LindwurmsPortent>()
            .DeactivateOnExit<IdyllicDreamHotBlooded>()
            .DeactivateOnExit<IdyllicDreamDoom>()
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(k =>
            {
                //k.Visible = true;
                k.WatchTeleport = true;
            });

        // black hole appears and absorbs one clone; remaining clones jump
        Cast(id + 0x300, AID.TemporalCurtain, 5.4f, 3);

        // circle transform
        CastStart(id + 0x310, AID.TwistedVision, 8.7f);
        ComponentCondition<IdyllicDreamArena>(id + 0x311, 5.2f, a => a.State == 0, "Platforms disappear")
            .ActivateOnEnter<IdyllicDreamManaBurstPlayer>()
            .ActivateOnEnter<IdyllicDreamHeavySlamPlayer>()
            .ActivateOnEnter<IdyllicDreamPlayerCastCounter>()
            .ExecOnEnter<IdyllicDreamManaBurstPlayer>(p => p.Predict(0))
            .ExecOnEnter<IdyllicDreamHeavySlamPlayer>(p => p.Predict(0));

        // clones replay stack/spread
        Cast(id + 0x320, AID.Reenactment, 1.9f, 3)
            .ExecOnEnter<IdyllicDreamManaBurstPlayer>(p => p.Risky = true)
            .ExecOnEnter<IdyllicDreamHeavySlamPlayer>(p => p.EnableHints = true);
        ComponentCondition<IdyllicDreamPlayerCastCounter>(id + 0x322, 3.6f, c => c.NumCasts == 4, "Reenactment 1");

        // platform transform, jumpy clones
        Cast(id + 0x330, AID.TwistedVision, 1.5f, 4)
            .ExecOnEnter<IdyllicDreamArena>(a => a.Predict(10))
            // TODO: fix aoe activation time, im tired
            .ExecOnEnter<IdyllicDreamPowerGusherSnakingKick>(k =>
            {
                k.Visible = true;
                k.Reset();
            });
        ComponentCondition<IdyllicDreamArena>(id + 0x332, 1.3f, a => a.State == 1, "Platforms appear");

        CastStart(id + 0x340, AID.TwistedVision, 2.3f);
        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x341, 0.9f, k => k.NumCasts > 0, "Safe platform")
            .ExecOnExit<IdyllicDreamManaBurstPlayer>(p => p.Predict(1))
            .ExecOnExit<IdyllicDreamHeavySlamPlayer>(p => p.Predict(1));

        ComponentCondition<IdyllicDreamArena>(id + 0x350, 4.3f, a => a.State == 0, "Platforms disappear")
            .ExecOnExit<IdyllicDreamManaBurstPlayer>(p => p.Risky = true)
            .ExecOnExit<IdyllicDreamHeavySlamPlayer>(p => p.EnableHints = true);
        ComponentCondition<IdyllicDreamPlayerCastCounter>(id + 0x351, 6.6f, p => p.NumCasts == 8, "Reenactment 2")
            .DeactivateOnExit<IdyllicDreamPlayerCastCounter>()
            .DeactivateOnExit<IdyllicDreamManaBurstPlayer>()
            .DeactivateOnExit<IdyllicDreamHeavySlamPlayer>()
            .DeactivateOnExit<IdyllicDreamStaging>()
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(k =>
            {
                k.Visible = true;
                k.Reset();
            });

        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x352, 4.8f, k => k.NumCasts > 0, "Stored AOE")
            .DeactivateOnExit<IdyllicDreamPowerGusherSnakingKick>();

        Cast(id + 0x400, AID.IdyllicDream, 1, 5, "Raidwide")
            .ActivateOnEnter<IdyllicDreamRaidwide>()
            .DeactivateOnExit<IdyllicDreamRaidwide>()
            .DeactivateOnExit<IdyllicDreamArena>();
    }

    void ArcadianHell(uint id, float delay)
    {
        Cast(id, AID.ReplicationHell, delay, 5)
            .ActivateOnEnter<ArcadianHell5x>()
            .ActivateOnEnter<ArcadianHell9x>();

        Cast(id + 0x10, AID.ArcadianHellRaidwide, 8.5f, 5, "Raidwide x5");
        Cast(id + 0x20, AID.ArcadianHellRaidwide, 11.3f, 5, "Raidwide x9");

        Cast(id + 0x100, AID.ArcadianHellEnrage, 12.9f, 10, "Enrage");
    }
}
