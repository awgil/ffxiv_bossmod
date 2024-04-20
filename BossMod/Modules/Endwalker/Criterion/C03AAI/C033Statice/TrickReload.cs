namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class TrickReload(BossModule module) : BossComponent(module)
{
    public bool FirstStack { get; private set; }
    public int SafeSlice { get; private set; }
    public int NumLoads { get; private set; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (SafeSlice > 0)
            hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> {SafeSlice} -> {(FirstStack ? "spread" : "stack")}");
        else if (NumLoads > 0)
            hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> ???");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LockedAndLoaded:
                ++NumLoads;
                break;
            case AID.Misload:
                if (NumLoads == 0)
                    FirstStack = true;
                else if (SafeSlice == 0)
                    SafeSlice = NumLoads;
                ++NumLoads;
                break;
        }
    }
}

class Trapshooting(BossModule module) : Components.UniformStackSpread(module, 6, 6, 4, alwaysShowSpreads: true)
{
    public int NumResolves { get; private set; }
    private readonly TrickReload? _reload = module.FindComponent<TrickReload>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NTrapshooting1 or AID.NTrapshooting2 or AID.STrapshooting1 or AID.STrapshooting2 && _reload != null)
        {
            bool stack = NumResolves == 0 ? _reload.FirstStack : !_reload.FirstStack;
            if (stack)
            {
                var target = Raid.WithoutSlot().FirstOrDefault(); // TODO: dunno how target is selected...
                if (target != null)
                    AddStack(target, spell.NPCFinishAt.AddSeconds(4.1f));
            }
            else
            {
                AddSpreads(Raid.WithoutSlot(true), spell.NPCFinishAt.AddSeconds(4.1f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NTrapshootingStack:
            case AID.STrapshootingStack:
                if (Stacks.Count > 0)
                {
                    Stacks.Clear();
                    ++NumResolves;
                }
                break;
            case AID.NTrapshootingSpread:
            case AID.STrapshootingSpread:
                if (Spreads.Count > 0)
                {
                    Spreads.Clear();
                    ++NumResolves;
                }
                break;
        }
    }
}

class TriggerHappy(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 30.Degrees()));
class NTriggerHappy(BossModule module) : TriggerHappy(module, AID.NTriggerHappyAOE);
class STriggerHappy(BossModule module) : TriggerHappy(module, AID.STriggerHappyAOE);
