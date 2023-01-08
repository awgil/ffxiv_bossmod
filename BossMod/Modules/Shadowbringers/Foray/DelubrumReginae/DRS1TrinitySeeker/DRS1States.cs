namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class DRS1States : StateMachineBuilder
    {
        public DRS1States(BossModule module) : base(module)
        {
            SimplePhase(0, Phase1, "P1")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.HP.Cur <= 1 || (module.PrimaryActor.CastInfo?.IsSpell(AID.VerdantPathSword) ?? false);
            SimplePhase(1, Phase2, "P2")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.HP.Cur <= 1 || (module.PrimaryActor.CastInfo?.IsSpell(AID.VerdantPathFist) ?? false);
            SimplePhase(2, Phase3, "P3")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.HP.Cur <= 1 || (module.PrimaryActor.CastInfo?.IsSpell(AID.VerdantPathKatana) ?? false);
            SimplePhase(3, Phase4, "P4")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.HP.Cur <= 1;
        }

        private void Phase1(uint id)
        {
            VerdantTempest(id, 6.2f);
            MercyFourfoldSeasonsOfMercy(id + 0x10000, 4.9f);
            // TODO: verdant tempest -> merciful arc -> forced phase change
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void Phase2(uint id)
        {
            VerdantPathSword(id, 0);
            BalefulOnslaughtDouble(id + 0x10000, 4.2f);
            BurningChainsBalefulBlade(id + 0x20000, 13.6f);
            BalefulFirestormBalefulBlade(id + 0x30000, 2.3f);
            // TODO: verdant tempest -> forced phase change
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void Phase3(uint id)
        {
            VerdantPathFist(id, 0);
            IronRoseIronSplitter(id + 0x10000, 5.3f);
            IronSplitterDeadIronIronRose(id + 0x20000, 5.5f);
            // TODO: verdant tempest -> forced phase change
            SimpleState(id + 0xFF0000, 100, "???")
                .ActivateOnEnter<IronSplitter>();
        }

        private void Phase4(uint id)
        {
            VerdantPathKatana(id, 0);
            BalefulFirestormMercyFourfoldSeasonsOfMercy(id + 0x10000, 7.5f);
            VerdantTempest(id + 0x20000, 8.3f);
            MercifulArc(id + 0x30000, 10.5f);
            MercyFourfoldIronSplitter(id + 0x40000, 4.6f);
            SeasonsOfMercyIronSplitterIronRose(id + 0x50000, 4.7f);
            VerdantTempest(id + 0x60000, 5.2f);
            VerdantPathSword(id + 0x70000, 9.5f);
            // TODO: mercy (clone) + baleful blade + chains -> verdant tempest -> iron splitter + baleful blade -> mercy (clone) + chains + baleful blade -> baleful onslaught -> verdant path (katana) -> enrage
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void VerdantTempest(uint id, float delay)
        {
            Cast(id, AID.VerdantTempest, delay, 5)
                .ActivateOnEnter<VerdantTempest>();
            ComponentCondition<VerdantTempest>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide")
                .DeactivateOnExit<VerdantTempest>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void VerdantPathKatana(uint id, float delay)
        {
            Cast(id, AID.VerdantPathKatana, delay, 3)
                .ActivateOnEnter<ActOfMercy>();
            ComponentCondition<ActOfMercy>(id + 2, 4.6f, comp => comp.NumCasts > 0, "Cross aoe")
                .DeactivateOnExit<ActOfMercy>();
        }

        private void MercyFourfoldHints(uint id, float delay)
        {
            Cast(id, AID.FirstMercy, delay, 3)
                .ActivateOnEnter<MercyFourfold>();
            Cast(id + 0x10, AID.SecondMercy, 0.2f, 3);
            Cast(id + 0x20, AID.ThirdMercy, 0.2f, 3);
            Cast(id + 0x30, AID.FourthMercy, 0.2f, 3);
        }

        private State MercyFourfoldResolve(uint id, float delay, bool activateSeasons)
        {
            ComponentCondition<MercyFourfold>(id, delay, comp => comp.NumCasts >= 1, "Mercy 1");
            ComponentCondition<MercyFourfold>(id + 0x10, 1.9f, comp => comp.NumCasts >= 2, "Mercy 2");
            // +0.7s: avatar finishes seasons of mercy cast
            ComponentCondition<MercyFourfold>(id + 0x20, 1.9f, comp => comp.NumCasts >= 3, "Mercy 3")
                .ActivateOnEnter<MercifulMoon>(activateSeasons); // orb spawns ~1.0s before 3rd mercy
            return ComponentCondition<MercyFourfold>(id + 0x30, 1.9f, comp => comp.NumCasts >= 4, "Mercy 4")
                .ActivateOnEnter<MercifulBreeze>(activateSeasons) // first set of breezes start ~1.1s before 4th mercy
                .ActivateOnEnter<MercifulBlooms>(activateSeasons) // bloom starts ~0.1s before 4th mercy
                .DeactivateOnExit<MercyFourfold>();
        }

        private State MercyFourfold(uint id, float delay, bool activateSeasons)
        {
            MercyFourfoldHints(id, delay);
            // if with seasons, right before 4th mercy cast end, avatar starts seasons of mercy cast
            Cast(id + 0x100, AID.MercyFourfold, 0.2f, 2);
            return MercyFourfoldResolve(id + 0x200, 0.2f, activateSeasons);
        }

        private void SeasonsOfMercy(uint id, float delay)
        {
            ComponentCondition<MercifulBreeze>(id, delay, comp => comp.NumCasts > 0, "Crisscross 1");
            ComponentCondition<MercifulMoon>(id + 0x10, 1.5f, comp => comp.NumCasts > 0, "Gaze")
                .DeactivateOnExit<MercifulMoon>();
            ComponentCondition<MercifulBreeze>(id + 0x20, 0.1f, comp => comp.Casters.Count > 0);
            ComponentCondition<MercifulBreeze>(id + 0x30, 2.5f, comp => comp.Casters.Count == 0, "Crisscross 2")
                .DeactivateOnExit<MercifulBreeze>();
            ComponentCondition<MercifulBlooms>(id + 0x40, 3.4f, comp => comp.NumCasts > 0, "Bloom")
                .DeactivateOnExit<MercifulBlooms>();
        }

        private void MercyFourfoldSeasonsOfMercy(uint id, float delay)
        {
            Cast(id, AID.ManifestAvatar, delay, 3);
            MercyFourfold(id + 0x1000, 5.2f, true);
            SeasonsOfMercy(id + 0x2000, 1.4f);
        }

        private void MercifulArc(uint id, float delay)
        {
            ComponentCondition<MercifulArc>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<MercifulArc>()
                .DeactivateOnExit<MercifulArc>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void VerdantPathSword(uint id, float delay)
        {
            Cast(id, AID.VerdantPathSword, delay, 3)
                .ActivateOnEnter<BalefulSwathe>();
            ComponentCondition<BalefulSwathe>(id + 2, 4.6f, comp => comp.NumCasts > 0, "Side aoes")
                .DeactivateOnExit<BalefulSwathe>();
        }

        // TODO: components for cleaves, figure out how to make optional casts...
        private void BalefulOnslaughtDouble(uint id, float delay)
        {
            Cast(id, AID.BalefulOnslaught, delay, 4, "Tankbuster (shared/invuln)");
            // +0.2s: resolve
            Cast(id + 0x10, AID.PhantomEdge, 3.4f, 4);
            Cast(id + 0x20, AID.BalefulOnslaught, 2.2f, 4, "Tankbuster (solo)");
            // +0.2s: resolve
        }

        // TODO: component for chains
        private void BurningChainsBalefulBlade(uint id, float delay)
        {
            // note: there could be an extra phantom edge cast (7.3 to 3.3 before next cast start), but it doesn't change the baleful blade delay
            Condition(id, delay, () => (Module.PrimaryActor.CastInfo?.IsSpell() ?? false) && (AID)Module.PrimaryActor.CastInfo!.Action.ID is AID.BalefulBlade1 or AID.BalefulBlade2, "", 10000); // this is a hack for phantom edge...
            CastMulti(id + 0x10, new[] { AID.BalefulBlade1, AID.BalefulBlade2 }, 0, 8, "Knockback")
                .ActivateOnEnter<BalefulBlade>()
                .DeactivateOnExit<BalefulBlade>();
        }

        private void BalefulFirestormBalefulBlade(uint id, float delay)
        {
            Cast(id, AID.ManifestAvatar, delay, 3);
            // +6.2s: optional phantom edge start
            // +6.7s: first comet (and then next every 1s after)
            // +10.2s: optional phantom edge end
            Condition(id + 0x10, 13.5f, () => (Module.PrimaryActor.CastInfo?.IsSpell() ?? false) && (AID)Module.PrimaryActor.CastInfo!.Action.ID is AID.BalefulBlade1 or AID.BalefulBlade2, "", 10000) // this is a hack for phantom edge...
                .ActivateOnEnter<BalefulFirestorm>(); // first comet happens 6.8s before, then every second; first firestorm starts right before this cast
            CastMulti(id + 0x20, new[] { AID.BalefulBlade1, AID.BalefulBlade2 }, 0, 8, "Dashes + Knockback")
                .ActivateOnEnter<BalefulBlade>()
                .DeactivateOnExit<BalefulBlade>()
                .DeactivateOnExit<BalefulFirestorm>(); // last firestorm ends ~1.1s before cast end
        }

        private void VerdantPathFist(uint id, float delay)
        {
            Cast(id, AID.VerdantPathFist, delay, 3)
                .ActivateOnEnter<IronImpact>();
            ComponentCondition<IronImpact>(id + 2, 4.8f, comp => comp.NumCasts > 0, "Line stack")
                .DeactivateOnExit<IronImpact>();
        }

        private void IronRoseIronSplitter(uint id, float delay)
        {
            Cast(id, AID.ManifestAvatar, delay, 3);
            ComponentCondition<IronRose>(id + 0x10, 9.4f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<IronRose>();
            ComponentCondition<IronRose>(id + 0x11, 3.5f, comp => comp.NumCasts > 0, "Line AOEs")
                .DeactivateOnExit<IronRose>();
            Cast(id + 0x20, AID.IronSplitter, 0.6f, 5, "Tiles/sands")
                .ActivateOnEnter<IronSplitter>()
                .DeactivateOnExit<IronSplitter>();
        }

        private void IronSplitterDeadIronIronRose(uint id, float delay)
        {
            Cast(id, AID.ManifestAvatar, delay, 3);

            Cast(id + 0x100, AID.IronSplitter, 6.0f, 5, "Tiles/sands")
                .ActivateOnEnter<IronSplitter>()
                .ActivateOnEnter<DeadIron>() // icons & tethers appear ~3.2s after cast start
                .DeactivateOnExit<IronSplitter>();
            ComponentCondition<DeadIron>(id + 0x110, 2.8f, comp => comp.NumCasts > 0, "Earthshakers 1")
                .DeactivateOnExit<DeadIron>();

            CastStart(id + 0x200, AID.IronSplitter, 4.2f)
                .ActivateOnEnter<DeadIron>(); // icons & tethers appear ~1.8s before cast start
            ComponentCondition<DeadIron>(id + 0x201, 3.2f, comp => comp.NumCasts > 0, "Earthshakers 2")
                .ActivateOnEnter<IronSplitter>()
                .DeactivateOnExit<DeadIron>();
            CastEnd(id + 0x202, 1.8f, "Tiles/sands")
                .DeactivateOnExit<IronSplitter>();

            // note: iron roses are slightly staggered
            ComponentCondition<IronRose>(id + 0x300, 1.4f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<IronRose>();
            ComponentCondition<IronRose>(id + 0x301, 3.6f, comp => comp.Casters.Count == 0, "Line AOEs")
                .DeactivateOnExit<IronRose>();
        }

        private void BalefulFirestormMercyFourfoldSeasonsOfMercy(uint id, float delay)
        {
            // TODO: consider starting showing mercies only after firestorms resolve
            Cast(id, AID.ManifestAvatar, delay, 3)
                .ActivateOnEnter<BalefulFirestorm>(); // first comet is ~6.2s after cast end
            MercyFourfold(id + 0x1000, 11.4f, true)
                .DeactivateOnExit<BalefulFirestorm>(); // last firestorm ends ~4.2s before 4th mercy cast end
            SeasonsOfMercy(id + 0x2000, 1.4f);
            // TODO: chains (icons ~0.7s before blooms resolve, tethers ~3.3s after blooms, resolve ~6.4s after blooms)
        }

        private void MercyFourfoldIronSplitter(uint id, float delay)
        {
            Cast(id, AID.ManifestAvatar, delay, 3)
                .ActivateOnEnter<IronSplitter>(); // splitter cast starts ~1.9s after 3rd mercy cast start
            MercyFourfoldHints(id + 0x100, 4.2f);

            CastStart(id + 0x200, AID.MercyFourfold, 0.2f);
            ComponentCondition<IronSplitter>(id + 0x210, 0.5f, comp => comp.NumCasts > 0, "Tiles/sands 1")
                .DeactivateOnExit<IronSplitter>();
            CastEnd(id + 0x220, 1.5f)
                .ActivateOnEnter<IronSplitter>(); // splitter cast starts ~2.5s after mercy fourfold cast start, during resolve

            MercyFourfoldResolve(id + 0x300, 0.2f, false);
            ComponentCondition<IronSplitter>(id + 0x400, 1.5f, comp => comp.NumCasts > 0, "Tiles/sands 2")
                .DeactivateOnExit<IronSplitter>();
        }

        private void SeasonsOfMercyIronSplitterIronRose(uint id, float delay)
        {
            CastStart(id, AID.SeasonsOfMercy, delay)
                .ActivateOnEnter<IronSplitter>(); // splitter starts ~1.3s before seasons cast start
            ComponentCondition<IronSplitter>(id + 0x10, 3.6f, comp => comp.NumCasts > 0, "Tiles/sands")
                .DeactivateOnExit<IronSplitter>();
            CastEnd(id + 0x20, 1.4f)
                .ActivateOnEnter<MercifulMoon>(); // orb appears right before cast end
            ComponentCondition<MercifulBreeze>(id + 0x30, 2, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<MercifulBreeze>();
            ComponentCondition<MercifulBreeze>(id + 0x40, 2.5f, comp => comp.Casters.Count == 0, "Crisscross 1")
                .ActivateOnEnter<MercifulBlooms>() // cast starts ~1.5s before this
                .DeactivateOnExit<MercifulBreeze>();
            ComponentCondition<MercifulMoon>(id + 0x50, 1.5f, comp => comp.NumCasts > 0, "Gaze")
                .DeactivateOnExit<MercifulMoon>();
            ComponentCondition<MercifulBreeze>(id + 0x60, 0.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<MercifulBreeze>();
            ComponentCondition<MercifulBreeze>(id + 0x70, 2.5f, comp => comp.Casters.Count == 0, "Crisscross 2")
                .ActivateOnEnter<IronRose>() // casts start ~1.4s before this
                .DeactivateOnExit<MercifulBreeze>();
            ComponentCondition<IronRose>(id + 0x80, 2.0f, comp => comp.NumCasts > 0, "Line AOEs")
                .DeactivateOnExit<IronRose>();
            ComponentCondition<MercifulBlooms>(id + 0x90, 1.4f, comp => comp.NumCasts > 0, "Bloom")
                .DeactivateOnExit<MercifulBlooms>();
        }
    }
}
