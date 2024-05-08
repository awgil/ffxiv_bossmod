namespace BossMod.Shadowbringers.Hunt.RankA.TheMudman;

public enum OID : uint
{
    Boss = 0x281F, // R=4.2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    FeculentFlood = 16828, // Boss->self, 3.0s cast, range 40 60-degree cone
    RoyalFlush = 16826, // Boss->self, 3.0s cast, range 8 circle
    BogBequest = 16827, // Boss->self, 5.0s cast, range 5-20 donut
    GravityForce = 16829, // Boss->player, 5.0s cast, range 6 circle, interruptible, applies heavy
}

public enum IconID : uint
{
    Baitaway = 140, // player
}

class BogBequest(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BogBequest), new AOEShapeDonut(5, 20));
class FeculentFlood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FeculentFlood), new AOEShapeCone(40, 30.Degrees()));
class RoyalFlush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RoyalFlush), new AOEShapeCircle(8));

class GravityForce(BossModule module) : Components.GenericBaitAway(module)
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

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GravityForce)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait away or interrupt!");
    }
}

class GravityForceHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.GravityForce));

class TheMudmanStates : StateMachineBuilder
{
    public TheMudmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BogBequest>()
            .ActivateOnEnter<FeculentFlood>()
            .ActivateOnEnter<RoyalFlush>()
            .ActivateOnEnter<GravityForce>()
            .ActivateOnEnter<GravityForceHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8654)]
public class TheMudman(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
