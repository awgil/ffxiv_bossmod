namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateSuppression(BossModule module) : BossComponent(module)
{
    bool EruptionsStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EruptionAOE)
            EruptionsStarted = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (((UWU)Module).Ultima() is { IsTargetable: true })
        {
            if (actor.Role != Role.Tank)
                hints.GoalZones.Add(AIHints.GoalProximity(new(86, 86), 12, 0.2f));
            return;
        }

        if (!EruptionsStarted)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(94, 108), 1));
            else if (assignment == PartyRolesConfig.Assignment.OT)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(96, 108), 1));
            else
            {
                var myOrder = Raid.WithoutSlot(includeDead: true).Where(a => a.Role != Role.Tank).OrderBy(o => o.InstanceID).Index().First(t => t.Item == actor).Index;
                var myAngle = -(125 + 5 * myOrder).Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + myAngle.ToDirection() * 18, 1));
            }
        }
    }
}
