namespace BossMod.Endwalker.TreasureHunt.Excitatron6000.LuckyFace;

public enum OID : uint
{
    Boss = 0x377F, // R3.240
    BossHelper = 0x233C, // R0.500
    ExcitingQueen = 0x380C, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingTomato = 0x380B, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingGarlic = 0x380A, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingEgg = 0x3809, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingOnion = 0x3808, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    AutoAttack = 27980, // Boss->player, no cast, single-target
    FakeLeftInTheDark = 27989, // Boss->self, 4.0s cast, range 20 180-degree cone
    LeftInTheDark = 27990, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    HeartOnFireII = 28001, // BossHelper->location, 3.0s cast, range 6 circle
    FakeHeartOnFireII = 27988, // Boss->self, 3.0s cast, single-target
    HeartOnFireIV = 27981, // Boss->player, 5.0s cast, single-target
    FakeQuakeInYourBoots = 27997, // Boss->self, 4.0s cast, range 10 circle
    QuakeInYourBoots = 27998, // BossHelper->self, 4.0s cast, range 10 circle
    MerryGoRound1 = 27983, // Boss->self, 3.0s cast, single-target, boss animation
    FakeLeftInTheDark2 = 27993, // Boss->self, 4.0s cast, range 20 180-degree cone
    LeftInTheDark2 = 27994, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    MerryGoRound2 = 27986, // Boss->self, no cast, single-target, boss animation
    FakeQuakeMeAway = 27999, // Boss->self, 4.0s cast, range 10-20 donut
    QuakeMeAway = 28000, // BossHelper->self, 4.0s cast, range 10-20 donut
    MerryGoRound3 = 27984, // Boss->self, 3.0s cast, single-target
    FakeRightInTheDark = 27991, // Boss->self, 4.0s cast, range 20 180-degree cone
    RightInTheDark1 = 27992, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    MerryGoRound4 = 27985, // Boss->self, no cast, single-target, boss animation
    applyspreadmarkers = 28045, // Boss->self, no cast, single-target
    FakeQuakeInYourBoots2 = 28090, // Boss->self, 4.0s cast, range 10 circle
    QuakeInYourBoots2 = 28189, // BossHelper->self, 4.0s cast, range 10-20 donut
    HeartOnFireIII = 28002, // BossHelper->player, 5.0s cast, range 6 circle
    TempersFlare = 27982, // Boss->self, 5.0s cast, range 60 circle
    FakeRightInTheDark2 = 27995, // Boss->self, 4.0s cast, range 20 180-degree cone
    RightInTheDark2 = 27996, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    FakeQuakeMeAway2 = 28091, // Boss->self, 4.0s cast, range 10-20 donut
    QuakeMeAway2 = 28190, // BossHelper->self, 4.0s cast, range 10 circle
    MerryGoRound5 = 27987, // Boss->self, no cast, single-target, boss animation
    unknown2 = 28145, // Boss->self, no cast, single-target, probably death animation since it was cast after death
    PluckAndPrune = 6449, // 3809->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // 3808->self, 3.5s cast, range 6+R circle
    Telega = 9630, // 380C->self, no cast, single-target, bonus add disappear
    HeirloomScream = 6451, // 380B->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // 380A->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // 380C->self, 3.5s cast, range 6+R circle
}

public enum IconID : uint
{
    tankbuster = 218,
    spreadmarker = 194,
}

class LeftInTheDark1(BossModule module) : Components.StandardAOEs(module, AID.LeftInTheDark, new AOEShapeCone(20, 90.Degrees()));
class LeftInTheDark2(BossModule module) : Components.StandardAOEs(module, AID.LeftInTheDark2, new AOEShapeCone(20, 90.Degrees()));
class RightInTheDark1(BossModule module) : Components.StandardAOEs(module, AID.RightInTheDark1, new AOEShapeCone(20, 90.Degrees()));
class RightInTheDark2(BossModule module) : Components.StandardAOEs(module, AID.RightInTheDark2, new AOEShapeCone(20, 90.Degrees()));
class QuakeInYourBoots1(BossModule module) : Components.StandardAOEs(module, AID.QuakeInYourBoots, new AOEShapeCircle(10));
class QuakeInYourBoots2(BossModule module) : Components.StandardAOEs(module, AID.QuakeInYourBoots2, new AOEShapeDonut(10, 20));
class QuakeMeAway1(BossModule module) : Components.StandardAOEs(module, AID.QuakeMeAway, new AOEShapeDonut(10, 20));
class QuakeMeAway2(BossModule module) : Components.StandardAOEs(module, AID.QuakeMeAway2, new AOEShapeCircle(10));
class HeartOnFireII(BossModule module) : Components.StandardAOEs(module, AID.HeartOnFireII, 6);
class HeartOnFireIV(BossModule module) : Components.SingleTargetCast(module, AID.HeartOnFireIV);

class HeartOnFireIII(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.spreadmarker)
            AddSpread(actor);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeartOnFireIII)
            Spreads.Clear();
    }
}

class TempersFlare(BossModule module) : Components.RaidwideCast(module, AID.TempersFlare);
class PluckAndPrune(BossModule module) : Components.StandardAOEs(module, AID.PluckAndPrune, new AOEShapeCircle(6.84f));
class TearyTwirl(BossModule module) : Components.StandardAOEs(module, AID.TearyTwirl, new AOEShapeCircle(6.84f));
class HeirloomScream(BossModule module) : Components.StandardAOEs(module, AID.HeirloomScream, new AOEShapeCircle(6.84f));
class PungentPirouette(BossModule module) : Components.StandardAOEs(module, AID.PungentPirouette, new AOEShapeCircle(6.84f));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(6.84f));

class LuckyFaceStates : StateMachineBuilder
{
    public LuckyFaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftInTheDark1>()
            .ActivateOnEnter<LeftInTheDark2>()
            .ActivateOnEnter<RightInTheDark1>()
            .ActivateOnEnter<RightInTheDark2>()
            .ActivateOnEnter<QuakeInYourBoots1>()
            .ActivateOnEnter<QuakeInYourBoots2>()
            .ActivateOnEnter<QuakeMeAway1>()
            .ActivateOnEnter<QuakeMeAway2>()
            .ActivateOnEnter<TempersFlare>()
            .ActivateOnEnter<HeartOnFireII>()
            .ActivateOnEnter<HeartOnFireIII>()
            .ActivateOnEnter<HeartOnFireIV>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.ExcitingEgg).All(e => e.IsDead) && module.Enemies(OID.ExcitingQueen).All(e => e.IsDead) && module.Enemies(OID.ExcitingOnion).All(e => e.IsDead) && module.Enemies(OID.ExcitingGarlic).All(e => e.IsDead) && module.Enemies(OID.ExcitingTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 819, NameID = 10831)]
public class LuckyFace(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -460), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ExcitingEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.ExcitingTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.ExcitingQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.ExcitingGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.ExcitingOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ExcitingOnion => 6,
                OID.ExcitingEgg => 5,
                OID.ExcitingGarlic => 4,
                OID.ExcitingTomato => 3,
                OID.ExcitingQueen => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
