namespace BossMod.Components;

[SkipLocalsInit]
public class StackTogether(BossModule module, uint iconId, float activationDelay, float radius = 3f) : BossComponent(module)
{
    public readonly List<Actor> Targets = [];
    public DateTime Activation;
    public readonly uint Icon = iconId;
    public readonly float Radius = radius;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                Targets.Add(actor);
                if (Activation == default)
                {
                    Activation = WorldState.FutureTime(activationDelay);
                }
            }
        }
    }

    // this type of mechanic is usually only indicated by an icon (which has a fixed animation length) and we only get a cast event from the server if someone fails the mechanic, so we just have to make an educated guess of when the mechanic has resolved
    public override void Update()
    {
        if (Activation != default && Activation < WorldState.CurrentTime)
        {
            Activation = default;
            Targets.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Targets.Count;
        if (count == 0)
        {
            return;
        }

        var actorFound = false;
        var foundTarget = false;

        for (var i = 0; i < count; ++i)
        {
            var target = Targets[i];

            if (target == actor)
            {
                actorFound = true;
            }
            else if (target.Position.InCircle(actor.Position, Radius))
            {
                foundTarget = true;
            }

            if (actorFound && foundTarget)
            {
                break;
            }
        }

        if (actorFound)
        {
            hints.Add("Stack with other targets!", !foundTarget);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Targets.Count;
        if (count == 0)
        {
            return;
        }

        var actorFound = false;

        var positions = new List<WPos>(count);
        for (var i = 0; i < count; ++i)
        {
            var target = Targets[i];

            if (target == pc)
            {
                actorFound = true;
            }
            else
            {
                positions.Add(target.Position);
            }
        }
        if (!actorFound)
        {
            return;
        }

        var countPos = positions.Count;
        for (var i = 0; i < countPos; ++i)
        {
            Arena.ZoneCircleOutline(positions[i], Radius, Colors.Safe);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Targets.Count;
        if (count == 0)
        {
            return;
        }
        var actorFound = false;
        var forbidden = new List<ShapeDistance>(count);
        for (var i = 0; i < count; ++i)
        {
            var target = Targets[i];

            if (target == actor)
            {
                actorFound = true;
            }
            else
            {
                forbidden.Add(new SDInvertedCircle(target.Position, Radius));
            }
        }
        if (actorFound)
        {
            hints.AddForbiddenZone(new SDIntersection([.. forbidden]), Activation);
        }
    }
}
