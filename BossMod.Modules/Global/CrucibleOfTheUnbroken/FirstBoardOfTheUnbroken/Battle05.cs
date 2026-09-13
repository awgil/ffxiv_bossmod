namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoardOfTheUnbroken.Battle05;

public enum OID : uint
{
    Boss = 0x4B90,
    SuccubusMage = 0x4B91,
    SuccubusKnight = 0x4B92,
    Pheromone = 0x4B93,
    Helper = 0x233C,
}
public enum AID : uint
{
    AutoAttack = 50396, // Boss/SuccubusKnight->player, no cast, single-target
    BloodRainVisual1 = 46923, // Boss->self, 5.0+1.0s cast, single-target
    BloodRainDonut = 46924, // Helper->self, 6.0s cast, range 8-40 donut
    BloodRainVisual2 = 46925, // Boss->self, 5.4+0.6s cast, single-target
    BloodRainCircle = 46926, // Helper->self, 6.0s cast, range 8 circle

    A = 46931, // Boss->location, no cast, single-target
    VoidAeroII = 46932, // Boss->self, 4.0s cast, range 60 width 8 rect
    VoidAeroIII = 46933, // Helper->self, 3.0s cast, range 60 20-degree cone
    ColdCaress = 46935, // Boss->player, 5.0s cast, single-target
    Summon = 46927, // Boss->self, 4.0s cast, single-target
    Aero = 50746, // SuccubusMage->player, no cast, single-target

    BloodSword = 46934, // Boss->player, 6.0s cast, single-target
    Lifeblood = 49508, // Helper->Boss, no cast, single-target
    BeguilingMist = 46936, // Boss->self, 5.0s cast, range 30 circle
    HeartShatterVisual = 46937, // 4B93->self, 1.0s cast, single-target
    HeartShatter = 46938, // Helper->self, 1.0s cast, range 24 circle
    Fanaticism = 46928, // SuccubusMage->Boss, 6.0s cast, single-target
}
class BloodRainDonut(BossModule module) : Components.StandardAOEs(module, AID.BloodRainDonut, new AOEShapeDonut(8, 40));
class BloodRainCircle(BossModule module) : Components.StandardAOEs(module, AID.BloodRainCircle, new AOEShapeCircle(8));
class VoidAeroII(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroII, new AOEShapeRect(60f, 4f));
class VoidAeroIII(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroIII, new AOEShapeCone(60f, 10.Degrees()));
class ColdCaress(BossModule module) : Components.SingleTargetCast(module, AID.ColdCaress, hint: "Applies Poison");
class BloodSword(BossModule module) : Components.SingleTargetCast(module, AID.BloodSword, hint: "Boss is healing!");
class BeguilingMist(BossModule module) : Components.RaidwideCast(module, AID.BeguilingMist, hint: "Applies Sleep");
class Pheromones(BossModule module) : Components.Voidzone(module, 24f, uint.MaxValue, moveHintLength: 15)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Pheromone && !actor.Position.AlmostEqual(Module.Center, 5))
            AddSource(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Pheromone)
            RemoveSource(actor);
    }
}
class HeartShatter(BossModule module) : Components.StandardAOEs(module, AID.HeartShatter, new AOEShapeCircle(24f));
class Fanaticism(BossModule module) : Components.CastInterruptHint(module, AID.Fanaticism);

class AddsMulti(BossModule module) : Components.AddsMulti(module, [(uint)OID.SuccubusKnight, (uint)OID.SuccubusMage]);

class Battle05States : StateMachineBuilder
{
    public Battle05States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodRainDonut>()
            .ActivateOnEnter<BloodRainCircle>()
            .ActivateOnEnter<VoidAeroII>()
            .ActivateOnEnter<VoidAeroIII>()
            .ActivateOnEnter<ColdCaress>()
            .ActivateOnEnter<BloodSword>()
            .ActivateOnEnter<BeguilingMist>()
            .ActivateOnEnter<Pheromones>()
            .ActivateOnEnter<HeartShatter>()
            .ActivateOnEnter<Fanaticism>()
            .ActivateOnEnter<AddsMulti>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088, NameID = 14541)]
public class Battle05(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), new ArenaBoundsRect(15, 20));
