namespace BossMod.Shadowbringers.Hunt.RankA.Nariphon;

public enum OID : uint
{
    Boss = 0x2890, // R=6.0
}

public enum AID : uint
{
    AutoAttack = 870, // 2890->player, no cast, single-target
    VineHammer = 16969, // 2890->player, no cast, single-target, attacks several random players in a row
    AllergenInjection = 16972, // 2890->player, 5.0s cast, range 6 circle
    RootsOfAtopy = 16971, // 2890->player, 5.0s cast, range 6 circle
    OdiousMiasma = 16970, // 2890->self, 3.0s cast, range 12 120-degree cone
}

public enum SID : uint
{
    PiercingResistanceDownII = 1435, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Baitaway = 140, // player
    Stackmarker = 62, // player
}

class OdiousMiasma(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OdiousMiasma), new AOEShapeCone(12, 60.Degrees()));

class AllergenInjection(BossModule module) : Components.GenericBaitAway(module)
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
        if ((AID)spell.Action.ID == AID.AllergenInjection)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait away!");
    }
}

class RootsOfAtopy(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RootsOfAtopy)
            Stacks.Add(new(WorldState.Actors.Find(spell.TargetID)!, 6, activation: spell.NPCFinishAt, forbiddenPlayers: _forbidden));
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PiercingResistanceDownII)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PiercingResistanceDownII)
            _forbidden.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RootsOfAtopy)
            Stacks.RemoveAt(0);
    }
}

class NariphonStates : StateMachineBuilder
{
    public NariphonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OdiousMiasma>()
            .ActivateOnEnter<RootsOfAtopy>()
            .ActivateOnEnter<AllergenInjection>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8907)]
public class Nariphon(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
