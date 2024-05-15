namespace BossMod.Endwalker.Variant.V01SS.V012Silkie;

class V012PuffTracker(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> BracingPuffs = [];
    private readonly List<Actor> ChillingPuffs = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(BracingPuffs, 0xff80ff80, true);
        Arena.Actors(ChillingPuffs, 0xffff8040, true);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BracingSudsPuff:
                BracingPuffs.Add(actor);
                ChillingPuffs.Remove(actor);
                break;
            case SID.ChillingSudsPuff:
                BracingPuffs.Remove(actor);
                ChillingPuffs.Add(actor);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BracingSudsPuff:
                BracingPuffs.Remove(actor);
                break;
            case SID.ChillingSudsPuff:
                ChillingPuffs.Remove(actor);
                break;
        }
    }
}
