namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

[SkipLocalsInit]
sealed class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(11)];
    private static readonly AOEShapeRect rect = new(4f, 25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MagicBullet)
        {
            var rot = actor.Rotation;
            _aoes.Add(new(rect, (actor.Position - 2f * rot.ToDirection()).Quantized(), rot, WorldState.FutureTime(9.8d), actorID: actor.InstanceID));
        }
    }
}
