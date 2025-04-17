namespace BossMod.Dawntrail.Ultimate.FRU;

class P2HallowedRay(BossModule module) : Components.GenericWildCharge(module, 3, AID.HallowedRayAOE, 65)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
            return;
        if (PlayerRoles[slot] == PlayerRole.Target || Activation > WorldState.FutureTime(2.5f))
        {
            // stay at the average direction to the raid
            // TODO: for melees, allow doing positionals...
            WDir averageDir = default;
            foreach (var p in Raid.WithoutSlot())
                averageDir += (p.Position - Source.Position).Normalized();
            hints.AddForbiddenZone(ShapeContains.InvertedRect(Source.Position, Angle.FromDirection(averageDir), 20, -6, 2), Activation);
        }
        else
        {
            // default hints (go to target)
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HallowedRay)
        {
            Source = actor;
            Activation = WorldState.FutureTime(5.7f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}
