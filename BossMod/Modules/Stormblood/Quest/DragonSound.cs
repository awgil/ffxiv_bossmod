namespace BossMod.Stormblood.Quest.DragonSound;

public enum OID : uint
{
    Boss = 0x1CDD, // R6.840, x1
    Faunehm = 0x18D6, // R0.500, x9
}

public enum AID : uint
{
    AbyssicBuster = 8929, // Boss->self, 2.0s cast, range 25+R 90-degree cone
    Heavensfall1 = 8935, // 18D6->location, 2.0s cast, range 5 circle
    DarkStar = 8931, // Boss->self, 2.0s cast, range 50+R circle
}

public enum SID : uint
{
    Enervation = 1401, // Boss->1CDE/player, extra=0x0
}

class AbyssicBuster(BossModule module) : Components.StandardAOEs(module, AID.AbyssicBuster, new AOEShapeCone(31.84f, 45.Degrees()));
class Heavensfall(BossModule module) : Components.StandardAOEs(module, AID.Heavensfall1, 5);
class DarkStar(BossModule module) : Components.RaidwideCast(module, AID.DarkStar);

// scripted interaction, no idea if it's required to complete the duty but might as well do it
class Enervation(BossModule module) : BossComponent(module)
{
    private bool Active;
    private Actor? OrnKhai;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor.OID == 0 && status.ID == (uint)SID.Enervation)
            Active = true;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor.OID == 0 && status.ID == (uint)SID.Enervation)
            Active = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        OrnKhai ??= WorldState.Actors.FirstOrDefault(x => x.OID == 0x1CDF);
        if (OrnKhai == null)
            return;

        hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.ElusiveJump), actor, ActionQueue.Priority.Medium, facingAngle: -actor.AngleTo(OrnKhai));

        hints.GoalZones.Add(p => p.InCircle(OrnKhai.Position, 3) ? 100 : 0);
    }
}

class FaunehmStates : StateMachineBuilder
{
    public FaunehmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssicBuster>()
            .ActivateOnEnter<Heavensfall>()
            .ActivateOnEnter<DarkStar>()
            .ActivateOnEnter<Enervation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68450, NameID = 6347)]
public class Faunehm(WorldState ws, Actor primary) : BossModule(ws, primary, new(4, 248.5f), new ArenaBoundsCircle(25));
