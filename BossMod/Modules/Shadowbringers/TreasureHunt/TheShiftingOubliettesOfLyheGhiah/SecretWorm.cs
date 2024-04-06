namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretWorm;

public enum OID : uint
{
    Boss = 0x3029, //R=6.0
    Bubble = 0x302A, //R=1.5
    BossHelper = 0x233C,
    SecretQueen = 0x3021, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdds->player, no cast, single-target
    Hydroburst = 21714, // 302A->self, 1,0s cast, range 8 circle
    Hydrocannon = 21713, // Boss->location, 3,0s cast, range 8 circle
    AquaBurst = 21715, // 302A->self, 5,0s cast, range 50 circle, damage fall off AOE, optimal range seems to be 10
    FreshwaterCannon = 21711, // Boss->self, 2,5s cast, range 46 width 4 rect
    BrineBreath = 21710, // Boss->player, 4,0s cast, single-target

    Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
    TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
    HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
    PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
    PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
};

public enum IconID : uint
{
    Baitaway = 23, // player
};

class Hydrocannon : Components.LocationTargetedAOEs
{
    public Hydrocannon() : base(ActionID.MakeSpell(AID.Hydrocannon), 8) { }
}

class FreshwaterCannon : Components.SelfTargetedAOEs
{
    public FreshwaterCannon() : base(ActionID.MakeSpell(AID.FreshwaterCannon), new AOEShapeRect(46, 2)) { }
}

class AquaBurst : Components.SelfTargetedAOEs
{
    public AquaBurst() : base(ActionID.MakeSpell(AID.AquaBurst), new AOEShapeCircle(10)) { }
}

class BrineBreath : Components.SingleTargetCast
{
    public BrineBreath() : base(ActionID.MakeSpell(AID.BrineBreath)) { }
}

class Hydroburst : Components.PersistentVoidzone
{
    public Hydroburst() : base(8, m => m.Enemies(OID.Bubble).Where(x => !x.IsDead && !(x.CastInfo != null && x.CastInfo.IsSpell(AID.AquaBurst)))) { }
}

class Bubble : Components.GenericBaitAway
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Baitaway)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(8)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hydrocannon)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 17.5f));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (target == actor && targeted)
            hints.Add("Bait bubble away!");
    }
}

class PluckAndPrune : Components.SelfTargetedAOEs
{
    public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f)) { }
}

class TearyTwirl : Components.SelfTargetedAOEs
{
    public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f)) { }
}

class HeirloomScream : Components.SelfTargetedAOEs
{
    public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f)) { }
}

class PungentPirouette : Components.SelfTargetedAOEs
{
    public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f)) { }
}

class Pollen : Components.SelfTargetedAOEs
{
    public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f)) { }
}

class WormStates : StateMachineBuilder
{
    public WormStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FreshwaterCannon>()
            .ActivateOnEnter<AquaBurst>()
            .ActivateOnEnter<BrineBreath>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Bubble>()
            .ActivateOnEnter<Hydroburst>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead);

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9780)]
public class Worm : BossModule
{
    public Worm(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.SecretEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SecretOnion => 6,
                OID.SecretEgg => 5,
                OID.SecretGarlic => 4,
                OID.SecretTomato => 3,
                OID.SecretQueen => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
