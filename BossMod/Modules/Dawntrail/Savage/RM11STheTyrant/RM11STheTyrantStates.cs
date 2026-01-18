namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class RM11STheTyrantStates : StateMachineBuilder
{
    private readonly RM11STheTyrantConfig _config = Service.Config.Get<RM11STheTyrantConfig>();

    public RM11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        TrophyWeapons(id, 5.2f);
        DanceOfDomination(id + 0x10000, 10.5f);
        UltimateTrophyWeapons(id + 0x20000, 10.3f);
        Meteorain(id + 0x30000, 7.2f);
        Flatliner(id + 0x40000, 9.2f);
        EclipticStampede(id + 0x50000, 6.3f);
        Heartbreaker(id + 0x60000, 8.7f);

        Cast(id + 0x70000, AID.HeartbreakerCast, 5, 8, "Boss disappears (enrage)");
    }

    void TrophyWeapons(uint id, float delay)
    {
        Cast(id, AID.CrownOfArcadia, delay, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();

        RawSteelTrophy(id + 0x10, 5.2f);

        Cast(id + 0x100, AID.TrophyWeapons, 10.5f, 3)
            .ActivateOnEnter<TrophyWeaponsAOE>()
            .ActivateOnEnter<TrophyWeaponsBait>()
            .ActivateOnEnter<TrophyWeaponsHints>(_config.WeaponHints);

        Cast(id + 0x110, AID.AssaultEvolvedLongCast, 3.4f, 6);
        ComponentCondition<TrophyWeaponsAOE>(id + 0x112, 2.2f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x113, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x114, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<TrophyWeaponsAOE>()
            .DeactivateOnExit<TrophyWeaponsBait>()
            .DeactivateOnExit<TrophyWeaponsHints>(_config.WeaponHints);

        Cast(id + 0x200, AID.TrophyWeapons, 9.2f, 3)
            .ActivateOnEnter<TrophyWeaponsAOE>()
            .ActivateOnEnter<TrophyWeaponsBait>()
            .ActivateOnEnter<TrophyWeaponsHints>(_config.WeaponHints)
            .ExecOnEnter<TrophyWeaponsAOE>(t => t.Risky = false)
            .ExecOnEnter<TrophyWeaponsBait>(t => t.EnableHints = false);

        Cast(id + 0x210, AID.VoidStardust, 3.1f, 4)
            .ActivateOnEnter<VoidStardustBait>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comets>();

        ComponentCondition<Cometite>(id + 0x212, 1.2f, c => c.ActiveCasters.Any(), "Puddles start");

        ComponentCondition<Comets>(id + 0x214, 8.9f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .DeactivateOnExit<VoidStardustBait>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comets>()
            .ExecOnExit<TrophyWeaponsAOE>(t => t.Risky = true)
            .ExecOnExit<TrophyWeaponsBait>(t => t.EnableHints = true);

        ComponentCondition<TrophyWeaponsAOE>(id + 0x220, 5.1f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x221, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x222, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<TrophyWeaponsAOE>()
            .DeactivateOnExit<TrophyWeaponsBait>()
            .DeactivateOnExit<TrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<Comets>(id + 0x230, 11.2f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .ActivateOnEnter<VoidStardustBait>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comets>()
            .DeactivateOnExit<VoidStardustBait>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comets>();

        Cast(id + 0x240, AID.CrownOfArcadia, 1, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    void RawSteelTrophy(uint id, float delay)
    {
        CastMulti(id, [AID.RawSteelTrophyAxe, AID.RawSteelTrophyScythe], delay, 2)
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>()
            .ActivateOnEnter<RawSteelTrophyCounter>();

        ComponentCondition<RawSteelTrophyCounter>(id + 2, 7.5f, c => c.NumCasts > 0, "Tankbuster + stack/spread").DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>()
            .DeactivateOnExit<RawSteelTrophyCounter>();
    }

    void DanceOfDomination(uint id, float delay)
    {
        Cast(id, AID.DanceOfDominationTrophy, delay, 2)
            .ActivateOnEnter<DanceOfDomination1>()
            .ActivateOnEnter<DanceOfDomination2>()
            .ActivateOnEnter<HurricaneExplosion>()
            .ActivateOnEnter<EyeOfTheHurricane>();

        ComponentCondition<DanceOfDomination1>(id + 0x10, 6.6f, d => d.NumCasts > 0, "Raidwide 1")
            .DeactivateOnExit<DanceOfDomination1>();
        ComponentCondition<DanceOfDomination2>(id + 0x11, 4.1f, d => d.NumCasts > 0, "Raidwide 7")
            .DeactivateOnExit<DanceOfDomination2>();

        ComponentCondition<EyeOfTheHurricane>(id + 0x20, 4.9f, e => e.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<EyeOfTheHurricane>()
            .DeactivateOnExit<HurricaneExplosion>();

        RawSteelTrophy(id + 0x100, 4.2f);
    }

    void UltimateTrophyWeapons(uint id, float delay)
    {
        Cast(id, AID.Charybdistopia, delay, 5, "1 HP");
        Cast(id + 0x10, AID.UltimateTrophyWeapons, 2.6f, 3)
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<UltimateTrophyWeaponsAOE>()
            .ActivateOnEnter<UltimateTrophyWeaponsBait>()
            .ActivateOnEnter<UltimateTrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x20, 10, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x21, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x22, 5.2f, t => t.NumCasts > 2, "Weapon 3");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x23, 5.2f, t => t.NumCasts > 3, "Weapon 4");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x24, 5.2f, t => t.NumCasts > 4, "Weapon 5");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x25, 5.2f, t => t.NumCasts > 5, "Weapon 6")
            .DeactivateOnExit<UltimateTrophyWeaponsAOE>()
            .DeactivateOnExit<UltimateTrophyWeaponsBait>()
            .DeactivateOnExit<UltimateTrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<PowerfulGust>(id + 0x30, 6.3f, p => p.NumCasts > 0, "Tornado baits")
            .ActivateOnEnter<PowerfulGust>()
            .DeactivateOnExit<PowerfulGust>();

        CastEnd(id + 0x40, 1).ActivateOnEnter<OneAndOnly>();
        Cast(id + 0x100, AID.OneAndOnlyBoss, 4.1f, 6);
        ComponentCondition<OneAndOnly>(id + 0x102, 3, o => o.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Maelstrom>()
            .DeactivateOnExit<OneAndOnly>();
    }

    void Meteorain(uint id, float delay)
    {
        Cast(id, AID.GreatWallOfFireCast, delay, 5)
            .ActivateOnEnter<Firewall>()
            .ActivateOnEnter<FirewallExplosion>();

        ComponentCondition<Firewall>(id + 0x10, 0.3f, f => f.NumCasts > 0, "Line buster 1");
        ComponentCondition<Firewall>(id + 0x11, 3.2f, f => f.NumCasts > 1, "Line buster 2")
            .DeactivateOnExit<Firewall>();

        OrbitalOmen(id + 0x20, 5.1f);

        Cast(id + 0x30, AID.MeteorainBoss, 7.5f, 5)
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<FearsomeFireball1>();

        Cast(id + 0x40, AID.FearsomeFireballBoss, 2.7f, 5);
        ComponentCondition<FearsomeFireball1>(id + 0x42, 0.4f, f => f.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<FearsomeFireball1>();
        ComponentCondition<CosmicKiss>(id + 0x43, 1.2f, c => c.NumCasts > 0, "Meteors 1")
            .ActivateOnEnter<ForegoneFatality>()
            .ActivateOnEnter<FearsomeFireball2>()
            .ActivateOnEnter<CometExplosion>();
        ComponentCondition<ForegoneFatality>(id + 0x44, 7.9f, f => f.NumCasts > 0, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x45, 0.8f, f => f.NumCasts > 0, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x46, 1.3f, c => c.NumCasts > 2, "Meteors 2");
        ComponentCondition<ForegoneFatality>(id + 0x47, 7.9f, f => f.NumCasts > 2, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x48, 0.8f, f => f.NumCasts > 1, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x49, 1.3f, c => c.NumCasts > 4, "Meteors 3")
            .ExecOnExit<Comet>(c => c.DrawTrigger = false);
        ComponentCondition<ForegoneFatality>(id + 0x4A, 7.9f, f => f.NumCasts > 4, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x4B, 0.8f, f => f.NumCasts > 2, "Wild charge")
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<ForegoneFatality>()
            .DeactivateOnExit<FearsomeFireball2>();

        Cast(id + 0x100, AID.TripleTyrannhilationCast, 5.6f, 7)
            .ActivateOnEnter<TripleTyrannhilation>()
            .ActivateOnEnter<ShockwaveCounter>();

        ComponentCondition<ShockwaveCounter>(id + 0x102, 1.1f, c => c.NumCasts > 0, "LoS 1");
        ComponentCondition<ShockwaveCounter>(id + 0x103, 3.2f, c => c.NumCasts > 2, "LoS 3")
            .DeactivateOnExit<TripleTyrannhilation>()
            .DeactivateOnExit<ShockwaveCounter>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CometExplosion>();
    }

    void Flatliner(uint id, float delay)
    {
        // flatliner knockback is 15 units
        // tower knockback is 23 units
        Cast(id, AID.FlatlinerBoss, delay, 4)
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerArena>();

        ComponentCondition<Flatliner>(id + 0x10, 2, f => f.NumCasts > 0, "Arena split")
            .DeactivateOnExit<Flatliner>()
            .DeactivateOnExit<FlatlinerArena>()
            .ActivateOnEnter<MajesticMeteorain>();

        Cast(id + 0x20, AID.MajesticMeteorBoss, 9.2f, 5)
            .ActivateOnEnter<ExplosionKnockback>()
            .ActivateOnEnter<ExplosionTower>()
            .ActivateOnEnter<FireBreathMeteowrath>()
            .ActivateOnEnter<FireBreathMeteowrathHints>()
            .ActivateOnEnter<MajesticMeteor>();

        ComponentCondition<ExplosionTower>(id + 0x100, 13.1f, t => t.NumCasts > 0, "Towers 1")
            .DeactivateOnExit<ExplosionTower>()
            .DeactivateOnExit<ExplosionKnockback>();

        ComponentCondition<FireBreathMeteowrath>(id + 0x101, 6.1f, m => m.PreyAssigned, "Prey assigned");
        CastStart(id + 0x102, AID.FireBreathBoss, 0.1f);
        ComponentCondition<MajesticMeteor>(id + 0x103, 2, m => m.ActiveCasters.Any(), "Puddles 1");
        ComponentCondition<MajesticMeteor>(id + 0x104, 4, m => m.BaitsDone, "Puddles 3")
            .ExecOnExit<FireBreathMeteowrath>(f => f.EnableHints = true)
            .ExecOnExit<MajesticMeteorain>(m => m.Risky = true)
            .ExecOnExit<FireBreathMeteowrathHints>(f => f.Safe = true);
        ComponentCondition<FireBreathMeteowrath>(id + 0x105, 2.8f, m => m.NumCasts > 0, "Baits + line meteors")
            .DeactivateOnExit<FireBreathMeteowrath>()
            .DeactivateOnExit<FireBreathMeteowrathHints>();

        ComponentCondition<MajesticMeteor>(id + 0x106, 0.2f, m => !m.ActiveCasters.Any())
            .DeactivateOnExit<MajesticMeteor>();

        ComponentCondition<ExplosionTower>(id + 0x200, 17, t => t.NumCasts > 0, "Towers 2")
            .ActivateOnEnter<ExplosionKnockback>()
            .ActivateOnEnter<ExplosionTower>()
            .ActivateOnEnter<FireBreathMeteowrath>()
            .ActivateOnEnter<FireBreathMeteowrathHints>()
            .ActivateOnEnter<MajesticMeteor>();

        ComponentCondition<FireBreathMeteowrath>(id + 0x201, 6, m => m.PreyAssigned, "Prey assigned");
        CastStart(id + 0x202, AID.FireBreathBoss, 0);
        ComponentCondition<MajesticMeteor>(id + 0x203, 2, m => m.ActiveCasters.Any(), "Puddles 1");
        ComponentCondition<MajesticMeteor>(id + 0x204, 4, m => m.BaitsDone, "Puddles 3")
            .ExecOnExit<FireBreathMeteowrath>(f => f.EnableHints = true)
            .ExecOnExit<MajesticMeteorain>(m => m.Risky = true)
            .ExecOnExit<FireBreathMeteowrathHints>(f => f.Safe = true);
        ComponentCondition<FireBreathMeteowrath>(id + 0x205, 2.8f, m => m.NumCasts > 0, "Baits + line meteors")
            .DeactivateOnExit<FireBreathMeteowrath>()
            .DeactivateOnExit<FireBreathMeteowrathHints>()
            .DeactivateOnExit<MajesticMeteorain>();

        CastStart(id + 0x300, AID.MassiveMeteorBoss, 4.4f)
            .ActivateOnEnter<MassiveMeteor>()
            .ExecOnEnter<ExplosionTower>(t => t.EnableHints = false)
            .DeactivateOnExit<MajesticMeteor>();
        ComponentCondition<MassiveMeteor>(id + 0x301, 6.1f, m => m.NumCasts > 0, "Stacks 1");
        ComponentCondition<MassiveMeteor>(id + 0x302, 5.9f, m => m.NumFinishedStacks > 0, "Stacks 5")
            .DeactivateOnExit<MassiveMeteor>()
            .ExecOnExit<ExplosionTower>(t => t.EnableHints = true);

        CastMulti(id + 0x310, [AID.ArcadionAvalancheBoss1, AID.ArcadionAvalancheBoss2, AID.ArcadionAvalancheBoss3, AID.ArcadionAvalancheBoss4], 1.2f, 6)
            .ActivateOnEnter<ArcadionAvalancheRect>()
            .ActivateOnEnter<ArcadionAvalancheBoss>()
            .ExecOnEnter<ArcadionAvalancheRect>(a => a.Risky = false)
            .ExecOnEnter<ArcadionAvalancheBoss>(a => a.Risky = false)
            .DeactivateOnExit<ExplosionTower>()
            .DeactivateOnExit<ExplosionKnockback>()
            .ExecOnExit<ArcadionAvalancheRect>(a => a.Risky = true);

        ComponentCondition<ArcadionAvalancheRect>(id + 0x312, 9.5f, r => r.NumCasts > 0, "Platform AOE")
            .DeactivateOnExit<ArcadionAvalancheRect>()
            .DeactivateOnExit<ArcadionAvalancheBoss>();

        Cast(id + 0x400, AID.CrownOfArcadia, 5.7f, 5, "Raidwide + restore arena")
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnEnter<CrownOfArcadiaArena>()
            .DeactivateOnExit<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadiaArena>();
    }

    void OrbitalOmen(uint id, float delay)
    {
        Cast(id, AID.OrbitalOmenBoss, delay, 5)
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .DeactivateOnExit<FirewallExplosion>();

        ComponentCondition<OrbitalOmen>(id + 2, 9.1f, o => o.NumCasts == 2, "Orbital start");
        ComponentCondition<FireAndFury>(id + 3, 0.6f, f => f.NumCasts > 0, "Sides safe")
            .DeactivateOnExit<FireAndFury>();
        ComponentCondition<OrbitalOmen>(id + 4, 4, o => o.NumCasts == 8, "Orbital end")
            .DeactivateOnExit<OrbitalOmen>();
    }

    void EclipticStampede(uint id, float delay)
    {
        Cast(id, AID.GreatWallOfFireCast, delay, 5)
            .ActivateOnEnter<Firewall>()
            .ActivateOnEnter<FirewallExplosion>()
            .ActivateOnEnter<CrownOfArcadia>();

        ComponentCondition<Firewall>(id + 0x10, 0.3f, f => f.NumCasts > 0, "Line buster 1");
        ComponentCondition<Firewall>(id + 0x11, 3.2f, f => f.NumCasts > 1, "Line buster 2")
            .DeactivateOnExit<Firewall>();

        OrbitalOmen(id + 0x20, 5.1f);

        Cast(id + 0x30, AID.CrownOfArcadia, 0, 4.2f, "Raidwide")
            .DeactivateOnExit<CrownOfArcadia>();

        Cast(id + 0x100, AID.EclipticStampede, 7.4f, 5)
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<StampedeMajesticMeteor>()
            .ActivateOnEnter<AtomicImpactPuddle>()
            .ActivateOnEnter<AtomicImpact>();

        ComponentCondition<MammothMeteor>(id + 0x110, 7.1f, m => m.NumCasts > 0, "Proximity + puddles start")
            .ActivateOnEnter<ImpactKiss>()
            .DeactivateOnExit<MammothMeteor>();

        ComponentCondition<ImpactKiss>(id + 0x120, 16, k => k.NumCasts > 0, "Towers")
            .ActivateOnEnter<StampedeMajesticMeteowrath>()
            .DeactivateOnExit<StampedeMajesticMeteor>()
            .DeactivateOnExit<ImpactKiss>()
            .DeactivateOnExit<AtomicImpact>();

        CastStartMulti(id + 0x130, [AID.TwoWayFireballBoss, AID.FourWayFireballBoss], 7)
            .ActivateOnEnter<NWayFireball>()
            .ExecOnEnter<NWayFireball>(n => n.EnableHints = false);

        ComponentCondition<StampedeMajesticMeteowrath>(id + 0x131, 2.6f, t => t.NumCasts > 0, "Tethers")
            .ExecOnExit<NWayFireball>(n => n.EnableHints = true)
            .DeactivateOnExit<StampedeMajesticMeteowrath>();

        ComponentCondition<NWayFireball>(id + 0x132, 4.3f, n => n.NumCasts > 0, "2/4 stack")
            .DeactivateOnExit<NWayFireball>();

        Cast(id + 0x200, AID.CrownOfArcadia, 3.3f, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    void Heartbreaker(uint id, float delay)
    {
        Cast(id, AID.HeartbreakKickBoss, delay, 5)
            .ActivateOnEnter<HeartbreakKick>()
            .ExecOnEnter<HeartbreakKick>(k => k.ExpectedHits = 5);
        ComponentCondition<HeartbreakKick>(id + 2, 1.2f, k => k.NumCasts > 0, "Tower 1");
        ComponentCondition<HeartbreakKick>(id + 3, 8, k => k.Towers.Count == 0, "Tower 5")
            .DeactivateOnExit<HeartbreakKick>();

        Cast(id + 0x100, AID.HeartbreakKickBoss, 5, 5)
            .ActivateOnEnter<HeartbreakKick>()
            .ExecOnEnter<HeartbreakKick>(k => k.ExpectedHits = 6);
        ComponentCondition<HeartbreakKick>(id + 0x102, 1.2f, k => k.NumCasts > 0, "Tower 1");
        ComponentCondition<HeartbreakKick>(id + 0x103, 10, k => k.Towers.Count == 0, "Tower 6")
            .DeactivateOnExit<HeartbreakKick>();

        Cast(id + 0x200, AID.HeartbreakKickBoss, 5, 5)
            .ActivateOnEnter<HeartbreakKick>()
            .ExecOnEnter<HeartbreakKick>(k => k.ExpectedHits = 7);
        ComponentCondition<HeartbreakKick>(id + 0x202, 1.2f, k => k.NumCasts > 0, "Tower 1");
        ComponentCondition<HeartbreakKick>(id + 0x203, 12, k => k.Towers.Count == 0, "Tower 7")
            .DeactivateOnExit<HeartbreakKick>();
    }
}
