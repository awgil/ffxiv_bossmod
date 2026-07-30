namespace BossMod.Stormblood.Quest.MSQ.ReturnOfTheBull;

public enum OID : uint
{
    Boss = 0x1FD2,
    DreamingKshatriya = 0x1FDD, // R1.0
    DreamingFighter = 0x1FDB, // R0.5
    Aether = 0x1FD3, // R1.0
    FordolaShield = 0x1EA080,
    Helper2 = 0x18D6,
    Helper = 0x233C
}

public enum AID : uint
{
    BlissfulSpear = 9872, // Lakshmi->self, 11.0s cast, range 40 width 8 cross
    BlissfulHammer = 9874, // Lakshmi->self, no cast, range 7 circle
    ThePallOfLight = 9877, // Boss->players/1FD8, 5.0s cast, range 6 circle
    ThePathOfLight = 9875 // Boss->self, 5.0s cast, range 40+R 120-degree cone
}

class PathOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThePathOfLight, new AOEShapeCone(43.5f, 60f.Degrees()));
class BlissfulSpear(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlissfulSpear, new AOEShapeCross(40f, 4f));
class ThePallOfLight(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThePallOfLight, 6f, 1);
class BlissfulHammer(BossModule module) : Components.BaitAwayIcon(module, 7f, 109u, (uint)AID.BlissfulHammer, 12.1f);
class FordolaShield(BossModule module) : BossComponent(module)
{
    public Actor? Shield => WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.FordolaShield);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield != null)
            Arena.ZoneCircle(Shield.Position, 4f, Colors.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield != null)
            hints.AddForbiddenZone(new SDInvertedCircle(Shield.Position, 4f), WorldState.FutureTime(5d));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shield != null && !actor.Position.InCircle(Shield.Position, 4f))
            hints.Add("Go to safe zone!");
    }
}

class Deflect(BossModule module) : BossComponent(module)
{
    public IEnumerable<Actor> Spheres => Module.Enemies((uint)OID.Aether).Where(x => !x.IsDeadOrDestroyed);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Spheres, Colors.Other9);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var deflectAction = WorldState.Client.DutyActions[0].Action;
        var deflectRadius = deflectAction.ID == 10006u ? 4 : 20;

        var closestSphere = Spheres.MaxBy(x => x.PosRot.Z);
        if (closestSphere != null)
        {
            var pos = closestSphere.Position;
            WPos optimalDeflectPosition = new(pos.X, pos.Z + 1);

            hints.GoalZones.Add(AIHints.GoalSingleTarget(optimalDeflectPosition, deflectRadius - 2, 10));

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
            .ActivateOnEnter<FordolaShield>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68508, NameID = 6385)]
public class Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -353), new ArenaBoundsSquare(23))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID.Aether => -1,
                _ => 0
            };
        }
    }
}

