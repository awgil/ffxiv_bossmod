namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Lightwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Waves = [];
    private static readonly AOEShapeRect WaveAOE = new(16, 8, 12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Waves.Count > 0)
            foreach (var w in Waves)
                yield return new(WaveAOE, w.Position, w.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.RayOfLight && !Waves.Contains(caster))
            Waves.Add(caster);
    }
}
