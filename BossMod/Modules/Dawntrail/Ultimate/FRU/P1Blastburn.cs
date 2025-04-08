namespace BossMod.Dawntrail.Ultimate.FRU;

class P1Blastburn(BossModule module) : Components.Knockback(module, default, true)
{
    private Actor? _caster;
    private bool _aoeDone;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster != null)
        {
            var dir = _caster.CastInfo?.Rotation ?? _caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - _caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            yield return new(_caster.Position, 15, Module.CastFinishAt(_caster.CastInfo), null, dir, kind);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // don't show kb hints until aoe is done
        if (_aoeDone)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster != null)
            hints.AddForbiddenZone(ShapeContains.InvertedRect(_caster.Position, _caster.CastInfo?.Rotation ?? _caster.Rotation, 40, 40, 2 + (_aoeDone ? 0 : 5)), Module.CastFinishAt(_caster.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TurnOfHeavensBlastburn or AID.ExplosionBlastburn)
        {
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TurnOfHeavensBlastburn:
            case AID.ExplosionBlastburn:
                _caster = null;
                ++NumCasts;
                break;
            case AID.TurnOfHeavensBurntStrikeFire:
            case AID.ExplosionBurntStrikeFire:
                _aoeDone = true;
                break;
        }
    }
}
