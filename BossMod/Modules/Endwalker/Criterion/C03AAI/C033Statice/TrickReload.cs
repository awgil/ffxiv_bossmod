using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice
{
    class TrickReload : BossComponent
    {
        public bool FirstStack { get; private set; }
        public int SafeSlice { get; private set; }
        public int NumLoads { get; private set; }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (SafeSlice > 0)
                hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> {SafeSlice} -> {(FirstStack ? "spread" : "stack")}");
            else if (NumLoads > 0)
                hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> ???");
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    class Trapshooting : Components.UniformStackSpread
    {
        public int NumResolves { get; private set; }
        private TrickReload? _reload;

        public Trapshooting() : base(6, 6, 4, alwaysShowSpreads: true) { }

        public override void Init(BossModule module)
        {
            _reload = module.FindComponent<TrickReload>();
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NTrapshooting1 or AID.NTrapshooting2 or AID.STrapshooting1 or AID.STrapshooting2 && _reload != null)
            {
                bool stack = NumResolves == 0 ? _reload.FirstStack : !_reload.FirstStack;
                if (stack)
                {
                    var target = module.Raid.WithoutSlot().FirstOrDefault(); // TODO: dunno how target is selected...
                    if (target != null)
                        AddStack(target, spell.NPCFinishAt.AddSeconds(4.1f));
                }
                else
                {
                    AddSpreads(module.Raid.WithoutSlot(true), spell.NPCFinishAt.AddSeconds(4.1f));
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    class TriggerHappy : Components.SelfTargetedAOEs
    {
        public TriggerHappy(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(40, 30.Degrees())) { }
    }
    class NTriggerHappy : TriggerHappy { public NTriggerHappy() : base(AID.NTriggerHappyAOE) { } }
    class STriggerHappy : TriggerHappy { public STriggerHappy() : base(AID.STriggerHappyAOE) { } }
}
