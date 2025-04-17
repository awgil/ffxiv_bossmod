namespace BossMod.RealmReborn.Raid.T05Twintania;

// mechanics used for the whole fight
class Plummet : Components.Cleave
{
    public Plummet(BossModule module) : base(module, AID.Plummet, new AOEShapeRect(20, 6)) // TODO: verify shape
    {
        NextExpected = WorldState.FutureTime(6.5f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if ((NextExpected - WorldState.CurrentTime).TotalSeconds < 3)
        {
            var boss = hints.FindEnemy(Module.PrimaryActor);
            if (boss != null)
                boss.AttackStrength += 0.3f;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            NextExpected = WorldState.FutureTime(12.5f);
    }
}

// note: happens every ~36s; various other mechanics can delay it somewhat - it seems that e.g. phase transitions don't affect the running timer...
// note: actual hit happens ~0.2s after watched cast end and has different IDs on different phases (P2+ is stronger and inflicts debuff)
// TODO: is it true that taunt mid cast makes OT eat debuff? is it true that boss can be single-tanked in p2+?
class DeathSentence(BossModule module) : Components.CastCounter(module, AID.DeathSentence)
{
    public DateTime NextCastStart { get; private set; } = module.WorldState.FutureTime(18);
    public bool TankedByOT { get; private set; }
    public PartyRolesConfig.Assignment TankRole => TankedByOT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT;

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Next death sentence in ~{(NextCastStart - WorldState.CurrentTime).TotalSeconds:f1}s");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var boss = hints.FindEnemy(Module.PrimaryActor);
        if (boss == null)
            return;

        if ((NextCastStart - WorldState.CurrentTime).TotalSeconds < 1)
        {
            boss.AttackStrength += 0.3f;
        }

        // cooldowns and tanking order:
        // - we assume MT starts tanking and every cast is tank swap
        // - we assume that min time between tankbusters is 30s and plan cooldown rotation around that; this means that every tank has to press something every minute
        // - WAR presses vengeance at 0/120/... and rampart + reprisal + ToB at 60/180/...
        // - PLD presses sheltron every time, sentinel at 0/120/... and rampart + reprisal at 60/180/...
        // - we assume standard raid composition, so we have 2 feints and 1 addle; we rotate them in order M1 feint -> M2 feint -> caster addle -> repeat
        // - component gets destroyed at the end of P2 and recreated at the beginning of P4 - it takes at least 150s, which is more than any cooldowns we use
        boss.ShouldBeTanked = TankRole == assignment;
        boss.PreferProvoking = true;
        if (Module.PrimaryActor.CastInfo?.Action == WatchedAction)
        {
            var cooldownWindowEnd = Module.PrimaryActor.CastInfo.NPCRemainingTime;
            switch (assignment)
            {
                case PartyRolesConfig.Assignment.MT:
                case PartyRolesConfig.Assignment.OT:
                    if (!boss.ShouldBeTanked) // cooldowns should be used by previous tank, who will eat death's sentence
                    {
                        bool useFirstCooldowns = (NumCasts & 2) == 0;
                        switch (actor.Class)
                        {
                            case Class.WAR:
                                if (useFirstCooldowns)
                                {
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.Vengeance), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                }
                                else
                                {
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Rampart), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.ThrillOfBattle), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                }
                                break;
                            case Class.PLD:
                                hints.ActionsToExecute.Push(ActionID.MakeSpell(PLD.AID.Sheltron), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                if (useFirstCooldowns)
                                {
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(PLD.AID.Sentinel), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                }
                                else
                                {
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Rampart), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), actor, ActionQueue.Priority.High, cooldownWindowEnd);
                                }
                                break;
                        }
                    }
                    break;
                case PartyRolesConfig.Assignment.M1:
                    if (NumCasts % 3 == 0)
                    {
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Feint), Module.PrimaryActor, ActionQueue.Priority.High, cooldownWindowEnd);
                    }
                    break;
                case PartyRolesConfig.Assignment.M2:
                    if (NumCasts % 3 == 1)
                    {
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Feint), Module.PrimaryActor, ActionQueue.Priority.High, cooldownWindowEnd);
                    }
                    break;
                case PartyRolesConfig.Assignment.R1:
                case PartyRolesConfig.Assignment.R2:
                    if (NumCasts % 3 == 2)
                    {
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Addle), Module.PrimaryActor, ActionQueue.Priority.High, cooldownWindowEnd);
                    }
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            NextCastStart = WorldState.FutureTime(36);
            TankedByOT = !TankedByOT;
        }
    }
}
