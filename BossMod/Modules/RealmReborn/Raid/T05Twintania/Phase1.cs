namespace BossMod.RealmReborn.Raid.T05Twintania;

// P1 mechanics
class P1LiquidHellAdds(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.LiquidHellAdds), m => m.Enemies(OID.LiquidHell).Where(z => z.EventState != 7), 0); // note: voidzone appears ~1.2s after cast ends, but we want to try avoiding initial damage too

// after divebombs (P4), boss reappears at (-6.67, 5) - it is a good idea to drop two neurolinks at melee range to keep uptime
// otherwise it's a simple phase - kill adds, then move near boss and focus it
// we let plummet & death sentence module handle tanking and healing hints, since they are common to multiple phases
class P1AI(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        bool stillHaveAdds = false;
        foreach (var e in hints.PotentialTargets)
        {
            switch ((OID)e.Actor.OID)
            {
                case OID.Boss:
                    e.Priority = 1;
                    e.DesiredPosition = new(0, 5);
                    e.DesiredRotation = -90.Degrees();
                    break;
                case OID.ScourgeOfMeracydia:
                    stillHaveAdds = true;
                    e.Priority = 2;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    break;
            }
        }

        // after adds are dead, everyone should stay near boss in preparation to P2; don't bother doing it with tanks, so that we don't interfere with positioning
        if (!stillHaveAdds && actor.Role != Role.Tank)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, 8), DateTime.MaxValue);
        }
    }
}
