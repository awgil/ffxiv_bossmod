namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class FocusedTremorLarge : Components.SelfTargetedAOEs
{
    public FocusedTremorLarge() : base(ActionID.MakeSpell(AID.FocusedTremorAOELarge), new AOEShapeRect(10, 10, 10), 2) { }
}

class ForcefulStrike : Components.SelfTargetedAOEs
{
    public ForcefulStrike() : base(ActionID.MakeSpell(AID.ForcefulStrike), new AOEShapeRect(44, 24)) { }
}

// combined with flailing strike, first bait should be into first square
class FocusedTremorSmall : Components.SelfTargetedAOEs
{
    public FocusedTremorSmall() : base(ActionID.MakeSpell(AID.FocusedTremorAOESmall), new AOEShapeRect(5, 5, 5), 1)
    {
        Color = ArenaColor.SafeFromAOE;
        Risky = false;
    }

    public void Activate()
    {
        Color = ArenaColor.AOE;
        Risky = true;
        MaxCasts = 3;
    }
}

class FlailingStrikeBait : Components.BaitAwayTethers
{
    public FlailingStrikeBait() : base(new AOEShapeCone(40, 30.Degrees()), (uint)TetherID.FlailingStrike) { }
}

class FlailingStrike : Components.GenericRotatingAOE
{
    private static readonly AOEShapeCone _shape = new(60, 30.Degrees());

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlailingStrikeFirst)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, 60.Degrees(), spell.NPCFinishAt, 1.6f, 6, 3));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlailingStrikeRest && Sequences.Count > 0)
        {
            AdvanceSequence(0, module.WorldState.CurrentTime);
        }
    }
}
