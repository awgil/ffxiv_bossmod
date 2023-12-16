using System;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P5NearDistantWorld : Components.GenericStackSpread
    {
        public int NumNearJumpsDone { get; private set; }
        public int NumDistantJumpsDone { get; private set; }
        public Actor? NearWorld;
        public Actor? DistantWorld;
        private BitMask _completedJumps;
        private BitMask _targets;
        private BitMask _risky;
        private DateTime _firstActivation;

        public P5NearDistantWorld() : base(true) { }

        public override void Update(BossModule module)
        {
            Spreads.Clear();
            _risky.Reset();
            _targets = _completedJumps;
            AddChain(module, NearWorld, NumNearJumpsDone, true);
            AddChain(module, DistantWorld, NumDistantJumpsDone, false);

            base.Update(module);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            if (_risky[slot])
                hints.Add("Avoid baiting jump!");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.HelloNearWorld:
                    NearWorld = actor;
                    _firstActivation = status.ExpireAt;
                    break;
                case SID.HelloDistantWorld:
                    DistantWorld = actor;
                    _firstActivation = status.ExpireAt;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.HelloNearWorld:
                case AID.HelloNearWorldJump:
                    ++NumNearJumpsDone;
                    var nearSlot = module.Raid.FindSlot(spell.MainTargetID);
                    _completedJumps.Set(nearSlot);
                    NearWorld = module.Raid[nearSlot];
                    break;
                case AID.HelloDistantWorld:
                case AID.HelloDistantWorldJump:
                    ++NumDistantJumpsDone;
                    var distantSlot = module.Raid.FindSlot(spell.MainTargetID);
                    _completedJumps.Set(distantSlot);
                    DistantWorld = module.Raid[distantSlot];
                    break;
            }
        }

        protected void Reset(Actor? near, Actor? distant, DateTime activation)
        {
            NumNearJumpsDone = NumDistantJumpsDone = 0;
            NearWorld = near;
            DistantWorld = distant;
            _completedJumps.Reset();
            _firstActivation = activation;
        }

        private void AddChain(BossModule module, Actor? start, int numDone, bool close)
        {
            if (numDone == 0)
            {
                if (start != null)
                    AddSpread(module, start, 8, 0);
            }
            if (numDone <= 1 && start != null)
            {
                start = close ? module.Raid.WithoutSlot().Exclude(start).Closest(start.Position) : module.Raid.WithoutSlot().Exclude(start).Farthest(start.Position);
                if (start != null)
                    AddSpread(module, start, 4, 1);
            }
            if (numDone <= 2 && start != null)
            {
                start = close ? module.Raid.WithoutSlot().Exclude(start).Closest(start.Position) : module.Raid.WithoutSlot().Exclude(start).Farthest(start.Position);
                if (start != null)
                    AddSpread(module, start, 4, 2);
            }
        }

        private void AddSpread(BossModule module, Actor target, float radius, int order)
        {
            Spreads.Add(new(target, radius, _firstActivation.AddSeconds(order * 1.0)));
            var slot = module.Raid.FindSlot(target.InstanceID);
            if (_targets[slot])
                _risky.Set(slot);
            else
                _targets.Set(slot);
        }
    }
}
