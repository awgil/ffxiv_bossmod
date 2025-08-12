namespace BossMod.Stormblood.Quest.ReturnOfTheBull;

public enum OID : uint
{
    Boss = 0x1FD2,
    Helper = 0x233C,
    Lakshmi = 0x18D6, // R0.500, x12, Helper type
    DreamingKshatriya = 0x1FDD, // R1.000, x0 (spawn during fight)
    DreamingFighter = 0x1FDB, // R0.500, x0 (spawn during fight)
    Aether = 0x1FD3, // R1.000, x0 (spawn during fight)
    FordolaShield = 0x1EA080,
}

public enum AID : uint
{
    BlissfulSpear = 9872, // Lakshmi->self, 11.0s cast, range 40 width 8 cross
    BlissfulHammer = 9874, // Lakshmi->self, no cast, range 7 circle
    ThePallOfLight = 9877, // Boss->players/1FD8, 5.0s cast, range 6 circle
    ThePathOfLight = 9875, // Boss->self, 5.0s cast, range 40+R 120-degree cone
}

class PathOfLight(BossModule module) : Components.StandardAOEs(module, AID.ThePathOfLight, new AOEShapeCone(43.5f, 60.Degrees()));
class BlissfulSpear(BossModule module) : Components.StandardAOEs(module, AID.BlissfulSpear, new AOEShapeCross(40, 4));
class ThePallOfLight(BossModule module) : Components.StackWithCastTargets(module, AID.ThePallOfLight, 6, 1);
class BlissfulHammer(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(7), 109, AID.BlissfulHammer, 12.15f, true);
class FordolaShield(BossModule module) : BossComponent(module)
{
    public Actor? Shield => WorldState.Actors.FirstOrDefault(a => (OID)a.OID == OID.FordolaShield);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield != null)
            Arena.AddCircleFilled(Shield.Position, 4, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield != null)
            hints.AddForbiddenZone(new AOEShapeDonut(4, 100), Shield.Position, default, WorldState.FutureTime(5));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield != null && !actor.Position.InCircle(Shield.Position, 4))
            hints.Add("Go to safe zone!");
    }
}

class Deflect(BossModule module) : BossComponent(module)
{
    public IEnumerable<Actor> Spheres => Module.Enemies(OID.Aether).Where(x => !x.IsDeadOrDestroyed);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Spheres, 0xFFFFA080);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var deflectAction = WorldState.Client.DutyActions[0].Action;
        var deflectRadius = deflectAction.ID == 10006 ? 4 : 20;

        var closestSphere = Spheres.MaxBy(x => x.Position.Z);
        if (closestSphere != null)
        {
            var optimalDeflectPosition = closestSphere.Position with { Z = closestSphere.Position.Z + 1 };

            hints.GoalZones.Add(hints.GoalSingleTarget(optimalDeflectPosition, deflectRadius - 2, 10));

            if (actor.DistanceToHitbox(closestSphere) < deflectRadius - 1)
                hints.ActionsToExecute.Push(deflectAction, actor, ActionQueue.Priority.VeryHigh);
        }
    }
}

class LakshmiStates : StateMachineBuilder
{
    public LakshmiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Deflect>()
            .ActivateOnEnter<BlissfulSpear>()
            .ActivateOnEnter<ThePallOfLight>()
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<BlissfulHammer>()
            .ActivateOnEnter<FordolaShield>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68508, NameID = 6385)]
public class Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -353), new ArenaBoundsSquare(23))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID.Aether => -1,
                _ => 0
            };
    }
}
