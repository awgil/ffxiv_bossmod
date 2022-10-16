using System.Linq;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class RearingRampageSecond : Components.CastCounter
    {
        public RearingRampageSecond() : base(ActionID.MakeSpell(AID.RearingRampageSecond)) { }
    }

    class RearingRampageLast : Components.CastCounter
    {
        public RearingRampageLast() : base(ActionID.MakeSpell(AID.RearingRampageLast)) { }
    }

    class UpliftStompDead : Components.StackSpread
    {
        public int NumUplifts { get; private set; }
        public int NumStomps { get; private set; }
        public int[] OrderPerSlot = new int[PartyState.MaxPartySize]; // 0 means not yet known

        public UpliftStompDead() : base(6, 6, 2, 2, true) { }

        public override void Init(BossModule module)
        {
            SpreadMask = module.Raid.WithSlot(true).Mask();
        }

        public override void Update(BossModule module)
        {
            if (SpreadMask.None())
            {
                StackMask.Reset();
                if (module.Raid.WithSlot().Any())
                    StackMask.Set(module.Raid.WithSlot().MaxBy(ia => (module.PrimaryActor.Position - ia.Item2.Position).LengthSq()).Item1);
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (OrderPerSlot[slot] > 0)
            {
                hints.Add($"Bait order: {OrderPerSlot[slot]}", false);
            }

            if (SpreadMask.Any())
            {
                // default implementation is fine during uplifts
                base.AddHints(module, slot, actor, hints, movementHints);
            }
            else
            {
                // custom hints for baiting stomps
                bool isBaiting = StackMask[slot] || module.Raid.WithSlot().IncludedInMask(StackMask).InRadius(actor.Position, StackRadius).Any();
                bool shouldBait = OrderPerSlot[slot] == NumStomps + 1;
                hints.Add(shouldBait ? "Bait jump!" : "Avoid jump!", isBaiting != shouldBait);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Uplift:
                    int slot = module.Raid.FindSlot(spell.MainTargetID);
                    if (slot >= 0)
                    {
                        SpreadMask.Clear(slot);
                        OrderPerSlot[slot] = 4 - SpreadMask.NumSetBits() / 2;
                    }
                    ++NumUplifts;
                    break;
                case AID.StompDeadAOE:
                    ++NumStomps;
                    break;
            }
        }
    }
}
