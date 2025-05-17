namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouPithekos;

public enum OID : uint
{
    Boss = 0x3D2B, //R=6
    BallOfLevin = 0x3E90,
    BossAdd = 0x3D2C, //R=4.2
    BossHelper = 0x233C,
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
}

public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target
    Thundercall = 32212, // Boss->location, 2.5s cast, range 3 circle
    LightningBolt = 32214, // Boss->self, 3.0s cast, single-target
    LightningBolt2 = 32215, // BossHelper->location, 3.0s cast, range 6 circle
    ThunderIV = 32213, // BallOfLevin->self, 7.0s cast, range 18 circle
    Spark = 32216, // Boss->self, 4.0s cast, range 14-30 donut
    AutoAttack2 = 870, // BossAdds->player, no cast, single-target
    RockThrow = 32217, // BossAdds->location, 3.0s cast, range 6 circle
    SweepingGouge = 32211, // Boss->player, 5.0s cast, single-target
    HeavySmash = 32317, // BossAdd_Lyssa -> location 3.0s cast, range 6 circle
}

public enum IconID : uint
{
    Thundercall = 111, // Thundercall marker
}

class Spark(BossModule module) : Components.StandardAOEs(module, AID.Spark, new AOEShapeDonut(14, 30));
class SweepingGouge(BossModule module) : Components.SingleTargetCast(module, AID.SweepingGouge);
class Thundercall(BossModule module) : Components.StandardAOEs(module, AID.Thundercall, 3);

class Thundercall2(BossModule module) : Components.GenericBaitAway(module)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Thundercall)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(18)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Thundercall)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 18));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait levinorb away!");
    }
}

class RockThrow(BossModule module) : Components.StandardAOEs(module, AID.RockThrow, 6);
class LightningBolt2(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt2, 6);
class ThunderIV(BossModule module) : Components.StandardAOEs(module, AID.ThunderIV, new AOEShapeCircle(18));
class HeavySmash(BossModule module) : Components.StandardAOEs(module, AID.HeavySmash, 6);

class PithekosStates : StateMachineBuilder
{
    public PithekosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spark>()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<Thundercall2>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<LightningBolt2>()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12001)]
public class Pithekos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddLyssa))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddLyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
