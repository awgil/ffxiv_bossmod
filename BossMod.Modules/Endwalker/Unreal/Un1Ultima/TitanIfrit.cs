namespace BossMod.Endwalker.Unreal.Un1Ultima;

// both phases use radiant plumes
class TitanIfrit(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, AOEShapeCircle)> _activeLocationTargetedAOEs = [];
    private readonly List<Actor> _crimsonCyclone = [];

    private static readonly AOEShapeCircle _aoeRadiantPlume = new(8);
    private static readonly AOEShapeCircle _aoeWeightOfLand = new(6);
    private static readonly AOEShapeCircle _aoeEruption = new(8);
    private static readonly AOEShapeRect _aoeCrimsonCyclone = new(38, 6);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activeLocationTargetedAOEs.Any(e => e.Item2.Check(actor.Position, e.Item1.CastInfo!.LocXZ)) || _crimsonCyclone.Any(a => _aoeCrimsonCyclone.Check(actor.Position, a)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (a, aoe) in _activeLocationTargetedAOEs)
            aoe.Draw(Arena, a.CastInfo!.LocXZ);
        foreach (var a in _crimsonCyclone)
            _aoeCrimsonCyclone.Draw(Arena, a);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RadiantPlume:
                _activeLocationTargetedAOEs.Add((caster, _aoeRadiantPlume));
                break;
            case AID.WeightOfTheLand:
                _activeLocationTargetedAOEs.Add((caster, _aoeWeightOfLand));
                break;
            case AID.Eruption:
                _activeLocationTargetedAOEs.Add((caster, _aoeEruption));
                break;
            case AID.CrimsonCyclone:
                _crimsonCyclone.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RadiantPlume:
            case AID.WeightOfTheLand:
            case AID.Eruption:
                _activeLocationTargetedAOEs.RemoveAll(e => e.Item1 == caster);
                break;
            case AID.CrimsonCyclone:
                _crimsonCyclone.Remove(caster);
                break;
        }
    }
}
