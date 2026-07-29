namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

//Wave 1
// Acrolith = 14839u
// Lamia Rover = 14832u

//Wave 2
// Lamia Jaeger = 14830u
// Qutrub Forayer = 14835u
// Pining Abazohn = 14834u

//Wave 3
// Nemean Lion = 14829u
// Lamia No.2 = 14831u
// Assault Bhoot = 14836u

//Wave 4
// Medusa Swarmsinger = 14828u

public enum OID : uint
{
    Boss = 0x4DA6, // R1.800, x1 : PiningAbazohn - Marking as Boss to trigger arena draw.
    Acrolith1 = 0x4DA4, // R3.000, x3
    LamiaRover1 = 0x4DA3, // R1.100, x6
    Acrolith2 = 0x4DB5, // R3.000, x3
    LamiaRover2 = 0x4DB4, // R1.100, x2
    LamiaJaeger = 0x4DA5, // R1.100, x1 (spawn during fight)
    QutrubForayer = 0x4DA7, // R1.200, x2
    LamiaNo2 = 0x4DA8, // R1.650, x0 (spawn during fight)
    NemeanLion = 0x4DA9, // R4.400, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x2 (spawn during fight), Helper type
    AssaultBhoot = 0x4DAA, // R1.170, x0 (spawn during fight)
    MedusaSwarmsinger = 0x4DAB, // R2.250, x0 (spawn during fight)

}

public enum AID : uint
{
    AutoAttack = 870, // 4DB4/Boss/4DA7/4DA9/4DAB->player/4DAF/4DAC, no cast, single-target

    AcrolithAuto = 872, // 4DB5/4DAA->player/4DB1/4DB2/4DAD, no cast, single-target
    LamiaAuto = 873, // 4DA5/4DA8->player, no cast, single-target
    Earthshatter = 50085, // 4DA4->self, 4.0s cast, range 8 circle
    TranscendentShot = 50086, // 4DA5->self, 5.0s cast, range 60 width 5 rect
    LeapingCleaveVisual = 50087, // Boss->location, 3.5+1.5s cast, single-target
    Unk1 = 50639, // Helper->4D66, 5.0s cast, single-target
    LeapingCleave = 50481, // Helper->location, 5.0s cast, range 40 circle
    FeralLungeCast = 50088, // 4DA7->location, 2.0+2.0s cast, single-target
    FeralLunge = 50482, // Helper->location, 4.0s cast, range 10 circle
    WhirlingSlash = 50659, // Boss->self, 3.0s cast, range 6 circle
    Perdition = 50094, // 4DAA->self, 4.0s cast, range 9 circle
    Tourbillion = 50091, // 4DA9->self, 5.0s cast, range 40 width 50 rect
    PinningShotCast = 50089, // 4DA8->self, 7.0s cast, single-target
    PinningShot = 50090, // Helper->player, 7.0s cast, range 13 circle
    FulminationKhalkeos = 50092, // 4DA9->self, 4.0s cast, range 70 circle
    DanceToDustVisual = 50095, // 4DAB->self, 5.0s cast, single-target
    DanceToDustFirst = 50096, // Helper->self, 5.0s cast, range 7 circle
    DanceToDustRest = 50097, // Helper->self, no cast, range 7 circle
    RightShadowSlash = 50098, // 4DAB->self, 5.0s cast, range 60 180-degree cone
    LeftShadowSlash = 50099, // 4DAB->self, 5.0s cast, range 60 180-degree cone
    BellowingGrunt = 50103, // 4DAB->self, 4.0s cast, range 60 circle
    DisregardRaidwide = 50100, // 4DAB->self, 4.0s cast, range 60 circle
    DisregardRect = 50101, // Helper->self, 4.0s cast, range 55 width 10 rect
    Petrifaction = 50102, // 4DAB->self, 5.0s cast, range 60 circle
}

sealed class Earthshatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthshatter, 8);
sealed class TranscendentShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TranscendentShot, new AOEShapeRect(60f, 2.5f), maxCasts: 4);
sealed class LeapingCleave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LeapingCleave, 22f);
sealed class FeralLunge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeralLunge, 10f);
sealed class WhirlingSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhirlingSlash, 6f);
sealed class Perdition(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Perdition, 9f);
sealed class Tourbillion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tourbillion, new AOEShapeRect(40f, 25f));
sealed class PinningShot(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PinningShot, 13f, tankbuster: true);
sealed class FulminationKhalkeos(BossModule module) : Components.RaidwideCast(module, (uint)AID.FulminationKhalkeos);
sealed class DanceToDust(BossModule module) : Components.Exaflare(module, 7f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DanceToDustFirst)
            Lines.Add(new(caster.Position, caster.Rotation.ToDirection() * 8, Module.CastFinishAt(spell), 2,
                caster.Rotation.AlmostEqual(default, 0.1f) || caster.Rotation.AlmostEqual(180.Degrees(), 0.1f) ? 2 : 3, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DanceToDustFirst or (uint)AID.DanceToDustRest)
        {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
sealed class ShadowSlash(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightShadowSlash, (uint)AID.LeftShadowSlash], new AOEShapeCone(60f, 90f.Degrees()));
sealed class BellowingGrunt(BossModule module) : Components.RaidwideCast(module, (uint)AID.BellowingGrunt);
sealed class Disregard(BossModule module) : Components.RaidwideCast(module, (uint)AID.DisregardRaidwide);
sealed class DisregardRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DisregardRect, new AOEShapeRect(55f, 5f));
sealed class Petrifaction(BossModule module) : Components.CastGaze(module, (uint)AID.Petrifaction);

sealed class AlZahbiTrash(BossModule module) : Components.AddsMulti(module, A31AlZahbi.AlZahbiMobs);

sealed class A31AlZahbiStates : StateMachineBuilder
{
    public A31AlZahbiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlZahbiTrash>()
            .ActivateOnEnter<Earthshatter>()
            .ActivateOnEnter<TranscendentShot>()
            .ActivateOnEnter<LeapingCleave>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<WhirlingSlash>()
            .ActivateOnEnter<Perdition>()
            .ActivateOnEnter<Tourbillion>()
            .ActivateOnEnter<PinningShot>()
            .ActivateOnEnter<FulminationKhalkeos>()
            .ActivateOnEnter<DanceToDust>()
            .ActivateOnEnter<ShadowSlash>()
            .ActivateOnEnter<BellowingGrunt>()
            .ActivateOnEnter<Disregard>()
            .ActivateOnEnter<DisregardRect>()
            .ActivateOnEnter<Petrifaction>()
            .Raw.Update = () => AllDeadOrDestroyed(A31AlZahbi.AlZahbiMobs);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(A31AlZahbiStates),
    ConfigType = null,
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    Contributors = "Xan, ported by wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Alliance,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1117u,
    NameID = 14834u,
    SortOrder = 2,
    PlanLevel = 0)]

/*
 * This one is different because there isn't one mob exactly that is the boss.  To get around this
 * we mark the PiningAbazohn mob as 'Boss' in order to trigger arena draw.  We then use AddMulti()
 * to pull in all the trash with there various abilities.  The end condition is 'when all dead or destroyed'
 * for the AlZahbiMobs[] list.  We could also probably finish on Swarmsinger being dead as well since she
 * is generally last wave.
 */
public class A31AlZahbi(WorldState ws, Actor primary) : BossModule(ws, primary, new(721f, 720f), new ArenaBoundsRect(25f, 20f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;

    public static readonly uint[] AlZahbiMobs =
        [
            (uint)OID.Acrolith1,
            (uint)OID.Acrolith2,
            (uint)OID.LamiaRover1,
            (uint)OID.LamiaRover2,
            (uint)OID.LamiaJaeger,
            (uint)OID.QutrubForayer,
            (uint)OID.Boss,
            (uint)OID.NemeanLion,
            (uint)OID.LamiaNo2,
            (uint)OID.AssaultBhoot,
            (uint)OID.MedusaSwarmsinger
        ];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, AlZahbiMobs);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
