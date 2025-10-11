namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class LaserTest(BossModule module) : BossComponent(module)
{
    private readonly List<string> _recording = [];
    private Angle _prev;

    public override void AddGlobalHints(GlobalHints hints)
    {
        foreach (var h in _recording)
            hints.Add(h);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsWeal1)
            _recording.Clear();

        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsWeal2)
            _prev = spell.Rotation;

        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsWeal3)
        {
            _recording.Add($"Rotation: {(spell.Rotation - _prev).Deg}");
            _prev = spell.Rotation;
        }
    }
}

class LaserTest2(BossModule module) : Components.DebugCasts(module, [AID._Weaponskill_WyvernsWeal2, AID._Weaponskill_WyvernsWeal3], new AOEShapeRect(60, 3));

class Ex6GuardianArkveldStates : StateMachineBuilder
{
    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<Roar2>()
            .ActivateOnEnter<Roar3>()
            .ActivateOnEnter<WyvernsRadianceExawave>()
            .ActivateOnEnter<WyvernsRadiancePuddle>()
            .ActivateOnEnter<WyvernsWealCast>()
            .ActivateOnEnter<WyvernsWeal>();
        //.ActivateOnEnter<LaserTest>()
        //.ActivateOnEnter<LaserTest2>();
        // -49 <=> 44
    }

    private void P1(uint id)
    {
        Roar(id, 0.1f);

        ChainbladeBlow(id + 0x100, 3.8f, true);

        Siegeflight(id + 0x200, 5, true);
        Siegeflight(id + 0x300, 7.9f, true);

        ChainbladeBlow(id + 0x400, 1.8f, true);

        Roar(id + 0x10000, 1.6f);
        Rush(id + 0x10100, 12.1f);
        SteeltailThrust(id + 0x10500, 3, true);
        ChainbladeCharge(id + 0x20000, 2.2f);
        ChainbladeBlow(id + 0x20100, 2.7f, true);
        Roar(id + 0x20200, 1.7f);

        AethericResonance(id + 0x30000, 14.8f);

        ExaCrystals(id + 0x40000, 10.3f);

        ForgedFury(id + 0x50000, 3.1f);
        Roar(id + 0x50100, 11.1f);

        ClamorousChase(id + 0x60000, 3.8f);
        Roar(id + 0x60100, 1.1f);

        AethericResonance(id + 0x70000, 13.5f);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    private static readonly AID[] slowCasts = [AID._Weaponskill_ChainbladeBlow, AID._Weaponskill_ChainbladeBlow6];
    private static readonly AID[] fastCasts = [AID._Weaponskill_ChainbladeBlow12, AID._Weaponskill_ChainbladeBlow18];

    private void Roar(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_Roar, AID._Weaponskill_Roar1, AID._Weaponskill_Roar2], delay, 5, "Raidwide");
    }

    private void ChainbladeBlow(uint id, float delay, bool slow, bool twice = true)
    {
        var casts = slow ? slowCasts : fastCasts;
        CastStartMulti(id, casts, delay)
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<ChainbladeRadiance>()
            .ActivateOnEnter<ChainbladeRepeat>(twice);

        var label = twice ? "Cleave 1" : "Cleave";
        var expected = slow ? 7.2f : 6.2f;
        ComponentCondition<ChainbladeRadiance>(id + 0x10, expected, c => c.NumCasts > 0, label)
            .DeactivateOnExit<ChainbladeRadiance>()
            .DeactivateOnExit<ChainbladeBlow>();

        if (!twice)
            return;

        ComponentCondition<ChainbladeRepeat>(id + 0x20, 4.1f, c => c.NumCasts > 0, "Cleave 2")
            .DeactivateOnExit<ChainbladeRepeat>();
    }

    private void Siegeflight(uint id, float delay, bool slow)
    {
        CastStartMulti(id, [AID._Weaponskill_GuardianSiegeflight, AID._Weaponskill_GuardianSiegeflight2, AID._Weaponskill_WyvernsSiegeflight, AID._Weaponskill_WyvernsSiegeflight2], delay)
            .ActivateOnEnter<BossSiegeflight>()
            .ActivateOnEnter<HelperSiegeflight>()
            .ActivateOnEnter<WhiteFlashDragonspark>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>()
            .ActivateOnEnter<GuardianResonance>()
            .ActivateOnEnter<WyvernsRadianceSides>();

        ComponentCondition<BossSiegeflight>(id + 0x10, slow ? 5 : 4, b => b.NumCasts > 0);
        ComponentCondition<HelperSiegeflight>(id + 0x12, 1.6f, b => b.NumCasts > 0, "Dive")
            .DeactivateOnExit<BossSiegeflight>()
            .DeactivateOnExit<HelperSiegeflight>();

        ComponentCondition<WhiteFlashDragonspark>(id + 0x20, 3.4f, w => w.NumCasts > 1, "In/out + stacks")
            .DeactivateOnExit<WhiteFlashDragonspark>()
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<Dragonspark>()
            .DeactivateOnExit<GuardianResonance>()
            .DeactivateOnExit<WyvernsRadianceSides>();
    }

    private void Rush(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_Rush, delay)
            .ActivateOnEnter<WyvernsRadianceQuake>()
            .ActivateOnEnter<Rush>();

        ComponentCondition<Rush>(id + 0x10, 6.1f, r => r.NumCasts > 0, "Dash 1");
        ComponentCondition<Rush>(id + 0x20, 2, r => r.NumCasts > 1, "Dash 2");
        ComponentCondition<Rush>(id + 0x30, 2, r => r.NumCasts > 2, "Dash 3")
            .DeactivateOnExit<Rush>();

        CastStartMulti(id + 0x100, [AID._Weaponskill_WyvernsOuroblade, AID._Weaponskill_WyvernsOuroblade2, AID._Weaponskill_WyvernsOuroblade4, AID._Weaponskill_WyvernsOuroblade6], 7.7f)
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<WildEnergy>()
            .DeactivateOnExit<WyvernsRadianceQuake>();

        ComponentCondition<WyvernsOuroblade>(id + 0x110, 7, w => w.NumCasts > 0, "Left/right")
            .DeactivateOnExit<WyvernsOuroblade>();
        ComponentCondition<WildEnergy>(id + 0x120, 0.2f, w => w.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<WildEnergy>();
    }

    private void SteeltailThrust(uint id, float delay, bool slow)
    {
        Cast(id, slow ? AID._Weaponskill_SteeltailThrust : AID._Weaponskill_SteeltailThrust2, delay, slow ? 4 : 3)
            .ActivateOnEnter<SteeltailThrust>();

        ComponentCondition<SteeltailThrust>(id + 2, 0.6f, s => s.NumCasts > 0, "Tail")
            .DeactivateOnExit<SteeltailThrust>();
    }

    private void ChainbladeCharge(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_ChainbladeCharge, delay)
            .ActivateOnEnter<ChainbladeCharge>();

        ComponentCondition<ChainbladeCharge>(id + 0x10, 8.4f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<ChainbladeCharge>();
    }

    private void AethericResonance(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_AethericResonance, delay)
            .ActivateOnEnter<GuardianResonancePuddle>()
            .ActivateOnEnter<GuardianResonanceTowerSmall>()
            .ActivateOnEnter<GuardianResonanceTowerLarge>();

        ComponentCondition<GuardianResonancePuddle>(id + 0x10, 3, p => p.NumStarted > 0, "Puddles start");

        ComponentCondition<GuardianResonancePuddle>(id + 0x20, 2.5f, p => p.NumStarted > 1)
            .ExecOnExit<GuardianResonanceTowerLarge>(l => l.EnableBaitHints(2.5f))
            .ExecOnExit<GuardianResonanceTowerSmall>(l => l.EnableBaitHints(2.5f));

        ComponentCondition<GuardianResonancePuddle>(id + 0x30, 2.5f, p => p.NumStarted > 2)
            .ExecOnExit<GuardianResonanceTowerLarge>(l => l.DisableBaitHints())
            .ExecOnExit<GuardianResonanceTowerSmall>(l => l.DisableBaitHints());

        ComponentCondition<GuardianResonanceTowerLarge>(id + 0x40, 3, l => l.NumCasts > 0, "Towers")
            .DeactivateOnExit<GuardianResonancePuddle>()
            .DeactivateOnExit<GuardianResonanceTowerLarge>()
            .DeactivateOnExit<GuardianResonanceTowerSmall>();
    }

    private void ExaCrystals(uint id, float delay)
    {
        ComponentCondition<WyvernsVengeance>(id, delay, v => v.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsRadianceCrystal>()
            .ActivateOnEnter<WildEnergy>();

        ComponentCondition<WildEnergy>(id + 0x10, 15, w => w.NumFinishedSpreads > 3, "Spreads 1");
        ComponentCondition<WildEnergy>(id + 0x20, 8.1f, w => w.NumFinishedSpreads > 4, "Spreads 2")
            .DeactivateOnExit<WildEnergy>();
    }

    private void ForgedFury(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_ForgedFury, delay)
            .ActivateOnEnter<ForgedFury>()
            .DeactivateOnExit<WyvernsVengeance>()
            .DeactivateOnExit<WyvernsRadianceCrystal>();

        ComponentCondition<ForgedFury>(id + 0x10, 7, f => f.NumCasts > 0, "Raidwide 1");
        ComponentCondition<ForgedFury>(id + 0x20, 0.8f, f => f.NumCasts > 1, "Raidwide 2");
        ComponentCondition<ForgedFury>(id + 0x30, 2.4f, f => f.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<ForgedFury>();
    }

    private void ClamorousChase(uint id, float delay)
    {
        // make sure icons are detected even if they go out before cast
        Timeout(id, 0)
            .ActivateOnEnter<ClamorousBait>();

        CastStartMulti(id + 0x10, [AID._Weaponskill_ClamorousChase, AID._Weaponskill_ClamorousChase3], delay)
            .ActivateOnEnter<ClamorousCleave>()
            .ActivateOnEnter<ClamorousJump>();

        ComponentCondition<ClamorousJump>(id + 0x20, 8.2f, j => j.NumCasts > 0, "Limit cut start");
        ComponentCondition<ClamorousCleave>(id + 0x30, 23.5f, c => c.NumCasts >= 8, "Limit cut finish")
            .DeactivateOnExit<ClamorousBait>()
            .DeactivateOnExit<ClamorousCleave>()
            .DeactivateOnExit<ClamorousJump>();
    }
}
