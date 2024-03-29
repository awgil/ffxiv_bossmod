namespace BossMod.Endwalker.Ultimate.DSW1;

// TODO: consider adding invuln hint for tether tank?..
class HolyShieldBash : Components.BaitAwayTethers
{
    public HolyShieldBash() : base(new AOEShapeRect(80, 4), (uint)TetherID.HolyBladedance, ActionID.MakeSpell(AID.HolyShieldBash))
    {
        BaiterPriority = PlayerPriority.Danger;
    }

    public override void Init(BossModule module)
    {
        // TODO: consider selecting specific tank rather than any
        ForbiddenPlayers = module.Raid.WithSlot(true).WhereActor(a => a.Role != Role.Tank).Mask();
    }
}

// note: this is not really a 'bait', but component works well enough
class HolyBladedance : Components.GenericBaitAway
{
    public HolyBladedance() : base(ActionID.MakeSpell(AID.HolyBladedanceAOE)) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HolyShieldBash && module.WorldState.Actors.Find(spell.MainTargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, new AOEShapeCone(16, 45.Degrees())));
    }
}

class Heavensblaze : Components.StackWithCastTargets
{
    public Heavensblaze() : base(ActionID.MakeSpell(AID.Heavensblaze), 4, 7) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        // bladedance target shouldn't stack
        if ((AID)spell.Action.ID == AID.HolyShieldBash)
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers.Set(module.Raid.FindSlot(spell.MainTargetID));
    }
}
