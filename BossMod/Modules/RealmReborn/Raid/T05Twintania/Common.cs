﻿using System;

namespace BossMod.RealmReborn.Raid.T05Twintania
{
    // mechanics used for the whole fight
    class Plummet : Components.Cleave
    {
        public Plummet() : base(ActionID.MakeSpell(AID.Plummet), new AOEShapeRect(20, 6)) { } // TODO: verify shape

        public override void Init(BossModule module)
        {
            base.Init(module);
            NextExpected = module.WorldState.CurrentTime.AddSeconds(6.5f);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if ((NextExpected - module.WorldState.CurrentTime).TotalSeconds < 3)
            {
                var boss = hints.PotentialTargets.Find(e => e.Actor == module.PrimaryActor);
                if (boss != null)
                    boss.AttackStrength += 0.3f;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
                NextExpected = module.WorldState.CurrentTime.AddSeconds(12.5f);
        }
    }

    // note: happens every ~36s; various other mechanics can delay it somewhat - it seems that e.g. phase transitions don't affect the running timer...
    // note: actual hit happens ~0.2s after watched cast end and has different IDs on different phases (P2+ is stronger and inflicts debuff)
    // TODO: is it true that taunt mid cast makes OT eat debuff? is it true that boss can be single-tanked in p2+?
    class DeathSentence : Components.CastCounter
    {
        public DateTime NextCastStart { get; private set; }
        public bool TankedByOT { get; private set; }
        public PartyRolesConfig.Assignment TankRole => TankedByOT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT;

        public DeathSentence() : base(ActionID.MakeSpell(AID.DeathSentence)) { }

        public override void Init(BossModule module)
        {
            NextCastStart = module.WorldState.CurrentTime.AddSeconds(18);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"Next death sentence in ~{(NextCastStart - module.WorldState.CurrentTime).TotalSeconds:f1}s");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var boss = hints.PotentialTargets.Find(e => e.Actor == module.PrimaryActor);
            if (boss == null)
                return;

            if ((NextCastStart - module.WorldState.CurrentTime).TotalSeconds < 1)
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
            if (module.PrimaryActor.CastInfo?.Action == WatchedAction)
            {
                var cooldownWindowEnd = (float)(module.PrimaryActor.CastInfo.NPCFinishAt - module.WorldState.CurrentTime).TotalSeconds;
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
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Vengeance), actor, cooldownWindowEnd, false));
                                    }
                                    else
                                    {
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Rampart), actor, cooldownWindowEnd, false));
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Reprisal), actor, cooldownWindowEnd, false));
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.ThrillOfBattle), actor, cooldownWindowEnd, false));
                                    }
                                    break;
                                case Class.PLD:
                                    hints.PlannedActions.Add((ActionID.MakeSpell(PLD.AID.Sheltron), actor, cooldownWindowEnd, false));
                                    if (useFirstCooldowns)
                                    {
                                        hints.PlannedActions.Add((ActionID.MakeSpell(PLD.AID.Sentinel), actor, cooldownWindowEnd, false));
                                    }
                                    else
                                    {
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Rampart), actor, cooldownWindowEnd, false));
                                        hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Reprisal), actor, cooldownWindowEnd, false));
                                    }
                                    break;
                            }
                        }
                        break;
                    case PartyRolesConfig.Assignment.M1:
                        if (NumCasts % 3 == 0)
                        {
                            hints.PlannedActions.Add((ActionID.MakeSpell(DRG.AID.Feint), module.PrimaryActor, cooldownWindowEnd, false));
                        }
                        break;
                    case PartyRolesConfig.Assignment.M2:
                        if (NumCasts % 3 == 1)
                        {
                            hints.PlannedActions.Add((ActionID.MakeSpell(DRG.AID.Feint), module.PrimaryActor, cooldownWindowEnd, false));
                        }
                        break;
                    case PartyRolesConfig.Assignment.R1:
                    case PartyRolesConfig.Assignment.R2:
                        if (NumCasts % 3 == 2)
                        {
                            hints.PlannedActions.Add((ActionID.MakeSpell(BLM.AID.Addle), module.PrimaryActor, cooldownWindowEnd, false));
                        }
                        break;
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                NextCastStart = module.WorldState.CurrentTime.AddSeconds(36);
                TankedByOT = !TankedByOT;
            }
        }
    }
}
