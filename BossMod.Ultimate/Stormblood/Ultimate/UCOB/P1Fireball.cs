namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Fireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, AID.Fireball, 4, 5.3f, 4)
{
    int _neurolinkCount = 0;

    public WPos Destination { get; private set; }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            if (_neurolinkCount == 0)
            {
                AddStack(actor, WorldState.FutureTime(5.3f), Raid.WithSlot().WhereActor(a => a.Role == Role.Tank).Mask());
                Destination = Module.PrimaryActor.Position + Module.PrimaryActor.DirectionTo(Arena.Center) * (Module.PrimaryActor.HitboxRadius + 3);
            }
            else
                AddStack(actor, WorldState.FutureTime(7.4f));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Neurolink)
            _neurolinkCount++;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Destination != default)
            Arena.AddCircle(Destination, 0.5f, ArenaColor.Safe, 2);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.Clear();
            Destination = default;
        }

        if ((AID)spell.Action.ID == AID.LiquidHell && Destination == default && _neurolinkCount > 0)
        {
            // stack on opposite side of boss from first fireball spawn
            var dirToBall = -Module.PrimaryActor.DirectionTo(spell.TargetXZ);
            Destination = Module.PrimaryActor.Position + dirToBall * Module.PrimaryActor.HitboxRadius;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Destination == default)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (!EnableHints || Stacks.Count == 0)
            return;

        var stack = Stacks[0];

        if (stack.Target == actor) // stack target shouldn't move around too much, just plant on boss
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(Destination, new(0, 1), 0.5f, actor.Position, 0.1f), stack.Activation);
        else if (!stack.ForbiddenPlayers[slot])
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Destination, stack.Radius - 1), stack.Activation);
        else
            hints.AddForbiddenZone(ShapeDistance.Circle(Destination, stack.Radius + 0.5f), stack.Activation);
    }
}
