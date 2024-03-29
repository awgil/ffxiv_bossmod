namespace BossMod.Endwalker.Alliance.A23Halone;

class A23HaloneStates : StateMachineBuilder
{
    public A23HaloneStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "Before adds")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead || Module.PrimaryActor.HP.Cur < 1 || !Module.PrimaryActor.IsTargetable;
        DeathPhase(1, Phase2);
    }

    private void Phase1(uint id)
    {
        RainOfSpears(id, 8.2f);
        Tetrapagos(id + 0x10000, 7.0f);
        Tetrapagos(id + 0x20000, 4.3f);
        DoomSpear(id + 0x30000, 1.3f);
        SpearsThree(id + 0x40000, 1.2f);
        ThousandfoldThrust(id + 0x50000, 8.1f);
        ThousandfoldThrust(id + 0x60000, 6.3f);
        Lochos(id + 0x70000, 7.2f);
        WillOfTheFury(id + 0x80000, 4.4f);
        Targetable(id + 0x90000, false, 17.3f, "Boss disappears");
    }

    private void Phase2(uint id)
    {
        AddPhase(id, 3.1f);
        LochosThousandfoldThrust(id + 0x10000, 8.2f);
        WillOfTheFury(id + 0x20000, 1.7f);
        TetrapagosThrust(id + 0x30000, 4.1f);
        DoomSpear(id + 0x40000, 1.4f);
        RainOfSpears(id + 0x50000, 1.2f);
        Chalaza(id + 0x60000, 9.9f);
        SpearsThree(id + 0x70000, 1.3f);
        LochosThousandfoldThrust(id + 0x80000, 6.9f);
        WillOfTheFury(id + 0x90000, 1.7f);
        RainOfSpears(id + 0xA0000, 2.5f);
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void RainOfSpears(uint id, float delay)
    {
        Cast(id, AID.RainOfSpears, delay, 4.3f);
        ComponentCondition<RainOfSpearsFirst>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<RainOfSpearsFirst>()
            .DeactivateOnExit<RainOfSpearsFirst>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RainOfSpearsRest>(id + 0x11, 2.1f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<RainOfSpearsRest>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RainOfSpearsRest>(id + 0x12, 2.1f, comp => comp.NumCasts > 1, "Raidwide 3")
            .DeactivateOnExit<RainOfSpearsRest>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TetrapagosResolve(uint id, float delay)
    {
        ComponentCondition<Tetrapagos>(id, delay, comp => comp.NumCasts >= 1, "AOE 1");
        ComponentCondition<Tetrapagos>(id + 1, 2.0f, comp => comp.NumCasts >= 2, "AOE 2");
        ComponentCondition<Tetrapagos>(id + 2, 2.0f, comp => comp.NumCasts >= 3, "AOE 3");
        ComponentCondition<Tetrapagos>(id + 3, 2.2f, comp => comp.NumCasts >= 4, "AOE 4")
            .DeactivateOnExit<Tetrapagos>();
    }

    private void Tetrapagos(uint id, float delay)
    {
        Cast(id, AID.Tetrapagos, delay, 13)
            .ActivateOnEnter<Tetrapagos>();
        TetrapagosResolve(id + 0x10, 0.8f);
    }

    private void DoomSpear(uint id, float delay)
    {
        Cast(id, AID.DoomSpear, delay, 7)
            .ActivateOnEnter<DoomSpear>();
        ComponentCondition<DoomSpear>(id + 0x10, 1, comp => comp.NumCasts >= 1, "Tower 1");
        ComponentCondition<DoomSpear>(id + 0x11, 2, comp => comp.NumCasts >= 2, "Tower 2");
        ComponentCondition<DoomSpear>(id + 0x12, 2, comp => comp.NumCasts >= 3, "Tower 3")
            .DeactivateOnExit<DoomSpear>();
    }

    private void SpearsThree(uint id, float delay)
    {
        Cast(id, AID.SpearsThree, delay, 4.3f)
            .ActivateOnEnter<SpearsThree>();
        ComponentCondition<SpearsThree>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<SpearsThree>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ThousandfoldThrust(uint id, float delay)
    {
        CastMulti(id, new[] { AID.ThousandfoldThrustFirst1, AID.ThousandfoldThrustFirst2, AID.ThousandfoldThrustFirst3, AID.ThousandfoldThrustFirst4 }, delay, 5)
            .ActivateOnEnter<ThousandfoldThrust>();
        ComponentCondition<ThousandfoldThrust>(id + 0x10, 1.3f, comp => comp.NumCasts > 0, "Safe half start");
        ComponentCondition<ThousandfoldThrust>(id + 0x20, 4.3f, comp => comp.NumCasts > 4, "Safe half resolve")
            .DeactivateOnExit<ThousandfoldThrust>();
    }

    private void Lochos(uint id, float delay)
    {
        CastStart(id, AID.Lochos, delay)
            .ActivateOnEnter<Lochos1>();
        CastEnd(id + 1, 5);
        ComponentCondition<Lochos>(id + 0x10, 5.9f, comp => comp.NumCasts > 0, "Safe quarter start");
        ComponentCondition<Lochos>(id + 0x20, 4.4f, comp => comp.NumCasts > 8, "Safe quarter resolve")
            .DeactivateOnExit<Lochos>();
    }

    private void LochosThousandfoldThrust(uint id, float delay)
    {
        CastStart(id, AID.Lochos, delay)
            .ActivateOnEnter<Lochos2>();
        CastEnd(id + 1, 5);
        CastMulti(id + 0x10, new[] { AID.ThousandfoldThrustFirst1, AID.ThousandfoldThrustFirst2, AID.ThousandfoldThrustFirst3, AID.ThousandfoldThrustFirst4 }, 3.5f, 5)
            .ActivateOnEnter<ThousandfoldThrust>();
        ComponentCondition<ThousandfoldThrust>(id + 0x20, 1.3f, comp => comp.NumCasts > 0, "Safe eighth start");
        ComponentCondition<Lochos>(id + 0x30, 4.4f, comp => comp.NumCasts > 8, "Safe eighth resolve")
            .ActivateOnEnter<IceDart>() // casts start ~1.1s before resolve
            .DeactivateOnExit<ThousandfoldThrust>()
            .DeactivateOnExit<Lochos>();
        ComponentCondition<IceDart>(id + 0x40, 4.9f, comp => comp.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<IceDart>();
    }

    private void WillOfTheFury(uint id, float delay)
    {
        Cast(id, AID.WillOfTheFury, delay, 3);
        ComponentCondition<WillOfTheFury>(id + 0x10, 0.8f, comp => comp.Active)
            .ActivateOnEnter<WillOfTheFury>();
        CastStart(id + 0x20, AID.WrathOfHalone, 3.3f);
        ComponentCondition<WillOfTheFury>(id + 0x30, 1.7f, comp => comp.NumCasts >= 1, "Ring 1");
        ComponentCondition<WillOfTheFury>(id + 0x31, 2, comp => comp.NumCasts >= 2)
            .ActivateOnEnter<WrathOfHalone>(); // note: activate only after first ring is done (?)
        ComponentCondition<WillOfTheFury>(id + 0x32, 2, comp => comp.NumCasts >= 3);
        ComponentCondition<WillOfTheFury>(id + 0x33, 2, comp => comp.NumCasts >= 4);
        CastEnd(id + 0x40, 0.3f);
        ComponentCondition<WrathOfHalone>(id + 0x41, 1, comp => comp.NumCasts > 0, "Proximity")
            .DeactivateOnExit<WrathOfHalone>();
        ComponentCondition<WillOfTheFury>(id + 0x50, 0.7f, comp => comp.NumCasts >= 5, "Rings resolve")
            .DeactivateOnExit<WillOfTheFury>();
    }

    private void TetrapagosThrust(uint id, float delay)
    {
        Cast(id, AID.TetrapagosThrust, delay, 13)
            .ActivateOnEnter<Tetrapagos>();
        TetrapagosResolve(id + 0x10, 0.8f);
        ComponentCondition<ThousandfoldThrust>(id + 0x20, 3.3f, comp => comp.NumCasts > 0, "Safe half start")
            .ActivateOnEnter<ThousandfoldThrust>();
        ComponentCondition<ThousandfoldThrust>(id + 0x30, 4.3f, comp => comp.NumCasts > 4, "Safe half resolve")
            .DeactivateOnExit<ThousandfoldThrust>();
    }

    private void Chalaza(uint id, float delay)
    {
        Cast(id, AID.Chalaza, delay, 3);
        CastStart(id + 0x10, AID.Tetrapagos, 5.2f)
            .ActivateOnEnter<IceDart>() // both stack & spreads start ~0.8s after previous cast end
            .ActivateOnEnter<IceRondel>();
        ComponentCondition<IceDart>(id + 0x20, 1.6f, comp => !comp.Active, "Stack/spread")
            .ActivateOnEnter<Tetrapagos>()
            .DeactivateOnExit<IceDart>()
            .DeactivateOnExit<IceRondel>();
        CastEnd(id + 0x30, 11.4f);
        TetrapagosResolve(id + 0x40, 0.8f);
    }

    private void AddPhase(uint id, float delay)
    {
        ComponentCondition<GlacialSpearSmall>(id + 0x10, delay, comp => comp.ActiveActors.Any(), "Adds appear")
            .ActivateOnEnter<GlacialSpearSmall>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<GlacialSpearLarge>(id + 0x20, 7.5f, comp => comp.ActiveActors.Any())
            .ActivateOnEnter<GlacialSpearLarge>();
        // TODO: correct enrage timer
        // TODO: Cheimon component (find out rotation direction, angle offset and num aoes)
        Condition(id + 0x100, 100, () => !Module.FindComponent<GlacialSpearSmall>()!.ActiveActors.Any() && !Module.FindComponent<GlacialSpearLarge>()!.ActiveActors.Any(), "Adds enrage")
            .ActivateOnEnter<IceDart>()
            .ActivateOnEnter<Niphas>()
            .DeactivateOnExit<Niphas>()
            .DeactivateOnExit<GlacialSpearSmall>()
            .DeactivateOnExit<GlacialSpearLarge>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<FurysAegis>(id + 0x200, 8.7f, comp => comp.NumCasts > 0, "Raidwide", 10) // TODO: these timings differ a lot, depending on whether large is killed last?..
            .ActivateOnEnter<FurysAegis>()
            .DeactivateOnExit<IceDart>() // note that these can linger a bit, depeding on add kill time
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x210, 7.9f, comp => comp.NumCasts > 1, "Multi raidwide start")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x211, 0.5f, comp => comp.NumCasts > 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x212, 0.4f, comp => comp.NumCasts > 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x213, 0.6f, comp => comp.NumCasts > 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x214, 0.6f, comp => comp.NumCasts > 5)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FurysAegis>(id + 0x220, 6.3f, comp => comp.NumCasts > 6, "Multi raidwide resolve")
            .DeactivateOnExit<FurysAegis>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x300, true, 8.0f, "Boss reappears");
    }
}
