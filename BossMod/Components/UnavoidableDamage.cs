using System;

namespace BossMod.Components
{
    // generic unavoidable raidwide cast
    public class RaidwideCast : CastHint
    {
        public RaidwideCast(ActionID aid, string hint = "Raidwide") : base(aid, hint) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), c.CastInfo!.NPCFinishAt));
        }
    }

    // generic unavoidable raidwide cast after NPC yell, typically used at the end of boss "limit break" phases
    public class RaidwideAfterNPCYell : CastHint
    {
        public uint NPCYellID;
        public float Delay; //delay from NPCyell for raidwide to cast event
        private bool casting;
        private DateTime _activation;
        public RaidwideAfterNPCYell(ActionID aid, uint nPCYellid, float delay, string hint = "Raidwide") : base(aid, hint)
        {
            NPCYellID = nPCYellid;
            Delay = delay;
        }

        public override void OnActorNpcYell(BossModule module, Actor actor, ushort id)
        {
            if (id == NPCYellID)
            {
                casting = true;
                _activation = module.WorldState.CurrentTime;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
                casting = false;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (casting)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _activation.AddSeconds(Delay)));
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting && Hint.Length > 0)
                hints.Add(Hint);
        }
    }

    // generic unavoidable single-target damage cast (typically tankbuster, but not necessary)
    public class SingleTargetCast : CastHint
    {
        public SingleTargetCast(ActionID aid, string hint = "Tankbuster") : base(aid, hint) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
            {
                BitMask targets = new();
                targets.Set(module.Raid.FindSlot(c.CastInfo!.TargetID));
                hints.PredictedDamage.Add((targets, c.CastInfo!.NPCFinishAt));
            }
        }
    }
}
