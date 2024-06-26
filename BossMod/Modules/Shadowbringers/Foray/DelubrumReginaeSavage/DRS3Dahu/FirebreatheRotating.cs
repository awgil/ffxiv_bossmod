namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class FirebreatheRotating(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FirebreatheRotating)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, spell.NPCFinishAt.AddSeconds(0.7f), 2, 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FirebreatheRotatingAOE && Sequences.Count > 0)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var angle = (IconID)iconID switch
        {
            IconID.FirebreatheCW => -90.Degrees(),
            IconID.FirebreatheCCW => 90.Degrees(),
            _ => default
        };
        if (angle != default)
            _increment = angle;
    }
}
