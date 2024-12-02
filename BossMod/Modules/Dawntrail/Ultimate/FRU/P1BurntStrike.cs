namespace BossMod.Dawntrail.Ultimate.FRU;

class P1TurnOfHeavensBurntStrikeFire(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TurnOfHeavensBurntStrikeFire), new AOEShapeRect(40, 5, 40));
class P1TurnOfHeavensBurntStrikeLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TurnOfHeavensBurntStrikeLightning), new AOEShapeRect(40, 5, 40));
class P1TurnOfHeavensBurnout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TurnOfHeavensBurnout), new AOEShapeRect(40, 10, 40));
class P1ExplosionBurntStrikeFire(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExplosionBurntStrikeFire), new AOEShapeRect(40, 5, 40));
class P1ExplosionBurntStrikeLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExplosionBurntStrikeLightning), new AOEShapeRect(40, 5, 40));
class P1ExplosionBurnout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExplosionBurnout), new AOEShapeRect(40, 10, 40));

class P1Blastburn(BossModule module) : Components.Knockback(module, default, true)
{
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster != null)
        {
            var dir = _caster.CastInfo?.Rotation ?? _caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - _caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            yield return new(_caster.Position, 15, Module.CastFinishAt(_caster.CastInfo), null, dir, kind);
        }
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
        if ((AID)spell.Action.ID is AID.TurnOfHeavensBlastburn or AID.ExplosionBlastburn)
        {
            _caster = null;
            ++NumCasts;
        }
    }
}
