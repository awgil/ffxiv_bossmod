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

        Timeout(id + 0xFF0000, 10000, "???");
    }

    void Replication1(uint id, float delay)
    {
        Cast(id, AID.ArcadiaAflame, delay, 5, "Raidwide")
            .ActivateOnEnter<ArcadiaAflame>()
            .DeactivateOnExit<ArcadiaAflame>();

        Cast(id + 0x100, AID.Replication, 9.3f, 3)
            .ActivateOnEnter<Replication1SecondBait>()
            .ActivateOnEnter<WingedScourge>()
            .ActivateOnEnter<WingedScourgeSecond>()
            .ActivateOnEnter<MightyMagicTopTierSlamFirstBait>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(k => k.Risky = false);

        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x110, 11.5f, m => m.NumFire > 0, "Fire bait");
        ComponentCondition<WingedScourge>(id + 0x111, 0.9f, w => w.NumCasts > 0, "Cones");
        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x112, 0.2f, m => m.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamFirstBait>()
            .ExecOnExit<SnakingKick>(k => k.Risky = true);

        ComponentCondition<SnakingKick>(id + 0x120, 4.8f, k => k.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<SnakingKick>();

        ComponentCondition<WingedScourge>(id + 0x130, 12.6f, w => w.Casters.Count > 0)
            .DeactivateOnExit<WingedScourgeSecond>();

        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x140, 3.3f, b => b.NumFire > 0, "Fire baits");
        ComponentCondition<WingedScourge>(id + 0x141, 0.8f, w => w.NumCasts > 4, "Cones");
        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x142, 0.2f, b => b.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<Replication1SecondBait>()
            .DeactivateOnExit<WingedScourge>();

        CastStart(id + 0x200, AID.DoubleSobatBoss1, 2.4f)
            .ActivateOnEnter<DoubleSobatBuster>()
            .ActivateOnEnter<DoubleSobatRepeat>();
        ComponentCondition<DoubleSobatBuster>(id + 0x201, 5.6f, b => b.NumCasts > 0, "Half-room buster")
            .DeactivateOnExit<DoubleSobatBuster>();
        ComponentCondition<DoubleSobatRepeat>(id + 0x210, 4.6f, b => b.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<EsotericFinisher>()
            .DeactivateOnExit<DoubleSobatRepeat>()
            .ExecOnEnter<EsotericFinisher>(f => f.EnableHints = false)
            .ExecOnExit<EsotericFinisher>(f => f.EnableHints = true);

        ComponentCondition<EsotericFinisher>(id + 0x300, 2.5f, f => f.NumCasts > 0, "Double tankbuster")
            .DeactivateOnExit<EsotericFinisher>();
    }

    void Replication2(uint id, float delay)
    {
        Cast(id, AID.Staging, delay, 3)
            .ActivateOnEnter<Replication2Assignments>();
        ComponentCondition<Replication2Assignments>(id + 3, 7.9f, r => r.Assigned, "Player clones appear");
        Cast(id + 0x10, AID.Replication, 3.2f, 3)
            .ActivateOnEnter<Replication2CloneTethers>();

        ComponentCondition<Replication2CloneTethers>(id + 0x100, 16.5f, t => t.Locked, "Clone tethers");
        CastStart(id + 0x101, AID.FirefallSplashCast, 0)
            .ActivateOnEnter<Replication2FirefallSplash>()
            .ActivateOnEnter<Replication2ScaldingWaves>()
            .ActivateOnEnter<Replication2ManaBurst>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(k => k.Risky = false);

        ComponentCondition<Replication2FirefallSplash>(id + 0x110, 5.9f, r => r.NumCasts > 0, "Boss jumps (spread)")
            .DeactivateOnExit<Replication2FirefallSplash>();
        ComponentCondition<Replication2ScaldingWaves>(id + 0x111, 0.6f, r => r.NumCasts > 0, "Proteans")
            .DeactivateOnExit<Replication2ScaldingWaves>();
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

        Cast(id + 0x200, AID._Weaponskill_Reenactment, 7.2f, 3, "Reenactment start")
            .ActivateOnEnter<Replication2ReenactmentOrder>()
            .ActivateOnEnter<Replication2ReenactmentAOEs>()
            .ActivateOnEnter<Replication2ReenactmentTowers>();
    }
}
