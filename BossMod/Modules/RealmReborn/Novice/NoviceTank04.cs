namespace BossMod.RealmReborn.Novice.NoviceTank04;

public enum OID : uint
{
    Boss = 0x1564,
    Helper = 0x233C,
}

class Tank04Dummies(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var count = 0;
        float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;

        foreach (var stateActor in WorldState.Actors)
        {
            if (stateActor.OID == (uint)OID.Boss)
            {
                count++;
                minX = Math.Min(minX, stateActor.Position.X);
                minZ = Math.Min(minZ, stateActor.Position.Z);
                maxX = Math.Max(maxX, stateActor.Position.X);
                maxZ = Math.Max(maxZ, stateActor.Position.Z);
                hints.SetPriority(stateActor, AIHints.Enemy.PriorityForbidden);
            }
        }

        if (count == 3)
        {
            var target = new WPos((maxX - minX) / 2f + minX, (maxZ - minZ) / 2f + minZ);
            hints.GoalZones.Add(hints.GoalSingleTarget(target, 1f));

            if (actor.DistanceToPoint(target) < 1f)
            {
                hints.ActionsToExecute.Clear();
                hints.ActionsToExecute.Push(new ActionID(ActionType.Spell, actor.Class switch
                {
                    Class.MRD or Class.WAR => 41u,
                    Class.GLA or Class.PLD => 7381u,
                    Class.DRK => 3621u,
                    _ => 11u
                }), null, 1f);
            }
        }
    }
}

class NoviceTank04States : StateMachineBuilder
{
    public NoviceTank04States(BossModule module) : base(module)
    {
        TrivialPhase()
           .ActivateOnEnter<Tank04Dummies>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 154, NameID = 541)]
public class NoviceTank04(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable;
}
