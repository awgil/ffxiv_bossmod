namespace BossMod.RealmReborn.Raid.T05Twintania;

public enum OID : uint
{
    Boss = 0x7E5, // R5.400, x1
    ScourgeOfMeracydia = 0x7E7, // R3.600, x3
    Neurolink = 0x7E6, // R2.000, spawn during fight
    Conflagration = 0x7F2, // R6.000, spawn during fight
    Hygieia = 0x7E8, // R1.200, spawn during fight
    Asclepius = 0x7E9, // R1.800, spawn during fight
    Dreadknight = 0x7EA, // R1.700, spawn during fight
    Oviform = 0x7F3, // R1.000, spawn during fight (hatch orbs)

    HelperTwister = 0x8EB, // R0.500, x8
    HelperMarker = 0x8EE, // R0.500, x2
    LiquidHell = 0x1E88FE, // R0.500, EventObj type, spawn during fight
    Twister = 0x1E8910, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttackBoss = 1461, // Boss->player, no cast, single-target
    AutoAttackAdds = 870, // ScourgeOfMeracydia/Hygieia/Asclepius->player, no cast, single-target
    Plummet = 1240, // Boss->self, no cast, ??? cleave
    DeathSentence = 1458, // Boss->player, 2.0s cast, single-target, visual (2.4s cast until last phase)
    DeathSentenceP1 = 1241, // Boss->player, no cast, single-target, tankbuster (only during p1)
    DeathSentenceP2 = 1242, // Boss->player, no cast, single-target, tankbuster + decrease healing received debuff

    LiquidHellAdds = 1243, // ScourgeOfMeracydia->location, 3.0s cast, range 6 circle voidzone

    FireballMarker = 1452, // HelperMarker->player, no cast, single-target, visual icon for fireball
    FireballAOE = 1246, // Boss->player, no cast, range 4 circle shared aoe
    FirestormMarker = 1451, // HelperMarker->player, no cast, single-target, visual icon for conflagration
    FirestormAOE = 1245, // Conflagration->self, no cast, one-shot if not killed in time

    DivebombMarker = 1456, // HelperMarker->player, no cast, single-target, visual icon for divebomb
    DivebombAOE = 1247, // Boss->self, 1.2s cast, range 35 width 12 rect aoe with knockback 30
    WildRattle = 669, // Asclepius/Hygieia->player, no cast, single-target
    Disseminate = 1212, // Hygieia->self, no cast, radius 8 circle aoe applying vuln debuff on death
    AethericProfusion = 1248, // Boss->self, no cast, raidwide

    Twister = 1249, // Boss->self, 2.4s cast, single-target, visual
    TwisterVisual = 671, // HelperTwister->self, no cast, visual explosion?
    TwisterKill = 1250, // HelperTwister->self, no cast, instant death for anyone caught
    UnwovenWill = 1251, // Boss->player, no cast, single-target, stun & fixate for dreadknight
    CaberToss = 1257, // Dreadknight->player, no cast, single-target, instant death if dreadknight reaches target

    HatchMarker = 1453, // HelperMarker->player, no cast, single-target, visual icon for hatch
    Hatch = 1256, // Oviform->self, no cast, ???
    LiquidHellMarker = 1457, // Boss->player, no cast, single-target, visual icon for liquid hell
    LiquidHellBoss = 670, // Boss->location, no cast, range 6 circle voidzone
};

public enum SID : uint
{
    Fetters = 292, // none->player, extra=0x0
    Disseminate = 348, // Hygieia->Asclepius/Hygieia/player, extra=0x1/0x2/0x3/0x4
};

// note: this is one of the very early fights that is not represented well by a state machine - it has multiple timers running, can delay casts randomly, can have different overlaps depending on raid dps, etc.
// the only thing that is well timed is P3 (divebombs phase)
class T05TwintaniaStates : StateMachineBuilder
{
    private T05Twintania _module;

    public T05TwintaniaStates(T05Twintania module) : base(module)
    {
        _module = module;
        // note: enrage: if staying in P1 - at 780 we get P2 version of death sentence instead of normal one, then on 791 we start getting profusion raidwides (onr per 5-8s)
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Condition(id, 100, () => _module.Neurolinks.Count > 0, "P1: adds and tankbusters (until 85%)", 1000)
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<DeathSentence>()
            .ActivateOnEnter<P1LiquidHellAdds>()
            .ActivateOnEnter<P1AI>()
            .DeactivateOnExit<P1LiquidHellAdds>()
            .DeactivateOnExit<P1AI>();

        Condition(id + 0x10000, 100, () => _module.Neurolinks.Count > 1, "P2: fireballs/conflags (until 55%)", 1000)
            .ActivateOnEnter<P2Fireball>()
            .ActivateOnEnter<P2Conflagrate>()
            .ActivateOnEnter<P2AI>()
            .DeactivateOnExit<Plummet>()
            .DeactivateOnExit<DeathSentence>()
            .DeactivateOnExit<P2Fireball>()
            .DeactivateOnExit<P2Conflagrate>()
            .DeactivateOnExit<P2AI>();

        Phase3(id + 0x20000, 3.1f);

        Condition(id + 0x30000, 100, () => _module.Neurolinks.Count > 2, "P4: twisters/dreadknights (until 30%)", 1000)
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<DeathSentence>()
            .ActivateOnEnter<P4Twisters>()
            .ActivateOnEnter<P4Dreadknights>()
            .ActivateOnEnter<P4AI>()
            .DeactivateOnExit<P4Twisters>()
            .DeactivateOnExit<P4Dreadknights>()
            .DeactivateOnExit<P4AI>();

        SimpleState(id + 0x40000, 100, "P5: hatch/liquid hell")
            .ActivateOnEnter<P5LiquidHell>()
            .ActivateOnEnter<P5Hatch>()
            .ActivateOnEnter<P5AI>();
    }

    private void Phase3(uint id, float delay)
    {
        Targetable(id, false, delay, "Disappear")
            .ActivateOnEnter<P3Divebomb>();
        Divebomb(id + 0x10, 5.2f, "Divebomb 1");
        Divebomb(id + 0x20, 5.9f, "Divebomb 2");
        Divebomb(id + 0x30, 6.2f, "Divebomb 3");
        ComponentCondition<P3Adds>(id + 0x100, 2.2f, comp => comp.Asclepius.Any(a => a.IsTargetable), "Adds")
            .ActivateOnEnter<P3Adds>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        Divebomb(id + 0x110, 45.5f, "Divebomb 4");
        Divebomb(id + 0x120, 5.7f, "Divebomb 5");
        Divebomb(id + 0x130, 5.7f, "Divebomb 6");
        Targetable(id + 0x200, true, 62.7f, "Reappear")
            .DeactivateOnExit<P3Divebomb>();
        ComponentCondition<P3AethericProfusion>(id + 0x300, 6.7f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P3AethericProfusion>()
            .DeactivateOnExit<P3AethericProfusion>();
        // note: we keep adds component, technically they can live until end...
    }

    private void Divebomb(uint id, float delay, string name)
    {
        ComponentCondition<P3Divebomb>(id, delay, comp => comp.Target != null)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<P3Divebomb>(id + 1, 1.7f, comp => comp.Target == null, name)
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 97, NameID = 1482)]
public class T05Twintania : BossModule
{
    public const float NeurolinkRadius = 2;

    public IReadOnlyList<Actor> ScourgeOfMeracydia;
    public IReadOnlyList<Actor> Neurolinks;

    public T05Twintania(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-3, -6.5f), 31))
    {
        ScourgeOfMeracydia = Enemies(OID.ScourgeOfMeracydia);
        Neurolinks = Enemies(OID.Neurolink);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
        foreach (var a in ScourgeOfMeracydia)
            Arena.Actor(a, ArenaColor.Enemy);
    }
}
