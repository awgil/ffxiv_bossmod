namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class FulminousField(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;

    public bool Active => _activation != default;

    private static readonly AOEShapeCone _shape = new(30, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            for (int i = 0; i < 8; ++i)
                yield return new(_shape, Module.Center, _dir + i * 45.Degrees(), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FulminousField)
        {
            _dir = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Active && (AID)spell.Action.ID is AID.FulminousField or AID.FulminousFieldRest)
        {
            ++NumCasts;
            _dir = caster.Rotation + 22.5f.Degrees();
            _activation = WorldState.FutureTime(3);
        }
    }
}

class ConductionPoint : Components.UniformStackSpread
{
    public ConductionPoint(BossModule module) : base(module, 0, 6)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(12));
    }
}

class ForkedFissures : Components.GenericWildCharge
{
    public ForkedFissures(BossModule module) : base(module, 5, AID.ForkedFissures, 40)
    {
        Array.Fill(PlayerRoles, PlayerRole.Share);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ConductionPoint:
                Source = caster;
                if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
                    PlayerRoles[slot] = PlayerRole.TargetNotFirst;
                break;
            case AID.ForkedFissures:
                ++NumCasts;
                break;
        }
    }
}
