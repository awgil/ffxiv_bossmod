﻿namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster;

class CastShadow(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> FirstAOECasters = [];
    public List<Actor> SecondAOECasters = [];

    private static readonly AOEShape _shape = new AOEShapeCone(65, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return (FirstAOECasters.Count > 0 ? FirstAOECasters : SecondAOECasters).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        ListForAction(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        ListForAction(spell.Action)?.Remove(caster);
    }

    private List<Actor>? ListForAction(ActionID action) => (AID)action.ID switch
    {
        AID.NCastShadowAOE1 or AID.SCastShadowAOE1 => FirstAOECasters,
        AID.NCastShadowAOE2 or AID.SCastShadowAOE2 => SecondAOECasters,
        _ => null
    };
}
