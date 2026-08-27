namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.LiquidHell, m => m.Enemies(OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f)
{
    bool BaitAtWall;
    DateTime NextCast;

    public void Reset(float delay, bool baitAtWall)
    {
        BaitAtWall = baitAtWall;
        NextCast = WorldState.FutureTime(delay);
        NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (BaitAtWall && NumCasts >= 5)
        {
            NextCast = default;
            BaitAtWall = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (BaitAtWall)
        {
            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);

            if (assignments.Length == 0)
                return;

            bool isBaiter;

            var slotR1 = assignments[(int)PartyRolesConfig.Assignment.R1];
            if (Module.FindComponent<Hatch>()?.IsTarget(slotR1) == true)
                isBaiter = assignment == PartyRolesConfig.Assignment.H1;
            else
                isBaiter = assignment == PartyRolesConfig.Assignment.R1;

            if (isBaiter)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 18), NextCast);

                // don't drop on neurolinks
                foreach (var nl in Module.Enemies(OID.Neurolink))
                    hints.AddForbiddenZone(ShapeDistance.Circle(nl.Position, 7), NextCast);
            }
            else
                hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor.Position, 16, 0.1f));
        }
    }
}

class P1LiquidHell : LiquidHell
{
    public P1LiquidHell(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
