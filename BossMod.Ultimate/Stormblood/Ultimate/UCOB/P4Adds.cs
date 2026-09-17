namespace BossMod.Stormblood.Ultimate.UCOB;

class P4AddsPositioning(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var spawned = false;

        if (hints.FindEnemy(((UCOB)Module).Twintania()) is { } t)
        {
            spawned = true;
            t.DesiredPosition = new(11.5f, -5.5f);
            t.DesiredRotation = 180.Degrees();
        }

        if (hints.FindEnemy(((UCOB)Module).Nael()) is { } n)
        {
            spawned = true;
            n.DesiredPosition = new(13, -1);
            n.DesiredRotation = 90.Degrees();
        }

        if (spawned)
        {
            // healers should chill next to bosses
            // TODO: check if needed, healer AOE rotation is a gain on 2
            //if (actor.Class.GetRole() == Role.Healer)
            //    hints.GoalZones.Add(AIHints.GoalSingleTarget(new(18, -9), 5, 0.5f));
        }
        else
        {
            switch (actor.Class.GetRole())
            {
                case Role.Melee:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(new(0, -11), 4));
                    break;
                case Role.Healer:
                case Role.Ranged:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(new(0, 0), 4));
                    break;
            }
        }
    }
}
