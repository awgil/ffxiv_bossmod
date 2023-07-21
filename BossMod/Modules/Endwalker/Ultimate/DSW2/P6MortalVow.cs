using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6MortalVowApply : Components.UniformStackSpread
    {
        public bool Done { get; private set; }

        public P6MortalVowApply() : base(0, 5, alwaysShowSpreads: true) { }

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true).Where(p => p.Class.IsDD())); // TODO: activation
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.MortalVowApply)
                Done = true;
        }
    }

    // TODO: hints for specific order
    class P6MortalVowPass : Components.UniformStackSpread
    {
        private BitMask _forbidden;

        public P6MortalVowPass() : base(5, 0, 2, 2) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.MortalVow:
                    AddStack(actor, status.ExpireAt, _forbidden);
                    break;
                case SID.MortalAtonement:
                    _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                    foreach (ref var stack in Stacks.AsSpan())
                        stack.ForbiddenPlayers = _forbidden;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.MortalVowPass)
                Stacks.Clear();
        }
    }
}
