namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDiresaur;

public enum OID : uint
{
    Boss = 0x253A, //R=6.6
    BossAdd = 0x256F, //R=4.0
    BossHelper = 0x233C,
    BonusAddAltarMatanga = 0x2545, // R3.420
    BonusAddGoldWhisker = 0x2544, // R0.540
    FireVoidzone = 0x1EA140,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/2544->player, no cast, single-target
    AutoAttack2 = 872, // BonusAddAltarMatanga->player, no cast, single-target
    AutoAttack3 = 6497, // 256F->player, no cast, single-target
    DeadlyHold = 13217, // Boss->player, 3.0s cast, single-target
    HeatBreath = 13218, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    TailSmash = 13220, // Boss->self, 3.0s cast, range 20+R 90-degree cone
    RagingInferno = 13283, // Boss->self, 3.0s cast, range 60 circle
    Comet = 13835, // BossHelper->location, 3.0s cast, range 4 circle
    HardStomp = 13743, // 256F->self, 3.0s cast, range 6+R circle
    Fireball = 13219, // Boss->location, 3.0s cast, range 6 circle

    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23, // player
}

class DeadlyHold(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.DeadlyHold));
class HeatBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeatBreath), new AOEShapeCone(14.6f, 45.Degrees()));
class TailSmash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(26.6f, 45.Degrees()));
class RagingInferno(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RagingInferno));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);
class HardStomp(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HardStomp), new AOEShapeCircle(10));
class Fireball(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Fireball), 6);

class FireballBait(BossModule module) : Components.GenericBaitAway(module)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Baitaway)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(6)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Fireball)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
            targeted = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, 18));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (target == actor && targeted)
            hints.Add("Bait voidzone away! (3 times)");
    }
}

class FireballVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7));
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class DiresaurStates : StateMachineBuilder
{
    public DiresaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<HeatBreath>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<RagingInferno>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<HardStomp>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<FireballBait>()
            .ActivateOnEnter<FireballVoidzone>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddGoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7627)]
public class Diresaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddGoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddGoldWhisker => 4,
                OID.BonusAddAltarMatanga => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
