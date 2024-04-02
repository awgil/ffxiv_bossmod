namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

class DRS5States : StateMachineBuilder
{
    public DRS5States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        MaledictionOfAgony(id, 7.1f);
        ManipulateInvertMiasma(id + 0x10000, 4.5f);
        ManipulateInvertMiasma(id + 0x20000, 3.0f);
        SummonMaledictionOfRuin(id + 0x30000, 0.4f);
        // TODO: summon + malediction of ruin > miasma + knockback > vile wave > ice spikes > excruciation > malediction of agony > repeat?
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void MaledictionOfAgony(uint id, float delay)
    {
        Cast(id, AID.MaledictionOfAgony, delay, 4);
        ComponentCondition<MaledictionOfAgony>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<MaledictionOfAgony>()
            .DeactivateOnExit<MaledictionOfAgony>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ManipulateInvertMiasma(uint id, float delay)
    {
        Cast(id, AID.WeaveMiasma, delay, 3)
            .ActivateOnEnter<Miasma>();
        // note: marker eobjs spawn ~0.2s after cast start, appear (eobjanim 00010002) ~1.0s after cast end, low row activates (eobjanim 00080010) right before next cast start
        // low row deactivates (eobjanim 00040020) and high row activates right when first set of aoes finish - ~1.0s after manipulate cast end
        CastMulti(id + 0x10, new[] { AID.ManipulateMiasma, AID.InvertMiasma }, 7.1f, 9, "Miasma start");
        // +1.0s: first set of 10-sec casts finish: rect covers whole lane, circle/donut are cast at Z+5
        // +1.0s: second set of 10-sec casts start - it's too early to show their hints though? but really, selecting donut/rect lane in the first place is a mistake...
        // +1.6s-2.6s: rest(1,1) cast (rect covers whole lane, circle/donut are cast at Z+11)
        // +3.2s-4.2s: rest(1,2) cast (rect, circle/donut at Z+17)
        // +4.8s-5.8s: rest(1,3) cast (no rects anymore, circle/donut at Z+23)
        // +6.4s-7.4s: rest(1,4) cast (circle/donut at Z+29)
        // +8.0s-9.0s: rest(1,5) cast (Z+35)
        // +9.6s-10.6s: rest(1,6) cast (Z+41)
        // +11.0s: second set of 10-sec casts finish
        // +11.2-12.2s: rest(1,7) cast (Z+47)
        // +11.6-12.6: rest(2,1) cast
        // ...
        // +21.2-22.2s: rest(1,7)
        ComponentCondition<Miasma>(id + 0x20, 22.2f, comp => comp.NumLanesFinished >= 8, "Miasma resolve")
            .DeactivateOnExit<Miasma>();
    }

    private void SummonMaledictionOfRuin(uint id, float delay)
    {
        Cast(id, AID.Summon, delay, 3);
        Targetable(id + 0x10, false, 1.0f, "Boss disappears");
        ComponentCondition<BloodyWraith>(id + 0x20, 3, comp => comp.ActiveActors.Any(), "Adds appear") // 2x bloody + 1x misty
            .ActivateOnEnter<BloodyWraith>()
            .ActivateOnEnter<MistyWraith>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        CastStart(id + 0x30, AID.MaledictionOfRuin, 2.1f);
        // +5.7s: second set of adds created (2x bloody + 2x misty)
        // +8.3s: second set of adds targetable
        // +17.7s: third set of adds created (3x bloody + 3x misty)
        // +20.6s: third set of adds targetable
        Timeout(id + 0x40, 43, "Adds resolve");
    }
}
