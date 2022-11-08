namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    class TankPurge : Components.RaidwideCast
    {
        public TankPurge() : base(ActionID.MakeSpell(AID.TankPurge)) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);

            // execute raid saves; it happens every ~45s, so 2 people in reprisal/feint/addle rotation
            if (Active)
            {
                switch (assignment)
                {
                    case PartyRolesConfig.Assignment.MT:
                        if ((NumCasts & 1) == 0)
                            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Reprisal), actor, 1, false));
                        break;
                    case PartyRolesConfig.Assignment.OT:
                        if ((NumCasts & 1) == 1)
                            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Reprisal), actor, 1, false));
                        break;
                    case PartyRolesConfig.Assignment.M1:
                        if ((NumCasts & 1) == 0)
                            hints.PlannedActions.Add((ActionID.MakeSpell(DRG.AID.Feint), module.PrimaryActor, 1, false));
                        break;
                    case PartyRolesConfig.Assignment.M2:
                        if ((NumCasts & 1) == 1)
                            hints.PlannedActions.Add((ActionID.MakeSpell(DRG.AID.Feint), module.PrimaryActor, 1, false));
                        break;
                    case PartyRolesConfig.Assignment.R1:
                        if ((NumCasts & 1) == 0)
                            hints.PlannedActions.Add((ActionID.MakeSpell(BLM.AID.Addle), module.PrimaryActor, 1, false));
                        break;
                    case PartyRolesConfig.Assignment.R2:
                        if ((NumCasts & 1) == 1)
                            hints.PlannedActions.Add((ActionID.MakeSpell(BLM.AID.Addle), module.PrimaryActor, 1, false));
                        break;
                }
            }
        }
    }
}
