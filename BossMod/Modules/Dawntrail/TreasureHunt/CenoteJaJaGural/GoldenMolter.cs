namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.GoldenMolter;

public enum OID : uint
{
    Boss = 0x4306, // R4.8
    VasoconstrictorPool = 0x1EBCB8, // R0.5

    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // UolonOfFortune->player, no cast, single-target
    Teleport = 38264, // Boss->location, no cast, single-target

    Lap = 38274, // Boss->player, 5s cast, single-target tankbuster

    LightburstVisual = 38276, // Helper->location, no cast, range 40 circle visual
    Lightburst = 38275, // Boss->self, 5s cast, single-target raidwide

    Crypsis1 = 38265, // Boss->self, 3s cast, single-target
    Crypsis2 = 38266, // Boss->self, 3s cast, single-target
    GoldenGall = 38267, // Boss->self, 7s cast, range 40 180-degree cone

    GoldenRadianceVisual = 38272, // Boss->self, 2.7+0.3s cast, single-target visual
    GoldenRadiance = 38273, // Helper->location, 3s cast, range 5 circle

    BlindingLightVisual = 38248, // Boss->self, no cast, single-target
    BlindingLight = 38580, // Helper->players, 5s cast, range 6 circle spread

    AetherialLightVisual = 38270, // Boss->self, 4.7+0.3s cast, single-target visual
    AetherialLight = 38271, // Helper->self, 5s cast, range 40 60-degree cone

    VasoconstrictorVisual = 38269, // Boss->location, no cast, range 6 circle visual
    Vasoconstrictor1 = 38335, // Helper->self, 5.7s cast, range 6 circle
    Vasoconstrictor2 = 38336, // Helper->self, 7.3s cast, range 6 circle
    Vasoconstrictor3 = 38337, // Helper->self, 9.3s cast, range 6 circle
    VasoconstrictorPool = 38268, // Boss->location, 4+0.6s cast, range 6 circle

    Inhale = 38280, // UolonOfFortune->self, 0.5s cast, range 27 120-degree cone
    Spin = 38279, // UolonOfFortune->self, 3.0s cast, range 11 circle
    Scoop = 38278, // UolonOfFortune->self, 4.0s cast, range 15 120-degree cone
    RottenSpores = 38277, // UolonOfFortune->location, 3.0s cast, range 6 circle
    TearyTwirl = 32301, // TuraliOnion->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum SID : uint
{
    Concealed = 3997, // Boss->Boss, extra=0x2
    Slime = 569 // Boss->player, extra=0x0
}

sealed class Lap(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Lap);
sealed class Lightburst(BossModule module) : Components.RaidwideCast(module, (uint)AID.Lightburst);

sealed class Crypsis(BossModule module) : BossComponent(module)
{
    private bool IsConcealed;
    private const int RevealDistance = 9;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsConcealed)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            hints.AddForbiddenZone(new SDInvertedCircle(Module.PrimaryActor.Position, RevealDistance));
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsConcealed)
            Arena.ZoneCircleOutline(Module.PrimaryActor.Position, RevealDistance, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Crypsis1 or (uint)AID.Crypsis2)
            IsConcealed = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Concealed)
            IsConcealed = false;
    }
}

sealed class GoldenGall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenGall, new AOEShapeCone(40f, 90f.Degrees()));
sealed class GoldenRadiance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenRadiance, 5f);
sealed class BlindingLight(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BlindingLight, 6f);

sealed class AetherialLight : Components.SimpleAOEs
{
    public AetherialLight(BossModule module) : base(module, (uint)AID.AetherialLight, new AOEShapeCone(40f, 30f.Degrees()), 4) { MaxDangerColor = 2; }
}

sealed class Vasoconstrictor(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Vasoconstrictor1, (uint)AID.Vasoconstrictor2, (uint)AID.Vasoconstrictor3], 6f);

sealed class VasoconstrictorPool(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(3)];
    private readonly AOEShapeCircle circle = new(17f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (_aoes.Count != 0 && state == 0x004)
            _aoes.RemoveAt(0);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VasoconstrictorPool)
            _aoes.Add(new(circle, actor.Position.Quantized()));
    }
}

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class RottenSpores(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenSpores, 6f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class GoldenMolterStates : StateMachineBuilder
{
    public GoldenMolterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Lap>()
            .ActivateOnEnter<Lightburst>()
            .ActivateOnEnter<Crypsis>()
            .ActivateOnEnter<GoldenGall>()
            .ActivateOnEnter<GoldenRadiance>()
            .ActivateOnEnter<BlindingLight>()
            .ActivateOnEnter<AetherialLight>()
            .ActivateOnEnter<Vasoconstrictor>()
            .ActivateOnEnter<VasoconstrictorPool>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<RottenSpores>()
            .Raw.Update = () => AllDeadOrDestroyed(GoldenMolter.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Kismet, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 13248)]
public sealed class GoldenMolter(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.TuraliEggplant, (uint)OID.TuraliTomato, (uint)OID.TuligoraQueen, (uint)OID.TuraliGarlic,
    (uint)OID.TuraliOnion, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.TuraliOnion => 6,
                (uint)OID.TuraliEggplant => 5,
                (uint)OID.TuraliGarlic => 4,
                (uint)OID.TuraliTomato => 3,
                (uint)OID.TuligoraQueen or (uint)OID.AlpacaOfFortune => 2,
                (uint)OID.UolonOfFortune => 1,
                _ => 0
            };
        }
    }
}
