namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class AlexandrianThunderII(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _rotation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.ThunderCCW:
                _rotation = 10.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Increment = _rotation;
                break;
            case IconID.ThunderCW:
                _rotation = -10.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Increment = _rotation;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AlexandrianThunderIIStart)
            Sequences.Add(new(new AOEShapeCone(24, 22.5f.Degrees()), caster.Position, caster.Rotation, _rotation, Module.CastFinishAt(spell), 1, 15, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AlexandrianThunderIIStart or AID.AlexandrianThunderIIRepeat)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}

class Bloom1AlexandrianThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AlexandrianThunderIII, AID.AlexandrianThunderIII, 4, 5)
{
    private readonly Tiles _tiles = module.FindComponent<Tiles>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Spreads.Count > 0)
            hints.AddForbiddenZone(_tiles.TileShape(), Spreads[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count > 0 && _tiles.InActiveTile(actor))
            hints.Add($"GTFO from rose tile!");
    }
}
