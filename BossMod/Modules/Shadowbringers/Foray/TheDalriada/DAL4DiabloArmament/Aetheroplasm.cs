namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class Aetheroplasm(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> orbs = [with(8)];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Aether)
        {
            orbs.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Aetheroplasm or (uint)AID.FusionBurst)
        {
            orbs.Remove(caster);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (orbs.Count != 0)
        {
            hints.Add("Soak the orbs!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = orbs.Count;
        if (count != 0)
        {
            var orbz = new ShapeDistance[count];
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            for (var i = 0; i < count; ++i)
            {
                var o = orbs[i];
                orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(new SDIntersection(orbz), WorldState.FutureTime(5d));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(orbs[i].Position, 1.5f, Colors.Safe);
        }
    }
}
