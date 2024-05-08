namespace BossMod.Shadowbringers.Hunt.RankA.Supay;

public enum OID : uint
{
    Boss = 0x2839, // R=3.6
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    BlasphemousHowl = 17858, // Boss->players, 3.0s cast, range 8 circle, spread, applies terror
    PetroEyes = 17856, // Boss->self, 3.0s cast, range 40 circle, gaze, inflicts petrification
    Beakaxe = 17857, // Boss->player, no cast, single-target, instantlyy kills petrified players
}

public enum IconID : uint
{
    Baitaway = 159, // player
}

class BlasphemousHowl(BossModule module) : Components.GenericBaitAway(module)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Baitaway)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(8)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BlasphemousHowl)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait away + look away!");
    }
}

class PetroEyes(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.PetroEyes));

class SupayStates : StateMachineBuilder
{
    public SupayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PetroEyes>()
            .ActivateOnEnter<BlasphemousHowl>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8891)]
public class Supay(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
