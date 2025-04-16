namespace BossMod.Dawntrail.Savage.RM07BruteAbominator;

class RM07BruteAbombinatorStates : StateMachineBuilder
{
    public RM07BruteAbombinatorStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID._Weaponskill_BrutalImpact, 5.15f, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 2, 5.6f, b => b.NumCasts >= 6, "Raidwide 6");

        CastMulti(id + 0x10, [AID.StoneringerClub, AID.StoneringerSword], 5.2f, 2)
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>();
        CastStartMulti(id + 0x20, [AID._Weaponskill_SmashHere, AID._Weaponskill_SmashThere], 5.9f);

        ComponentCondition<P1Stoneringer>(id + 0x22, 3.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x23, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        id += 0x10000;
        Cast(id, AID._Weaponskill_SporeSac, 6.3f, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<SinisterSeedsStored>()
            .ActivateOnEnter<TendrilsOfTerror1>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BloomingAbomination>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<HurricaneForce>()
            .ActivateOnEnter<QuarrySwamp>()
            .ActivateOnEnter<Explosion>();

        ComponentCondition<SporeSac>(id + 2, 5.05f, s => s.NumCasts > 0, "Seed AOEs 1");
        ComponentCondition<Pollen>(id + 3, 5.75f, s => s.NumCasts > 0, "Seed AOEs 2");

        ComponentCondition<TendrilsOfTerror1>(id + 0x10, 11.5f, t => t.NumCasts > 0, "Safe spot + stacks")
            .ActivateOnEnter<RootsOfEvil>()
            .ExecOnExit<SinisterSeedsStored>(s => s.Activate())
            .DeactivateOnExit<Impact>();

        ComponentCondition<RootsOfEvil>(id + 0x20, 5.4f, r => r.NumCasts > 0, "Stored AOEs");

        ComponentCondition<WindingWildwinds>(id + 0x30, 0.7f, w => w.Casters.Count > 0);

        Timeout(id + 0x32, 7, "Interruptible casts");

        CastStart(id + 0x40, AID._Weaponskill_QuarrySwamp, 11);
        Timeout(id + 0x41, 0.5f, "Adds enrage");
        CastEnd(id + 0x42, 3.5f, "LoS");

        id += 0x10000;

        ComponentCondition<Explosion>(id, 12, e => e.NumCasts > 0, "Gigaflare 1");
        ComponentCondition<Explosion>(id + 1, 2.5f, e => e.NumCasts > 1, "Gigaflare 2");
        ComponentCondition<Explosion>(id + 2, 2.5f, e => e.NumCasts > 2, "Gigaflare 3")
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>()
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<PulpSmashProtean>()
            .ActivateOnEnter<ItCameFromTheDirt>();

        id += 0x10000;

        CastStartMulti(id + 0x10, [AID._Weaponskill_SmashHere, AID._Weaponskill_SmashThere], 3.9f);

        ComponentCondition<P1Stoneringer>(id + 0x11, 3.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x12, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        ComponentCondition<PulpSmash>(id + 0x20, 7.8f, p => p.NumFinishedStacks > 0, "Stack");
        ComponentCondition<PulpSmashProtean>(id + 0x21, 2, p => p.NumCasts > 0, "Proteans");

        id += 0x10000;
        Cast(id, AID._Weaponskill_NeoBombarianSpecial, 10.8f, 8, "Arena change")
            .ActivateOnEnter<NeoBombarianSpecial>()
            .SetHint(StateMachine.StateHint.DowntimeStart)
            .Raw.Exit = () =>
            {
                Module.Arena.Center = new(100, 5);
                Module.Arena.Bounds = new ArenaBoundsRect(12.5f, 25);
            };

        id += 0x100000;

        Targetable(id, true, 15, "Boss appears");

        Timeout(id + 0xFF0000, 9999, "Enrage");
    }

    //private void XXX(uint id, float delay)
}
