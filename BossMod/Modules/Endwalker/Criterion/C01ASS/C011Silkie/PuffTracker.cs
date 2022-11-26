using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    class PuffTracker : BossComponent
    {
        public List<Actor> BracingPuffs = new();
        public List<Actor> ChillingPuffs = new();
        public List<Actor> FizzlingPuffs = new();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(BracingPuffs, 0xff80ff80, true);
            arena.Actors(ChillingPuffs, 0xffff8040, true);
            arena.Actors(FizzlingPuffs, 0xff40c0c0, true);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.BracingSudsPuff:
                    BracingPuffs.Add(actor);
                    ChillingPuffs.Remove(actor);
                    FizzlingPuffs.Remove(actor);
                    break;
                case SID.ChillingSudsPuff:
                    BracingPuffs.Remove(actor);
                    ChillingPuffs.Add(actor);
                    FizzlingPuffs.Remove(actor);
                    break;
                case SID.FizzlingSudsPuff:
                    BracingPuffs.Remove(actor);
                    ChillingPuffs.Remove(actor);
                    FizzlingPuffs.Add(actor);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.BracingSudsPuff:
                    BracingPuffs.Remove(actor);
                    break;
                case SID.ChillingSudsPuff:
                    ChillingPuffs.Remove(actor);
                    break;
                case SID.FizzlingSudsPuff:
                    FizzlingPuffs.Remove(actor);
                    break;
            }
        }
    }
}
