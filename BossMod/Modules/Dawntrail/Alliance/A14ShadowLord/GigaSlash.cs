namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class GigaSlash(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone[] _shapes = [new(60, 112.5f.Degrees()), new(60, 135.Degrees()), new(60, 105.Degrees())];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle if there is next imminent aoe from same origin
        if (_aoes.Count > 1 && _aoes[0].Origin == _aoes[1].Origin)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(_aoes[0].Origin, 3), _aoes[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GigaSlashL:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation + 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                break;
            case AID.GigaSlashR:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation - 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                break;
            case AID.GigaSlashNightfallLRF:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation + 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                _aoes.Add(new(_shapes[2], caster.Position, spell.Rotation, Module.CastFinishAt(spell, 5.2f)));
                break;
            case AID.GigaSlashNightfallLRB:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation + 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                _aoes.Add(new(_shapes[2], caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 5.2f)));
                break;
            case AID.GigaSlashNightfallRLF:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation - 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                _aoes.Add(new(_shapes[2], caster.Position, spell.Rotation, Module.CastFinishAt(spell, 5.2f)));
                break;
            case AID.GigaSlashNightfallRLB:
                _aoes.Add(new(_shapes[0], caster.Position, spell.Rotation - 67.5f.Degrees(), Module.CastFinishAt(spell, 1)));
                _aoes.Add(new(_shapes[1], caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 3.1f)));
                _aoes.Add(new(_shapes[2], caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 5.2f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GigaSlashLAOE1 or AID.GigaSlashRAOE2 or AID.GigaSlashRAOE1 or AID.GigaSlashLAOE2 or AID.GigaSlashNightfallFAOE3 or AID.GigaSlashNightfallBAOE3
            or AID.GigaSlashNightfallLAOE1 or AID.GigaSlashNightfallRAOE2 or AID.GigaSlashNightfallRAOE1 or AID.GigaSlashNightfallLAOE2)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
