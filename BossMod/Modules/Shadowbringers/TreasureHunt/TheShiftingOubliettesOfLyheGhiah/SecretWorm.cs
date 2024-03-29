// CONTRIB: made by malediktus, not checked
namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretWorm;

public enum OID : uint
{
    Boss = 0x3029, //R=6.0
    Bubble = 0x302A, //R=1.5
    BossHelper = 0x233C,
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Hydroburst = 21714, // 302A->self, 1,0s cast, range 8 circle
    Hydrocannon = 21713, // Boss->location, 3,0s cast, range 8 circle
    AquaBurst = 21715, // 302A->self, 5,0s cast, range 50 circle, damage fall off AOE, optimal range seems to be 10
    FreshwaterCannon = 21711, // Boss->self, 2,5s cast, range 46 width 4 rect
    BrineBreath = 21710, // Boss->player, 4,0s cast, single-target
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
            .ActivateOnEnter<Hydroburst>();
    }
}

[ModuleInfo(CFCID = 745, NameID = 9780)]
public class Worm : BossModule
{
    public Worm(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }
}
