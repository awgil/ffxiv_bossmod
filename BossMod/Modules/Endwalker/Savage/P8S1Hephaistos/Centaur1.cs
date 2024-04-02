namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class RearingRampageSecond : Components.CastCounter
{
    public RearingRampageSecond() : base(ActionID.MakeSpell(AID.RearingRampageSecond)) { }
}

class RearingRampageLast : Components.CastCounter
{
    public RearingRampageLast() : base(ActionID.MakeSpell(AID.RearingRampageLast)) { }
}

class UpliftStompDead : Components.UniformStackSpread
{
    public int NumUplifts { get; private set; }
    public int NumStomps { get; private set; }
    public int[] OrderPerSlot = new int[PartyState.MaxPartySize]; // 0 means not yet known

    public UpliftStompDead() : base(6, 6, 2, 2, true) { }

    public override void Init(BossModule module)
    {
        AddSpreads(module.Raid.WithoutSlot(true));
    }

    public override void Update(BossModule module)
    {
        if (Spreads.Count == 0)
        {
            Stacks.Clear();
            if (module.Raid.WithoutSlot().Farthest(module.PrimaryActor.Position) is var target && target != null)
                AddStack(target);
        }
        base.Update(module);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (OrderPerSlot[slot] > 0)
        {
            hints.Add($"Bait order: {OrderPerSlot[slot]}", false);
        }

        if (Spreads.Count > 0)
        {
            // default implementation is fine during uplifts
            base.AddHints(module, slot, actor, hints, movementHints);
        }
        else
        {
            // custom hints for baiting stomps
            bool isBaiting = Stacks.Any(s => actor.Position.InCircle(s.Target.Position, s.Radius));
            bool shouldBait = OrderPerSlot[slot] == NumStomps + 1;
            hints.Add(shouldBait ? "Bait jump!" : "Avoid jump!", isBaiting != shouldBait);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Uplift:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                int slot = module.Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                {
                    OrderPerSlot[slot] = 4 - Spreads.Count / 2;
                }
                ++NumUplifts;
                break;
            case AID.StompDeadAOE:
                ++NumStomps;
                break;
        }
    }
}
