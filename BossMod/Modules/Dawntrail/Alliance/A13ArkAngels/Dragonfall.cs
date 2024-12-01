namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Dragonfall(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public int NumCasts;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Dragonfall)
            AddStack(source, WorldState.FutureTime(9.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DragonfallAOE)
        {
            ++NumCasts;
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            var forbidden = Raid.WithSlot().WhereActor(a => spell.Targets.Any(t => t.ID == a.InstanceID)).Mask();
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers |= forbidden;
        }
    }
}
