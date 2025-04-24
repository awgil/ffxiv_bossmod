﻿namespace BossMod.Stormblood.Ultimate.UWU;

class P4CeruleumVent(BossModule module) : Components.GenericAOEs(module, AID.CeruleumVent)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(14);

    public bool Active => _source != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Position, _source.Rotation, _activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.UltimaWeapon && id == 0x1E43)
        {
            _source = actor;
            _activation = WorldState.FutureTime(10.1f);
        }
    }
}
